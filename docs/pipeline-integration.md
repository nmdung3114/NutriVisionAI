# Tích hợp Nền tảng dữ liệu (pipeline) với web NutriVision AI

Tài liệu cho nhóm làm pipeline (dbt / Astronomer Cosmos / Airflow / Superset): đọc dữ liệu nào, đọc thế nào cho đúng, ghi vào đâu.
Nguồn dữ liệu là **SQL Server** của web (database `NutriVisionDb`); schema tạo bởi migration EF Core (`database/schema.sql`).

## 1. Kết nối và quyền

- Tài khoản riêng `nutrivision_pipeline`, tạo bằng `database/pipeline-access.sql` (quyền tối thiểu). Mật khẩu nên lưu trong Vault.
  - **Đọc** mọi bảng `dbo` trừ bí mật (`TokenLamMoi`, cột mật khẩu/token của `NguoiDung`).
  - **Ghi** duy nhất `dbo.ThucPhamThamChieu` (thêm/sửa, không xóa).
  - **Toàn quyền** trong schema `staging`, `marts` (đầu ra của dbt). Không tạo bảng trong `dbo`: schema `dbo` do migration của web quản lý.
- Chuỗi kết nối mẫu (dbt-sqlserver / pyodbc): `Server=<host>,1433; Database=NutriVisionDb; UID=nutrivision_pipeline; PWD=<từ Vault>; TrustServerCertificate=yes`.

## 2. Quy ước dữ liệu quan trọng

- **Thời điểm** (`datetimeoffset`): lưu theo **UTC**. **Ngày** (`date`, vd `NgayAn`, `Ngay`): là ngày theo giờ Việt Nam (Asia/Ho_Chi_Minh).
- **Id**: `uniqueidentifier` (GUID), không phải số tăng dần → không dùng Id làm mốc nạp tăng dần.
- **Bữa ăn hợp lệ** = `BuaAn.DaXacNhan = 1 AND BuaAn.DaXoa = 0`. Bản nháp (`DaXacNhan = 0`) là kết quả AI chưa được người dùng chốt; bữa đã xóa là **xóa mềm** (`DaXoa = 1`), vẫn còn trong DB.
- **Giá trị liệt kê**:
  - `BuaAn.LoaiBuaAn`: `Sang`, `Trua`, `Toi`, `Phu`
  - `NguoiDung.TrangThai`: `PENDING`, `ACTIVE`, `LOCKED` · `NguoiDung.VaiTro`: `USER`, `ADMIN`
  - `HoSoNguoiDung.GioiTinh`: `Nam`, `Nu` · `MucDoVanDong`: `ItVanDong`, `VanDongNhe`, `VuaPhai`, `NangDong`, `RatNangDong`
- **Đơn vị**: năng lượng kcal, khối lượng gram, cân nặng kg, chiều cao cm, nước ml. Giá trị `...Tren100g` là trên 100 g.

## 3. Bảng nguồn và cách nạp tăng dần (incremental)

| Bảng | Nội dung | Cột mốc (watermark) | Lưu ý khi nạp |
|---|---|---|---|
| `BuaAn` | Bữa ăn, tổng dinh dưỡng đã chốt | `NgayCapNhat` | Lọc `DaXacNhan=1`; giữ cờ `DaXoa` để loại bữa đã xóa |
| `ThanhPhanBuaAn` | Thành phần từng bữa | *không có* → theo `BuaAn.NgayCapNhat` | Khi bữa được sửa, **thành phần cũ bị xóa hẳn và thay mới** → với mỗi `BuaAnId` thay đổi, nạp lại toàn bộ thành phần của bữa đó (delete + insert) |
| `LichSuNhanDienAI` | Kết quả AI gốc vs người dùng chốt | `NgayCapNhat` | Được cập nhật khi người dùng xác nhận/sửa/hoàn tác |
| `TongHopDinhDuongNgay` | Tổng dinh dưỡng theo ngày (chỉ bữa hợp lệ) | `NgayCapNhat` | Khóa tự nhiên (`NguoiDungId`, `Ngay`) |
| `NhatKyCanNang`, `NhatKyNuocUong` | Cân nặng, nước uống theo ngày | `NgayCapNhat` | Khóa tự nhiên (`NguoiDungId`, `Ngay`); bản ghi cân nặng có thể bị người dùng xóa hẳn |
| `NguoiDung`, `HoSoNguoiDung` | Người dùng, thể trạng, mục tiêu calo | `NgayCapNhat` | Không đọc cột bí mật (đã bị DENY) |
| `ThucPhamThamChieu` | Thực phẩm tham chiếu | – | Pipeline **ghi** bảng này (mục 5) |
| `NhatKyHeThong` | Nhật ký hệ thống (audit) | `NgayTao` | Chỉ thêm, không sửa |

**Xóa cứng** chỉ xảy ra với: bản nháp bị bỏ dở quá 24 giờ (job dọn của web, xóa cả `LichSuNhanDienAI` liên quan), thành phần bữa ăn khi bữa được sửa, bản ghi cân nặng. Nếu cần đối soát tuyệt đối, chạy định kỳ một lần nạp toàn bộ (full refresh) cho các bảng này.

## 4. Đánh giá độ chính xác AI

`LichSuNhanDienAI` mỗi dòng = một lần quét ảnh:

| Cột | Ý nghĩa |
|---|---|
| `TenMonAIDoan` | Tên món AI đoán (NULL nếu AI lỗi / không nhận diện được) |
| `TenMonNguoiDungChot` | Tên món người dùng chốt khi xác nhận |
| `NguoiDungCoSua` | 1 nếu người dùng đã sửa tên hoặc thành phần so với AI |
| `DoTinCay` | Độ tin cậy AI tự báo (0–1) |
| `ThoiGianXuLyMs` | Thời gian AI xử lý |
| `PhanHoiAIGoc` | JSON kết quả AI **đã chuẩn hóa** (trước khi người dùng sửa), xem bên dưới |
| `BuaAnId` | Bữa tương ứng → so với `ThanhPhanBuaAn` người dùng chốt |

```json
{ "tenMonAn": "Phở bò tái nạm", "doTinCay": 0.91,
  "thanhPhan": [ { "tenThanhPhan": "Phở bò tái nạm", "khauPhan": 480, "donViTinh": "g", "khoiLuongGram": 480,
                   "thucPhamThamChieuId": "…guid hoặc null nếu không khớp bảng tham chiếu…" } ] }
```

Ví dụ chỉ số cho mart (SQL Server):
```sql
SELECT CAST(l.NgayTao AS date)                                         AS ngay,
       COUNT(*)                                                         AS so_lan_quet,
       AVG(CASE WHEN l.TenMonAIDoan IS NULL THEN 1.0 ELSE 0 END)        AS ty_le_ai_that_bai,
       AVG(CASE WHEN b.DaXacNhan = 1 AND l.NguoiDungCoSua = 0 THEN 1.0 ELSE 0 END) AS ty_le_giu_nguyen_ket_qua_ai,
       AVG(CAST(l.DoTinCay AS float))                                   AS do_tin_cay_tb,
       AVG(CAST(l.ThoiGianXuLyMs AS float))                             AS thoi_gian_xu_ly_tb_ms
FROM dbo.LichSuNhanDienAI l
LEFT JOIN dbo.BuaAn b ON b.Id = l.BuaAnId
GROUP BY CAST(l.NgayTao AS date);
```
Sai số khối lượng / thành phần thêm-bớt: tách `PhanHoiAIGoc.thanhPhan` bằng `OPENJSON` rồi so với `ThanhPhanBuaAn` cùng `BuaAnId`.

## 5. Pipeline ghi dữ liệu thực phẩm vào `ThucPhamThamChieu`

| Cột | Bắt buộc | Quy tắc |
|---|---|---|
| `Id` | ✔ | GUID mới khi thêm (`NEWID()`), **không đổi** khi cập nhật |
| `MaThucPham` | ✔ | Khóa nghiệp vụ, duy nhất, ≤ 50 ký tự (vd `PHO_BO`) → **upsert theo cột này** (`MERGE`) |
| `TenThucPham` | ✔ | Tên tiếng Việt có dấu, ≤ 200 ký tự (web tìm kiếm không dấu tự động) |
| `NhomThucPham` | ✔ | ≤ 100 ký tự |
| `DonViChuan`, `KhoiLuongChuan` | ✔ | Đơn vị khẩu phần chuẩn (vd `tô`) và số gram của 1 đơn vị đó |
| `CaloTren100g`, `ChatDamTren100g`, `TinhBotTren100g`, `ChatBeoTren100g`, `ChatXoTren100g` | ✔ | Giá trị trên 100 g, ≥ 0; calo ≤ 900, các chất ≤ 100 |
| `DangHoatDong` | ✔ | `1` = dùng; món ngừng cung cấp → đặt `0`, **không xóa** (bữa ăn cũ đang tham chiếu) |

- Web đọc danh mục qua cache 10 phút → dữ liệu mới xuất hiện trên web tối đa sau 10 phút, không cần khởi động lại.
- Khi pipeline đã cung cấp dữ liệu thật, tắt dữ liệu mẫu 110 món của web: `Database__SeedFoods=false`.
- Đề xuất dbt test cho bảng này: `unique`/`not_null` trên `MaThucPham`, `accepted_range` cho các cột dinh dưỡng, tổng năng lượng ước tính (4·đạm + 4·bột + 9·béo) lệch calo không quá ~20%.

## 6. Phụ lục: cột của các bảng (SQL Server)

### NguoiDung

| Cột | Kiểu SQL Server |
|---|---|
| Id | `uniqueidentifier NOT NULL` |
| Email | `nvarchar(256) NOT NULL` |
| EmailDaXacThuc | `bit NOT NULL` |
| MatKhauHash | `nvarchar(max) NOT NULL` |
| HoTen | `nvarchar(100) NOT NULL` |
| TrangThai | `nvarchar(30) NOT NULL` |
| VaiTro | `nvarchar(30) NOT NULL` |
| SoLanDangNhapSai | `int NOT NULL` |
| ThoiGianHetHanKhoa | `datetimeoffset NULL` |
| TokenXacThucEmail | `nvarchar(128) NULL` |
| ThoiGianHetHanXacThuc | `datetimeoffset NULL` |
| TokenDatLaiMatKhau | `nvarchar(128) NULL` |
| ThoiGianHetHanDatLai | `datetimeoffset NULL` |
| NgayTao | `datetimeoffset NOT NULL` |
| NgayCapNhat | `datetimeoffset NOT NULL` |

### ThucPhamThamChieu

| Cột | Kiểu SQL Server |
|---|---|
| Id | `uniqueidentifier NOT NULL` |
| MaThucPham | `nvarchar(50) NOT NULL` |
| TenThucPham | `nvarchar(200) NOT NULL` |
| NhomThucPham | `nvarchar(100) NOT NULL` |
| DonViChuan | `nvarchar(50) NOT NULL` |
| KhoiLuongChuan | `decimal(6,2) NOT NULL` |
| CaloTren100g | `decimal(6,2) NOT NULL` |
| ChatDamTren100g | `decimal(6,2) NOT NULL` |
| TinhBotTren100g | `decimal(6,2) NOT NULL` |
| ChatBeoTren100g | `decimal(6,2) NOT NULL` |
| ChatXoTren100g | `decimal(6,2) NOT NULL` |
| DangHoatDong | `bit NOT NULL` |

### BuaAn

| Cột | Kiểu SQL Server |
|---|---|
| Id | `uniqueidentifier NOT NULL` |
| NguoiDungId | `uniqueidentifier NOT NULL` |
| LoaiBuaAn | `nvarchar(20) NOT NULL` |
| NgayAn | `date NOT NULL` |
| ThoiGianGhiNhan | `datetimeoffset NOT NULL` |
| TenMonAn | `nvarchar(200) NOT NULL` |
| DuongDanHinhAnh | `nvarchar(500) NULL` |
| TongCalo | `decimal(7,2) NOT NULL` |
| TongChatDam | `decimal(6,2) NOT NULL` |
| TongTinhBot | `decimal(6,2) NOT NULL` |
| TongChatBeo | `decimal(6,2) NOT NULL` |
| TongChatXo | `decimal(6,2) NOT NULL` |
| TaoTuAI | `bit NOT NULL` |
| DaXacNhan | `bit NOT NULL` |
| DaXoa | `bit NOT NULL` |
| NgayTao | `datetimeoffset NOT NULL` |
| NgayCapNhat | `datetimeoffset NOT NULL` |

### HoSoNguoiDung

| Cột | Kiểu SQL Server |
|---|---|
| Id | `uniqueidentifier NOT NULL` |
| NguoiDungId | `uniqueidentifier NOT NULL` |
| NgaySinh | `date NOT NULL` |
| GioiTinh | `nvarchar(10) NOT NULL` |
| ChieuCao | `decimal(5,2) NOT NULL` |
| CanNang | `decimal(5,2) NOT NULL` |
| MucDoVanDong | `nvarchar(30) NOT NULL` |
| ChiSoBMI | `decimal(4,2) NOT NULL` |
| ChiSoBMR | `decimal(6,2) NOT NULL` |
| ChiSoTDEE | `decimal(6,2) NOT NULL` |
| MucTieuCalo | `decimal(6,2) NOT NULL` |
| DuongDanAnhDaiDien | `nvarchar(500) NULL` |
| NgayCapNhat | `datetimeoffset NOT NULL` |

### NhatKyHeThong

| Cột | Kiểu SQL Server |
|---|---|
| Id | `uniqueidentifier NOT NULL` |
| NguoiDungThucHienId | `uniqueidentifier NULL` |
| LoaiHanhDong | `nvarchar(50) NOT NULL` |
| BangTacDong | `nvarchar(50) NULL` |
| IdDoiTuong | `nvarchar(100) NULL` |
| ChiTiet | `nvarchar(max) NULL` |
| DiaChiIP | `nvarchar(50) NULL` |
| NgayTao | `datetimeoffset NOT NULL` |

### TokenLamMoi

| Cột | Kiểu SQL Server |
|---|---|
| Id | `uniqueidentifier NOT NULL` |
| NguoiDungId | `uniqueidentifier NOT NULL` |
| MaToken | `nvarchar(500) NOT NULL` |
| ThoiGianHetHan | `datetimeoffset NOT NULL` |
| DaSuDung | `bit NOT NULL` |
| DaThuHoi | `bit NOT NULL` |
| NgayTao | `datetimeoffset NOT NULL` |

### TongHopDinhDuongNgay

| Cột | Kiểu SQL Server |
|---|---|
| Id | `uniqueidentifier NOT NULL` |
| NguoiDungId | `uniqueidentifier NOT NULL` |
| Ngay | `date NOT NULL` |
| TongCalo | `decimal(7,2) NOT NULL` |
| TongChatDam | `decimal(6,2) NOT NULL` |
| TongTinhBot | `decimal(6,2) NOT NULL` |
| TongChatBeo | `decimal(6,2) NOT NULL` |
| TongChatXo | `decimal(6,2) NOT NULL` |
| SoBuaAnDaGhi | `int NOT NULL` |
| MucTieuCaloNgay | `decimal(6,2) NOT NULL` |
| NgayCapNhat | `datetimeoffset NOT NULL` |

### LichSuNhanDienAI

| Cột | Kiểu SQL Server |
|---|---|
| Id | `uniqueidentifier NOT NULL` |
| NguoiDungId | `uniqueidentifier NOT NULL` |
| BuaAnId | `uniqueidentifier NULL` |
| DuongDanHinhAnh | `nvarchar(500) NOT NULL` |
| PhanHoiAIGoc | `nvarchar(max) NULL` |
| TenMonAIDoan | `nvarchar(200) NULL` |
| TenMonNguoiDungChot | `nvarchar(200) NULL` |
| DoTinCay | `decimal(4,3) NULL` |
| NguoiDungCoSua | `bit NOT NULL` |
| ThoiGianXuLyMs | `int NULL` |
| NgayTao | `datetimeoffset NOT NULL` |
| NgayCapNhat | `datetimeoffset NOT NULL` |

### ThanhPhanBuaAn

| Cột | Kiểu SQL Server |
|---|---|
| Id | `uniqueidentifier NOT NULL` |
| BuaAnId | `uniqueidentifier NOT NULL` |
| ThucPhamThamChieuId | `uniqueidentifier NULL` |
| TenThanhPhan | `nvarchar(200) NOT NULL` |
| KhauPhan | `decimal(6,2) NOT NULL` |
| DonViTinh | `nvarchar(50) NOT NULL` |
| KhoiLuongQuyDoiGram | `decimal(6,2) NOT NULL` |
| LuongCalo | `decimal(7,2) NOT NULL` |
| LuongChatDam | `decimal(6,2) NOT NULL` |
| LuongTinhBot | `decimal(6,2) NOT NULL` |
| LuongChatBeo | `decimal(6,2) NOT NULL` |
| LuongChatXo | `decimal(6,2) NOT NULL` |

### NhatKyCanNang

| Cột | Kiểu SQL Server |
|---|---|
| Id | `uniqueidentifier NOT NULL` |
| NguoiDungId | `uniqueidentifier NOT NULL` |
| Ngay | `date NOT NULL` |
| CanNang | `decimal(5,2) NOT NULL` |
| GhiChu | `nvarchar(200) NULL` |
| NgayTao | `datetimeoffset NOT NULL` |
| NgayCapNhat | `datetimeoffset NOT NULL` |

### NhatKyNuocUong

| Cột | Kiểu SQL Server |
|---|---|
| Id | `uniqueidentifier NOT NULL` |
| NguoiDungId | `uniqueidentifier NOT NULL` |
| Ngay | `date NOT NULL` |
| LuongMl | `int NOT NULL` |
| NgayCapNhat | `datetimeoffset NOT NULL` |

