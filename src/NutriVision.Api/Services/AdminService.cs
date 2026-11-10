using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NutriVision.Api.Common;
using NutriVision.Api.Data;
using NutriVision.Api.DTOs.Admin;
using NutriVision.Api.Models.Entities;
using NutriVision.Api.Services.Interfaces;

namespace NutriVision.Api.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<AdminUserDto>>> GetUsersAsync(string? search, string? status, int page = 1, int pageSize = 10)
        {
            var query = _context.NguoiDungs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(u => u.Email.ToLower().Contains(s) || u.HoTen.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(u => u.TrangThai.ToUpper() == status.ToUpper());
            }

            var users = await query
                .OrderByDescending(u => u.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new AdminUserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    HoTen = u.HoTen,
                    VaiTro = u.VaiTro,
                    TrangThai = u.TrangThai,
                    EmailDaXacThuc = u.EmailDaXacThuc,
                    SoBuaAnDaGhi = u.BuaAns.Count(b => b.DaXacNhan && !b.DaXoa),
                    NgayTao = u.NgayTao
                })
                .ToListAsync();

            return ApiResponse<List<AdminUserDto>>.Ok(users);
        }

        public async Task<ApiResponse<bool>> ToggleUserLockAsync(Guid currentAdminId, Guid targetUserId)
        {
            // BR-13 & AC-16: Admin không được tự khóa chính mình
            if (currentAdminId == targetUserId)
            {
                return ApiResponse<bool>.Fail("Quản trị viên không thể tự khóa tài khoản của chính mình.");
            }

            var targetUser = await _context.NguoiDungs.FindAsync(targetUserId);
            if (targetUser == null) return ApiResponse<bool>.Fail("Không tìm thấy người dùng.");

            var isCurrentlyLocked = targetUser.TrangThai.Equals("LOCKED", StringComparison.OrdinalIgnoreCase);
            targetUser.TrangThai = isCurrentlyLocked ? "ACTIVE" : "LOCKED";
            targetUser.ThoiGianHetHanKhoa = isCurrentlyLocked ? null : DateTimeOffset.UtcNow.AddYears(1);
            targetUser.NgayCapNhat = DateTimeOffset.UtcNow;

            // Ghi nhật ký thao tác (BR-44, AC-56)
            await _context.NhatKyHeThongs.AddAsync(new NhatKyHeThong
            {
                NguoiDungThucHienId = currentAdminId,
                LoaiHanhDong = isCurrentlyLocked ? "MO_KHOA_USER" : "KHOA_USER",
                BangTacDong = "NguoiDung",
                IdDoiTuong = targetUserId.ToString(),
                ChiTiet = $"Thay đổi trạng thái tài khoản {targetUser.Email} sang {targetUser.TrangThai}",
                NgayTao = DateTimeOffset.UtcNow
            });

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, $"Đã {(isCurrentlyLocked ? "mở khóa" : "khóa")} tài khoản thành công!");
        }

        public async Task<ApiResponse<bool>> ChangeUserRoleAsync(Guid currentAdminId, Guid targetUserId, string newRole)
        {
            if (currentAdminId == targetUserId)
            {
                return ApiResponse<bool>.Fail("Quản trị viên không thể tự thay đổi vai trò của chính mình.");
            }

            var targetUser = await _context.NguoiDungs.FindAsync(targetUserId);
            if (targetUser == null) return ApiResponse<bool>.Fail("Không tìm thấy người dùng.");

            var upperRole = newRole.Trim().ToUpperInvariant();
            if (upperRole != "USER" && upperRole != "ADMIN")
            {
                return ApiResponse<bool>.Fail("Vai trò chỉ có thể là USER hoặc ADMIN.");
            }

            var oldRole = targetUser.VaiTro;
            targetUser.VaiTro = upperRole;
            targetUser.NgayCapNhat = DateTimeOffset.UtcNow;

            // Ghi log (BR-44, AC-57)
            await _context.NhatKyHeThongs.AddAsync(new NhatKyHeThong
            {
                NguoiDungThucHienId = currentAdminId,
                LoaiHanhDong = "DOI_ROLE",
                BangTacDong = "NguoiDung",
                IdDoiTuong = targetUserId.ToString(),
                ChiTiet = $"Thay đổi vai trò của {targetUser.Email} từ {oldRole} sang {upperRole}",
                NgayTao = DateTimeOffset.UtcNow
            });

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, $"Đã cập nhật vai trò người dùng sang {upperRole}!");
        }

        public async Task<ApiResponse<List<SystemLogDto>>> GetSystemLogsAsync(string? actionType, int page = 1, int pageSize = 20)
        {
            var query = _context.NhatKyHeThongs
                .Include(l => l.NguoiDungThucHien)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(actionType) && !actionType.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(l => l.LoaiHanhDong.ToUpper() == actionType.ToUpper());
            }

            var logs = await query
                .OrderByDescending(l => l.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new SystemLogDto
                {
                    Id = l.Id,
                    NguoiThucHienEmail = l.NguoiDungThucHien != null ? l.NguoiDungThucHien.Email : "Hệ thống",
                    LoaiHanhDong = l.LoaiHanhDong,
                    BangTacDong = l.BangTacDong,
                    IdDoiTuong = l.IdDoiTuong,
                    ChiTiet = l.ChiTiet,
                    DiaChiIP = l.DiaChiIP,
                    NgayTao = l.NgayTao
                })
                .ToListAsync();

            return ApiResponse<List<SystemLogDto>>.Ok(logs);
        }

        public async Task<ApiResponse<AdminStatsDto>> GetAdminStatsAsync()
        {
            // BR-47 & AC-61: Thống kê ẩn danh, không lộ thông tin cá nhân
            var totalUsers = await _context.NguoiDungs.CountAsync();
            var activeUsers = await _context.NguoiDungs.CountAsync(u => u.TrangThai == "ACTIVE");
            var totalMeals = await _context.BuaAns.CountAsync(b => b.DaXacNhan && !b.DaXoa);
            var totalAiScans = await _context.LichSuNhanDienAIs.CountAsync();

            var accurateAiCount = await _context.LichSuNhanDienAIs.CountAsync(l => !l.NguoiDungCoSua);
            var aiAccuracy = totalAiScans > 0 ? Math.Round(((decimal)accurateAiCount / totalAiScans) * 100m, 1) : 95.0m;

            var avgMs = await _context.LichSuNhanDienAIs
                .Where(l => l.ThoiGianXuLyMs.HasValue)
                .Select(l => l.ThoiGianXuLyMs!.Value)
                .DefaultIfEmpty(850)
                .AverageAsync();

            var stats = new AdminStatsDto
            {
                TongNguoiDung = totalUsers,
                TongNguoiDungActive = activeUsers,
                TongBuaAnDaGhi = totalMeals,
                TongLuotGoiAI = totalAiScans,
                TiLeAIChinhXac = aiAccuracy,
                ThoiGianPhanHoiTrungBinhMs = (int)avgMs
            };

            return ApiResponse<AdminStatsDto>.Ok(stats);
        }
    }
}
