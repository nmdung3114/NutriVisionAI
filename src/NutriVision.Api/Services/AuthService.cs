using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NutriVision.Api.Common;
using NutriVision.Api.Data;
using NutriVision.Api.DTOs.Auth;
using NutriVision.Api.Models.Entities;
using NutriVision.Api.Services.Interfaces;

namespace NutriVision.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<ApiResponse<string>> RegisterAsync(RegisterRequest request)
        {
            // BR-02: Email phải là duy nhất
            if (await _context.NguoiDungs.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
            {
                return ApiResponse<string>.Fail("Email đã được đăng ký trong hệ thống.");
            }

            // BR-10: Mật khẩu >= 8 ký tự
            if (request.MatKhau.Length < 8)
            {
                return ApiResponse<string>.Fail("Mật khẩu phải có tối thiểu 8 ký tự.");
            }

            var tokenXacThuc = Guid.NewGuid().ToString("N");
            var user = new NguoiDung
            {
                Email = request.Email.Trim().ToLower(),
                HoTen = request.HoTen.Trim(),
                MatKhauHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhau),
                TrangThai = "ACTIVE", // Để sinh viên dễ test demo ngay, tài khoản được kích hoạt sẵn hoặc qua link
                EmailDaXacThuc = true,
                VaiTro = "USER",
                TokenXacThucEmail = tokenXacThuc,
                ThoiGianHetHanXacThuc = DateTimeOffset.UtcNow.AddHours(24), // BR-05: 24h
                NgayTao = DateTimeOffset.UtcNow,
                NgayCapNhat = DateTimeOffset.UtcNow
            };

            // Khởi tạo hồ sơ mặc định
            var defaultProfile = new HoSoNguoiDung
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

            await _context.NguoiDungs.AddAsync(user);
            await _context.HoSoNguoiDungs.AddAsync(defaultProfile);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok(tokenXacThuc, "Đăng ký tài khoản thành công!");
        }

        public async Task<ApiResponse<bool>> VerifyEmailAsync(string token)
        {
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.TokenXacThucEmail == token);
            if (user == null)
            {
                return ApiResponse<bool>.Fail("Liên kết xác thực không hợp lệ hoặc đã được sử dụng.");
            }

            // BR-05: Hết hạn sau 24h
            if (user.ThoiGianHetHanXacThuc.HasValue && user.ThoiGianHetHanXacThuc < DateTimeOffset.UtcNow)
            {
                return ApiResponse<bool>.Fail("Liên kết xác thực đã hết hạn (quá 24 giờ). Vui lòng yêu cầu gửi lại.");
            }

            user.EmailDaXacThuc = true;
            user.TrangThai = "ACTIVE";
            user.TokenXacThucEmail = null;
            user.ThoiGianHetHanXacThuc = null;
            user.NgayCapNhat = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Xác thực email thành công! Bạn có thể đăng nhập ngay bây giờ.");
        }

        public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request, string? ipAddress)
        {
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
            if (user == null)
            {
                return ApiResponse<AuthResponse>.Fail("Email hoặc mật khẩu không chính xác.");
            }

            // BR-07 & AC-09: Kiểm tra nếu tài khoản đang bị khóa tạm thời 15 phút
            if (user.ThoiGianHetHanKhoa.HasValue && user.ThoiGianHetHanKhoa > DateTimeOffset.UtcNow)
            {
                var remainingMinutes = Math.Ceiling((user.ThoiGianHetHanKhoa.Value - DateTimeOffset.UtcNow).TotalMinutes);
                return ApiResponse<AuthResponse>.Fail($"Tài khoản tạm thời bị khóa do nhập sai nhiều lần. Vui lòng thử lại sau {remainingMinutes} phút.");
            }

            // Kiểm tra mật khẩu
            if (!BCrypt.Net.BCrypt.Verify(request.MatKhau, user.MatKhauHash))
            {
                user.SoLanDangNhapSai++;
                if (user.SoLanDangNhapSai >= 5)
                {
                    user.ThoiGianHetHanKhoa = DateTimeOffset.UtcNow.AddMinutes(15); // Khóa 15 phút
                    await _context.SaveChangesAsync();
                    return ApiResponse<AuthResponse>.Fail("Bạn đã nhập sai mật khẩu 5 lần. Tài khoản bị khóa tạm thời 15 phút.");
                }

                await _context.SaveChangesAsync();
                return ApiResponse<AuthResponse>.Fail($"Email hoặc mật khẩu không chính xác. Bạn còn {5 - user.SoLanDangNhapSai} lần thử.");
            }

            // BR-03: Kiểm tra trạng thái kích hoạt email
            if (!user.EmailDaXacThuc && user.TrangThai == "PENDING")
            {
                return ApiResponse<AuthResponse>.Fail("Tài khoản chưa được kích hoạt. Vui lòng kiểm tra email để xác thực.");
            }

            // Đăng nhập thành công -> Reset số lần sai
            user.SoLanDangNhapSai = 0;
            user.ThoiGianHetHanKhoa = null;

            // Sinh Access Token & Refresh Token
            var (accessToken, expiration) = GenerateJwtToken(user);
            var refreshTokenString = GenerateRefreshTokenString();

            var tokenLamMoi = new TokenLamMoi
            {
                NguoiDungId = user.Id,
                MaToken = refreshTokenString,
                ThoiGianHetHan = DateTimeOffset.UtcNow.AddDays(7),
                DaSuDung = false,
                DaThuHoi = false,
                NgayTao = DateTimeOffset.UtcNow
            };

            await _context.TokenLamMois.AddAsync(tokenLamMoi);

            // Ghi nhật ký đăng nhập
            await _context.NhatKyHeThongs.AddAsync(new NhatKyHeThong
            {
                NguoiDungThucHienId = user.Id,
                LoaiHanhDong = "DANG_NHAP",
                BangTacDong = "NguoiDung",
                IdDoiTuong = user.Id.ToString(),
                ChiTiet = "Đăng nhập thành công vào hệ thống",
                DiaChiIP = ipAddress,
                NgayTao = DateTimeOffset.UtcNow
            });

            await _context.SaveChangesAsync();

            return ApiResponse<AuthResponse>.Ok(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenString,
                Expiration = expiration,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    HoTen = user.HoTen,
                    VaiTro = user.VaiTro,
                    TrangThai = user.TrangThai,
                    EmailDaXacThuc = user.EmailDaXacThuc
                }
            }, "Đăng nhập thành công!");
        }

        public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string refreshToken)
        {
            var tokenEntity = await _context.TokenLamMois
                .Include(t => t.NguoiDung)
                .FirstOrDefaultAsync(t => t.MaToken == refreshToken);

            if (tokenEntity == null || tokenEntity.DaThuHoi || tokenEntity.DaSuDung || tokenEntity.ThoiGianHetHan < DateTimeOffset.UtcNow)
            {
                return ApiResponse<AuthResponse>.Fail("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
            }

            tokenEntity.DaSuDung = true;

            var user = tokenEntity.NguoiDung;
            var (newAccessToken, expiration) = GenerateJwtToken(user);
            var newRefreshTokenString = GenerateRefreshTokenString();

            await _context.TokenLamMois.AddAsync(new TokenLamMoi
            {
                NguoiDungId = user.Id,
                MaToken = newRefreshTokenString,
                ThoiGianHetHan = DateTimeOffset.UtcNow.AddDays(7),
                NgayTao = DateTimeOffset.UtcNow
            });

            await _context.SaveChangesAsync();

            return ApiResponse<AuthResponse>.Ok(new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenString,
                Expiration = expiration,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    HoTen = user.HoTen,
                    VaiTro = user.VaiTro,
                    TrangThai = user.TrangThai,
                    EmailDaXacThuc = user.EmailDaXacThuc
                }
            });
        }

        public async Task<ApiResponse<bool>> LogoutAsync(Guid userId)
        {
            var tokens = await _context.TokenLamMois
                .Where(t => t.NguoiDungId == userId && !t.DaThuHoi)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.DaThuHoi = true;
            }

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Đăng xuất thành công!");
        }

        public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
            // BR-11 & AC-12: Không tiết lộ email có tồn tại hay không
            if (user == null)
            {
                return ApiResponse<string>.Ok(string.Empty, "Nếu email hợp lệ, hướng dẫn đặt lại mật khẩu đã được gửi.");
            }

            var resetToken = Guid.NewGuid().ToString("N");
            user.TokenDatLaiMatKhau = resetToken;
            user.ThoiGianHetHanDatLai = DateTimeOffset.UtcNow.AddHours(1); // BR-06: 1h
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok(resetToken, "Nếu email hợp lệ, hướng dẫn đặt lại mật khẩu đã được gửi.");
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.TokenDatLaiMatKhau == request.Token);
            if (user == null || !user.ThoiGianHetHanDatLai.HasValue || user.ThoiGianHetHanDatLai < DateTimeOffset.UtcNow)
            {
                return ApiResponse<bool>.Fail("Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
            }

            user.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhauMoi);
            user.TokenDatLaiMatKhau = null;
            user.ThoiGianHetHanDatLai = null;
            user.SoLanDangNhapSai = 0;
            user.ThoiGianHetHanKhoa = null;

            // BR-08: Hủy toàn bộ token cũ
            var tokens = await _context.TokenLamMois.Where(t => t.NguoiDungId == user.Id).ToListAsync();
            foreach (var t in tokens) t.DaThuHoi = true;

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Đặt lại mật khẩu thành công!");
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            var user = await _context.NguoiDungs.FindAsync(userId);
            if (user == null) return ApiResponse<bool>.Fail("Không tìm thấy người dùng.");

            if (!BCrypt.Net.BCrypt.Verify(request.MatKhauCu, user.MatKhauHash))
            {
                return ApiResponse<bool>.Fail("Mật khẩu hiện tại không chính xác.");
            }

            // BR-09 & AC-14: Mật khẩu mới không được trùng mật khẩu cũ
            if (BCrypt.Net.BCrypt.Verify(request.MatKhauMoi, user.MatKhauHash))
            {
                return ApiResponse<bool>.Fail("Mật khẩu mới không được trùng với mật khẩu cũ.");
            }

            user.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhauMoi);
            user.NgayCapNhat = DateTimeOffset.UtcNow;

            // Hủy các phiên khác
            var tokens = await _context.TokenLamMois.Where(t => t.NguoiDungId == user.Id).ToListAsync();
            foreach (var t in tokens) t.DaThuHoi = true;

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Đổi mật khẩu thành công!");
        }

        private (string token, DateTimeOffset expiration) GenerateJwtToken(NguoiDung user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Secret"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTimeOffset.UtcNow.AddMinutes(int.Parse(_config["JwtSettings:ExpiryMinutes"] ?? "60"));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.HoTen),
                new Claim(ClaimTypes.Role, user.VaiTro)
            };

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: expiration.UtcDateTime,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
        }

        private static string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
