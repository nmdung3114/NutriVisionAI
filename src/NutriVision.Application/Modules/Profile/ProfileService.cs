using Microsoft.EntityFrameworkCore;
using NutriVision.Application.Abstractions;
using NutriVision.Application.Common;
using NutriVision.Application.Modules.Audit;
using NutriVision.Application.Settings;
using NutriVision.Domain.Constants;
using NutriVision.Domain.Entities;
using NutriVision.Domain.Services;

namespace NutriVision.Application.Modules.Profile;

public interface IProfileService
{
    Task<ProfileDto> GetAsync(CancellationToken ct = default);
    Task<ProfileDto> UpdateAsync(UpdateProfileRequest request, CancellationToken ct = default);
    Task<AvatarResult> UpdateAvatarAsync(Stream content, long length, CancellationToken ct = default);
    /// <summary>Mục tiêu calo hiện tại của người dùng (mặc định 2000 khi chưa có hồ sơ).</summary>
    Task<decimal> GetCalorieTargetAsync(Guid userId, CancellationToken ct = default);
}

public class ProfileService(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IFileStorage storage,
    IFileUrlSigner signer,
    IImageInspector imageInspector,
    IAuditService audit,
    AppClock clock) : IProfileService
{
    public async Task<ProfileDto> GetAsync(CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var user = await db.NguoiDungs.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct) ?? throw new UnauthorizedAppException();
        var hs = await db.HoSoNguoiDungs.AsNoTracking().FirstOrDefaultAsync(h => h.NguoiDungId == userId, ct);
        return ToDto(user, hs);
    }

    public async Task<ProfileDto> UpdateAsync(UpdateProfileRequest r, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var user = await db.NguoiDungs.FirstOrDefaultAsync(u => u.Id == userId, ct) ?? throw new UnauthorizedAppException();
        var hs = await db.HoSoNguoiDungs.FirstOrDefaultAsync(h => h.NguoiDungId == userId, ct);
        var now = clock.UtcNow;

        var tuoi = HealthMetricsCalculator.TinhTuoi(r.NgaySinh, clock.Today);
        var m = HealthMetricsCalculator.Tinh(r.GioiTinh, r.ChieuCao, r.CanNang, tuoi, r.MucDoVanDong);
        var mucTieu = r.MucTieuCalo ?? Math.Round(m.Tdee, 0, MidpointRounding.AwayFromZero);

        if (hs is null)
        {
            hs = new HoSoNguoiDung { NguoiDungId = userId };
            db.HoSoNguoiDungs.Add(hs);
        }
        hs.NgaySinh = r.NgaySinh;
        hs.GioiTinh = r.GioiTinh;
        hs.ChieuCao = r.ChieuCao;
        hs.CanNang = r.CanNang;
        hs.MucDoVanDong = r.MucDoVanDong;
        hs.ChiSoBMI = m.Bmi;
        hs.ChiSoBMR = m.Bmr;
        hs.ChiSoTDEE = m.Tdee;
        hs.MucTieuCalo = mucTieu;
        hs.NgayCapNhat = now;

        user.HoTen = r.HoTen.Trim();
        user.NgayCapNhat = now;

        // Cập nhật mục tiêu của hôm nay để dashboard phản ánh ngay.
        var today = clock.Today;
        var homNay = await db.TongHopDinhDuongNgays.FirstOrDefaultAsync(t => t.NguoiDungId == userId && t.Ngay == today, ct);
        if (homNay is not null) { homNay.MucTieuCaloNgay = mucTieu; homNay.NgayCapNhat = now; }

        audit.Ghi(HanhDong.CapNhatHoSo, "HoSoNguoiDung", hs.Id.ToString(), $"Cập nhật hồ sơ: BMI {m.Bmi}, BMR {m.Bmr}, TDEE {m.Tdee}");
        await db.SaveChangesAsync(ct);
        return ToDto(user, hs);
    }

    public async Task<AvatarResult> UpdateAvatarAsync(Stream content, long length, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        if (length > UploadLimits.AvatarMaxBytes)
            throw new AppException(ErrorCodes.FileTooLarge, "Ảnh không được vượt quá 5MB.", 413);

        var hs = await db.HoSoNguoiDungs.FirstOrDefaultAsync(h => h.NguoiDungId == userId, ct)
                 ?? throw new AppException(ErrorCodes.ProfileRequired, "Vui lòng cập nhật hồ sơ cá nhân trước khi đổi ảnh đại diện.", 400);

        var info = await imageInspector.InspectAsync(content, ct)
                   ?? throw new AppException(ErrorCodes.InvalidImage, "Định dạng ảnh không được hỗ trợ (chỉ nhận JPG, PNG, WEBP).", 400);
        content.Position = 0;

        var saved = await storage.SaveAsync($"avatars/{userId:N}", content, info.Extension, ct);
        var old = hs.DuongDanAnhDaiDien;
        hs.DuongDanAnhDaiDien = saved.RelativePath;
        hs.NgayCapNhat = clock.UtcNow;
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            await storage.DeleteAsync(saved.RelativePath, CancellationToken.None); // không để ảnh mồ côi (UC-09 7a)
            throw;
        }
        if (!string.IsNullOrEmpty(old)) await storage.DeleteAsync(old, ct);
        return new AvatarResult(signer.Sign(saved.RelativePath)!);
    }

    public async Task<decimal> GetCalorieTargetAsync(Guid userId, CancellationToken ct = default)
    {
        var target = await db.HoSoNguoiDungs.AsNoTracking().Where(h => h.NguoiDungId == userId).Select(h => (decimal?)h.MucTieuCalo).FirstOrDefaultAsync(ct);
        return target is > 0 ? target.Value : HealthMetricsCalculator.DefaultCalorieTarget;
    }

    private ProfileDto ToDto(NguoiDung user, HoSoNguoiDung? hs)
    {
        if (hs is null)
            return new ProfileDto(user.HoTen, user.Email, null, false, null, null, null, null, null, null, null, null, null, null,
                HealthMetricsCalculator.DefaultCalorieTarget, null);

        return new ProfileDto(user.HoTen, user.Email, signer.Sign(hs.DuongDanAnhDaiDien), true,
            hs.NgaySinh, HealthMetricsCalculator.TinhTuoi(hs.NgaySinh, clock.Today), hs.GioiTinh, hs.ChieuCao, hs.CanNang, hs.MucDoVanDong,
            hs.ChiSoBMI, HealthMetricsCalculator.PhanLoaiBmi(hs.ChiSoBMI), hs.ChiSoBMR, hs.ChiSoTDEE, hs.MucTieuCalo, hs.NgayCapNhat);
    }
}
