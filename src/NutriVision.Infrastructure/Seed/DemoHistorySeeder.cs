using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NutriVision.Application.Common;
using NutriVision.Application.Modules.Meals;
using NutriVision.Domain.Constants;
using NutriVision.Domain.Entities;
using NutriVision.Domain.Services;
using NutriVision.Domain.ValueObjects;
using NutriVision.Infrastructure.Persistence;

namespace NutriVision.Infrastructure.Seed;

/// <summary>
/// Dữ liệu minh họa CHỈ cho tài khoản demo ở môi trường dev: ~30 ngày bữa ăn (từ bảng thực phẩm
/// tham chiếu thật), cân nặng và nước uống, để dashboard/lịch tháng/chuỗi ngày/nhận xét có nội dung.
/// Chỉ chạy khi tài khoản demo chưa có lịch sử cũ; không bao giờ đụng tới tài khoản khác.
/// </summary>
public static class DemoHistorySeeder
{
    public const string DemoEmail = "user@nutrivision.ai";
    private const int Days = 30;

    // Thực đơn mẫu: mỗi bữa gồm (mã thực phẩm, số khẩu phần chuẩn)
    private static readonly Dictionary<string, (string Ten, (string Ma, decimal SoPhan)[] Mon)[]> Menu = new()
    {
        [LoaiBuaAn.Sang] =
        [
            ("Phở bò", [("PHO_BO", 1)]), ("Bánh mì ốp la", [("BANH_MI_TRUNG", 1), ("SUA_TUOI", 1)]),
            ("Xôi mặn", [("XOI_MAN", 1)]), ("Bún bò Huế", [("BUN_BO_HUE", 1)]), ("Cháo gà", [("CHAO_GA", 1)]),
            ("Bánh cuốn", [("BANH_CUON", 1), ("CA_PHE_SUA_DA", 1)]), ("Bánh mì thịt", [("BANH_MI_THIT", 1)]),
        ],
        [LoaiBuaAn.Trua] =
        [
            ("Cơm tấm sườn", [("COM_TAM_SUON", 1), ("DUA_CHUOT", 1)]),
            ("Cơm gà luộc, rau cải", [("COM_TRANG", 1.5m), ("GA_LUOC", 1), ("RAU_CAI_LUOC", 1)]),
            ("Bún chả Hà Nội", [("BUN_CHA", 1), ("RAU_SONG", 1)]),
            ("Cơm cá kho, canh chua", [("COM_TRANG", 1.5m), ("CA_KHO_TO", 1), ("CANH_CHUA_CA", 1)]),
            ("Cơm thịt kho, rau muống", [("COM_TRANG", 1.5m), ("THIT_KHO_TAU", 1), ("RAU_MUONG_XAO", 1)]),
            ("Mì Quảng", [("MI_QUANG", 1)]), ("Cơm gà Hải Nam", [("COM_GA_HAI_NAM", 1)]),
        ],
        [LoaiBuaAn.Toi] =
        [
            ("Cơm cá hấp, súp lơ", [("COM_TRANG", 1), ("CA_HAP_GUNG", 1), ("SUP_LO_XANH", 1)]),
            ("Cơm đậu hũ sốt cà, canh rau ngót", [("COM_TRANG", 1), ("DAU_HU_SOT_CA", 1), ("CANH_RAU_NGOT_THIT_BAM", 1)]),
            ("Bún thịt nướng", [("BUN_THIT_NUONG", 1)]),
            ("Cơm tôm luộc, cải xanh", [("COM_TRANG", 1), ("TOM_LUOC", 1), ("CAI_XANH_XAO", 1)]),
            ("Cơm bò lúc lắc", [("COM_TRANG", 1), ("BO_LUC_LAC", 1), ("CA_CHUA", 1)]),
            ("Gỏi cuốn", [("GOI_CUON", 4)]), ("Cơm gà kho gừng", [("COM_TRANG", 1), ("GA_KHO_GUNG", 1), ("SU_SU_XAO", 1)]),
        ],
        [LoaiBuaAn.Phu] =
        [
            ("Chuối", [("CHUOI", 1)]), ("Sữa chua", [("SUA_CHUA", 1)]), ("Táo", [("TAO", 1)]),
            ("Khoai lang luộc", [("KHOAI_LANG_LUOC", 1)]), ("Sinh tố bơ", [("SINH_TO_BO", 1)]), ("Ổi", [("OI", 1)]),
        ],
    };

    private static readonly (string Loai, int Gio, int Phut, double XacSuat)[] Slots =
        [(LoaiBuaAn.Sang, 7, 15, 0.85), (LoaiBuaAn.Trua, 12, 0, 1.0), (LoaiBuaAn.Toi, 18, 45, 0.95), (LoaiBuaAn.Phu, 15, 30, 0.45)];

    public static async Task SeedAsync(ApplicationDbContext db, IDailySummaryService summaries, AppClock clock, ILogger logger, CancellationToken ct = default)
    {
        var user = await db.NguoiDungs.Include(u => u.HoSo).FirstOrDefaultAsync(u => u.Email == DemoEmail, ct);
        if (user is null) return;
        var today = clock.Today;
        var from = today.AddDays(-Days);
        if (await db.BuaAns.IgnoreQueryFilters().AnyAsync(b => b.NguoiDungId == user.Id && b.NgayAn < today.AddDays(-2), ct))
            return; // đã có lịch sử → không seed

        var foods = await db.ThucPhamThamChieus.AsNoTracking().ToDictionaryAsync(f => f.MaThucPham, ct);
        var target = user.HoSo is { MucTieuCalo: > 0 } hs ? hs.MucTieuCalo : HealthMetricsCalculator.DefaultCalorieTarget;
        var rnd = new Random(2026);
        var now = clock.UtcNow;
        var days = new List<DateOnly>();

        for (var d = from; d < today; d = d.AddDays(1))
        {
            // Bỏ trống 2 ngày giữa tháng để chuỗi ngày có điểm đứt (minh họa chuỗi hiện tại < kỷ lục).
            if (d == today.AddDays(-17) || d == today.AddDays(-16)) continue;
            days.Add(d);
            // Chọn món cho từng bữa, rồi co giãn khẩu phần để tổng ngày đạt tỷ lệ mục tiêu mong muốn:
            // phần lớn ngày 90–110% (đạt chuẩn), thỉnh thoảng thiếu (~80%) hoặc vượt (~118%).
            var plan = new List<(string Loai, DateTimeOffset ThoiGian, string Ten, List<(ThucPhamThamChieu F, decimal Phan)> Mon)>();
            foreach (var (loai, gio, phut, xs) in Slots)
            {
                if (rnd.NextDouble() > xs) continue;
                var (ten, mon) = Menu[loai][rnd.Next(Menu[loai].Length)];
                var items = mon.Where(m => foods.ContainsKey(m.Ma)).Select(m => (foods[m.Ma], m.SoPhan * (decimal)(0.9 + rnd.NextDouble() * 0.2))).ToList();
                if (items.Count > 0) plan.Add((loai, clock.StartOfDay(d).AddHours(gio).AddMinutes(phut + rnd.Next(-20, 25)), ten, items));
            }
            var raw = plan.SelectMany(p => p.Mon).Sum(m => m.F.CaloTren100g * m.F.KhoiLuongChuan * m.Phan / 100m);
            var roll = rnd.NextDouble();
            var ratio = roll < 0.12 ? 0.78 + rnd.NextDouble() * 0.08 : roll < 0.22 ? 1.13 + rnd.NextDouble() * 0.08 : 0.92 + rnd.NextDouble() * 0.16;
            var scale = raw > 0 ? Math.Clamp(target * (decimal)ratio / raw, 0.6m, 2.5m) : 1m;

            foreach (var (loai, thoiGian, ten, mon) in plan)
            {
                var meal = new BuaAn { NguoiDungId = user.Id, LoaiBuaAn = loai, NgayAn = d, ThoiGianGhiNhan = thoiGian, TenMonAn = ten, NgayTao = thoiGian, NgayCapNhat = thoiGian };
                foreach (var (f, phanGoc) in mon)
                {
                    var phan = Math.Round(phanGoc * scale, 1);
                    var gram = Math.Round(f.KhoiLuongChuan * phan, 1);
                    var tp = new ThanhPhanBuaAn
                    {
                        BuaAnId = meal.Id, ThucPhamThamChieuId = f.Id, TenThanhPhan = f.TenThucPham,
                        KhauPhan = phan, DonViTinh = f.DonViChuan, KhoiLuongQuyDoiGram = gram,
                    };
                    tp.GanDinhDuong(f.DinhDuongTren100g * (gram / 100m));
                    meal.ThanhPhans.Add(tp);
                }
                meal.GanTongDinhDuong(NutritionCalculator.Tong(meal.ThanhPhans.Select(t => t.DinhDuong)), thoiGian);
                meal.XacNhan(thoiGian);
                db.BuaAns.Add(meal);
            }

            // Nước uống mỗi ngày 1,2–2,6 lít
            if (!await db.NhatKyNuocUongs.AnyAsync(w => w.NguoiDungId == user.Id && w.Ngay == d, ct))
                db.NhatKyNuocUongs.Add(new NhatKyNuocUong { NguoiDungId = user.Id, Ngay = d, LuongMl = 1200 + rnd.Next(0, 29) * 50, NgayCapNhat = now });
        }

        // Cân nặng giảm dần nhẹ, ghi mỗi 3–4 ngày (chỉ khi chưa có nhật ký cân nặng)
        if (!await db.NhatKyCanNangs.AnyAsync(w => w.NguoiDungId == user.Id, ct))
        {
            var kg = (user.HoSo?.CanNang ?? 68m) + 2.4m;
            for (var d = from; d < today; d = d.AddDays(rnd.Next(3, 5)))
            {
                db.NhatKyCanNangs.Add(new NhatKyCanNang { NguoiDungId = user.Id, Ngay = d, CanNang = Math.Round(kg, 1), NgayTao = now, NgayCapNhat = now });
                kg -= 0.15m + (decimal)rnd.NextDouble() * 0.35m;
            }
        }

        await db.SaveChangesAsync(ct);
        foreach (var d in days) await summaries.RecomputeAsync(user.Id, d, ct);
        logger.LogInformation("Đã nạp dữ liệu minh họa {Days} ngày cho tài khoản demo", days.Count);
    }
}
