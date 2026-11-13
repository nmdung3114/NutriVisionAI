# Bổ sung ERD – 2 bảng theo dõi sức khỏe

Ngoài 9 bảng theo ERD gốc, ứng dụng thêm 2 bảng phục vụ chức năng theo dõi cân nặng và nước uống. Cả hai đều quan hệ N–1 với `NguoiDung` (xóa người dùng → xóa theo), mỗi người dùng tối đa 1 bản ghi mỗi ngày.

### NhatKyCanNang

| Cột | Kiểu (SQL Server) | Ràng buộc |
|---|---|---|
| Id | uniqueidentifier | PK |
| NguoiDungId | uniqueidentifier | FK → NguoiDung(Id), ON DELETE CASCADE |
| Ngay | date | UNIQUE (NguoiDungId, Ngay) |
| CanNang | decimal(5,2) | 10–150 kg (kiểm tra ở tầng ứng dụng) |
| GhiChu | nvarchar(200) | NULL |
| NgayTao, NgayCapNhat | datetimeoffset | |

### NhatKyNuocUong

| Cột | Kiểu (SQL Server) | Ràng buộc |
|---|---|---|
| Id | uniqueidentifier | PK |
| NguoiDungId | uniqueidentifier | FK → NguoiDung(Id), ON DELETE CASCADE |
| Ngay | date | UNIQUE (NguoiDungId, Ngay) |
| LuongMl | int | CHECK 0 ≤ LuongMl ≤ 10000 |
| NgayCapNhat | datetimeoffset | |

Migration: `ThemNhatKyCanNang`, `ThemNhatKyNuocUong` (đã gộp trong `database/schema.sql`).

Các chức năng chuỗi ngày, huy hiệu, lịch tháng, nhận xét tuần **không thêm bảng**: chúng được tính từ `BuaAn` và `TongHopDinhDuongNgay`.
