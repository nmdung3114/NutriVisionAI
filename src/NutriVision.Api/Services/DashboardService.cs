using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NutriVision.Api.Common;
using NutriVision.Api.Data;
using NutriVision.Api.DTOs.Dashboard;
using NutriVision.Api.DTOs.Meal;
using NutriVision.Api.Models.Entities;
using NutriVision.Api.Services.Interfaces;

namespace NutriVision.Api.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<DashboardSummaryDto>> GetDashboardSummaryAsync(Guid userId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Lấy mục tiêu calo từ hồ sơ
            var profile = await _context.HoSoNguoiDungs.FirstOrDefaultAsync(h => h.NguoiDungId == userId);
            var targetCalo = profile?.MucTieuCalo ?? 2000m;

            // BR-28 & AC-49: Chỉ tính các bữa ăn đã confirmed
            var todayMeals = await _context.BuaAns
                .Include(b => b.ThanhPhans)
                .Where(b => b.NguoiDungId == userId && b.NgayAn == today && b.DaXacNhan)
                .OrderBy(b => b.ThoiGianGhiNhan)
                .ToListAsync();

            var totalCalo = todayMeals.Sum(m => m.TongCalo);
            var totalDam = todayMeals.Sum(m => m.TongChatDam);
            var totalTinhBot = todayMeals.Sum(m => m.TongTinhBot);
            var totalBeo = todayMeals.Sum(m => m.TongChatBeo);
            var totalXo = todayMeals.Sum(m => m.TongChatXo);

            var percent = targetCalo > 0 ? Math.Round((totalCalo / targetCalo) * 100m, 1) : 0m;

            // BR-39: Lấy xu hướng 7 ngày gần nhất
            var last7Days = Enumerable.Range(0, 7)
                .Select(offset => today.AddDays(-6 + offset))
                .ToList();

            var minDate = last7Days.First();
            var maxDate = last7Days.Last();

            var summaries = await _context.TongHopDinhDuongNgays
                .Where(s => s.NguoiDungId == userId && s.Ngay >= minDate && s.Ngay <= maxDate)
                .ToDictionaryAsync(s => s.Ngay, s => s.TongCalo);

            var xuHuongList = new List<DailyTrendDto>();
            foreach (var date in last7Days)
            {
                var calo = summaries.TryGetValue(date, out var c) ? c : 0m;
                var dayOfWeek = date.DayOfWeek switch
                {
                    DayOfWeek.Monday => "T2",
                    DayOfWeek.Tuesday => "T3",
                    DayOfWeek.Wednesday => "T4",
                    DayOfWeek.Thursday => "T5",
                    DayOfWeek.Friday => "T6",
                    DayOfWeek.Saturday => "T7",
                    DayOfWeek.Sunday => "CN",
                    _ => string.Empty
                };

                xuHuongList.Add(new DailyTrendDto
                {
                    Ngay = date.ToString("yyyy-MM-dd"),
                    Thu = $"{dayOfWeek} ({date.Day}/{date.Month})",
                    CaloDaNap = calo,
                    MucTieuCalo = targetCalo
                });
            }

            var dto = new DashboardSummaryDto
            {
                CaloHomNay = totalCalo,
                MucTieuCalo = targetCalo,
                PhanTramDatDuoc = percent,
                SoBuaAnHomNay = todayMeals.Count,
                TongChatDam = totalDam,
                TongTinhBot = totalTinhBot,
                TongChatBeo = totalBeo,
                TongChatXo = totalXo,
                BuaAnsHomNay = todayMeals.Select(m => new MealDto
                {
                    Id = m.Id,
                    LoaiBuaAn = m.LoaiBuaAn,
                    NgayAn = m.NgayAn,
                    ThoiGianGhiNhan = m.ThoiGianGhiNhan,
                    TenMonAn = m.TenMonAn,
                    DuongDanHinhAnh = m.DuongDanHinhAnh,
                    TongCalo = m.TongCalo,
                    TongChatDam = m.TongChatDam,
                    TongTinhBot = m.TongTinhBot,
                    TongChatBeo = m.TongChatBeo,
                    TongChatXo = m.TongChatXo,
                    TaoTuAI = m.TaoTuAI,
                    DaXacNhan = m.DaXacNhan
                }).ToList(),
                XuHuong7Ngay = xuHuongList
            };

            return ApiResponse<DashboardSummaryDto>.Ok(dto);
        }

        public async Task<ApiResponse<ReportSummaryDto>> GetReportSummaryAsync(Guid userId, DateOnly fromDate, DateOnly toDate)
        {
            var profile = await _context.HoSoNguoiDungs.FirstOrDefaultAsync(h => h.NguoiDungId == userId);
            var targetCalo = profile?.MucTieuCalo ?? 2000m;

            var meals = await _context.BuaAns
                .Where(b => b.NguoiDungId == userId && b.NgayAn >= fromDate && b.NgayAn <= toDate && b.DaXacNhan)
                .ToListAsync();

            var totalDays = (toDate.ToDateTime(TimeOnly.MinValue) - fromDate.ToDateTime(TimeOnly.MinValue)).Days + 1;
            if (totalDays <= 0) totalDays = 1;

            var totalCalo = meals.Sum(m => m.TongCalo);
            var totalDam = meals.Sum(m => m.TongChatDam);
            var totalTinhBot = meals.Sum(m => m.TongTinhBot);
            var totalBeo = meals.Sum(m => m.TongChatBeo);
            var totalXo = meals.Sum(m => m.TongChatXo);

            var avgCalo = Math.Round(totalCalo / totalDays, 1);
            var avgDam = Math.Round(totalDam / totalDays, 1);
            var avgTinhBot = Math.Round(totalTinhBot / totalDays, 1);
            var avgBeo = Math.Round(totalBeo / totalDays, 1);
            var avgXo = Math.Round(totalXo / totalDays, 1);

            // Tính cơ cấu tỷ lệ năng lượng từ 3 nhóm chất: Đạm (4 kcal/g), Tinh bột (4 kcal/g), Béo (9 kcal/g)
            var caloFromDam = totalDam * 4m;
            var caloFromTinhBot = totalTinhBot * 4m;
            var caloFromBeo = totalBeo * 9m;
            var sumMacroCalo = caloFromDam + caloFromTinhBot + caloFromBeo;

            decimal pctDam = 25, pctTinhBot = 50, pctBeo = 25;
            if (sumMacroCalo > 0)
            {
                pctDam = Math.Round((caloFromDam / sumMacroCalo) * 100m, 1);
                pctTinhBot = Math.Round((caloFromTinhBot / sumMacroCalo) * 100m, 1);
                pctBeo = Math.Round((caloFromBeo / sumMacroCalo) * 100m, 1);
            }

            // Chi tiết từng ngày
            var mealsByDate = meals.GroupBy(m => m.NgayAn).ToDictionary(g => g.Key, g => g.Sum(m => m.TongCalo));
            var dailyDetails = new List<DailyTrendDto>();
            var currentDate = fromDate;
            while (currentDate <= toDate)
            {
                var calo = mealsByDate.TryGetValue(currentDate, out var c) ? c : 0m;
                dailyDetails.Add(new DailyTrendDto
                {
                    Ngay = currentDate.ToString("yyyy-MM-dd"),
                    Thu = $"{currentDate.Day}/{currentDate.Month}",
                    CaloDaNap = calo,
                    MucTieuCalo = targetCalo
                });
                currentDate = currentDate.AddDays(1);
            }

            // Nhận xét thông minh
            var recommendations = new List<string>();
            if (avgCalo > targetCalo * 1.1m)
            {
                recommendations.Add($"Lượng calo trung bình ({avgCalo} kcal) đang cao hơn mục tiêu TDEE ({targetCalo} kcal). Bạn nên giảm nhẹ khẩu phần tinh bột hoặc tăng cường cardio.");
            }
            else if (avgCalo < targetCalo * 0.85m && avgCalo > 0)
            {
                recommendations.Add($"Lượng calo trung bình ({avgCalo} kcal) đang thấp hơn nhu cầu duy trì. Cần bổ sung năng lượng để tránh mất cơ bắp.");
            }
            else
            {
                recommendations.Add("Mức tiêu thụ năng lượng của bạn rất cân đối và bám sát mục tiêu đề ra. Tiếp tục duy trì phong độ!");
            }

            if (avgDam >= 60)
            {
                recommendations.Add("Lượng protein hàng ngày đạt chuẩn tối ưu, hỗ trợ phục hồi và phát triển cơ bắp rất tốt.");
            }
            else
            {
                recommendations.Add("Nên bổ sung thêm thực phẩm giàu đạm như ức gà, trứng, đậu phụ để đảm bảo sức bền thể lực.");
            }

            var report = new ReportSummaryDto
            {
                TuNgay = fromDate.ToString("yyyy-MM-dd"),
                DenNgay = toDate.ToString("yyyy-MM-dd"),
                TongSoNgay = totalDays,
                TongSoBuaAn = meals.Count,
                CaloTrungBinhNgay = avgCalo,
                ChatDamTrungBinhNgay = avgDam,
                TinhBotTrungBinhNgay = avgTinhBot,
                ChatBeoTrungBinhNgay = avgBeo,
                ChatXoTrungBinhNgay = avgXo,
                TiLePhanBoDam = pctDam,
                TiLePhanBoTinhBot = pctTinhBot,
                TiLePhanBoBeo = pctBeo,
                ChiTietCacNgay = dailyDetails,
                NhanXetThongMinh = recommendations
            };

            return ApiResponse<ReportSummaryDto>.Ok(report);
        }

        public async Task<ApiResponse<byte[]>> ExportCsvAsync(Guid userId, DateOnly fromDate, DateOnly toDate)
        {
            var user = await _context.NguoiDungs.FindAsync(userId);
            var meals = await _context.BuaAns
                .Include(b => b.ThanhPhans)
                .Where(b => b.NguoiDungId == userId && b.NgayAn >= fromDate && b.NgayAn <= toDate && b.DaXacNhan)
                .OrderBy(b => b.NgayAn)
                .ThenBy(b => b.ThoiGianGhiNhan)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("NgayAn,LoaiBuaAn,TenMonAn,TongCalo_kcal,ChatDam_g,TinhBot_g,ChatBeo_g,ChatXo_g,TaoTuAI");

            foreach (var m in meals)
            {
                sb.AppendLine($"{m.NgayAn:yyyy-MM-dd},{m.LoaiBuaAn},\"{m.TenMonAn}\",{m.TongCalo},{m.TongChatDam},{m.TongTinhBot},{m.TongChatBeo},{m.TongChatXo},{(m.TaoTuAI ? "AI" : "ThuCong")}");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return ApiResponse<byte[]>.Ok(bytes, "Xuất CSV thành công!");
        }
    }
}
