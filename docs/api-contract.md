# NutriVisionAI – API Contract (v1)

Base URL: `http://localhost:5000` (frontend dùng biến `NEXT_PUBLIC_API_URL`, mặc định `http://localhost:5000`). Mọi route có prefix `/api`.
JSON **camelCase**. Ngày `yyyy-MM-dd` (kiểu `date`), thời điểm ISO-8601 (UTC, có offset). Số thập phân là `number`.
Header bảo vệ: `Authorization: Bearer <accessToken>`.

## 1. Envelope chung
```jsonc
{ "success": true,  "message": "…", "code": null, "data": <T|null>, "errors": null }
{ "success": false, "message": "Thông báo tiếng Việt cho người dùng", "code": "MA_LOI", "data": null, "errors": ["chi tiết 1", …] }
```
Lỗi validation: HTTP 400, `code:"VALIDATION_ERROR"`, `errors` = mảng chuỗi (mỗi chuỗi dạng `"Tên trường: thông báo"`), và `data` = object `{ "fieldErrors": { "tenTruong": ["msg", …] } }` (tên trường camelCase, lồng nhau dùng dấu chấm, vd `thanhPhan[0].khauPhan`).
Mã HTTP: 400 validation/nghiệp vụ · 401 chưa đăng nhập/token hết hạn · 403 không đủ quyền / sở hữu · 404 không tồn tại · 409 xung đột · 413 file quá lớn · 429 quá nhiều request · 500.
Danh sách `code`: `VALIDATION_ERROR, INVALID_CREDENTIALS, EMAIL_NOT_VERIFIED, ACCOUNT_TEMP_LOCKED (data.moKhoaLuc), ACCOUNT_DISABLED, EMAIL_EXISTS, TOKEN_INVALID, TOKEN_EXPIRED, TOKEN_USED, WRONG_PASSWORD, SAME_PASSWORD, UNAUTHORIZED, FORBIDDEN, NOT_FOUND, CONFLICT, SELF_ACTION_FORBIDDEN, INVALID_IMAGE, FILE_TOO_LARGE, NUTRITION_DATA_MISSING (data.thanhPhanThieu = [index…]), RATE_LIMITED, SERVER_ERROR`.
Phân trang: `data = { items: T[], page, pageSize, totalCount, totalPages, … }`.

## 2. Auth  (`/api/auth`)
| Method | Route | Auth | Body → data |
|---|---|---|---|
| POST | `/register` | – | `{hoTen,email,matKhau,xacNhanMatKhau,dongYDieuKhoan:true}` → `{email, emailDaGui:boolean}` (tài khoản `PENDING`, gửi email xác thực; email trùng → 409 `EMAIL_EXISTS`) |
| POST | `/verify-email` | – | `{token}` → `{daXacThucTruocDo:boolean}`; lỗi `TOKEN_INVALID/TOKEN_EXPIRED/TOKEN_USED` |
| POST | `/resend-verification` | – | `{email}` → luôn 200 thông báo chung |
| POST | `/login` | – | `{email,matKhau}` → `AuthResult` |
| POST | `/refresh` | – | `{refreshToken}` → `AuthResult` (refresh token xoay vòng, token cũ bị vô hiệu) |
| POST | `/logout` | Bearer | `{refreshToken}` → null (thu hồi token đó) |
| POST | `/forgot-password` | – | `{email}` → luôn 200 thông báo chung (không lộ email) |
| POST | `/reset-password` | – | `{token,matKhauMoi,xacNhanMatKhau}` → null; thu hồi mọi refresh token |
| POST | `/change-password` | Bearer | `{matKhauHienTai,matKhauMoi,xacNhanMatKhau}` → `AuthResult` mới cho thiết bị hiện tại (mọi phiên khác bị thu hồi) |
| GET | `/me` | Bearer | `UserInfo` + `ngayTao` |

`AuthResult = { accessToken, refreshToken, accessTokenExpiresAt, user: UserInfo }`
`UserInfo = { id, email, hoTen, vaiTro: "USER"|"ADMIN", trangThai: "PENDING"|"ACTIVE"|"LOCKED", emailDaXacThuc, anhDaiDienUrl: string|null }`
Mật khẩu: ≥8 ký tự, có chữ và số. Access token sống 30 phút; refresh 7 ngày. Email xác thực / reset chứa link tới **frontend**: `{FrontendUrl}/verify-email?token=…` và `{FrontendUrl}/reset-password?token=…`.

## 3. Profile (`/api/profile`, Bearer)
- `GET /api/profile` → `Profile`
- `PUT /api/profile` body `{hoTen, ngaySinh, gioiTinh:"Nam"|"Nu", chieuCao(50-200), canNang(10-150), mucDoVanDong, mucTieuCalo?: number|null}` (tuổi 10–100; `mucTieuCalo` null ⇒ = TDEE; nếu có: 800–6000) → `Profile`
- `POST /api/profile/avatar` multipart field `file` (jpg/png/webp ≤5MB) → `{ anhDaiDienUrl }`

`mucDoVanDong ∈ ItVanDong(1.2) | VanDongNhe(1.375) | VuaPhai(1.55) | NangDong(1.725) | RatNangDong(1.9)`.
```ts
Profile = { hoTen, email, anhDaiDienUrl|null, daDayDu:boolean,        // false nếu chưa nhập hồ sơ (các trường dưới có thể null)
  ngaySinh|null, tuoi|null, gioiTinh|null, chieuCao|null, canNang|null, mucDoVanDong|null,
  chiSoBMI|null, phanLoaiBMI|null /* "Gầy"|"Bình thường"|"Thừa cân"|"Béo phì" */, chiSoBMR|null, chiSoTDEE|null,
  mucTieuCalo: number /* mặc định 2000 khi chưa có hồ sơ */, ngayCapNhat|null }
```
Công thức Mifflin–St Jeor (server tính; client chỉ hiển thị).

## 4. Thực phẩm tham chiếu (`/api/foods`, Bearer)
- `GET /api/foods?q=&nhom=&take=20` (tìm không phân biệt dấu) → `Food[]`
- `GET /api/foods/match?name=` → `[{ food: Food, diem: number(0..1) }]` (tối đa 5 gợi ý gần đúng)
- `GET /api/foods/units` → `[{ donVi: string, gram: number }]` (bảng quy đổi mặc định: g=1, ml=1, kg=1000, bát, chén, đĩa, tô, miếng, quả, cái, cuốn, lát, muỗng, ly, phần, ổ …)
- `GET /api/foods/groups` → `string[]`
```ts
Food = { id, maThucPham, tenThucPham, nhomThucPham, donViChuan, khoiLuongChuan, caloTren100g, chatDamTren100g, tinhBotTren100g, chatBeoTren100g, chatXoTren100g }
Nutrition = { calo, chatDam, tinhBot, chatBeo, chatXo }   // kcal, g, g, g, g
```
**Quy tắc quy đổi gram (client phải dùng đúng để tính nhanh, server là nguồn chính thức):**
`gram = khauPhan × gramPerUnit` với `gramPerUnit`: `g/gram`=1, `ml`=1, `kg`/`lít`=1000; nếu `donViTinh` (bỏ dấu, lower) trùng `food.donViChuan` ⇒ `food.khoiLuongChuan`; ngược lại tra bảng `/foods/units`. Dinh dưỡng = `tren100g × gram / 100`. Làm tròn 2 chữ số.

## 5. Bữa ăn (`/api/meals`, Bearer, role USER hoặc ADMIN nhưng chỉ chủ sở hữu mới thao tác)
```ts
LoaiBuaAn = "Sang" | "Trua" | "Toi" | "Phu"
MealComponent = { id|null, thucPhamThamChieuId|null, tenThanhPhan, khauPhan, donViTinh, khoiLuongQuyDoiGram,
  luongCalo, luongChatDam, luongTinhBot, luongChatBeo, luongChatXo,
  thamChieu: Food|null, dinhDuongTren100g: Nutrition|null, thieuDuLieuDinhDuong: boolean }
ComponentInput = { tenThanhPhan, thucPhamThamChieuId?: guid|null, khauPhan:>0, donViTinh,
  nhapTay?: { caloTren100g, chatDamTren100g, tinhBotTren100g, chatBeoTren100g, chatXoTren100g } | null }
MealUpsert = { loaiBuaAn, ngayAn?: date (mặc định hôm nay, không được ở tương lai), tenMonAn (1-200), thanhPhan: ComponentInput[] (≥1) }
AiOriginal = { tenMonAn, doTinCay: number|null, thanhPhan: [{ tenThanhPhan, khauPhan, donViTinh, khoiLuongGram, thucPhamThamChieuId|null }] }
Meal = { id, loaiBuaAn, ngayAn, thoiGianGhiNhan, tenMonAn, hinhAnhUrl|null, taoTuAI, daXacNhan,
  thanhPhan: MealComponent[], tongDinhDuong: Nutrition, mucTieuCalo, phanTramNhuCauNgay /* calo bữa / mục tiêu *100 */,
  doTinCay|null, aiGoc: AiOriginal|null, nguoiDungCoSua: boolean|null,
  aiThatBai: boolean, thongBaoAI: string|null }     // 2 trường cuối chỉ có ý nghĩa ngay sau /scan
MealSummary = { id, loaiBuaAn, ngayAn, thoiGianGhiNhan, tenMonAn, hinhAnhUrl|null, taoTuAI, tongDinhDuong: Nutrition, soThanhPhan }
```
| Method | Route | Mô tả |
|---|---|---|
| POST | `/scan` | multipart: `image` (jpg/png/webp ≤10MB, ≥224×224), `loaiBuaAn`, `ngayAn?`. Lưu ảnh, gọi AI (timeout 30s, retry 2), tạo **bản nháp** `daXacNhan=false`. Trả `Meal` (HTTP 200). Nếu AI lỗi/rỗng/chưa cấu hình: `aiThatBai=true`, `thanhPhan=[]`, `tenMonAn=""`, `thongBaoAI` giải thích ⇒ UI chuyển sang nhập tay **nhưng vẫn dùng bản nháp + ảnh này** (gọi `/{id}/confirm`). Lỗi ảnh: 400 `INVALID_IMAGE`/413 `FILE_TOO_LARGE`. |
| POST | `/` | Tạo bữa **nhập tay** (không ảnh), body `MealUpsert` → `Meal` đã xác nhận (201) |
| GET | `/{id}` | Chi tiết (bản nháp hoặc đã xác nhận). Của người khác → 403 `FORBIDDEN` |
| POST | `/{id}/confirm` | Body `MealUpsert` (trạng thái cuối cùng sau hiệu chỉnh) → `Meal` đã xác nhận, tính dinh dưỡng, cập nhật tổng hợp ngày. Thành phần không có dữ liệu dinh dưỡng (không có `thucPhamThamChieuId` và không có `nhapTay`) → 400 `NUTRITION_DATA_MISSING`. Đã xác nhận rồi → 409 |
| PUT | `/{id}` | Sửa bữa đã xác nhận, body `MealUpsert` → `Meal` (tính lại, cập nhật tổng hợp ngày cũ/mới) |
| POST | `/{id}/revert` | Khôi phục thành phần + tên món về kết quả AI gốc (chỉ bữa có `aiGoc`) → `Meal` |
| DELETE | `/{id}/cancel` | Hủy bản nháp: xóa bản nháp và ảnh đã upload (chỉ khi `daXacNhan=false`) |
| DELETE | `/{id}` | Xóa mềm bữa đã xác nhận |
| GET | `/?from=&to=&loaiBuaAn=&page=1&pageSize=20` | Lịch sử: chỉ bữa đã xác nhận, chưa xóa, mới nhất trước; mặc định 7 ngày gần nhất. `data = {items: MealSummary[], page, pageSize, totalCount, totalPages, tongHopNgay: [{ngay, tongCalo, mucTieuCalo, phanTram, soBuaAn}]}` |

Bản nháp `ngayAn` mặc định hôm nay; thành phần của bản nháp do AI đề xuất đã được server tự khớp với thực phẩm tham chiếu (nếu khớp: có `thamChieu`; nếu không: `thieuDuLieuDinhDuong=true`, dinh dưỡng = 0; UI cho người dùng chọn gần đúng (`/foods/match`) / nhập tay / xóa).

## 6. Gợi ý món (`GET /api/suggestions`, Bearer)
→ `[{ food: Food, lyDo: string }]` (tối đa 6; dựa trên lịch sử 30 ngày + mục tiêu calo; chỉ lấy từ bảng tham chiếu).

## 7. Dashboard (`GET /api/dashboard`, Bearer)
```ts
{ ngay, mucTieuCalo, hoSoDayDu:boolean, coDuLieu:boolean /* đã từng có bữa xác nhận */,
  homNay: { calo, chatDam, tinhBot, chatBeo, chatXo, soBuaAn, phanTramMucTieu, trangThai: "Thieu"|"DatChuan"|"Vuot" },
  mucTieuMacro: { chatDam, tinhBot, chatBeo },           // gram/ngày (20/50/30 % năng lượng)
  tyLeMacro: { chatDam, tinhBot, chatBeo },              // % năng lượng thực tế hôm nay (0 nếu chưa có)
  buaAnHomNay: MealSummary[],
  theoLoaiBuaAn: [{ loaiBuaAn, calo, soBua }],           // luôn đủ 4 loại
  bayNgay: [{ ngay, calo, mucTieuCalo, soBuaAn }]        // 7 ngày kết thúc hôm nay, đủ 7 phần tử
}
```

## 8. Báo cáo (`/api/reports`, Bearer)
`GET /api/reports?from=&to=` (mặc định 7 ngày gần nhất; tối đa 366 ngày):
```ts
{ from, to, soNgay, soNgayCoDuLieu, tongSoBuaAn, mucTieuCaloTrungBinh,
  trungBinhNgay: Nutrition /* trung bình trên các ngày có dữ liệu */, tong: Nutrition, phanTramMucTieu,
  danhGia: { calo, chatDam, tinhBot, chatBeo, chatXo: "Thap"|"DatChuan"|"Cao" },
  tyLeNangLuong: { chatDam, tinhBot, chatBeo },         // % kcal
  xuHuong: "Tang"|"Giam"|"OnDinh"|"KhongDuDuLieu",
  soNgayVuotMucTieu, soNgayThieuNangLuong,
  theoNgay: [{ ngay, calo, chatDam, tinhBot, chatBeo, chatXo, soBuaAn, mucTieuCalo }],   // đủ mọi ngày trong khoảng
  khuyenNghi: [{ muc: "info"|"warning"|"success", noiDung }] }
```
`GET /api/reports/export?from=&to=&format=pdf|csv` → file (Content-Disposition attachment). Frontend tải bằng `fetch` + Bearer rồi tạo blob. Chỉ chứa dữ liệu của chính người dùng.

## 9. Admin (`/api/admin`, role ADMIN, user thường → 403)
- `GET /users?q=&trangThai=&vaiTro=&page=1&pageSize=20` → paged `{id,email,hoTen,vaiTro,trangThai,emailDaXacThuc,ngayTao}` (không có dữ liệu cá nhân/bữa ăn/mật khẩu)
- `PUT /users/{id}/status` `{trangThai:"ACTIVE"|"LOCKED", lyDo?}` → user item (tự khóa mình → 400 `SELF_ACTION_FORBIDDEN`)
- `PUT /users/{id}/role` `{vaiTro:"USER"|"ADMIN"}` → user item (tự hạ quyền → 400 `SELF_ACTION_FORBIDDEN`)
- `GET /logs?userId=&loai=&from=&to=&page=1&pageSize=30` → paged `{id,nguoiDungThucHienId,emailNguoiThucHien,loaiHanhDong,bangTacDong,idDoiTuong,chiTiet,diaChiIP,ngayTao}`
- `GET /logs/types` → `string[]`
- `GET /statistics?from=&to=` (mặc định 30 ngày) → `{ tongNguoiDung, nguoiDungHoatDong, nguoiDungBiKhoa, tongBuaAn, soAnhAIXuLy, soLanAIThatBai, tyLeAIChinhXacPhanTram|null, thoiGianXuLyAITrungBinhMs|null, nguoiDungMoiTheoNgay:[{ngay,soLuong}], buaAnTheoNgay:[{ngay,soLuong}], aiTheoNgay:[{ngay,soLuong}] }`

## 10. Khác
- `GET /health` → `{status:"Healthy"}` (không envelope).
- Ảnh: `hinhAnhUrl` / `anhDaiDienUrl` là URL **tương đối đã ký, có hạn** dạng `/api/files/meals/…?exp=…&sig=…` → frontend ghép `API_URL + url` và dùng trực tiếp trong `<img src>` (không cần Bearer). Không cache lâu hơn 30 phút.
- CORS cho `http://localhost:3000`.

## 11. Tài khoản seed (dev)
`admin@nutrivision.ai / Admin@123` (ADMIN) · `user@nutrivision.ai / User@123` (USER, có hồ sơ) – đã xác thực email.
