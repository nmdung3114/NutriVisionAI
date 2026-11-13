# Ghi chú từ frontend (nutrivision-web)

Các diễn giải/yêu cầu phía backend mà frontend đang giả định:

1. **CORS expose header**: tải PDF/CSV lấy tên file từ `Content-Disposition` nên cần `Access-Control-Expose-Headers: Content-Disposition` (nếu không, FE dùng tên dự phòng `bao-cao-dinh-duong.pdf|csv`).
2. **Lỗi AI sau `/scan`**: `aiThatBai`/`thongBaoAI` chỉ có ở response của `/scan`; FE lưu `thongBaoAI` vào `sessionStorage` để hiển thị ở trang review. Nếu reload mà không còn, FE suy ra "AI thất bại" từ bản nháp không có thành phần và không có `aiGoc`.
3. **`doTinCay`**: FE chấp nhận cả thang 0..1 lẫn 0..100 (≤1 thì nhân 100).
4. **Hoàn tác AI gốc**: bản nháp -> khôi phục cục bộ từ bản nháp ban đầu (server chưa bị sửa tới khi confirm); bữa đã xác nhận có `aiGoc` -> `POST /{id}/revert`.
5. **Thành phần nhập tay khi sửa bữa đã lưu**: thành phần không có `thucPhamThamChieuId` nhưng có `dinhDuongTren100g` được gửi lại dưới dạng `nhapTay` (để PUT không bị `NUTRITION_DATA_MISSING`).
6. **`PROFILE_REQUIRED`** (bổ sung theo yêu cầu): `POST /api/profile/avatar` trả 400 khi chưa lưu hồ sơ -> FE hiển thị "Hãy lưu hồ sơ trước khi đổi ảnh đại diện." (và nhắc trước nếu `daDayDu=false`).
7. **Mục tiêu calo tùy chỉnh vs TDEE**: FE coi là "tùy chỉnh" khi `mucTieuCalo != chiSoTDEE`; chọn "Dùng TDEE" gửi `mucTieuCalo: null`.
8. **Admin khóa/mở khóa**: nút bị vô hiệu với tài khoản `PENDING` (contract chỉ cho `ACTIVE|LOCKED`).
9. **Xơ (chất xơ)**: contract không có mục tiêu xơ; FE dùng 25 g/ngày để hiển thị % (chỉ mang tính tham khảo). Mục tiêu macro cục bộ theo 20/50/30% năng lượng giống dashboard.
10. **Mục tiêu "% nhu cầu ngày" ở trang manual**: lấy `mucTieuCalo` từ `/api/profile`.
11. Ảnh preview dùng `hinhAnhUrl` ghép `API_URL` (URL ký, có hạn) nên khi mở lại trang sau >30 phút cần tải lại dữ liệu.
