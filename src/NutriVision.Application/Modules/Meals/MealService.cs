using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NutriVision.Application.Abstractions;
using NutriVision.Application.Common;
using NutriVision.Application.Modules.Audit;
using NutriVision.Application.Modules.Foods;
using NutriVision.Application.Modules.Profile;
using NutriVision.Application.Settings;
using NutriVision.Domain.Constants;
using NutriVision.Domain.Entities;
using NutriVision.Domain.Services;
using NutriVision.Domain.ValueObjects;

namespace NutriVision.Application.Modules.Meals;

public interface IMealService
{
    Task<MealDto> ScanAsync(Stream image, long length, string loaiBuaAn, DateOnly? ngayAn, CancellationToken ct = default);
    Task<MealDto> CreateManualAsync(MealUpsertRequest request, CancellationToken ct = default);
    Task<MealDto> GetAsync(Guid id, CancellationToken ct = default);
    Task<MealDto> ConfirmAsync(Guid id, MealUpsertRequest request, CancellationToken ct = default);
    Task<MealDto> UpdateAsync(Guid id, MealUpsertRequest request, CancellationToken ct = default);
    Task<MealDto> RevertAsync(Guid id, CancellationToken ct = default);
    Task CancelDraftAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<MealHistoryResult> ListAsync(MealHistoryQuery query, CancellationToken ct = default);
    /// <summary>Ghi nhanh: tạo bữa đã xác nhận mới từ một bữa cũ (không sao chép ảnh).</summary>
    Task<MealDto> RepeatAsync(Guid id, MealRepeatRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<FrequentMealDto>> FrequentAsync(int limit, CancellationToken ct = default);
    /// <summary>Dọn bản nháp bị bỏ dở quá hạn (xóa cả ảnh đã upload – BR-25). Trả về số bản nháp đã dọn.</summary>
    Task<int> CleanupAbandonedDraftsAsync(CancellationToken ct = default);
}

public class MealService(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IFileStorage storage,
    IImageInspector imageInspector,
    IVisionAiClient vision,
    IFoodCatalog catalog,
    IComponentNutritionService nutrition,
    IDailySummaryService dailySummary,
    IProfileService profiles,
    IAuditService audit,
    MealMapper mapper,
    AppClock clock,
    Microsoft.Extensions.Options.IOptions<AppSettings> appOptions,
    ILogger<MealService> logger) : IMealService
{
    // ---------- FR-13..FR-18: quét ảnh bằng AI → tạo bản nháp ----------
    public async Task<MealDto> ScanAsync(Stream image, long length, string loaiBuaAn, DateOnly? ngayAn, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        if (!LoaiBuaAn.IsValid(loaiBuaAn)) throw ValidationAppException.Single("loaiBuaAn", "Loại bữa ăn không hợp lệ (Sang, Trua, Toi, Phu).");
        var today = clock.Today;
        var ngay = ngayAn ?? today;
        if (ngay > today) throw ValidationAppException.Single("ngayAn", "Ngày ăn không được ở tương lai.");

        if (length > UploadLimits.MealImageMaxBytes)
            throw new AppException(ErrorCodes.FileTooLarge, "Ảnh vượt quá 10MB.", 413);

        // BR-19: kiểm tra hợp lệ trước khi gửi AI.
        var buffer = new MemoryStream();
        await image.CopyToAsync(buffer, ct);
        if (buffer.Length > UploadLimits.MealImageMaxBytes)
            throw new AppException(ErrorCodes.FileTooLarge, "Ảnh vượt quá 10MB.", 413);
        buffer.Position = 0;
        var info = await imageInspector.InspectAsync(buffer, ct)
                   ?? throw new AppException(ErrorCodes.InvalidImage, "Định dạng không hỗ trợ. Chỉ nhận ảnh JPG, PNG hoặc WEBP.", 400);
        if (Math.Min(info.Width, info.Height) < UploadLimits.MealImageMinSide)
            throw new AppException(ErrorCodes.InvalidImage, $"Ảnh quá nhỏ ({info.Width}×{info.Height}). Kích thước tối thiểu {UploadLimits.MealImageMinSide}×{UploadLimits.MealImageMinSide} px.", 400);

        buffer.Position = 0;
        var saved = await storage.SaveAsync($"meals/{userId:N}", buffer, info.Extension, ct);

        VisionResult ai;
        try
        {
            ai = await vision.AnalyzeAsync(buffer.ToArray(), info.ContentType, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            await storage.DeleteAsync(saved.RelativePath, CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gọi dịch vụ AI thất bại không mong đợi");
            ai = VisionResult.Fail("Dịch vụ AI đang gặp sự cố. Vui lòng nhập thủ công hoặc thử lại sau.");
        }

        var dish = ai.Success ? ai.Dish : null;
        var ingredients = (dish?.ThanhPhan ?? [])
            .Where(i => !string.IsNullOrWhiteSpace(i.Ten) && i.Gram > 0)
            .Take(30)
            .ToList();
        var aiUsable = dish is not null && ingredients.Count > 0;

        string? notice = null;
        if (!aiUsable) notice = ai.Success
            ? "AI không nhận diện được món ăn trong ảnh. Vui lòng nhập thủ công."
            : ai.Error ?? "Không thể nhận diện bằng AI. Vui lòng nhập thủ công.";

        var now = clock.UtcNow;
        var meal = new BuaAn
        {
            NguoiDungId = userId,
            LoaiBuaAn = loaiBuaAn,
            NgayAn = ngay,
            ThoiGianGhiNhan = now,
            TenMonAn = aiUsable ? Truncate(dish!.TenMonAn.Trim(), 200) : string.Empty,
            DuongDanHinhAnh = saved.RelativePath,
            TaoTuAI = aiUsable,
            NgayTao = now,
            NgayCapNhat = now,
        };

        var lichSu = new LichSuNhanDienAI
        {
            NguoiDungId = userId,
            BuaAn = meal,
            DuongDanHinhAnh = saved.RelativePath,
            ThoiGianXuLyMs = ai.ProcessingMs,
            NgayTao = now,
        };

        try
        {
            if (aiUsable)
            {
                var foods = await catalog.GetActiveAsync(ct);
                var inputs = new List<ComponentInput>();
                var originals = new List<AiOriginalComponent>();
                foreach (var ing in ingredients)
                {
                    var name = Truncate(ing.Ten.Trim(), 200);
                    var gram = Math.Min(ing.Gram, 5000m);
                    var food = FoodMatcher.BestAutoMatch(name, foods);
                    inputs.Add(new ComponentInput(name, food?.Id, gram, "g", null));
                    originals.Add(new AiOriginalComponent(name, gram, "g", gram, food?.Id));
                }

                var computed = await nutrition.ComputeAsync(inputs, allowMissing: true, ct);
                AttachComponents(meal, computed);

                var conf = dish!.DoTinCay is { } c ? Math.Round(Math.Clamp(c, 0m, 1m), 3) : (decimal?)null;
                var original = new AiOriginal(meal.TenMonAn, conf, originals);
                lichSu.DoTinCay = conf;
                lichSu.TenMonAIDoan = meal.TenMonAn;
                lichSu.PhanHoiAIGoc = MealJson.Serialize(original);
            }

            db.BuaAns.Add(meal);
            db.LichSuNhanDienAIs.Add(lichSu);
            audit.Ghi(HanhDong.NhanDienAI, "LichSuNhanDienAI", lichSu.Id.ToString(),
                aiUsable ? $"AI nhận diện '{meal.TenMonAn}' ({ingredients.Count} thành phần, {ai.ProcessingMs} ms)" : $"AI không dùng được: {ai.Error ?? "kết quả rỗng"}");
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            await storage.DeleteAsync(saved.RelativePath, CancellationToken.None);
            throw;
        }

        var target = await profiles.GetCalorieTargetAsync(userId, ct);
        var loaded = await LoadAsync(userId, meal.Id, tracking: false, ct);
        return mapper.ToDto(loaded, target, aiThatBai: !aiUsable, thongBaoAI: notice);
    }

    // ---------- FR-19: nhập tay ----------
    public async Task<MealDto> CreateManualAsync(MealUpsertRequest request, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var computed = await nutrition.ComputeAsync(request.ThanhPhan, false, ct);
        var now = clock.UtcNow;
        var meal = new BuaAn
        {
            NguoiDungId = userId,
            LoaiBuaAn = request.LoaiBuaAn,
            NgayAn = request.NgayAn ?? clock.Today,
            ThoiGianGhiNhan = now,
            TenMonAn = request.TenMonAn.Trim(),
            TaoTuAI = false,
            NgayTao = now,
            NgayCapNhat = now,
        };
        AttachComponents(meal, computed);
        meal.GanTongDinhDuong(NutritionCalculator.Tong(computed.Select(c => c.Nutrition)), now);
        meal.XacNhan(now);

        await db.ExecuteInTransactionAsync(async () =>
        {
            db.BuaAns.Add(meal);
            audit.Ghi(HanhDong.XacNhanBuaAn, "BuaAn", meal.Id.ToString(), $"Thêm bữa nhập tay '{meal.TenMonAn}' ({meal.TongCalo} kcal)");
            await db.SaveChangesAsync(ct);
            await dailySummary.RecomputeAsync(userId, meal.NgayAn, ct);
        }, ct);

        return await GetAsync(meal.Id, ct);
    }

    public async Task<MealDto> RepeatAsync(Guid id, MealRepeatRequest request, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var source = await LoadAsync(userId, id, tracking: false, ct);
        if (!source.DaXacNhan) throw new ConflictException("Chỉ có thể ăn lại bữa đã được xác nhận.");

        var now = clock.UtcNow;
        var meal = new BuaAn
        {
            NguoiDungId = userId,
            LoaiBuaAn = request.LoaiBuaAn ?? source.LoaiBuaAn,
            NgayAn = request.NgayAn ?? clock.Today,
            ThoiGianGhiNhan = now,
            TenMonAn = source.TenMonAn,
            TaoTuAI = false, // bữa mới do người dùng chủ động ghi, không qua AI
            NgayTao = now,
            NgayCapNhat = now,
        };
        // Giữ nguyên giá trị dinh dưỡng đã chốt của từng thành phần (không tính lại theo bảng tham chiếu hiện tại).
        foreach (var t in source.ThanhPhans)
        {
            var copy = new ThanhPhanBuaAn
            {
                BuaAnId = meal.Id, ThucPhamThamChieuId = t.ThucPhamThamChieuId, TenThanhPhan = t.TenThanhPhan,
                KhauPhan = t.KhauPhan, DonViTinh = t.DonViTinh, KhoiLuongQuyDoiGram = t.KhoiLuongQuyDoiGram,
            };
            copy.GanDinhDuong(t.DinhDuong);
            meal.ThanhPhans.Add(copy);
        }
        meal.GanTongDinhDuong(NutritionCalculator.Tong(meal.ThanhPhans.Select(t => t.DinhDuong)), now);
        meal.XacNhan(now);

        await db.ExecuteInTransactionAsync(async () =>
        {
            db.BuaAns.Add(meal);
            audit.Ghi(HanhDong.XacNhanBuaAn, "BuaAn", meal.Id.ToString(), $"Ăn lại '{meal.TenMonAn}' từ bữa {source.Id} ({meal.TongCalo} kcal)");
            await db.SaveChangesAsync(ct);
            await dailySummary.RecomputeAsync(userId, meal.NgayAn, ct);
        }, ct);
        return await GetAsync(meal.Id, ct);
    }

    public async Task<IReadOnlyList<FrequentMealDto>> FrequentAsync(int limit, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        limit = Math.Clamp(limit, 1, 20);
        var since = clock.Today.AddDays(-90);
        var meals = await db.BuaAns.AsNoTracking()
            .Where(b => b.NguoiDungId == userId && b.DaXacNhan && !b.DaXoa && b.NgayAn >= since)
            .Select(b => new { b.Id, b.TenMonAn, b.TongCalo, b.LoaiBuaAn, b.NgayAn, b.ThoiGianGhiNhan, b.DuongDanHinhAnh })
            .ToListAsync(ct);

        return meals
            .GroupBy(m => TextNormalizer.Chuan(m.TenMonAn))
            .Select(g =>
            {
                var latest = g.OrderByDescending(m => m.NgayAn).ThenByDescending(m => m.ThoiGianGhiNhan).First();
                var loai = g.GroupBy(m => m.LoaiBuaAn).OrderByDescending(x => x.Count()).First().Key;
                var anh = g.OrderByDescending(m => m.ThoiGianGhiNhan).Select(m => m.DuongDanHinhAnh).FirstOrDefault(p => p != null);
                return new FrequentMealDto(latest.Id, latest.TenMonAn, g.Count(), latest.TongCalo, loai, latest.NgayAn, mapper.SignPath(anh));
            })
            .OrderByDescending(x => x.SoLan).ThenByDescending(x => x.LanCuoi)
            .Take(limit).ToList();
    }

    public async Task<MealDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var meal = await LoadAsync(userId, id, tracking: false, ct);
        var target = await profiles.GetCalorieTargetAsync(userId, ct);
        return mapper.ToDto(meal, target);
    }

    // ---------- FR-21, FR-27: xác nhận & lưu ----------
    public async Task<MealDto> ConfirmAsync(Guid id, MealUpsertRequest request, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var meal = await LoadAsync(userId, id, tracking: true, ct);
        if (meal.DaXacNhan) throw new ConflictException("Bữa ăn này đã được xác nhận trước đó.");

        var computed = await nutrition.ComputeAsync(request.ThanhPhan, false, ct);
        var now = clock.UtcNow;
        var oldDate = meal.NgayAn;

        await db.ExecuteInTransactionAsync(async () =>
        {
            ApplyUpsert(meal, request, computed, now);
            meal.XacNhan(now);

            if (meal.LichSuNhanDien is { } ls)
            {
                var original = MealJson.Deserialize(ls.PhanHoiAIGoc);
                ls.TenMonNguoiDungChot = meal.TenMonAn;
                ls.NguoiDungCoSua = original is null || MealMapper.IsModified(original, meal.TenMonAn, computed);
            }

            audit.Ghi(HanhDong.XacNhanBuaAn, "BuaAn", meal.Id.ToString(), $"Xác nhận bữa '{meal.TenMonAn}' ({meal.TongCalo} kcal)");
            await db.SaveChangesAsync(ct);
            await dailySummary.RecomputeAsync(userId, meal.NgayAn, ct);
            if (oldDate != meal.NgayAn) await dailySummary.RecomputeAsync(userId, oldDate, ct);
        }, ct);

        return await GetAsync(id, ct);
    }

    // ---------- FR-36: sửa bữa đã lưu ----------
    public async Task<MealDto> UpdateAsync(Guid id, MealUpsertRequest request, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var meal = await LoadAsync(userId, id, tracking: true, ct);
        if (!meal.DaXacNhan) throw new ConflictException("Bữa ăn chưa được xác nhận. Hãy dùng chức năng xác nhận.");

        var computed = await nutrition.ComputeAsync(request.ThanhPhan, false, ct);
        var now = clock.UtcNow;
        var oldDate = meal.NgayAn;

        await db.ExecuteInTransactionAsync(async () =>
        {
            ApplyUpsert(meal, request, computed, now);

            if (meal.LichSuNhanDien is { } ls)
            {
                var original = MealJson.Deserialize(ls.PhanHoiAIGoc);
                ls.TenMonNguoiDungChot = meal.TenMonAn;
                ls.NguoiDungCoSua = original is null || MealMapper.IsModified(original, meal.TenMonAn, computed);
            }

            audit.Ghi(HanhDong.SuaBuaAn, "BuaAn", meal.Id.ToString(), $"Sửa bữa '{meal.TenMonAn}' ({meal.TongCalo} kcal)");
            await db.SaveChangesAsync(ct);
            await dailySummary.RecomputeAsync(userId, meal.NgayAn, ct);
            if (oldDate != meal.NgayAn) await dailySummary.RecomputeAsync(userId, oldDate, ct);
        }, ct);

        return await GetAsync(id, ct);
    }

    // ---------- FR-26: hoàn tác về kết quả AI gốc ----------
    public async Task<MealDto> RevertAsync(Guid id, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var meal = await LoadAsync(userId, id, tracking: true, ct);
        var original = MealJson.Deserialize(meal.LichSuNhanDien?.PhanHoiAIGoc)
                       ?? throw new AppException(ErrorCodes.Validation, "Bữa ăn này không có kết quả AI gốc để hoàn tác.", 400);

        var inputs = original.ThanhPhan
            .Select(o => new ComponentInput(o.TenThanhPhan, o.ThucPhamThamChieuId, o.KhauPhan, o.DonViTinh, null))
            .ToList();
        var computed = await nutrition.ComputeAsync(inputs, allowMissing: !meal.DaXacNhan, ct);
        var now = clock.UtcNow;

        await db.ExecuteInTransactionAsync(async () =>
        {
            ReplaceComponents(meal, computed);
            meal.TenMonAn = original.TenMonAn;
            meal.GanTongDinhDuong(NutritionCalculator.Tong(computed.Select(c => c.Nutrition)), now);
            if (meal.LichSuNhanDien is { } ls)
            {
                ls.NguoiDungCoSua = false;
                if (meal.DaXacNhan) ls.TenMonNguoiDungChot = original.TenMonAn;
            }
            if (meal.DaXacNhan)
                audit.Ghi(HanhDong.SuaBuaAn, "BuaAn", meal.Id.ToString(), "Hoàn tác về kết quả AI gốc");
            await db.SaveChangesAsync(ct);
            if (meal.DaXacNhan) await dailySummary.RecomputeAsync(userId, meal.NgayAn, ct);
        }, ct);

        return await GetAsync(id, ct);
    }

    // ---------- FR-20, BR-24, BR-25: hủy bản nháp ----------
    public async Task CancelDraftAsync(Guid id, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var meal = await LoadAsync(userId, id, tracking: true, ct);
        if (meal.DaXacNhan) throw new ConflictException("Bữa ăn đã được lưu, không thể hủy. Hãy dùng chức năng xóa.");
        await RemoveDraftAsync(meal, ct);
    }

    private async Task RemoveDraftAsync(BuaAn meal, CancellationToken ct)
    {
        var path = meal.DuongDanHinhAnh;
        if (meal.LichSuNhanDien is { } ls) db.LichSuNhanDienAIs.Remove(ls);
        db.BuaAns.Remove(meal); // thành phần xóa theo cascade
        await db.SaveChangesAsync(ct);
        if (!string.IsNullOrEmpty(path)) await storage.DeleteAsync(path, ct);
    }

    // ---------- FR-37: xóa mềm ----------
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var meal = await LoadAsync(userId, id, tracking: true, ct);
        if (!meal.DaXacNhan) throw new ConflictException("Bữa ăn chưa được lưu. Hãy dùng chức năng hủy.");

        await db.ExecuteInTransactionAsync(async () =>
        {
            meal.XoaMem(clock.UtcNow); // BR-35: không xóa vĩnh viễn, ảnh giữ lại theo BR-36
            audit.Ghi(HanhDong.XoaBuaAn, "BuaAn", meal.Id.ToString(), $"Xóa bữa '{meal.TenMonAn}'");
            await db.SaveChangesAsync(ct);
            await dailySummary.RecomputeAsync(userId, meal.NgayAn, ct);
        }, ct);
    }

    // ---------- FR-32..FR-34: lịch sử ----------
    public async Task<MealHistoryResult> ListAsync(MealHistoryQuery q, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var today = clock.Today;
        var to = q.To ?? today;
        var from = q.From ?? to.AddDays(-6);
        if (from > to) throw ValidationAppException.Single("from", "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");
        if (to.DayNumber - from.DayNumber > 366) throw ValidationAppException.Single("from", "Khoảng thời gian tối đa 366 ngày.");
        if (!string.IsNullOrWhiteSpace(q.LoaiBuaAn) && !LoaiBuaAn.IsValid(q.LoaiBuaAn))
            throw ValidationAppException.Single("loaiBuaAn", "Loại bữa ăn không hợp lệ.");
        var (page, pageSize) = Paging.Normalize(q.Page, q.PageSize, 20, 50);

        var query = db.BuaAns.AsNoTracking()
            .Where(b => b.NguoiDungId == userId && b.DaXacNhan && !b.DaXoa && b.NgayAn >= from && b.NgayAn <= to);
        if (!string.IsNullOrWhiteSpace(q.LoaiBuaAn)) query = query.Where(b => b.LoaiBuaAn == q.LoaiBuaAn);

        var total = await query.CountAsync(ct);
        var rows = await query
            .OrderByDescending(b => b.NgayAn).ThenByDescending(b => b.ThoiGianGhiNhan)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(b => new
            {
                b.Id, b.LoaiBuaAn, b.NgayAn, b.ThoiGianGhiNhan, b.TenMonAn, b.DuongDanHinhAnh, b.TaoTuAI,
                b.TongCalo, b.TongChatDam, b.TongTinhBot, b.TongChatBeo, b.TongChatXo, SoThanhPhan = b.ThanhPhans.Count,
            })
            .ToListAsync(ct);

        var days = rows.Select(r => r.NgayAn).Distinct().ToList();
        var summaries = days.Count == 0
            ? new List<TongHopDinhDuongNgay>()
            : await db.TongHopDinhDuongNgays.AsNoTracking().Where(t => t.NguoiDungId == userId && days.Contains(t.Ngay)).ToListAsync(ct);

        return new MealHistoryResult
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Items = rows.Select(r => new MealSummaryDto(r.Id, r.LoaiBuaAn, r.NgayAn, r.ThoiGianGhiNhan, r.TenMonAn, mapperSign(r.DuongDanHinhAnh),
                r.TaoTuAI, new NutritionDto(r.TongCalo, r.TongChatDam, r.TongTinhBot, r.TongChatBeo, r.TongChatXo), r.SoThanhPhan)).ToList(),
            TongHopNgay = summaries.OrderByDescending(s => s.Ngay)
                .Select(s => new DaySummaryDto(s.Ngay, s.TongCalo, s.MucTieuCaloNgay,
                    s.MucTieuCaloNgay > 0 ? Math.Round(s.TongCalo / s.MucTieuCaloNgay * 100m, 1) : 0m, s.SoBuaAnDaGhi)).ToList(),
        };

        string? mapperSign(string? path) => mapper.SignPath(path);
    }

    public async Task<int> CleanupAbandonedDraftsAsync(CancellationToken ct = default)
    {
        var cutoff = clock.UtcNow.AddHours(-Math.Max(1, appOptions.Value.DraftRetentionHours));
        var stale = await db.BuaAns.Include(b => b.LichSuNhanDien)
            .Where(b => !b.DaXacNhan && !b.DaXoa && b.NgayTao < cutoff)
            .Take(200)
            .ToListAsync(ct);

        foreach (var meal in stale)
        {
            try { await RemoveDraftAsync(meal, ct); }
            catch (Exception ex) { logger.LogWarning(ex, "Không dọn được bản nháp {MealId}", meal.Id); }
        }
        return stale.Count;
    }

    // ---------- helpers ----------
    private async Task<BuaAn> LoadAsync(Guid userId, Guid id, bool tracking, CancellationToken ct)
    {
        IQueryable<BuaAn> q = db.BuaAns
            .Include(b => b.ThanhPhans).ThenInclude(t => t.ThucPhamThamChieu)
            .Include(b => b.LichSuNhanDien);
        if (!tracking) q = q.AsNoTracking();

        var meal = await q.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (meal is null || meal.DaXoa) throw new NotFoundException("Không tìm thấy bữa ăn.");
        if (meal.NguoiDungId != userId) throw new ForbiddenException("Bạn không có quyền truy cập bữa ăn này."); // BR-33
        return meal;
    }

    private void ApplyUpsert(BuaAn meal, MealUpsertRequest request, IReadOnlyList<ComputedComponent> computed, DateTimeOffset now)
    {
        meal.LoaiBuaAn = request.LoaiBuaAn;
        meal.NgayAn = request.NgayAn ?? meal.NgayAn;
        meal.TenMonAn = request.TenMonAn.Trim();
        ReplaceComponents(meal, computed);
        meal.GanTongDinhDuong(NutritionCalculator.Tong(computed.Select(c => c.Nutrition)), now);
    }

    private void ReplaceComponents(BuaAn meal, IReadOnlyList<ComputedComponent> computed)
    {
        db.ThanhPhanBuaAns.RemoveRange(meal.ThanhPhans.ToList());
        meal.ThanhPhans.Clear();
        AttachComponents(meal, computed);
        // Bữa ăn đang được theo dõi: thêm tường minh để EF sinh INSERT (khóa Guid gán sẵn sẽ bị hiểu nhầm là bản ghi đã tồn tại).
        foreach (var tp in meal.ThanhPhans) db.ThanhPhanBuaAns.Add(tp);
    }

    private static void AttachComponents(BuaAn meal, IReadOnlyList<ComputedComponent> computed)
    {
        foreach (var c in computed)
        {
            var tp = new ThanhPhanBuaAn
            {
                BuaAnId = meal.Id,
                ThucPhamThamChieuId = c.Food?.Id,
                TenThanhPhan = Truncate(c.Input.TenThanhPhan.Trim(), 200),
                KhauPhan = c.Input.KhauPhan,
                DonViTinh = Truncate(c.Input.DonViTinh.Trim(), 50),
                KhoiLuongQuyDoiGram = c.Gram,
            };
            tp.GanDinhDuong(c.Nutrition);
            meal.ThanhPhans.Add(tp);
        }
        meal.GanTongDinhDuong(NutritionCalculator.Tong(computed.Select(c => c.Nutrition)), meal.NgayCapNhat);
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];
}
