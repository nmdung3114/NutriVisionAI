using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NutriVision.Api.Common;
using NutriVision.Api.Data;
using NutriVision.Api.DTOs.Profile;
using NutriVision.Api.Models.Entities;
using NutriVision.Api.Services.Interfaces;

namespace NutriVision.Api.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ApplicationDbContext _context;

        public ProfileService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<ProfileDto>> GetProfileAsync(Guid userId)
        {
            var user = await _context.NguoiDungs
                .Include(u => u.HoSo)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return ApiResponse<ProfileDto>.Fail("Không tìm thấy người dùng.");
            }

            var profile = user.HoSo;
            if (profile == null)
            {
                profile = new HoSoNguoiDung
                {
                    NguoiDungId = user.Id,
                    NgaySinh = new DateOnly(2000, 1, 1),
                    GioiTinh = "NAM",
                    ChieuCao = 170m,
                    CanNang = 65m,
                    MucDoVanDong = "VAN_DONG_VUA",
                    ChiSoBMI = 22.49m,
                    ChiSoBMR = 1600m,
                    ChiSoTDEE = 2480m,
                    MucTieuCalo = 2200m,
                    NgayCapNhat = DateTimeOffset.UtcNow
                };
                await _context.HoSoNguoiDungs.AddAsync(profile);
                await _context.SaveChangesAsync();
            }

            var dto = MapToDto(user, profile);
            return ApiResponse<ProfileDto>.Ok(dto);
        }

        public async Task<ApiResponse<ProfileDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
        {
            var user = await _context.NguoiDungs
                .Include(u => u.HoSo)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return ApiResponse<ProfileDto>.Fail("Không tìm thấy người dùng.");
            }

            // BR-14, BR-15, BR-16: Kiểm tra giới hạn hợp lệ
            if (request.ChieuCao < 50 || request.ChieuCao > 200)
            {
                return ApiResponse<ProfileDto>.Fail("Chiều cao phải nằm trong khoảng 50 - 200 cm.");
            }
            if (request.CanNang < 10 || request.CanNang > 150)
            {
                return ApiResponse<ProfileDto>.Fail("Cân nặng phải nằm trong khoảng 10 - 150 kg.");
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - request.NgaySinh.Year;
            if (age < 10 || age > 100)
            {
                return ApiResponse<ProfileDto>.Fail("Độ tuổi phải từ 10 đến 100 tuổi.");
            }

            user.HoTen = request.HoTen.Trim();

            var profile = user.HoSo ?? new HoSoNguoiDung { NguoiDungId = user.Id };
            profile.NgaySinh = request.NgaySinh;
            profile.GioiTinh = request.GioiTinh;
            profile.ChieuCao = request.ChieuCao;
            profile.CanNang = request.CanNang;
            profile.MucDoVanDong = request.MucDoVanDong;

            // Tính toán lại các chỉ số tự động (BR-17, AC-18)
            CalculateHealthMetrics(profile, age);
            profile.NgayCapNhat = DateTimeOffset.UtcNow;

            if (user.HoSo == null)
            {
                await _context.HoSoNguoiDungs.AddAsync(profile);
            }

            await _context.SaveChangesAsync();

            var dto = MapToDto(user, profile);
            return ApiResponse<ProfileDto>.Ok(dto, "Cập nhật hồ sơ thành công!");
        }

        public async Task<ApiResponse<string>> UpdateAvatarAsync(Guid userId, string avatarUrl)
        {
            var profile = await _context.HoSoNguoiDungs.FirstOrDefaultAsync(h => h.NguoiDungId == userId);
            if (profile == null)
            {
                profile = new HoSoNguoiDung { NguoiDungId = userId };
                await _context.HoSoNguoiDungs.AddAsync(profile);
            }

            profile.DuongDanAnhDaiDien = avatarUrl;
            profile.NgayCapNhat = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return ApiResponse<string>.Ok(avatarUrl, "Cập nhật ảnh đại diện thành công!");
        }

        private static void CalculateHealthMetrics(HoSoNguoiDung profile, int age)
        {
            // 1. BMI = Cân nặng (kg) / (Chiều cao (m) ^ 2)
            var heightInMeters = profile.ChieuCao / 100m;
            profile.ChiSoBMI = Math.Round(profile.CanNang / (heightInMeters * heightInMeters), 2);

            // 2. BMR (Mifflin–St Jeor)
            // Nam: 10 * weight + 6.25 * height - 5 * age + 5
            // Nữ: 10 * weight + 6.25 * height - 5 * age - 161
            decimal bmr;
            if (profile.GioiTinh.Equals("NU", StringComparison.OrdinalIgnoreCase))
            {
                bmr = (10m * profile.CanNang) + (6.25m * profile.ChieuCao) - (5m * age) - 161m;
            }
            else
            {
                bmr = (10m * profile.CanNang) + (6.25m * profile.ChieuCao) - (5m * age) + 5m;
            }
            profile.ChiSoBMR = Math.Round(bmr, 2);

            // 3. TDEE = BMR * Activity multiplier
            var multiplier = profile.MucDoVanDong switch
            {
                "IT_VAN_DONG" => 1.2m,
                "VAN_DONG_NHE" => 1.375m,
                "VAN_DONG_VUA" => 1.55m,
                "VAN_DONG_NHIEU" => 1.725m,
                "VAN_DONG_RAT_NHIEU" => 1.9m,
                _ => 1.55m
            };

            profile.ChiSoTDEE = Math.Round(profile.ChiSoBMR * multiplier, 2);
            profile.MucTieuCalo = profile.ChiSoTDEE; // Mặc định mục tiêu = TDEE duy trì
        }

        private static ProfileDto MapToDto(NguoiDung user, HoSoNguoiDung profile)
        {
            var age = DateOnly.FromDateTime(DateTime.UtcNow).Year - profile.NgaySinh.Year;
            
            var phanLoaiBmi = profile.ChiSoBMI switch
            {
                < 18.5m => "Thiếu cân (Cần bổ sung dinh dưỡng)",
                < 23.0m => "Bình thường (Thể trạng cân đối)",
                < 25.0m => "Tiền béo phì (Cần chú ý calo)",
                _ => "Béo phì (Cần giảm calo & tập luyện)"
            };

            return new ProfileDto
            {
                NguoiDungId = user.Id,
                HoTen = user.HoTen,
                Email = user.Email,
                NgaySinh = profile.NgaySinh,
                Tuoi = age,
                GioiTinh = profile.GioiTinh,
                ChieuCao = profile.ChieuCao,
                CanNang = profile.CanNang,
                MucDoVanDong = profile.MucDoVanDong,
                ChiSoBMI = profile.ChiSoBMI,
                PhanLoaiBMI = phanLoaiBmi,
                ChiSoBMR = profile.ChiSoBMR,
                ChiSoTDEE = profile.ChiSoTDEE,
                MucTieuCalo = profile.MucTieuCalo,
                DuongDanAnhDaiDien = profile.DuongDanAnhDaiDien
            };
        }
    }
}
