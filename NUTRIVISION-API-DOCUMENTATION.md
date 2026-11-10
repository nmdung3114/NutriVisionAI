# NutriVisionAI API Documentation

Tài liệu này mô tả các API hiện có trong backend NutriVisionAI, bao gồm route, phương thức HTTP, quyền truy cập, request, response, quy tắc nghiệp vụ, mã lỗi và các điểm cần lưu ý khi tích hợp frontend.

## 1. Thông tin chung

### 1.1 Backend

- Framework: ASP.NET Core .NET 8
- Database: SQLite hoặc SQL Server tùy `DatabaseProvider`
- Authentication: JWT Bearer
- ORM: Entity Framework Core 8
- API prefix: `/api`
- Swagger: bật trong môi trường Development

### 1.2 Base URL

Frontend hiện cấu hình mặc định:

```text
http://localhost:5000/api
```

Ví dụ:

```text
http://localhost:5000/api/Auth/login
```

### 1.3 JWT header

Các endpoint có `[Authorize]` yêu cầu:

```http
Authorization: Bearer {accessToken}
```

JWT chứa các claim:

| Claim | Ý nghĩa |
|---|---|
| `NameIdentifier` | ID người dùng |
| `Email` | Email |
| `Name` | Họ tên |
| `Role` | `USER` hoặc `ADMIN` |

Access token có thời hạn theo `JwtSettings:ExpiryMinutes`. Refresh token có thời hạn 7 ngày.

### 1.4 Response chuẩn

Hầu hết API trả về `ApiResponse<T>`:

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": {},
  "errors": null
}
```

Response lỗi:

```json
{
  "success": false,
  "message": "Nội dung lỗi",
  "data": null,
  "errors": [
    "Chi tiết lỗi nếu có"
  ]
}
```

Định nghĩa nằm tại `src/NutriVision.Api/Common/ApiResponse.cs`.

Lỗi không được xử lý sẽ đi qua `ExceptionHandlingMiddleware` và trả:

```http
500 Internal Server Error
```

```json
{
  "success": false,
  "message": "Đã xảy ra lỗi máy chủ nội bộ. Vui lòng thử lại sau.",
  "errors": [
    "Thông tin exception"
  ]
}
```

## 2. Authentication API

Nguồn: `Controllers/AuthController.cs`, `DTOs/Auth/AuthDtos.cs`.

### 2.1 Đăng ký

```http
POST /api/Auth/register
```

Quyền: không yêu cầu đăng nhập.

Request:

```json
{
  "hoTen": "Nguyễn Văn A",
  "email": "user@example.com",
  "matKhau": "User@123",
  "xacNhanMatKhau": "User@123"
}
```

| Trường | Kiểu | Bắt buộc | Ràng buộc |
|---|---|---:|---|
| `hoTen` | string | Có | Tối đa 100 ký tự |
| `email` | string | Có | Đúng định dạng email, duy nhất |
| `matKhau` | string | Có | Tối thiểu 8 ký tự |
| `xacNhanMatKhau` | string | Có về nghiệp vụ | Phải trùng mật khẩu |

Xử lý:

- Email được chuẩn hóa chữ thường.
- Mật khẩu hash bằng BCrypt.
- Tạo user với `VaiTro = USER`, `TrangThai = ACTIVE`.
- Tạo hồ sơ mặc định.
- Token xác thực email có thời hạn 24 giờ, tuy nhiên tài khoản đăng ký hiện được đánh dấu đã xác thực ngay để phục vụ demo.

Response:

```json
{
  "success": true,
  "message": "Đăng ký tài khoản thành công!",
  "data": "email-verification-token",
  "errors": null
}
```

Lỗi:

- `400 Bad Request`: email đã đăng ký.
- `400 Bad Request`: mật khẩu dưới 8 ký tự.
- `400 Bad Request`: body không hợp lệ.

### 2.2 Xác thực email

```http
GET /api/Auth/verify-email?token={token}
```

Quyền: không yêu cầu đăng nhập.

| Query | Kiểu | Bắt buộc |
|---|---|---:|
| `token` | string | Có |

Token hết hạn sau 24 giờ. Khi thành công, hệ thống đặt:

- `EmailDaXacThuc = true`
- `TrangThai = ACTIVE`
- Xóa token xác thực.

Response:

```json
{
  "success": true,
  "message": "Xác thực email thành công! Bạn có thể đăng nhập ngay bây giờ.",
  "data": true,
  "errors": null
}
```

### 2.3 Đăng nhập

```http
POST /api/Auth/login
```

Quyền: không yêu cầu đăng nhập.

Request:

```json
{
  "email": "user@nutrivision.ai",
  "matKhau": "User@123"
}
```

Xử lý:

- Kiểm tra mật khẩu bằng BCrypt.
- Sai 5 lần liên tiếp sẽ khóa tài khoản 15 phút.
- Ghi log đăng nhập và IP.
- Tạo access token và refresh token.

Response:

```json
{
  "success": true,
  "message": "Đăng nhập thành công!",
  "data": {
    "accessToken": "jwt-access-token",
    "refreshToken": "refresh-token",
    "expiration": "2026-11-04T19:25:08+00:00",
    "user": {
      "id": "22222222-2222-2222-2222-222222222222",
      "email": "user@nutrivision.ai",
      "hoTen": "Nguyễn Văn Dinh Dưỡng",
      "vaiTro": "USER",
      "trangThai": "ACTIVE",
      "emailDaXacThuc": true
    }
  },
  "errors": null
}
```

Lỗi:

- Email hoặc mật khẩu không chính xác.
- Tài khoản bị khóa tạm thời.
- Tài khoản chưa xác thực.
- Đã vượt quá số lần đăng nhập sai.

### 2.4 Làm mới token

```http
POST /api/Auth/refresh-token
```

Quyền: không yêu cầu access token hiện tại.

Request:

```json
{
  "refreshToken": "refresh-token"
}
```

Refresh token bị từ chối nếu không tồn tại, đã sử dụng, đã thu hồi hoặc hết hạn.

Response có cấu trúc giống đăng nhập:

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": {
    "accessToken": "new-access-token",
    "refreshToken": "new-refresh-token",
    "expiration": "2026-11-04T19:25:08+00:00",
    "user": {}
  },
  "errors": null
}
```

Lỗi HTTP: `401 Unauthorized`.

### 2.5 Đăng xuất

```http
POST /api/Auth/logout
```

Quyền: yêu cầu đăng nhập.

Request body: không có.

Xử lý: thu hồi toàn bộ refresh token chưa bị thu hồi của người dùng.

Response:

```json
{
  "success": true,
  "message": "Đăng xuất thành công!",
  "data": null,
  "errors": null
}
```

### 2.6 Quên mật khẩu

```http
POST /api/Auth/forgot-password
```

Quyền: không yêu cầu đăng nhập.

Request:

```json
{
  "email": "user@example.com"
}
```

Response luôn dùng thông báo chung để không làm lộ email có tồn tại:

```json
{
  "success": true,
  "message": "Nếu email hợp lệ, hướng dẫn đặt lại mật khẩu đã được gửi.",
  "data": "reset-token",
  "errors": null
}
```

Token reset có hạn 1 giờ.

> Trong code hiện tại, reset token được trả trực tiếp trong `data`. Đây phù hợp cho demo nhưng không nên dùng như vậy trong production.

### 2.7 Đặt lại mật khẩu

```http
POST /api/Auth/reset-password
```

Request:

```json
{
  "email": "user@example.com",
  "token": "reset-token",
  "matKhauMoi": "NewPassword@123"
}
```

Mật khẩu mới tối thiểu 8 ký tự. Khi thành công, toàn bộ refresh token cũ bị thu hồi.

Response:

```json
{
  "success": true,
  "message": "Đặt lại mật khẩu thành công!",
  "data": true,
  "errors": null
}
```

### 2.8 Đổi mật khẩu

```http
POST /api/Auth/change-password
```

Quyền: yêu cầu đăng nhập.

Request:

```json
{
  "matKhauCu": "User@123",
  "matKhauMoi": "NewPassword@123"
}
```

Quy tắc:

- Mật khẩu cũ phải đúng.
- Mật khẩu mới tối thiểu 8 ký tự.
- Mật khẩu mới không được trùng mật khẩu cũ.
- Thu hồi các phiên cũ.

## 3. Meal API

Nguồn: `Controllers/MealController.cs`, `DTOs/Meal/MealDtos.cs`.

Tất cả endpoint nhóm Meal yêu cầu đăng nhập.

### 3.1 Upload và phân tích ảnh

```http
POST /api/Meal/upload-scan
```

Content-Type:

```http
multipart/form-data
```

Form-data:

| Tên | Kiểu | Bắt buộc | Giá trị |
|---|---|---:|---|
| `file` | file | Có | JPG, JPEG, PNG, WEBP |
| `loaiBuaAn` | string | Không | `SANG`, `TRUA`, `TOI`, `PHU` |

Giới hạn file: `10MB`.

Quy trình:

1. Kiểm tra file.
2. Lưu file vào thư mục `uploads`.
3. Gọi `AiVisionService`.
4. Nếu có `AiSettings:ApiKey`, gọi Google Gemini Vision.
5. Gemini timeout 30 giây, retry tối đa 2 lần.
6. Nếu gọi AI thất bại, dùng Offline AI Simulator.
7. Tạo bữa ăn nháp với `DaXacNhan = false`.
8. Tra cứu thực phẩm tham chiếu và tính dinh dưỡng.
9. Lưu lịch sử nhận diện AI.

Response:

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": {
    "buaAnId": "guid",
    "tenMonAn": "Phở bò tái nạm",
    "duongDanHinhAnh": "/uploads/image.jpg",
    "doTinCay": 0.945,
    "canManualInput": false,
    "thongBao": "AI đã nhận diện thành công! Vui lòng kiểm tra và hiệu chỉnh nếu cần.",
    "thanhPhans": [
      {
        "id": "guid",
        "thucPhamThamChieuId": "guid",
        "tenThanhPhan": "Phở bò tái chín",
        "khauPhan": 1,
        "donViTinh": "Bát lớn",
        "khoiLuongQuyDoiGram": 350,
        "luongCalo": 332.5,
        "luongChatDam": 21.7,
        "luongTinhBot": 43.75,
        "luongChatBeo": 7.7,
        "luongChatXo": 1.75
      }
    ],
    "tongDinhDuongTamTinh": {
      "tongCalo": 485,
      "tongChatDam": 28,
      "tongTinhBot": 65,
      "tongChatBeo": 12,
      "tongChatXo": 3
    }
  },
  "errors": null
}
```

Lỗi:

- Không chọn file.
- File rỗng.
- File vượt 10MB.
- Định dạng không hỗ trợ.
- Lỗi filesystem hoặc database.

### 3.2 Hiệu chỉnh kết quả AI

```http
PUT /api/Meal/{id}/adjust
```

Request:

```json
{
  "tenMonAn": "Phở bò tái nạm",
  "thanhPhans": [
    {
      "id": "component-guid",
      "thucPhamThamChieuId": "food-guid",
      "tenThanhPhan": "Phở bò tái chín",
      "khauPhan": 1,
      "donViTinh": "Bát lớn",
      "khoiLuongQuyDoiGram": 350
    }
  ]
}
```

Ràng buộc:

- Tên món không rỗng.
- Có ít nhất một thành phần.
- `khauPhan` trong khoảng `0.01–10000`.
- `khoiLuongQuyDoiGram` trong khoảng `1–50000`.
- Thành phần phải có tên.

Xử lý:

- Chỉ chủ sở hữu mới được chỉnh sửa.
- Xóa các thành phần cũ và tạo lại thành phần mới.
- Tra cứu thực phẩm tham chiếu theo ID hoặc tên gần đúng.
- Tính lại tổng dinh dưỡng.
- Ghi nhận người dùng có sửa tên món AI hay không.

Response: `ApiResponse<MealDto>`.

### 3.3 Xác nhận bữa ăn

```http
POST /api/Meal/{id}/confirm
```

Request body: không có.

Xử lý:

- Kiểm tra tồn tại và quyền sở hữu.
- Đặt `DaXacNhan = true`.
- Cập nhật bảng tổng hợp dinh dưỡng ngày.

Response:

```json
{
  "success": true,
  "message": "Xác nhận và lưu bữa ăn thành công!",
  "data": {
    "id": "guid",
    "daXacNhan": true
  }
}
```

### 3.4 Hoàn tác về AI ban đầu

```http
POST /api/Meal/{id}/revert
```

Request body: không có.

Xử lý:

- Lấy tên món AI từ `LichSuNhanDienAI`.
- Khôi phục tên món AI.
- Đặt `NguoiDungCoSua = false`.
- Tính lại tổng dinh dưỡng.

> Code hiện tại khôi phục tên món và trạng thái chỉnh sửa. Nó chưa khôi phục đầy đủ danh sách thành phần ban đầu nếu thành phần đã bị thay đổi.

### 3.5 Hủy bữa ăn nháp

```http
DELETE /api/Meal/{id}/cancel
```

Xử lý:

- Kiểm tra quyền sở hữu.
- Xóa file ảnh nếu có.
- Xóa bản ghi bữa ăn vật lý.

Nếu không tìm thấy bữa ăn, service hiện trả về thành công `true`.

### 3.6 Lấy lịch sử bữa ăn

```http
GET /api/Meal/history
```

Query:

| Tên | Kiểu | Mặc định | Mô tả |
|---|---|---:|---|
| `fromDate` | DateOnly? | null | `yyyy-MM-dd` |
| `toDate` | DateOnly? | null | `yyyy-MM-dd` |
| `loaiBuaAn` | string? | null | `SANG`, `TRUA`, `TOI`, `PHU`, `ALL` |
| `page` | int | 1 | Trang |
| `pageSize` | int | 10 | Số bản ghi |

Ví dụ:

```http
GET /api/Meal/history?fromDate=2026-10-29&toDate=2026-11-04&loaiBuaAn=TRUA&page=1&pageSize=10
```

Chỉ trả bữa ăn:

- Thuộc người dùng hiện tại.
- Đã xác nhận.
- Chưa bị soft delete.

Sắp xếp theo ngày và thời gian giảm dần.

Response:

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": [
    {
      "id": "guid",
      "loaiBuaAn": "TRUA",
      "ngayAn": "2026-11-04",
      "thoiGianGhiNhan": "2026-11-04T12:15:00+00:00",
      "tenMonAn": "Cơm tấm sườn bì chả",
      "duongDanHinhAnh": null,
      "tongCalo": 1011,
      "tongChatDam": 60,
      "tongTinhBot": 100,
      "tongChatBeo": 37,
      "tongChatXo": 5,
      "taoTuAI": true,
      "daXacNhan": true,
      "thanhPhans": []
    }
  ]
}
```

> Response hiện chưa có `totalCount`, `totalPages`, `currentPage`.

### 3.7 Lấy chi tiết bữa ăn

```http
GET /api/Meal/{id}
```

Trả `ApiResponse<MealDto>` cùng danh sách thành phần.

Mã lỗi:

- `404 Not Found`: không tìm thấy bữa ăn.
- Lỗi nghiệp vụ nếu bữa ăn thuộc user khác.

### 3.8 Xóa bữa ăn

```http
DELETE /api/Meal/{id}
```

Đây là soft delete:

```text
DaXoa = true
```

Sau khi xóa, global query filter của Entity Framework tự động loại bản ghi khỏi các truy vấn thông thường.

### 3.9 Tạo bữa ăn thủ công

```http
POST /api/Meal/manual
```

Request:

```json
{
  "loaiBuaAn": "TOI",
  "tenMonAn": "Cơm gạo lứt ức gà",
  "ngayAn": "2026-11-04",
  "thanhPhans": [
    {
      "tenThanhPhan": "Cơm gạo lứt",
      "khauPhan": 1,
      "donViTinh": "Bát",
      "khoiLuongQuyDoiGram": 150
    },
    {
      "tenThanhPhan": "Ức gà không da luộc",
      "khauPhan": 1,
      "donViTinh": "Miếng",
      "khoiLuongQuyDoiGram": 120
    }
  ]
}
```

Quy tắc:

- `loaiBuaAn`: `SANG`, `TRUA`, `TOI`, `PHU`.
- Có tên món.
- Có ít nhất một thành phần.
- `ngayAn` bỏ trống thì dùng ngày hiện tại UTC.
- `TaoTuAI = false`.
- `DaXacNhan = true`.
- Tính và cập nhật tổng hợp dinh dưỡng ngay.

## 4. Dashboard API

Nguồn: `Controllers/DashboardController.cs`, `DTOs/Dashboard/DashboardDtos.cs`.

### 4.1 Lấy tổng quan Dashboard

```http
GET /api/Dashboard/summary
```

Response:

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": {
    "caloHomNay": 1496,
    "mucTieuCalo": 2200,
    "phanTramDatDuoc": 68,
    "soBuaAnHomNay": 2,
    "tongChatDam": 88,
    "tongTinhBot": 165,
    "tongChatBeo": 49,
    "tongChatXo": 16,
    "buaAnsHomNay": [],
    "xuHuong7Ngay": [
      {
        "ngay": "2026-10-29",
        "thu": "T5 (29/10)",
        "caloDaNap": 1800,
        "mucTieuCalo": 2200
      }
    ]
  }
}
```

Quy tắc:

- Chỉ tính bữa `DaXacNhan = true`.
- Chỉ tính ngày hiện tại theo UTC.
- Mục tiêu lấy từ hồ sơ.
- Nếu không có hồ sơ, mặc định `2000 kcal`.
- Xu hướng gồm 7 ngày gần nhất.
- Ngày chưa có dữ liệu trả `0`.

### 4.2 Báo cáo dinh dưỡng

```http
GET /api/Dashboard/report
```

Query:

| Tên | Kiểu | Mặc định |
|---|---|---|
| `fromDate` | DateOnly? | Hôm nay trừ 6 ngày |
| `toDate` | DateOnly? | Hôm nay |

Response:

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": {
    "tuNgay": "2026-10-29",
    "denNgay": "2026-11-04",
    "tongSoNgay": 7,
    "tongSoBuaAn": 12,
    "caloTrungBinhNgay": 2050,
    "chatDamTrungBinhNgay": 92,
    "tinhBotTrungBinhNgay": 240,
    "chatBeoTrungBinhNgay": 68,
    "chatXoTrungBinhNgay": 24,
    "tiLePhanBoDam": 24.5,
    "tiLePhanBoTinhBot": 48.2,
    "tiLePhanBoBeo": 27.3,
    "chiTietCacNgay": [],
    "nhanXetThongMinh": [
      "Mức tiêu thụ năng lượng của bạn rất cân đối.",
      "Lượng protein hàng ngày đạt chuẩn."
    ]
  }
}
```

Cách tính năng lượng:

- Đạm: `4 kcal/g`.
- Tinh bột: `4 kcal/g`.
- Chất béo: `9 kcal/g`.

Chỉ tính bữa đã xác nhận.

### 4.3 Xuất CSV

```http
GET /api/Dashboard/export-csv
```

Query:

| Tên | Kiểu | Mặc định |
|---|---|---|
| `fromDate` | DateOnly? | 30 ngày trước |
| `toDate` | DateOnly? | Hôm nay |

Response là file:

```http
Content-Type: text/csv; charset=utf-8
Content-Disposition: attachment; filename=NutriVision_BaoCao_yyyyMMdd_yyyyMMdd.csv
```

Header CSV:

```csv
NgayAn,LoaiBuaAn,TenMonAn,TongCalo_kcal,ChatDam_g,TinhBot_g,ChatBeo_g,ChatXo_g,TaoTuAI
```

Ví dụ:

```csv
2026-11-04,TRUA,"Cơm tấm sườn bì chả",1011,60,100,37,5,AI
```

## 5. Profile API

Nguồn: `Controllers/ProfileController.cs`, `DTOs/Profile/ProfileDtos.cs`.

### 5.1 Lấy hồ sơ

```http
GET /api/Profile
```

Response:

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": {
    "nguoiDungId": "guid",
    "hoTen": "Nguyễn Văn Dinh Dưỡng",
    "email": "user@nutrivision.ai",
    "ngaySinh": "2000-01-01",
    "tuoi": 26,
    "gioiTinh": "NAM",
    "chieuCao": 172,
    "canNang": 68,
    "mucDoVanDong": "VAN_DONG_VUA",
    "chiSoBMI": 23,
    "phanLoaiBMI": "Bình thường (Thể trạng cân đối)",
    "chiSoBMR": 1630,
    "chiSoTDEE": 2542,
    "mucTieuCalo": 2200,
    "duongDanAnhDaiDien": null
  }
}
```

### 5.2 Cập nhật hồ sơ

```http
PUT /api/Profile
```

Request:

```json
{
  "hoTen": "Nguyễn Văn Dinh Dưỡng",
  "ngaySinh": "2000-01-01",
  "gioiTinh": "NAM",
  "chieuCao": 172,
  "canNang": 68,
  "mucDoVanDong": "VAN_DONG_VUA"
}
```

Ràng buộc:

- Họ tên bắt buộc.
- Ngày sinh bắt buộc.
- Chiều cao: `50–200 cm`.
- Cân nặng: `10–150 kg`.
- Tuổi: `10–100`.
- Giới tính: `NAM`, `NU`.

Tính BMI:

```text
BMI = cân nặng / chiều cao(m)^2
```

BMR nam:

```text
10 × cân nặng + 6.25 × chiều cao − 5 × tuổi + 5
```

BMR nữ:

```text
10 × cân nặng + 6.25 × chiều cao − 5 × tuổi − 161
```

Hệ số TDEE:

| Mức vận động | Hệ số |
|---|---:|
| `IT_VAN_DONG` | 1.2 |
| `VAN_DONG_NHE` | 1.375 |
| `VAN_DONG_VUA` | 1.55 |
| `VAN_DONG_NHIEU` | 1.725 |
| `VAN_DONG_RAT_NHIEU` | 1.9 |

Mục tiêu calo mặc định bằng TDEE.

### 5.3 Upload ảnh đại diện

```http
POST /api/Profile/avatar
```

Content-Type:

```http
multipart/form-data
```

Form-data:

| Tên | Kiểu | Bắt buộc |
|---|---|---:|
| `file` | file | Có |

Định dạng: JPG, JPEG, PNG, WEBP.

Giới hạn: `5MB`.

Response:

```json
{
  "success": true,
  "message": "Cập nhật ảnh đại diện thành công!",
  "data": "/uploads/avatars/avatar_guid_random.webp",
  "errors": null
}
```

## 6. Admin API

Nguồn: `Controllers/AdminController.cs`, `DTOs/Admin/AdminDtos.cs`.

Tất cả endpoint nhóm Admin yêu cầu:

```http
Authorization: Bearer {admin-token}
```

và role:

```text
ADMIN
```

User thường nhận `403 Forbidden`.

### 6.1 Danh sách người dùng

```http
GET /api/Admin/users
```

Query:

| Tên | Kiểu | Mặc định |
|---|---|---:|
| `search` | string? | null |
| `status` | string? | null |
| `page` | int | 1 |
| `pageSize` | int | 10 |

Tìm kiếm theo email hoặc họ tên. Lọc trạng thái có thể dùng `ACTIVE`, `LOCKED`, `ALL`.

Response:

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": [
    {
      "id": "guid",
      "email": "user@nutrivision.ai",
      "hoTen": "Nguyễn Văn Dinh Dưỡng",
      "vaiTro": "USER",
      "trangThai": "ACTIVE",
      "emailDaXacThuc": true,
      "soBuaAnDaGhi": 18,
      "ngayTao": "2026-11-04T10:00:00+00:00"
    }
  ]
}
```

### 6.2 Khóa hoặc mở khóa người dùng

```http
PUT /api/Admin/users/{id}/toggle-lock
```

Request body: không có.

Xử lý:

- `ACTIVE` chuyển thành `LOCKED`.
- `LOCKED` chuyển thành `ACTIVE`.
- Khi khóa, thời gian khóa được đặt 1 năm.
- Admin không thể tự khóa chính mình.
- Ghi log hệ thống.

Response:

```json
{
  "success": true,
  "message": "Đã khóa tài khoản thành công!",
  "data": true
}
```

### 6.3 Thay đổi vai trò

```http
PUT /api/Admin/users/{id}/change-role
```

Request:

```json
{
  "role": "ADMIN"
}
```

Giá trị hợp lệ: `USER`, `ADMIN`.

Admin không thể tự đổi role của mình. Thao tác được ghi log.

### 6.4 Lấy system logs

```http
GET /api/Admin/logs
```

Query:

| Tên | Kiểu | Mặc định |
|---|---|---:|
| `actionType` | string? | null |
| `page` | int | 1 |
| `pageSize` | int | 20 |

Response:

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": [
    {
      "id": "guid",
      "nguoiThucHienEmail": "admin@nutrivision.ai",
      "loaiHanhDong": "DOI_ROLE",
      "bangTacDong": "NguoiDung",
      "idDoiTuong": "target-user-guid",
      "chiTiet": "Thay đổi vai trò...",
      "diaChiIP": "127.0.0.1",
      "ngayTao": "2026-11-04T10:00:00+00:00"
    }
  ]
}
```

### 6.5 Thống kê quản trị

```http
GET /api/Admin/statistics
```

Response:

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": {
    "tongNguoiDung": 2,
    "tongNguoiDungActive": 2,
    "tongBuaAnDaGhi": 0,
    "tongLuotGoiAI": 0,
    "tiLeAIChinhXac": 95,
    "thoiGianPhanHoiTrungBinhMs": 850
  }
}
```

Cách tính:

- `TongNguoiDung`: đếm bảng `NguoiDung`.
- `TongNguoiDungActive`: `TrangThai = ACTIVE`.
- `TongBuaAnDaGhi`: bữa đã xác nhận và chưa xóa.
- `TongLuotGoiAI`: đếm `LichSuNhanDienAI`.
- `TiLeAIChinhXac`: số lượt không sửa tên / tổng lượt AI.
- Khi chưa có lượt AI, độ chính xác mặc định `95%`.
- Khi chưa có thời gian AI, thời gian mặc định `850ms`.

## 7. Endpoint mẫu

```http
GET /WeatherForecast
```

Đây là endpoint mẫu mặc định của ASP.NET Core, không thuộc nghiệp vụ NutriVisionAI.

Response:

```json
[
  {
    "date": "2026-11-05",
    "temperatureC": 27,
    "temperatureF": 80,
    "summary": "Warm"
  }
]
```

## 8. DTO chính

### 8.1 `MealDto`

```json
{
  "id": "guid",
  "loaiBuaAn": "SANG",
  "ngayAn": "2026-11-04",
  "thoiGianGhiNhan": "2026-11-04T07:30:00+00:00",
  "tenMonAn": "Phở bò",
  "duongDanHinhAnh": "/uploads/image.jpg",
  "tongCalo": 485,
  "tongChatDam": 28,
  "tongTinhBot": 65,
  "tongChatBeo": 12,
  "tongChatXo": 3,
  "taoTuAI": true,
  "daXacNhan": true,
  "thanhPhans": []
}
```

### 8.2 `MealComponentDto`

```json
{
  "id": "guid",
  "thucPhamThamChieuId": "guid",
  "tenThanhPhan": "Ức gà không da luộc",
  "khauPhan": 1,
  "donViTinh": "Miếng",
  "khoiLuongQuyDoiGram": 120,
  "luongCalo": 198,
  "luongChatDam": 37.2,
  "luongTinhBot": 0,
  "luongChatBeo": 4.32,
  "luongChatXo": 0
}
```

### 8.3 `ProfileDto`

Gồm ID user, họ tên, email, ngày sinh, tuổi, giới tính, chiều cao, cân nặng, mức vận động, BMI, phân loại BMI, BMR, TDEE, mục tiêu calo và đường dẫn avatar.

### 8.4 `AdminStatsDto`

Gồm tổng user, user active, tổng bữa, tổng lượt AI, tỷ lệ AI chính xác và thời gian AI trung bình.

## 9. Tính dinh dưỡng

Thực phẩm tham chiếu được lưu trong bảng `ThucPhamThamChieu`. Hệ thống seed khoảng 30 thực phẩm phổ biến như cơm, phở, bún, bánh mì, thịt, cá, trứng, rau, trái cây, sữa và hạt.

Nếu tìm thấy thực phẩm:

```text
Giá trị thực tế = Giá trị trên 100g × khối lượng / 100
```

Nếu không tìm thấy:

```text
Calo: 150 kcal / 100g
Đạm: 5g / 100g
Tinh bột: 20g / 100g
Chất béo: 4g / 100g
Chất xơ: 1g / 100g
```

## 10. Cấu trúc dữ liệu chính

Các bảng EF Core:

- `NguoiDung`
- `HoSoNguoiDung`
- `TokenLamMoi`
- `ThucPhamThamChieu`
- `BuaAn`
- `ThanhPhanBuaAn`
- `TongHopDinhDuongNgay`
- `LichSuNhanDienAI`
- `NhatKyHeThong`

Quan hệ chính:

- Một `NguoiDung` có một `HoSoNguoiDung`.
- Một `NguoiDung` có nhiều `BuaAn`.
- Một `BuaAn` có nhiều `ThanhPhanBuaAn`.
- Một thành phần có thể liên kết `ThucPhamThamChieu`.
- Một bữa ăn có lịch sử nhận diện AI.
- User có nhiều refresh token.
- Admin thao tác tạo system log.

`BuaAn` có global query filter:

```text
!DaXoa
```

Vì vậy bữa ăn soft delete không xuất hiện trong truy vấn thông thường.

## 11. Dữ liệu demo

Tài khoản seed trong `Data/SeedData.cs`:

| Vai trò | Email | Mật khẩu |
|---|---|---|
| ADMIN | `admin@nutrivision.ai` | `Admin@123` |
| USER | `user@nutrivision.ai` | `User@123` |

User demo:

- Họ tên: Nguyễn Văn Dinh Dưỡng
- Ngày sinh: `2000-01-01`
- Chiều cao: `172cm`
- Cân nặng: `68kg`
- Giới tính: `NAM`
- Mức vận động: `VAN_DONG_VUA`
- Mục tiêu calo: `2200`

## 12. Sai lệch giữa frontend và backend hiện tại

Frontend API client nằm tại `src/nutrivision-web/lib/api.ts` đang gọi một số route khác với backend.

| Frontend đang gọi | Backend thực tế | Trạng thái |
|---|---|---|
| `GET /auth/me` | Không có endpoint tương ứng | Sai |
| `POST /meal/scan` | `POST /meal/upload-scan` | Sai |
| `POST /meal` | `POST /meal/manual` | Sai |
| `GET /meal` | `GET /meal/history` | Sai |
| `PUT /meal/{id}` | `PUT /meal/{id}/adjust` | Sai |
| `POST /meal/{id}/undo` | `POST /meal/{id}/revert` | Sai |
| `GET /dashboard/today` | `GET /dashboard/summary` | Sai |
| `GET /dashboard/weekly` | Không có endpoint riêng | Sai |
| `GET /admin/stats` | `GET /admin/statistics` | Sai |
| `PUT /admin/users/{id}/lock` | `PUT /admin/users/{id}/toggle-lock` | Sai |
| `PUT /admin/users/{id}/role` | `PUT /admin/users/{id}/change-role` | Sai |

DTO frontend cũng chưa khớp hoàn toàn:

Frontend sử dụng các trường như:

```typescript
{
  tongProtein,
  tongCarbs,
  tongFat,
  urlAnh,
  nguonNhap
}
```

Backend trả:

```json
{
  "tongChatDam": 0,
  "tongTinhBot": 0,
  "tongChatBeo": 0,
  "duongDanHinhAnh": null,
  "taoTuAI": true,
  "daXacNhan": true
}
```

Frontend cần được cập nhật route và mapping DTO trước khi tích hợp thật.

## 13. Tổng hợp endpoint

| Nhóm | Số lượng |
|---|---:|
| Authentication | 8 |
| Meal | 9 |
| Dashboard | 3 |
| Profile | 3 |
| Admin | 5 |
| Weather mẫu | 1 |
| **Tổng cộng** | **29** |

### Danh sách route đầy đủ

```text
POST   /api/Auth/register
GET    /api/Auth/verify-email
POST   /api/Auth/login
POST   /api/Auth/refresh-token
POST   /api/Auth/logout
POST   /api/Auth/forgot-password
POST   /api/Auth/reset-password
POST   /api/Auth/change-password

POST   /api/Meal/upload-scan
PUT    /api/Meal/{id}/adjust
POST   /api/Meal/{id}/confirm
POST    /api/Meal/{id}/revert
DELETE /api/Meal/{id}/cancel
GET    /api/Meal/history
GET    /api/Meal/{id}
DELETE /api/Meal/{id}
POST   /api/Meal/manual

GET    /api/Dashboard/summary
GET    /api/Dashboard/report
GET    /api/Dashboard/export-csv

GET    /api/Profile
PUT    /api/Profile
POST   /api/Profile/avatar

GET    /api/Admin/users
PUT    /api/Admin/users/{id}/toggle-lock
PUT    /api/Admin/users/{id}/change-role
GET    /api/Admin/logs
GET    /api/Admin/statistics

GET    /WeatherForecast
```

## 14. Tài liệu nguồn

- `src/NutriVision.Api/Controllers/AuthController.cs`
- `src/NutriVision.Api/Controllers/MealController.cs`
- `src/NutriVision.Api/Controllers/DashboardController.cs`
- `src/NutriVision.Api/Controllers/ProfileController.cs`
- `src/NutriVision.Api/Controllers/AdminController.cs`
- `src/NutriVision.Api/DTOs/Auth/AuthDtos.cs`
- `src/NutriVision.Api/DTOs/Meal/MealDtos.cs`
- `src/NutriVision.Api/DTOs/Dashboard/DashboardDtos.cs`
- `src/NutriVision.Api/DTOs/Profile/ProfileDtos.cs`
- `src/NutriVision.Api/DTOs/Admin/AdminDtos.cs`
- `src/NutriVision.Api/Services/AuthService.cs`
- `src/NutriVision.Api/Services/MealService.cs`
- `src/NutriVision.Api/Services/DashboardService.cs`
- `src/NutriVision.Api/Services/ProfileService.cs`
- `src/NutriVision.Api/Services/AdminService.cs`
- `src/NutriVision.Api/Services/AiVisionService.cs`
- `src/NutriVision.Api/Data/ApplicationDbContext.cs`
- `src/NutriVision.Api/Data/SeedData.cs`
- `src/nutrivision-web/lib/api.ts`
