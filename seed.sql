-- ======================================================================
-- NutriVision AI - Seed Data cho Microsoft SQL Server
-- Tài khoản mặc định:
-- 1. Quản trị viên: admin@nutrivision.ai / Admin@123 (Role: ADMIN)
-- 2. Người dùng:   user@nutrivision.ai  / User@123  (Role: USER)
-- ======================================================================

USE [NutriVisionDb];
GO

-- 1. Insert Tai khoan Admin & User mau (Mat khau da ma hoa BCrypt)
DECLARE @AdminId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @UserId UNIQUEIDENTIFIER  = '22222222-2222-2222-2222-222222222222';

IF NOT EXISTS (SELECT 1 FROM [NguoiDung] WHERE [Email] = 'admin@nutrivision.ai')
BEGIN
    INSERT INTO [NguoiDung] ([Id], [Email], [EmailDaXacThuc], [MatKhauHash], [HoTen], [TrangThai], [VaiTro], [SoLanDangNhapSai], [NgayTao], [NgayCapNhat])
    VALUES (@AdminId, 'admin@nutrivision.ai', 1, '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', N'Trần Quản Trị', 'HoatDong', 'ADMIN', 0, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
END

IF NOT EXISTS (SELECT 1 FROM [NguoiDung] WHERE [Email] = 'user@nutrivision.ai')
BEGIN
    INSERT INTO [NguoiDung] ([Id], [Email], [EmailDaXacThuc], [MatKhauHash], [HoTen], [TrangThai], [VaiTro], [SoLanDangNhapSai], [NgayTao], [NgayCapNhat])
    VALUES (@UserId, 'user@nutrivision.ai', 1, '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', N'Nguyễn Minh Anh', 'HoatDong', 'USER', 0, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
    
    -- Ho so suc khoe mau cho User
    INSERT INTO [HoSoNguoiDung] ([Id], [NguoiDungId], [NgaySinh], [GioiTinh], [ChieuCao], [CanNang], [MucDoVanDong], [ChiSoBMI], [ChiSoBMR], [ChiSoTDEE], [MucTieuCalo], [NgayCapNhat])
    VALUES (NEWID(), @UserId, '2000-01-01', 'Nam', 172.0, 68.0, 'VuaPhai', 23.0, 1650.0, 2200.0, 2200.0, SYSDATETIMEOFFSET());
END
GO

-- 2. Insert Danh muc 30 mon an Viet Nam tieu bieu
INSERT INTO [ThucPhamThamChieu] ([Id], [MaThucPham], [TenThucPham], [NhomThucPham], [DonViChuan], [KhoiLuongChuan], [CaloTren100g], [ChatDamTren100g], [TinhBotTren100g], [ChatBeoTren100g], [ChatXoTren100g], [DangHoatDong])
VALUES
(NEWID(), 'PHO_BO', N'Phở bò tái nạm', N'Món nước', N'tô', 450.0, 108.0, 6.2, 14.5, 2.7, 0.8, 1),
(NEWID(), 'PHO_GA', N'Phở gà xé', N'Món nước', N'tô', 450.0, 95.0, 7.1, 13.8, 1.8, 0.5, 1),
(NEWID(), 'BUN_CHA', N'Bún chả Hà Nội', N'Món bún', N'phần', 400.0, 145.0, 8.5, 16.2, 5.2, 1.2, 1),
(NEWID(), 'BUN_BO_HUE', N'Bún bò Huế', N'Món nước', N'tô', 500.0, 116.0, 7.0, 14.0, 3.5, 1.0, 1),
(NEWID(), 'COM_TAM_SUON', N'Cơm tấm sườn bì chả', N'Món cơm', N'dĩa', 450.0, 168.0, 8.2, 21.5, 6.1, 0.9, 1),
(NEWID(), 'BANH_MI_THIT', N'Bánh mì kẹp thịt pate', N'Bánh mì', N'ổ', 160.0, 260.0, 9.5, 34.0, 9.8, 1.5, 1),
(NEWID(), 'GOI_CUON', N'Gỏi cuốn tôm thịt', N'Khai vị', N'cuốn', 70.0, 115.0, 6.0, 16.0, 2.0, 1.1, 1),
(NEWID(), 'CHA_GIO', N'Chả giò chiên giòn', N'Khai vị', N'cuốn', 50.0, 240.0, 7.8, 18.2, 15.5, 0.9, 1),
(NEWID(), 'CANH_CHUA_CA', N'Canh chua cá lóc', N'Món canh', N'tô', 300.0, 48.0, 4.5, 3.2, 1.5, 1.2, 1),
(NEWID(), 'THIT_KHO_TAU', N'Thịt heo kho trứng cút', N'Món mặn', N'chén', 150.0, 220.0, 14.0, 2.5, 17.5, 0.0, 1),
(NEWID(), 'RAU_MUONG_XAO', N'Rau muống xào tỏi', N'Món xào', N'dĩa', 150.0, 72.0, 2.4, 4.1, 5.2, 2.8, 1),
(NEWID(), 'COM_TRANG', N'Cơm trắng gạo tẻ', N'Tinh bột', N'chén', 150.0, 130.0, 2.7, 28.2, 0.3, 0.4, 1),
(NEWID(), 'TRUNG_OP_LA', N'Trứng gà ốp la', N'Món chiên', N'quả', 50.0, 155.0, 12.6, 1.1, 10.6, 0.0, 1),
(NEWID(), 'SUON_NUONG', N'Sườn heo nướng mật ong', N'Món nướng', N'miếng', 120.0, 245.0, 20.2, 1.8, 17.6, 0.0, 1),
(NEWID(), 'BANH_XEO', N'Bánh xèo miền Tây', N'Bánh mặn', N'cái', 250.0, 185.0, 6.5, 19.8, 9.2, 1.8, 1);
GO

PRINT N'NutriVision AI: Seed du lieu SQL Server thanh cong!';
GO
