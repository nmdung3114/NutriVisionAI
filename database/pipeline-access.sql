/* =====================================================================
   Tài khoản SQL Server cho nhóm Pipeline (dbt / Airflow) – chạy 1 lần, chạy lại an toàn.
   Nguyên tắc quyền tối thiểu:
     - ĐỌC toàn bộ bảng nghiệp vụ của web (schema dbo), TRỪ dữ liệu bí mật (mật khẩu, token).
     - GHI (thêm/sửa) duy nhất bảng dbo.ThucPhamThamChieu (pipeline cung cấp dữ liệu thực phẩm cho web).
     - Toàn quyền tạo bảng/view trong 2 schema riêng: staging, marts (đầu ra của dbt).
   Chạy bằng sqlcmd (mật khẩu truyền qua biến, không ghi cứng vào file):
     sqlcmd -S localhost,1433 -U sa -P "<mật khẩu sa>" -C -d NutriVisionDb \
            -v PIPELINE_PASSWORD="<mật khẩu mạnh>" -i database/pipeline-access.sql
   Trong đề tài, mật khẩu này nên được lưu trong HashiCorp Vault.
   ===================================================================== */
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'nutrivision_pipeline')
    CREATE LOGIN [nutrivision_pipeline] WITH PASSWORD = N'$(PIPELINE_PASSWORD)', CHECK_POLICY = ON;
ELSE
    ALTER LOGIN [nutrivision_pipeline] WITH PASSWORD = N'$(PIPELINE_PASSWORD)';
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'nutrivision_pipeline')
    CREATE USER [nutrivision_pipeline] FOR LOGIN [nutrivision_pipeline];
GO

-- Schema riêng cho dbt
IF SCHEMA_ID(N'staging') IS NULL EXEC (N'CREATE SCHEMA [staging] AUTHORIZATION [nutrivision_pipeline]');
IF SCHEMA_ID(N'marts')   IS NULL EXEC (N'CREATE SCHEMA [marts] AUTHORIZATION [nutrivision_pipeline]');
GO
GRANT CREATE TABLE, CREATE VIEW TO [nutrivision_pipeline];
GO

-- Đọc dữ liệu nghiệp vụ
GRANT SELECT ON SCHEMA::[dbo] TO [nutrivision_pipeline];
-- Không cho đọc bí mật
DENY SELECT ON OBJECT::[dbo].[TokenLamMoi] TO [nutrivision_pipeline];
DENY SELECT ON OBJECT::[dbo].[NguoiDung] ([MatKhauHash], [TokenXacThucEmail], [TokenDatLaiMatKhau]) TO [nutrivision_pipeline];
DENY SELECT ON OBJECT::[dbo].[__EFMigrationsHistory] TO [nutrivision_pipeline];
GO

-- Ghi dữ liệu thực phẩm tham chiếu (upsert theo MaThucPham; ngừng dùng thì đặt DangHoatDong = 0, KHÔNG xóa
-- vì ThanhPhanBuaAn đang tham chiếu tới)
GRANT INSERT, UPDATE ON OBJECT::[dbo].[ThucPhamThamChieu] TO [nutrivision_pipeline];
DENY DELETE ON OBJECT::[dbo].[ThucPhamThamChieu] TO [nutrivision_pipeline];
GO

PRINT N'Đã cấp quyền cho nutrivision_pipeline.';
