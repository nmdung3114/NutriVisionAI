using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NutriVision.Api.Models.Entities;

namespace NutriVision.Api.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            // 1. Tạo tài khoản Admin & User mẫu nếu chưa có
            if (!await context.NguoiDungs.AnyAsync())
            {
                var adminUser = new NguoiDung
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Email = "admin@nutrivision.ai",
                    EmailDaXacThuc = true,
                    MatKhauHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), // Mật khẩu mẫu
                    HoTen = "Quản trị viên Hệ thống",
                    TrangThai = "ACTIVE",
                    VaiTro = "ADMIN",
                    NgayTao = DateTimeOffset.UtcNow,
                    NgayCapNhat = DateTimeOffset.UtcNow
                };

                var demoUser = new NguoiDung
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Email = "user@nutrivision.ai",
                    EmailDaXacThuc = true,
                    MatKhauHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                    HoTen = "Nguyễn Văn Dinh Dưỡng",
                    TrangThai = "ACTIVE",
                    VaiTro = "USER",
                    NgayTao = DateTimeOffset.UtcNow,
                    NgayCapNhat = DateTimeOffset.UtcNow
                };

                var demoProfile = new HoSoNguoiDung
                {
                    Id = Guid.NewGuid(),
                    NguoiDungId = demoUser.Id,
                    NgaySinh = new DateOnly(2000, 1, 1),
                    GioiTinh = "NAM",
                    ChieuCao = 172m,
                    CanNang = 68m,
                    MucDoVanDong = "VAN_DONG_VUA", // hệ số 1.55
                    ChiSoBMI = 23.0m, // 68 / (1.72^2)
                    ChiSoBMR = 1630m, // 10*68 + 6.25*172 - 5*24 + 5 = 680 + 1075 - 120 + 5 = 1640
                    ChiSoTDEE = 2542m, // 1640 * 1.55
                    MucTieuCalo = 2200m,
                    NgayCapNhat = DateTimeOffset.UtcNow
                };

                await context.NguoiDungs.AddRangeAsync(adminUser, demoUser);
                await context.HoSoNguoiDungs.AddAsync(demoProfile);
                await context.SaveChangesAsync();
            }

            // 2. Nạp dữ liệu Danh mục Thực phẩm Chuẩn (ThucPhamThamChieu)
            if (!await context.ThucPhamThamChieus.AnyAsync())
            {
                var danhMucThucPham = new List<ThucPhamThamChieu>
                {
                    // Tinh bột & Món sợi phổ biến
                    new() { MaThucPham = "TP001", TenThucPham = "Cơm trắng", NhomThucPham = "Ngũ cốc & Tinh bột", DonViChuan = "Bát", KhoiLuongChuan = 150, CaloTren100g = 130, ChatDamTren100g = 2.7m, TinhBotTren100g = 28.2m, ChatBeoTren100g = 0.3m, ChatXoTren100g = 0.4m },
                    new() { MaThucPham = "TP002", TenThucPham = "Cơm gạo lứt", NhomThucPham = "Ngũ cốc & Tinh bột", DonViChuan = "Bát", KhoiLuongChuan = 150, CaloTren100g = 111, ChatDamTren100g = 2.6m, TinhBotTren100g = 23.0m, ChatBeoTren100g = 0.9m, ChatXoTren100g = 1.8m },
                    new() { MaThucPham = "TP003", TenThucPham = "Phở bò tái chín", NhomThucPham = "Món nước & Sợi", DonViChuan = "Bát lớn", KhoiLuongChuan = 450, CaloTren100g = 95, ChatDamTren100g = 6.2m, TinhBotTren100g = 12.5m, ChatBeoTren100g = 2.2m, ChatXoTren100g = 0.5m },
                    new() { MaThucPham = "TP004", TenThucPham = "Phở gà", NhomThucPham = "Món nước & Sợi", DonViChuan = "Bát lớn", KhoiLuongChuan = 450, CaloTren100g = 88, ChatDamTren100g = 5.8m, TinhBotTren100g = 12.0m, ChatBeoTren100g = 1.8m, ChatXoTren100g = 0.4m },
                    new() { MaThucPham = "TP005", TenThucPham = "Bún chả Hà Nội", NhomThucPham = "Món bún", DonViChuan = "Suất", KhoiLuongChuan = 350, CaloTren100g = 168, ChatDamTren100g = 7.5m, TinhBotTren100g = 18.0m, ChatBeoTren100g = 7.2m, ChatXoTren100g = 1.0m },
                    new() { MaThucPham = "TP006", TenThucPham = "Bún bò Huế", NhomThucPham = "Món bún", DonViChuan = "Tô", KhoiLuongChuan = 500, CaloTren100g = 105, ChatDamTren100g = 6.5m, TinhBotTren100g = 13.0m, ChatBeoTren100g = 3.0m, ChatXoTren100g = 0.6m },
                    new() { MaThucPham = "TP007", TenThucPham = "Bánh mì kẹp thịt pate", NhomThucPham = "Bánh mì", DonViChuan = "Ổ", KhoiLuongChuan = 160, CaloTren100g = 280, ChatDamTren100g = 11.2m, TinhBotTren100g = 34.0m, ChatBeoTren100g = 10.5m, ChatXoTren100g = 1.5m },
                    new() { MaThucPham = "TP008", TenThucPham = "Khoai lang luộc", NhomThucPham = "Củ & Tinh bột", DonViChuan = "Củ", KhoiLuongChuan = 150, CaloTren100g = 86, ChatDamTren100g = 1.6m, TinhBotTren100g = 20.1m, ChatBeoTren100g = 0.1m, ChatXoTren100g = 3.0m },
                    new() { MaThucPham = "TP009", TenThucPham = "Yến mạch cán dẹt", NhomThucPham = "Ngũ cốc", DonViChuan = "Chén", KhoiLuongChuan = 50, CaloTren100g = 389, ChatDamTren100g = 16.9m, TinhBotTren100g = 66.3m, ChatBeoTren100g = 6.9m, ChatXoTren100g = 10.6m },

                    // Thịt, Cá & Hải sản (Protein)
                    new() { MaThucPham = "TP010", TenThucPham = "Ức gà không da luộc", NhomThucPham = "Thịt & Gia cầm", DonViChuan = "Miếng", KhoiLuongChuan = 100, CaloTren100g = 165, ChatDamTren100g = 31.0m, TinhBotTren100g = 0m, ChatBeoTren100g = 3.6m, ChatXoTren100g = 0m },
                    new() { MaThucPham = "TP011", TenThucPham = "Thịt thăn bò áp chảo", NhomThucPham = "Thịt đỏ", DonViChuan = "Miếng", KhoiLuongChuan = 100, CaloTren100g = 217, ChatDamTren100g = 26.1m, TinhBotTren100g = 0m, ChatBeoTren100g = 11.8m, ChatXoTren100g = 0m },
                    new() { MaThucPham = "TP012", TenThucPham = "Thịt lợn nạc luộc", NhomThucPham = "Thịt lợn", DonViChuan = "Miếng", KhoiLuongChuan = 100, CaloTren100g = 143, ChatDamTren100g = 26.0m, TinhBotTren100g = 0m, ChatBeoTren100g = 3.5m, ChatXoTren100g = 0m },
                    new() { MaThucPham = "TP013", TenThucPham = "Cá hồi áp chảo", NhomThucPham = "Thủy hải sản", DonViChuan = "Miếng", KhoiLuongChuan = 120, CaloTren100g = 208, ChatDamTren100g = 22.0m, TinhBotTren100g = 0m, ChatBeoTren100g = 13.0m, ChatXoTren100g = 0m },
                    new() { MaThucPham = "TP014", TenThucPham = "Tôm sú hấp", NhomThucPham = "Thủy hải sản", DonViChuan = "Con", KhoiLuongChuan = 100, CaloTren100g = 99, ChatDamTren100g = 24.0m, TinhBotTren100g = 0.2m, ChatBeoTren100g = 0.3m, ChatXoTren100g = 0m },
                    new() { MaThucPham = "TP015", TenThucPham = "Trứng gà luộc", NhomThucPham = "Trứng", DonViChuan = "Quả", KhoiLuongChuan = 55, CaloTren100g = 155, ChatDamTren100g = 13.0m, TinhBotTren100g = 1.1m, ChatBeoTren100g = 11.0m, ChatXoTren100g = 0m },
                    new() { MaThucPham = "TP016", TenThucPham = "Trứng gà ốp la (chiên)", NhomThucPham = "Trứng", DonViChuan = "Quả", KhoiLuongChuan = 55, CaloTren100g = 196, ChatDamTren100g = 13.6m, TinhBotTren100g = 0.8m, ChatBeoTren100g = 15.3m, ChatXoTren100g = 0m },
                    new() { MaThucPham = "TP017", TenThucPham = "Đậu phụ luộc", NhomThucPham = "Sản phẩm từ đậu", DonViChuan = "Bìa", KhoiLuongChuan = 100, CaloTren100g = 76, ChatDamTren100g = 8.1m, TinhBotTren100g = 1.9m, ChatBeoTren100g = 4.8m, ChatXoTren100g = 0.4m },

                    // Rau củ & Quả (Chất xơ & Vitamin)
                    new() { MaThucPham = "TP018", TenThucPham = "Rau muống luộc", NhomThucPham = "Rau xanh", DonViChuan = "Đĩa", KhoiLuongChuan = 150, CaloTren100g = 23, ChatDamTren100g = 3.0m, TinhBotTren100g = 2.4m, ChatBeoTren100g = 0.4m, ChatXoTren100g = 2.1m },
                    new() { MaThucPham = "TP019", TenThucPham = "Súp lơ xanh luộc (Bông cải xanh)", NhomThucPham = "Rau xanh", DonViChuan = "Đĩa", KhoiLuongChuan = 150, CaloTren100g = 34, ChatDamTren100g = 2.8m, TinhBotTren100g = 6.6m, ChatBeoTren100g = 0.4m, ChatXoTren100g = 2.6m },
                    new() { MaThucPham = "TP020", TenThucPham = "Xà lách xắt nhỏ", NhomThucPham = "Rau sống", DonViChuan = "Bát", KhoiLuongChuan = 80, CaloTren100g = 15, ChatDamTren100g = 1.4m, TinhBotTren100g = 2.9m, ChatBeoTren100g = 0.2m, ChatXoTren100g = 1.3m },
                    new() { MaThucPham = "TP021", TenThucPham = "Cà rốt luộc", NhomThucPham = "Củ quả", DonViChuan = "Củ", KhoiLuongChuan = 100, CaloTren100g = 35, ChatDamTren100g = 0.8m, TinhBotTren100g = 8.2m, ChatBeoTren100g = 0.2m, ChatXoTren100g = 3.0m },
                    new() { MaThucPham = "TP022", TenThucPham = "Dưa leo (dưa chuột)", NhomThucPham = "Rau củ", DonViChuan = "Quả", KhoiLuongChuan = 120, CaloTren100g = 16, ChatDamTren100g = 0.7m, TinhBotTren100g = 3.6m, ChatBeoTren100g = 0.1m, ChatXoTren100g = 0.5m },
                    new() { MaThucPham = "TP023", TenThucPham = "Cà chua tươi", NhomThucPham = "Rau củ", DonViChuan = "Quả", KhoiLuongChuan = 100, CaloTren100g = 18, ChatDamTren100g = 0.9m, TinhBotTren100g = 3.9m, ChatBeoTren100g = 0.2m, ChatXoTren100g = 1.2m },

                    // Trái cây
                    new() { MaThucPham = "TP024", TenThucPham = "Chuối tiêu chín", NhomThucPham = "Trái cây", DonViChuan = "Quả", KhoiLuongChuan = 100, CaloTren100g = 89, ChatDamTren100g = 1.1m, TinhBotTren100g = 22.8m, ChatBeoTren100g = 0.3m, ChatXoTren100g = 2.6m },
                    new() { MaThucPham = "TP025", TenThucPham = "Táo tây tươi", NhomThucPham = "Trái cây", DonViChuan = "Quả", KhoiLuongChuan = 150, CaloTren100g = 52, ChatDamTren100g = 0.3m, TinhBotTren100g = 13.8m, ChatBeoTren100g = 0.2m, ChatXoTren100g = 2.4m },
                    new() { MaThucPham = "TP026", TenThucPham = "Cam tươi", NhomThucPham = "Trái cây", DonViChuan = "Quả", KhoiLuongChuan = 150, CaloTren100g = 47, ChatDamTren100g = 0.9m, TinhBotTren100g = 11.8m, ChatBeoTren100g = 0.1m, ChatXoTren100g = 2.4m },
                    new() { MaThucPham = "TP027", TenThucPham = "Bơ sáp", NhomThucPham = "Trái cây & Chất béo tốt", DonViChuan = "Trái", KhoiLuongChuan = 150, CaloTren100g = 160, ChatDamTren100g = 2.0m, TinhBotTren100g = 8.5m, ChatBeoTren100g = 14.7m, ChatXoTren100g = 6.7m },

                    // Sữa & Đồ uống
                    new() { MaThucPham = "TP028", TenThucPham = "Sữa chua không đường", NhomThucPham = "Sữa & Chế phẩm", DonViChuan = "Hộp", KhoiLuongChuan = 100, CaloTren100g = 61, ChatDamTren100g = 3.5m, TinhBotTren100g = 4.7m, ChatBeoTren100g = 3.3m, ChatXoTren100g = 0m },
                    new() { MaThucPham = "TP029", TenThucPham = "Sữa tươi tiệt trùng không đường", NhomThucPham = "Sữa & Chế phẩm", DonViChuan = "Hộp", KhoiLuongChuan = 180, CaloTren100g = 60, ChatDamTren100g = 3.0m, TinhBotTren100g = 4.5m, ChatBeoTren100g = 3.3m, ChatXoTren100g = 0m },
                    new() { MaThucPham = "TP030", TenThucPham = "Hạnh nhân hạt sấy khô", NhomThucPham = "Hạt dinh dưỡng", DonViChuan = "Nắm tay", KhoiLuongChuan = 30, CaloTren100g = 579, ChatDamTren100g = 21.2m, TinhBotTren100g = 21.6m, ChatBeoTren100g = 49.9m, ChatXoTren100g = 12.5m }
                };

                await context.ThucPhamThamChieus.AddRangeAsync(danhMucThucPham);
                await context.SaveChangesAsync();
            }
        }
    }
}
