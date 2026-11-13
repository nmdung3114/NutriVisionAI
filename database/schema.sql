IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE TABLE [NguoiDung] (
        [Id] uniqueidentifier NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [EmailDaXacThuc] bit NOT NULL,
        [MatKhauHash] nvarchar(max) NOT NULL,
        [HoTen] nvarchar(100) NOT NULL,
        [TrangThai] nvarchar(30) NOT NULL,
        [VaiTro] nvarchar(30) NOT NULL,
        [SoLanDangNhapSai] int NOT NULL,
        [ThoiGianHetHanKhoa] datetimeoffset NULL,
        [TokenXacThucEmail] nvarchar(128) NULL,
        [ThoiGianHetHanXacThuc] datetimeoffset NULL,
        [TokenDatLaiMatKhau] nvarchar(128) NULL,
        [ThoiGianHetHanDatLai] datetimeoffset NULL,
        [NgayTao] datetimeoffset NOT NULL,
        [NgayCapNhat] datetimeoffset NOT NULL,
        CONSTRAINT [PK_NguoiDung] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE TABLE [ThucPhamThamChieu] (
        [Id] uniqueidentifier NOT NULL,
        [MaThucPham] nvarchar(50) NOT NULL,
        [TenThucPham] nvarchar(200) NOT NULL,
        [NhomThucPham] nvarchar(100) NOT NULL,
        [DonViChuan] nvarchar(50) NOT NULL,
        [KhoiLuongChuan] decimal(6,2) NOT NULL,
        [CaloTren100g] decimal(6,2) NOT NULL,
        [ChatDamTren100g] decimal(6,2) NOT NULL,
        [TinhBotTren100g] decimal(6,2) NOT NULL,
        [ChatBeoTren100g] decimal(6,2) NOT NULL,
        [ChatXoTren100g] decimal(6,2) NOT NULL,
        [DangHoatDong] bit NOT NULL,
        CONSTRAINT [PK_ThucPhamThamChieu] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE TABLE [BuaAn] (
        [Id] uniqueidentifier NOT NULL,
        [NguoiDungId] uniqueidentifier NOT NULL,
        [LoaiBuaAn] nvarchar(20) NOT NULL,
        [NgayAn] date NOT NULL,
        [ThoiGianGhiNhan] datetimeoffset NOT NULL,
        [TenMonAn] nvarchar(200) NOT NULL,
        [DuongDanHinhAnh] nvarchar(500) NULL,
        [TongCalo] decimal(7,2) NOT NULL,
        [TongChatDam] decimal(6,2) NOT NULL,
        [TongTinhBot] decimal(6,2) NOT NULL,
        [TongChatBeo] decimal(6,2) NOT NULL,
        [TongChatXo] decimal(6,2) NOT NULL,
        [TaoTuAI] bit NOT NULL,
        [DaXacNhan] bit NOT NULL,
        [DaXoa] bit NOT NULL,
        [NgayTao] datetimeoffset NOT NULL,
        [NgayCapNhat] datetimeoffset NOT NULL,
        CONSTRAINT [PK_BuaAn] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BuaAn_NguoiDung_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [NguoiDung] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE TABLE [HoSoNguoiDung] (
        [Id] uniqueidentifier NOT NULL,
        [NguoiDungId] uniqueidentifier NOT NULL,
        [NgaySinh] date NOT NULL,
        [GioiTinh] nvarchar(10) NOT NULL,
        [ChieuCao] decimal(5,2) NOT NULL,
        [CanNang] decimal(5,2) NOT NULL,
        [MucDoVanDong] nvarchar(30) NOT NULL,
        [ChiSoBMI] decimal(4,2) NOT NULL,
        [ChiSoBMR] decimal(6,2) NOT NULL,
        [ChiSoTDEE] decimal(6,2) NOT NULL,
        [MucTieuCalo] decimal(6,2) NOT NULL,
        [DuongDanAnhDaiDien] nvarchar(500) NULL,
        [NgayCapNhat] datetimeoffset NOT NULL,
        CONSTRAINT [PK_HoSoNguoiDung] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HoSoNguoiDung_NguoiDung_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [NguoiDung] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE TABLE [NhatKyHeThong] (
        [Id] uniqueidentifier NOT NULL,
        [NguoiDungThucHienId] uniqueidentifier NULL,
        [LoaiHanhDong] nvarchar(50) NOT NULL,
        [BangTacDong] nvarchar(50) NULL,
        [IdDoiTuong] nvarchar(100) NULL,
        [ChiTiet] nvarchar(max) NULL,
        [DiaChiIP] nvarchar(50) NULL,
        [NgayTao] datetimeoffset NOT NULL,
        CONSTRAINT [PK_NhatKyHeThong] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_NhatKyHeThong_NguoiDung_NguoiDungThucHienId] FOREIGN KEY ([NguoiDungThucHienId]) REFERENCES [NguoiDung] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE TABLE [TokenLamMoi] (
        [Id] uniqueidentifier NOT NULL,
        [NguoiDungId] uniqueidentifier NOT NULL,
        [MaToken] nvarchar(500) NOT NULL,
        [ThoiGianHetHan] datetimeoffset NOT NULL,
        [DaSuDung] bit NOT NULL,
        [DaThuHoi] bit NOT NULL,
        [NgayTao] datetimeoffset NOT NULL,
        CONSTRAINT [PK_TokenLamMoi] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TokenLamMoi_NguoiDung_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [NguoiDung] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE TABLE [TongHopDinhDuongNgay] (
        [Id] uniqueidentifier NOT NULL,
        [NguoiDungId] uniqueidentifier NOT NULL,
        [Ngay] date NOT NULL,
        [TongCalo] decimal(7,2) NOT NULL,
        [TongChatDam] decimal(6,2) NOT NULL,
        [TongTinhBot] decimal(6,2) NOT NULL,
        [TongChatBeo] decimal(6,2) NOT NULL,
        [TongChatXo] decimal(6,2) NOT NULL,
        [SoBuaAnDaGhi] int NOT NULL,
        [MucTieuCaloNgay] decimal(6,2) NOT NULL,
        [NgayCapNhat] datetimeoffset NOT NULL,
        CONSTRAINT [PK_TongHopDinhDuongNgay] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TongHopDinhDuongNgay_NguoiDung_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [NguoiDung] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE TABLE [LichSuNhanDienAI] (
        [Id] uniqueidentifier NOT NULL,
        [NguoiDungId] uniqueidentifier NOT NULL,
        [BuaAnId] uniqueidentifier NULL,
        [DuongDanHinhAnh] nvarchar(500) NOT NULL,
        [PhanHoiAIGoc] nvarchar(max) NULL,
        [TenMonAIDoan] nvarchar(200) NULL,
        [TenMonNguoiDungChot] nvarchar(200) NULL,
        [DoTinCay] decimal(4,3) NULL,
        [NguoiDungCoSua] bit NOT NULL,
        [ThoiGianXuLyMs] int NULL,
        [NgayTao] datetimeoffset NOT NULL,
        CONSTRAINT [PK_LichSuNhanDienAI] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LichSuNhanDienAI_BuaAn_BuaAnId] FOREIGN KEY ([BuaAnId]) REFERENCES [BuaAn] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_LichSuNhanDienAI_NguoiDung_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [NguoiDung] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE TABLE [ThanhPhanBuaAn] (
        [Id] uniqueidentifier NOT NULL,
        [BuaAnId] uniqueidentifier NOT NULL,
        [ThucPhamThamChieuId] uniqueidentifier NULL,
        [TenThanhPhan] nvarchar(200) NOT NULL,
        [KhauPhan] decimal(6,2) NOT NULL,
        [DonViTinh] nvarchar(50) NOT NULL,
        [KhoiLuongQuyDoiGram] decimal(6,2) NOT NULL,
        [LuongCalo] decimal(7,2) NOT NULL,
        [LuongChatDam] decimal(6,2) NOT NULL,
        [LuongTinhBot] decimal(6,2) NOT NULL,
        [LuongChatBeo] decimal(6,2) NOT NULL,
        [LuongChatXo] decimal(6,2) NOT NULL,
        CONSTRAINT [PK_ThanhPhanBuaAn] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ThanhPhanBuaAn_BuaAn_BuaAnId] FOREIGN KEY ([BuaAnId]) REFERENCES [BuaAn] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ThanhPhanBuaAn_ThucPhamThamChieu_ThucPhamThamChieuId] FOREIGN KEY ([ThucPhamThamChieuId]) REFERENCES [ThucPhamThamChieu] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BuaAn_NguoiDungId_NgayAn] ON [BuaAn] ([NguoiDungId], [NgayAn]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_HoSoNguoiDung_NguoiDungId] ON [HoSoNguoiDung] ([NguoiDungId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_LichSuNhanDienAI_BuaAnId] ON [LichSuNhanDienAI] ([BuaAnId]) WHERE [BuaAnId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LichSuNhanDienAI_NguoiDungId] ON [LichSuNhanDienAI] ([NguoiDungId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NguoiDung_Email] ON [NguoiDung] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NguoiDung_TokenDatLaiMatKhau] ON [NguoiDung] ([TokenDatLaiMatKhau]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NguoiDung_TokenXacThucEmail] ON [NguoiDung] ([TokenXacThucEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NhatKyHeThong_NgayTao] ON [NhatKyHeThong] ([NgayTao]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NhatKyHeThong_NguoiDungThucHienId] ON [NhatKyHeThong] ([NguoiDungThucHienId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ThanhPhanBuaAn_BuaAnId] ON [ThanhPhanBuaAn] ([BuaAnId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ThanhPhanBuaAn_ThucPhamThamChieuId] ON [ThanhPhanBuaAn] ([ThucPhamThamChieuId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ThucPhamThamChieu_MaThucPham] ON [ThucPhamThamChieu] ([MaThucPham]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TokenLamMoi_MaToken] ON [TokenLamMoi] ([MaToken]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TokenLamMoi_NguoiDungId] ON [TokenLamMoi] ([NguoiDungId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TongHopDinhDuongNgay_NguoiDungId_Ngay] ON [TongHopDinhDuongNgay] ([NguoiDungId], [Ngay]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112053122_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261112053122_InitialCreate', N'8.0.34');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112090413_ThemNhatKyCanNang'
)
BEGIN
    CREATE TABLE [NhatKyCanNang] (
        [Id] uniqueidentifier NOT NULL,
        [NguoiDungId] uniqueidentifier NOT NULL,
        [Ngay] date NOT NULL,
        [CanNang] decimal(5,2) NOT NULL,
        [GhiChu] nvarchar(200) NULL,
        [NgayTao] datetimeoffset NOT NULL,
        [NgayCapNhat] datetimeoffset NOT NULL,
        CONSTRAINT [PK_NhatKyCanNang] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_NhatKyCanNang_NguoiDung_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [NguoiDung] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112090413_ThemNhatKyCanNang'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NhatKyCanNang_NguoiDungId_Ngay] ON [NhatKyCanNang] ([NguoiDungId], [Ngay]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112090413_ThemNhatKyCanNang'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261112090413_ThemNhatKyCanNang', N'8.0.34');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112090837_ThemNhatKyNuocUong'
)
BEGIN
    CREATE TABLE [NhatKyNuocUong] (
        [Id] uniqueidentifier NOT NULL,
        [NguoiDungId] uniqueidentifier NOT NULL,
        [Ngay] date NOT NULL,
        [LuongMl] int NOT NULL,
        [NgayCapNhat] datetimeoffset NOT NULL,
        CONSTRAINT [PK_NhatKyNuocUong] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_NhatKyNuocUong_LuongMl] CHECK ([LuongMl] >= 0 AND [LuongMl] <= 10000),
        CONSTRAINT [FK_NhatKyNuocUong_NguoiDung_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [NguoiDung] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112090837_ThemNhatKyNuocUong'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NhatKyNuocUong_NguoiDungId_Ngay] ON [NhatKyNuocUong] ([NguoiDungId], [Ngay]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112090837_ThemNhatKyNuocUong'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261112090837_ThemNhatKyNuocUong', N'8.0.34');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113091934_ThemNgayCapNhatLichSuAI'
)
BEGIN
    ALTER TABLE [LichSuNhanDienAI] ADD [NgayCapNhat] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113091934_ThemNgayCapNhatLichSuAI'
)
BEGIN
    UPDATE [LichSuNhanDienAI] SET [NgayCapNhat] = [NgayTao]
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113091934_ThemNgayCapNhatLichSuAI'
)
BEGIN
    CREATE INDEX [IX_LichSuNhanDienAI_NgayCapNhat] ON [LichSuNhanDienAI] ([NgayCapNhat]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113091934_ThemNgayCapNhatLichSuAI'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261113091934_ThemNgayCapNhatLichSuAI', N'8.0.34');
END;
GO

COMMIT;
GO

