using Microsoft.EntityFrameworkCore;
using NutriVision.Application.Abstractions;
using NutriVision.Application.Common;
using NutriVision.Application.Modules.Audit;
using NutriVision.Domain.Constants;
using NutriVision.Domain.Entities;

namespace NutriVision.Application.Modules.Admin;

public interface IAdminService
{
    Task<PagedResult<AdminUserDto>> ListUsersAsync(string? q, string? trangThai, string? vaiTro, int page, int pageSize, CancellationToken ct = default);
    Task<AdminUserDto> ChangeStatusAsync(Guid id, ChangeStatusRequest request, CancellationToken ct = default);
    Task<AdminUserDto> ChangeRoleAsync(Guid id, ChangeRoleRequest request, CancellationToken ct = default);
    Task<PagedResult<AdminLogDto>> ListLogsAsync(Guid? userId, string? loai, DateOnly? from, DateOnly? to, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<string>> LogTypesAsync(CancellationToken ct = default);
    Task<AdminStatisticsDto> StatisticsAsync(DateOnly? from, DateOnly? to, CancellationToken ct = default);
}

/// <summary>
/// Nghiệp vụ quản trị. Cố ý KHÔNG có truy vấn nào tới bữa ăn chi tiết, ảnh, hồ sơ hay mật khẩu của người dùng
/// (BR-45, BR-46, BR-47): chỉ số liệu đếm tổng hợp.
/// </summary>
public class AdminService(IApplicationDbContext db, ICurrentUser currentUser, IAuditService audit, AppClock clock) : IAdminService
{
    public async Task<PagedResult<AdminUserDto>> ListUsersAsync(string? q, string? trangThai, string? vaiTro, int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = Paging.Normalize(page, pageSize);
        IQueryable<NguoiDung> query = db.NguoiDungs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLowerInvariant();
            query = query.Where(u => u.Email.ToLower().Contains(kw) || u.HoTen.ToLower().Contains(kw));
        }
        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            if (!TrangThaiTaiKhoan.IsValid(trangThai)) throw ValidationAppException.Single("trangThai", "Trạng thái không hợp lệ.");
            query = query.Where(u => u.TrangThai == trangThai);
        }
        if (!string.IsNullOrWhiteSpace(vaiTro))
        {
            if (!VaiTro.IsValid(vaiTro)) throw ValidationAppException.Single("vaiTro", "Vai trò không hợp lệ.");
            query = query.Where(u => u.VaiTro == vaiTro);
        }

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(u => u.NgayTao).ThenBy(u => u.Email)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(u => new AdminUserDto(u.Id, u.Email, u.HoTen, u.VaiTro, u.TrangThai, u.EmailDaXacThuc, u.NgayTao))
            .ToListAsync(ct);
        return new PagedResult<AdminUserDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<AdminUserDto> ChangeStatusAsync(Guid id, ChangeStatusRequest request, CancellationToken ct = default)
    {
        var adminId = currentUser.RequireUserId();
        if (id == adminId)
            throw new AppException(ErrorCodes.SelfActionForbidden, "Bạn không thể tự khóa tài khoản của chính mình.", 400); // BR-13

        var user = await db.NguoiDungs.FirstOrDefaultAsync(u => u.Id == id, ct) ?? throw new NotFoundException("Không tìm thấy người dùng.");
        var now = clock.UtcNow;

        if (request.TrangThai == TrangThaiTaiKhoan.Locked)
        {
            user.DoiTrangThai(TrangThaiTaiKhoan.Locked, now);
            var tokens = await db.TokenLamMois.Where(t => t.NguoiDungId == id && !t.DaThuHoi && !t.DaSuDung).ToListAsync(ct);
            foreach (var t in tokens) t.DaThuHoi = true; // ép đăng xuất mọi thiết bị
            audit.Ghi(HanhDong.AdminKhoaTaiKhoan, "NguoiDung", id.ToString(), $"Khóa tài khoản {user.Email}. Lý do: {request.LyDo ?? "(không nêu)"}");
        }
        else
        {
            // Mở khóa không được bỏ qua bước xác thực email.
            user.DoiTrangThai(user.EmailDaXacThuc ? TrangThaiTaiKhoan.Active : TrangThaiTaiKhoan.Pending, now);
            user.DangNhapThanhCong(now); // reset bộ đếm đăng nhập sai / khóa tạm
            audit.Ghi(HanhDong.AdminMoKhoaTaiKhoan, "NguoiDung", id.ToString(), $"Mở khóa tài khoản {user.Email}");
        }

        await db.SaveChangesAsync(ct);
        return ToDto(user);
    }

    public async Task<AdminUserDto> ChangeRoleAsync(Guid id, ChangeRoleRequest request, CancellationToken ct = default)
    {
        var adminId = currentUser.RequireUserId();
        if (id == adminId)
            throw new AppException(ErrorCodes.SelfActionForbidden, "Bạn không thể tự thay đổi vai trò của chính mình.", 400);

        var user = await db.NguoiDungs.FirstOrDefaultAsync(u => u.Id == id, ct) ?? throw new NotFoundException("Không tìm thấy người dùng.");
        var old = user.VaiTro;
        if (old != request.VaiTro)
        {
            user.DoiVaiTro(request.VaiTro, clock.UtcNow);
            audit.Ghi(HanhDong.AdminDoiVaiTro, "NguoiDung", id.ToString(), $"Đổi vai trò {user.Email}: {old} → {request.VaiTro}");
            await db.SaveChangesAsync(ct);
        }
        return ToDto(user);
    }

    public async Task<PagedResult<AdminLogDto>> ListLogsAsync(Guid? userId, string? loai, DateOnly? from, DateOnly? to, int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = Paging.Normalize(page, pageSize, 30, 100);
        IQueryable<NhatKyHeThong> query = db.NhatKyHeThongs.AsNoTracking();
        if (userId.HasValue) query = query.Where(l => l.NguoiDungThucHienId == userId);
        if (!string.IsNullOrWhiteSpace(loai)) query = query.Where(l => l.LoaiHanhDong == loai);
        if (from.HasValue) { var f = clock.StartOfDay(from.Value); query = query.Where(l => l.NgayTao >= f); }
        if (to.HasValue) { var t = clock.StartOfDay(to.Value.AddDays(1)); query = query.Where(l => l.NgayTao < t); }

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(l => l.NgayTao)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(l => new AdminLogDto(l.Id, l.NguoiDungThucHienId, l.NguoiDungThucHien != null ? l.NguoiDungThucHien.Email : null,
                l.LoaiHanhDong, l.BangTacDong, l.IdDoiTuong, l.ChiTiet, l.DiaChiIP, l.NgayTao))
            .ToListAsync(ct);
        return new PagedResult<AdminLogDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<IReadOnlyList<string>> LogTypesAsync(CancellationToken ct = default) =>
        await db.NhatKyHeThongs.AsNoTracking().Select(l => l.LoaiHanhDong).Distinct().OrderBy(x => x).ToListAsync(ct);

    public async Task<AdminStatisticsDto> StatisticsAsync(DateOnly? fromIn, DateOnly? toIn, CancellationToken ct = default)
    {
        var to = toIn ?? clock.Today;
        var from = fromIn ?? to.AddDays(-29);
        if (from > to) throw ValidationAppException.Single("from", "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");
        if (to.DayNumber - from.DayNumber > 365) throw ValidationAppException.Single("from", "Khoảng thời gian tối đa 366 ngày.");
        var start = clock.StartOfDay(from);
        var end = clock.StartOfDay(to.AddDays(1));

        var tongNguoiDung = await db.NguoiDungs.CountAsync(ct);
        var hoatDong = await db.NguoiDungs.CountAsync(u => u.TrangThai == TrangThaiTaiKhoan.Active, ct);
        var biKhoa = await db.NguoiDungs.CountAsync(u => u.TrangThai == TrangThaiTaiKhoan.Locked, ct);
        var tongBuaAn = await db.BuaAns.CountAsync(b => b.DaXacNhan && !b.DaXoa, ct);

        var aiInRange = db.LichSuNhanDienAIs.AsNoTracking().Where(l => l.NgayTao >= start && l.NgayTao < end);
        var soAnh = await aiInRange.CountAsync(ct);
        var thatBai = await aiInRange.CountAsync(l => l.PhanHoiAIGoc == null, ct);
        var thoiGian = await aiInRange.Where(l => l.ThoiGianXuLyMs != null && l.PhanHoiAIGoc != null).Select(l => (double?)l.ThoiGianXuLyMs).AverageAsync(ct);

        // Độ chính xác: tỷ lệ bữa AI đã xác nhận mà người dùng KHÔNG phải chỉnh sửa.
        var confirmedAi = aiInRange.Where(l => l.PhanHoiAIGoc != null && l.BuaAn != null && l.BuaAn.DaXacNhan && !l.BuaAn.DaXoa);
        var daXacNhan = await confirmedAi.CountAsync(ct);
        var khongSua = await confirmedAi.CountAsync(l => !l.NguoiDungCoSua, ct);
        decimal? tyLe = daXacNhan == 0 ? null : Math.Round(khongSua * 100m / daXacNhan, 1);

        var newUsers = (await db.NguoiDungs.AsNoTracking().Where(u => u.NgayTao >= start && u.NgayTao < end).Select(u => u.NgayTao).ToListAsync(ct))
            .Select(clock.ToLocalDate).GroupBy(d => d).ToDictionary(g => g.Key, g => g.Count());
        var aiDates = (await aiInRange.Select(l => l.NgayTao).ToListAsync(ct))
            .Select(clock.ToLocalDate).GroupBy(d => d).ToDictionary(g => g.Key, g => g.Count());
        var meals = await db.BuaAns.AsNoTracking().Where(b => b.DaXacNhan && !b.DaXoa && b.NgayAn >= from && b.NgayAn <= to)
            .GroupBy(b => b.NgayAn).Select(g => new { Ngay = g.Key, Count = g.Count() }).ToListAsync(ct);
        var mealsDict = meals.ToDictionary(m => m.Ngay, m => m.Count);

        var days = Enumerable.Range(0, to.DayNumber - from.DayNumber + 1).Select(i => from.AddDays(i)).ToList();
        return new AdminStatisticsDto(
            tongNguoiDung, hoatDong, biKhoa, tongBuaAn, soAnh, thatBai, tyLe, thoiGian is null ? null : (int)Math.Round(thoiGian.Value),
            days.Select(d => new DayCountDto(d, newUsers.GetValueOrDefault(d))).ToList(),
            days.Select(d => new DayCountDto(d, mealsDict.GetValueOrDefault(d))).ToList(),
            days.Select(d => new DayCountDto(d, aiDates.GetValueOrDefault(d))).ToList());
    }

    private static AdminUserDto ToDto(NguoiDung u) => new(u.Id, u.Email, u.HoTen, u.VaiTro, u.TrangThai, u.EmailDaXacThuc, u.NgayTao);
}
