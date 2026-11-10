using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NutriVision.Api.Common;
using NutriVision.Api.Data;
using NutriVision.Api.DTOs.Meal;
using NutriVision.Api.Models.Entities;
using NutriVision.Api.Services.Interfaces;

namespace NutriVision.Api.Services
{
    public class MealService : IMealService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAiVisionService _aiService;
        private readonly IWebHostEnvironment _env;

        public MealService(ApplicationDbContext context, IAiVisionService aiService, IWebHostEnvironment env)
        {
            _context = context;
            _aiService = aiService;
            _env = env;
        }

        public async Task<ApiResponse<ScanMealResponse>> UploadAndScanMealAsync(Guid userId, IFormFile file, string loaiBuaAn)
        {
            // BR-19 & AC-25: Kiểm tra dung lượng <= 10MB
            if (file == null || file.Length == 0)
            {
                return ApiResponse<ScanMealResponse>.Fail("Vui lòng chọn ảnh bữa ăn.");
            }
            if (file.Length > 10 * 1024 * 1024)
            {
                return ApiResponse<ScanMealResponse>.Fail("Ảnh vượt quá 10MB. Vui lòng chọn ảnh nhỏ hơn.");
            }

            // AC-24: Kiểm tra định dạng hợp lệ
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowedExts.Contains(extension))
            {
                return ApiResponse<ScanMealResponse>.Fail("Định dạng không hỗ trợ. Vui lòng tải lên ảnh .jpg, .png hoặc .webp.");
            }

            // Lưu file vào thư mục uploads
            var uploadsFolder = Path.Combine(_env.ContentRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var imageUrl = $"/uploads/{fileName}";

            // Phân tích hình ảnh bằng AI Service
            using var readStream = file.OpenReadStream();
            var aiResult = await _aiService.AnalyzeMealImageAsync(filePath, readStream, file.FileName);

            // Tạo bữa ăn tạm thời (DaXacNhan = false)
            var buaAn = new BuaAn
            {
                NguoiDungId = userId,
                LoaiBuaAn = string.IsNullOrWhiteSpace(loaiBuaAn) ? "SANG" : loaiBuaAn.ToUpperInvariant(),
                NgayAn = DateOnly.FromDateTime(DateTime.UtcNow),
                ThoiGianGhiNhan = DateTimeOffset.UtcNow,
                TenMonAn = aiResult.TenMonAn,
                DuongDanHinhAnh = imageUrl,
                TaoTuAI = true,
                DaXacNhan = false, // Chưa xác nhận -> Chưa tính vào báo cáo (BR-26, BR-27)
                NgayTao = DateTimeOffset.UtcNow,
                NgayCapNhat = DateTimeOffset.UtcNow
            };

            // Tra cứu và tính toán dinh dưỡng từng thành phần
            var componentsList = new List<MealComponentDto>();
            foreach (var aiComp in aiResult.ThanhPhans)
            {
                var refFood = await FindMatchingFoodAsync(aiComp.TenThanhPhan);
                var compEntity = CalculateComponentNutrition(buaAn.Id, aiComp.TenThanhPhan, aiComp.KhauPhan, aiComp.DonViTinh, aiComp.KhoiLuongQuyDoiGram, refFood);
                buaAn.ThanhPhans.Add(compEntity);
            }

            // Cộng dồn toàn bộ dinh dưỡng bữa ăn
            SumMealNutrition(buaAn);

            await _context.BuaAns.AddAsync(buaAn);

            // Lưu lịch sử nhận diện AI
            var lichSuAI = new LichSuNhanDienAI
            {
                NguoiDungId = userId,
                BuaAnId = buaAn.Id,
                DuongDanHinhAnh = imageUrl,
                PhanHoiAIGoc = aiResult.RawJsonResponse,
                TenMonAIDoan = aiResult.TenMonAn,
                TenMonNguoiDungChot = aiResult.TenMonAn,
                DoTinCay = aiResult.DoTinCay,
                NguoiDungCoSua = false,
                ThoiGianXuLyMs = aiResult.ThoiGianXuLyMs,
                NgayTao = DateTimeOffset.UtcNow
            };
            await _context.LichSuNhanDienAIs.AddAsync(lichSuAI);
            await _context.SaveChangesAsync();

            return ApiResponse<ScanMealResponse>.Ok(new ScanMealResponse
            {
                BuaAnId = buaAn.Id,
                TenMonAn = buaAn.TenMonAn,
                DuongDanHinhAnh = imageUrl,
                DoTinCay = aiResult.DoTinCay,
                CanManualInput = false,
                ThongBao = "AI đã nhận diện thành công! Vui lòng kiểm tra và hiệu chỉnh nếu cần.",
                ThanhPhans = buaAn.ThanhPhans.Select(MapComponentToDto).ToList(),
                TongDinhDuongTamTinh = new NutritionSummaryDto
                {
                    TongCalo = buaAn.TongCalo,
                    TongChatDam = buaAn.TongChatDam,
                    TongTinhBot = buaAn.TongTinhBot,
                    TongChatBeo = buaAn.TongChatBeo,
                    TongChatXo = buaAn.TongChatXo
                }
            });
        }

        public async Task<ApiResponse<MealDto>> AdjustMealAsync(Guid userId, Guid buaAnId, AdjustMealRequest request)
        {
            var meal = await _context.BuaAns
                .Include(b => b.ThanhPhans)
                .FirstOrDefaultAsync(b => b.Id == buaAnId);

            if (meal == null) return ApiResponse<MealDto>.Fail("Không tìm thấy bữa ăn.");
            if (meal.NguoiDungId != userId) return ApiResponse<MealDto>.Fail("Bạn không có quyền chỉnh sửa bữa ăn này."); // BR-33, AC-44

            // BR-23 & AC-34: Tên món không được để trống
            if (string.IsNullOrWhiteSpace(request.TenMonAn))
            {
                return ApiResponse<MealDto>.Fail("Tên món ăn không được để trống.");
            }

            // BR-21 & AC-32: Bữa ăn phải có ít nhất 1 thành phần
            if (request.ThanhPhans == null || request.ThanhPhans.Count == 0)
            {
                return ApiResponse<MealDto>.Fail("Bữa ăn phải có ít nhất 1 thành phần thực phẩm.");
            }

            // BR-22 & AC-33: Khẩu phần phải > 0
            if (request.ThanhPhans.Any(t => t.KhauPhan <= 0 || t.KhoiLuongQuyDoiGram <= 0))
            {
                return ApiResponse<MealDto>.Fail("Khẩu phần và khối lượng của từng thành phần phải lớn hơn 0.");
            }

            // Kiểm tra xem người dùng có sửa tên món so với AI ban đầu không (BR-31, AC-31)
            var aiHistory = await _context.LichSuNhanDienAIs.FirstOrDefaultAsync(l => l.BuaAnId == buaAnId);
            if (aiHistory != null)
            {
                if (!string.Equals(aiHistory.TenMonAIDoan, request.TenMonAn, StringComparison.OrdinalIgnoreCase))
                {
                    aiHistory.NguoiDungCoSua = true;
                    aiHistory.TenMonNguoiDungChot = request.TenMonAn;
                }
            }

            meal.TenMonAn = request.TenMonAn.Trim();
            meal.NgayCapNhat = DateTimeOffset.UtcNow;

            // Xóa thành phần cũ và thay bằng thành phần đã hiệu chỉnh
            _context.ThanhPhanBuaAns.RemoveRange(meal.ThanhPhans);
            meal.ThanhPhans.Clear();

            foreach (var compDto in request.ThanhPhans)
            {
                var refFood = compDto.ThucPhamThamChieuId.HasValue
                    ? await _context.ThucPhamThamChieus.FindAsync(compDto.ThucPhamThamChieuId.Value)
                    : await FindMatchingFoodAsync(compDto.TenThanhPhan);

                var compEntity = CalculateComponentNutrition(meal.Id, compDto.TenThanhPhan, compDto.KhauPhan, compDto.DonViTinh, compDto.KhoiLuongQuyDoiGram, refFood);
                meal.ThanhPhans.Add(compEntity);
            }

            SumMealNutrition(meal);

            // Nếu bữa ăn đã xác nhận trước đó thì tính lại bảng tổng hợp ngày
            if (meal.DaXacNhan)
            {
                await RecalculateDailySummaryAsync(userId, meal.NgayAn);
            }

            await _context.SaveChangesAsync();
            return ApiResponse<MealDto>.Ok(MapMealToDto(meal), "Hiệu chỉnh bữa ăn thành công!");
        }

        public async Task<ApiResponse<MealDto>> ConfirmMealAsync(Guid userId, Guid buaAnId)
        {
            var meal = await _context.BuaAns
                .Include(b => b.ThanhPhans)
                .FirstOrDefaultAsync(b => b.Id == buaAnId);

            if (meal == null) return ApiResponse<MealDto>.Fail("Không tìm thấy bữa ăn.");
            if (meal.NguoiDungId != userId) return ApiResponse<MealDto>.Fail("Bạn không có quyền xác nhận bữa ăn này.");

            meal.DaXacNhan = true; // BR-26, AC-29
            meal.NgayCapNhat = DateTimeOffset.UtcNow;

            // Cập nhật bảng tổng hợp dinh dưỡng ngày
            await RecalculateDailySummaryAsync(userId, meal.NgayAn);

            await _context.SaveChangesAsync();
            return ApiResponse<MealDto>.Ok(MapMealToDto(meal), "Xác nhận và lưu bữa ăn thành công!");
        }

        public async Task<ApiResponse<MealDto>> RevertToAiOriginalAsync(Guid userId, Guid buaAnId)
        {
            var meal = await _context.BuaAns
                .Include(b => b.ThanhPhans)
                .FirstOrDefaultAsync(b => b.Id == buaAnId);

            if (meal == null) return ApiResponse<MealDto>.Fail("Không tìm thấy bữa ăn.");
            if (meal.NguoiDungId != userId) return ApiResponse<MealDto>.Fail("Bạn không có quyền thao tác.");

            var aiHistory = await _context.LichSuNhanDienAIs.FirstOrDefaultAsync(l => l.BuaAnId == buaAnId);
            if (aiHistory == null || string.IsNullOrWhiteSpace(aiHistory.TenMonAIDoan))
            {
                return ApiResponse<MealDto>.Fail("Không tìm thấy kết quả AI ban đầu để hoàn tác.");
            }

            // Hoàn tác tên món về tên AI đoán (AC-35)
            meal.TenMonAn = aiHistory.TenMonAIDoan;
            aiHistory.NguoiDungCoSua = false;
            aiHistory.TenMonNguoiDungChot = aiHistory.TenMonAIDoan;

            // Tính toán lại dinh dưỡng
            SumMealNutrition(meal);
            await _context.SaveChangesAsync();

            return ApiResponse<MealDto>.Ok(MapMealToDto(meal), "Đã hoàn tác về kết quả ban đầu của AI.");
        }

        public async Task<ApiResponse<bool>> CancelMealAsync(Guid userId, Guid buaAnId)
        {
            var meal = await _context.BuaAns.FirstOrDefaultAsync(b => b.Id == buaAnId);
            if (meal == null) return ApiResponse<bool>.Ok(true);
            if (meal.NguoiDungId != userId) return ApiResponse<bool>.Fail("Bạn không có quyền hủy bữa ăn này.");

            // BR-25 & AC-28: Hủy bữa ăn thì xóa ảnh đã upload để tiết kiệm dung lượng
            if (!string.IsNullOrWhiteSpace(meal.DuongDanHinhAnh))
            {
                var localPath = Path.Combine(_env.ContentRootPath, meal.DuongDanHinhAnh.TrimStart('/'));
                if (File.Exists(localPath))
                {
                    try { File.Delete(localPath); } catch { }
                }
            }

            _context.BuaAns.Remove(meal);
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Đã hủy bữa ăn thành công.");
        }

        public async Task<ApiResponse<List<MealDto>>> GetMealHistoryAsync(Guid userId, DateOnly? fromDate, DateOnly? toDate, string? loaiBuaAn, int page = 1, int pageSize = 10)
        {
            var query = _context.BuaAns
                .Include(b => b.ThanhPhans)
                .Where(b => b.NguoiDungId == userId && b.DaXacNhan); // BR-28: Chỉ tính các bữa đã confirmed

            if (fromDate.HasValue)
            {
                query = query.Where(b => b.NgayAn >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                query = query.Where(b => b.NgayAn <= toDate.Value);
            }
            if (!string.IsNullOrWhiteSpace(loaiBuaAn) && !loaiBuaAn.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(b => b.LoaiBuaAn.ToUpper() == loaiBuaAn.ToUpper());
            }

            var meals = await query
                .OrderByDescending(b => b.NgayAn)
                .ThenByDescending(b => b.ThoiGianGhiNhan)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = meals.Select(MapMealToDto).ToList();
            return ApiResponse<List<MealDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<MealDto>> GetMealDetailAsync(Guid userId, Guid buaAnId)
        {
            var meal = await _context.BuaAns
                .Include(b => b.ThanhPhans)
                .FirstOrDefaultAsync(b => b.Id == buaAnId);

            if (meal == null) return ApiResponse<MealDto>.Fail("Không tìm thấy bữa ăn.");
            if (meal.NguoiDungId != userId) return ApiResponse<MealDto>.Fail("Truy cập bị từ chối."); // AC-44

            return ApiResponse<MealDto>.Ok(MapMealToDto(meal));
        }

        public async Task<ApiResponse<bool>> DeleteMealAsync(Guid userId, Guid buaAnId)
        {
            var meal = await _context.BuaAns.FirstOrDefaultAsync(b => b.Id == buaAnId);
            if (meal == null) return ApiResponse<bool>.Fail("Không tìm thấy bữa ăn.");
            if (meal.NguoiDungId != userId) return ApiResponse<bool>.Fail("Bạn không có quyền xóa bữa ăn này.");

            // BR-35 & AC-46: Soft Delete
            meal.DaXoa = true;
            meal.NgayCapNhat = DateTimeOffset.UtcNow;

            await RecalculateDailySummaryAsync(userId, meal.NgayAn);
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Đã xóa bữa ăn thành công.");
        }

        public async Task<ApiResponse<MealDto>> CreateManualMealAsync(Guid userId, ManualMealRequest request)
        {
            var mealDate = request.NgayAn ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var buaAn = new BuaAn
            {
                NguoiDungId = userId,
                LoaiBuaAn = request.LoaiBuaAn.ToUpperInvariant(),
                NgayAn = mealDate,
                ThoiGianGhiNhan = DateTimeOffset.UtcNow,
                TenMonAn = request.TenMonAn.Trim(),
                TaoTuAI = false,
                DaXacNhan = true,
                NgayTao = DateTimeOffset.UtcNow,
                NgayCapNhat = DateTimeOffset.UtcNow
            };

            foreach (var comp in request.ThanhPhans)
            {
                var refFood = comp.ThucPhamThamChieuId.HasValue
                    ? await _context.ThucPhamThamChieus.FindAsync(comp.ThucPhamThamChieuId.Value)
                    : await FindMatchingFoodAsync(comp.TenThanhPhan);

                var compEntity = CalculateComponentNutrition(buaAn.Id, comp.TenThanhPhan, comp.KhauPhan, comp.DonViTinh, comp.KhoiLuongQuyDoiGram, refFood);
                buaAn.ThanhPhans.Add(compEntity);
            }

            SumMealNutrition(buaAn);

            await _context.BuaAns.AddAsync(buaAn);
            await RecalculateDailySummaryAsync(userId, mealDate);
            await _context.SaveChangesAsync();

            return ApiResponse<MealDto>.Ok(MapMealToDto(buaAn), "Thêm bữa ăn thủ công thành công!");
        }

        private async Task<ThucPhamThamChieu?> FindMatchingFoodAsync(string name)
        {
            var trimmed = name.Trim().ToLower();
            // Tra cứu chính xác hoặc gần đúng
            return await _context.ThucPhamThamChieus
                .FirstOrDefaultAsync(t => t.TenThucPham.ToLower().Contains(trimmed) || trimmed.Contains(t.TenThucPham.ToLower()));
        }

        private static ThanhPhanBuaAn CalculateComponentNutrition(Guid buaAnId, string tenThanhPhan, decimal khauPhan, string donVi, decimal gram, ThucPhamThamChieu? refFood)
        {
            decimal calo = 0, dam = 0, tinhBot = 0, beo = 0, xo = 0;
            if (refFood != null)
            {
                var factor = gram / 100m;
                calo = Math.Round(refFood.CaloTren100g * factor, 2);
                dam = Math.Round(refFood.ChatDamTren100g * factor, 2);
                tinhBot = Math.Round(refFood.TinhBotTren100g * factor, 2);
                beo = Math.Round(refFood.ChatBeoTren100g * factor, 2);
                xo = Math.Round(refFood.ChatXoTren100g * factor, 2);
            }
            else
            {
                // Ước tính trung bình nếu không có trong danh mục (200 calo/100g)
                var factor = gram / 100m;
                calo = Math.Round(150m * factor, 2);
                dam = Math.Round(5m * factor, 2);
                tinhBot = Math.Round(20m * factor, 2);
                beo = Math.Round(4m * factor, 2);
                xo = Math.Round(1m * factor, 2);
            }

            return new ThanhPhanBuaAn
            {
                BuaAnId = buaAnId,
                ThucPhamThamChieuId = refFood?.Id,
                TenThanhPhan = tenThanhPhan,
                KhauPhan = khauPhan,
                DonViTinh = donVi,
                KhoiLuongQuyDoiGram = gram,
                LuongCalo = calo,
                LuongChatDam = dam,
                LuongTinhBot = tinhBot,
                LuongChatBeo = beo,
                LuongChatXo = xo
            };
        }

        private static void SumMealNutrition(BuaAn meal)
        {
            meal.TongCalo = meal.ThanhPhans.Sum(t => t.LuongCalo);
            meal.TongChatDam = meal.ThanhPhans.Sum(t => t.LuongChatDam);
            meal.TongTinhBot = meal.ThanhPhans.Sum(t => t.LuongTinhBot);
            meal.TongChatBeo = meal.ThanhPhans.Sum(t => t.LuongChatBeo);
            meal.TongChatXo = meal.ThanhPhans.Sum(t => t.LuongChatXo);
        }

        private async Task RecalculateDailySummaryAsync(Guid userId, DateOnly date)
        {
            var confirmedMeals = await _context.BuaAns
                .Where(b => b.NguoiDungId == userId && b.NgayAn == date && b.DaXacNhan && !b.DaXoa)
                .ToListAsync();

            var summary = await _context.TongHopDinhDuongNgays
                .FirstOrDefaultAsync(s => s.NguoiDungId == userId && s.Ngay == date);

            var profile = await _context.HoSoNguoiDungs.FirstOrDefaultAsync(h => h.NguoiDungId == userId);
            var targetCalo = profile?.MucTieuCalo ?? 2000m;

            if (summary == null)
            {
                summary = new TongHopDinhDuongNgay
                {
                    NguoiDungId = userId,
                    Ngay = date,
                    MucTieuCaloNgay = targetCalo
                };
                await _context.TongHopDinhDuongNgays.AddAsync(summary);
            }

            summary.SoBuaAnDaGhi = confirmedMeals.Count;
            summary.TongCalo = confirmedMeals.Sum(m => m.TongCalo);
            summary.TongChatDam = confirmedMeals.Sum(m => m.TongChatDam);
            summary.TongTinhBot = confirmedMeals.Sum(m => m.TongTinhBot);
            summary.TongChatBeo = confirmedMeals.Sum(m => m.TongChatBeo);
            summary.TongChatXo = confirmedMeals.Sum(m => m.TongChatXo);
            summary.NgayCapNhat = DateTimeOffset.UtcNow;
        }

        private static MealDto MapMealToDto(BuaAn meal) => new()
        {
            Id = meal.Id,
            LoaiBuaAn = meal.LoaiBuaAn,
            NgayAn = meal.NgayAn,
            ThoiGianGhiNhan = meal.ThoiGianGhiNhan,
            TenMonAn = meal.TenMonAn,
            DuongDanHinhAnh = meal.DuongDanHinhAnh,
            TongCalo = meal.TongCalo,
            TongChatDam = meal.TongChatDam,
            TongTinhBot = meal.TongTinhBot,
            TongChatBeo = meal.TongChatBeo,
            TongChatXo = meal.TongChatXo,
            TaoTuAI = meal.TaoTuAI,
            DaXacNhan = meal.DaXacNhan,
            ThanhPhans = meal.ThanhPhans.Select(MapComponentToDto).ToList()
        };

        private static MealComponentDto MapComponentToDto(ThanhPhanBuaAn comp) => new()
        {
            Id = comp.Id,
            ThucPhamThamChieuId = comp.ThucPhamThamChieuId,
            TenThanhPhan = comp.TenThanhPhan,
            KhauPhan = comp.KhauPhan,
            DonViTinh = comp.DonViTinh,
            KhoiLuongQuyDoiGram = comp.KhoiLuongQuyDoiGram,
            LuongCalo = comp.LuongCalo,
            LuongChatDam = comp.LuongChatDam,
            LuongTinhBot = comp.LuongTinhBot,
            LuongChatBeo = comp.LuongChatBeo,
            LuongChatXo = comp.LuongChatXo
        };
    }
}
