Mô tả luồng thao tác chính của người dùng – NutriVision AI

1. Đăng nhập / Đăng ký: Người dùng đăng nhập hoặc đăng ký tài khoản để truy cập hệ thống.
2. Xem Dashboard: Theo dõi lượng calo tiêu thụ, biểu đồ dinh dưỡng và truy cập các chức năng chính.
3. Thêm bữa ăn: Chọn loại bữa ăn và tải ảnh để AI nhận diện hoặc nhập thủ công.
4. Nhận diện bằng AI: Hệ thống phân tích ảnh và trả về kết quả. Nếu thất bại, người dùng có thể nhập thủ công.
5. Hiệu chỉnh kết quả AI: Kiểm tra, chỉnh sửa tên món ăn, thành phần và khẩu phần trước khi xác nhận lưu.
6. Nhập món ăn thủ công: Nhập tên món ăn, thành phần và khẩu phần, sau đó lưu thông tin.
7. Tính toán và lưu dinh dưỡng: Hệ thống tính toán các chỉ số dinh dưỡng, lưu vào cơ sở dữ liệu và cập nhật Dashboard.
8. Xem lịch sử bữa ăn: Tìm kiếm, lọc, xem chi tiết, chỉnh sửa hoặc xóa các bữa ăn đã lưu.
9. Xem báo cáo dinh dưỡng: Theo dõi thống kê theo tuần/tháng và xuất báo cáo PDF hoặc CSV.
10. Đăng xuất: Người dùng đăng xuất để kết thúc phiên sử dụng.
Bản phác thảo các màn hình tải ảnh, hiệu chỉnh, lịch sử và báo cáo.
Note: tất cả chỉ ở giai đoạn phác thảo, minh họa chưa chốt chuẩn xác
1. Màn hình tải ảnh bữa ăn
Màn hình khởi đầu của quy trình ghi nhận bữa ăn. Cho phép chọn bữa ăn và tải tệp hình ảnh từ thiết bị hoặc camera.
1.1. Bản phác thảo giao diện 

1.2. Khối thông tin hiển thị
Loại bữa ăn: 4 lựa chọn Radio/Pills gồm: Bữa Sáng, Bữa Trưa, Bữa Tối, Bữa Phụ (tự động gợi ý dựa trên giờ thực tế: 6h-9h sáng, 11h-13h trưa, 18h-20h tối).
Khu vực kéo thả ảnh (Dropzone): Hiển thị icon máy ảnh, quy định kích thước dung lượng (tối đa 5MB) và định dạng (.jpg, .jpeg, .png).
Khung xem trước ảnh: Khi chọn xong sẽ hiện ảnh thu nhỏ, tên file và dung lượng tệp.
Thông báo lỗi (nếu có): Hiện khung cảnh báo đỏ nếu ảnh vượt dung lượng hoặc không đúng định dạng.
1.3. Danh sách các nút thao tác
Tên nút / Thao tác
Vị trí
Sự kiện kích hoạt
Chọn loại bữa ăn
Nhóm Radio phía trên
Người dùng click chọn
Chọn ảnh từ máy
Trong khung Dropzone
Click chuột
Chụp ảnh trực tiếp
Trong khung Dropzone
Click chuột
Xóa ảnh
Cạnh ảnh Preview
Click chuột
Nhập tay món ăn
Góc dưới bên trái
Click chuột
TIẾP TỤC NHẬN DIỆN AI
Góc dưới bên phải (Nổi bật)
Click chuột

2. Màn hình xem trước và hiệu chỉnh kết quả AI
Màn hình hiển thị kết quả do mô hình AI trả về (Tên món, danh sách thành phần, khối lượng phán đoán). Cho phép người dùng chỉnh sửa, bổ sung trước khi lưu chính thức.
2.1. Bản phác thảo giao diện

2.2. Khối thông tin hiển thị
Cột trái:
Hình ảnh món ăn đã chụp/tải lên.
Hộp tóm tắt tổng giá trị dinh dưỡng tạm tính (tự động nhảy số theo thời gian thực khi người dùng tăng giảm khẩu phần): Lượng Calo, Chất đạm, Tinh bột, Chất béo, Chất xơ.
Cột phải:
Ô nhập Tên món ăn (cho phép click icon cây bút  để sửa lại nếu AI nhận diện nhầm).
Badge hiển thị độ tin cậy của AI (Confidence Score).
Danh sách từng thành phần món ăn:
Tên nguyên liệu.
Ô nhập khối lượng/khẩu phần (input số).
Dropdown chọn đơn vị tính (Gram, ml, Bát, Đĩa, Miếng).
Calo tính riêng của từng thành phần.
Nút xóa thành phần.
2.3. Danh sách các nút thao tác
Tên nút / Thao tác
Vị trí
Sự kiện kích hoạt
Hoàn tác AI gốc
Góc trên bên phải
Click chuột
Sửa tên món
Cạnh ô Tên món ăn
Click chuột
Thêm thành phần
Phía trên danh sách
Click chuột
Sửa khẩu phần / Đơn vị
Trên từng dòng nguyên liệu
Thay đổi số / chọn dropdown
Xóa
Cuối mỗi dòng thành phần
Click chuột
HỦY BỎ
Dưới cùng bên trái
Click chuột
XÁC NHẬN VÀ LƯU BỮA ĂN
Dưới cùng bên phải (Nổi bật)
Click chuột

3. Màn hình lịch sử bữa ăn
Cho phép xem danh sách các bữa ăn đã lưu trong quá khứ, hỗ trợ lọc theo ngày và loại bữa, xem chi tiết, sửa hoặc xóa bữa ăn.
3.1. Bản phác thảo giao diện 

3.2. Khối thông tin hiển thị
Thanh công cụ lọc:
Date-range Picker: Lọc theo 7 ngày qua, 30 ngày qua hoặc tùy chọn ngày bắt đầu - ngày kết thúc.
Dropdown Loại bữa: Tất cả, Bữa Sáng, Bữa Trưa, Bữa Tối, Bữa Phụ.
Tiêu đề nhóm ngày: Hiển thị tổng Calo đã nạp trong ngày đó so với mục tiêu ngày và tỷ lệ phần trăm (%).
Thẻ bữa ăn (Meal Card):
Ảnh thu nhỏ món ăn (thumbnail).
Nhãn loại bữa ăn và thời gian ghi nhận (vd: Bữa Trưa • 12:15).
Tên món ăn chính.
Các tag dinh dưỡng nổi bật: Calo, Đạm, Đường, Béo.
Trạng thái danh sách rỗng (Empty state): Nếu không có bữa ăn nào trong khoảng thời gian đã chọn, hiển thị hình minh họa đĩa ăn rỗng kèm dòng chữ "Bạn chưa ghi nhận bữa ăn nào trong khoảng thời gian này" và nút "+ Thêm bữa ăn ngay".
Phân trang (Pagination): Chuyển trang nếu số lượng bữa ăn nhiều.
3.3. Danh sách các nút thao tác
Tên nút / Thao tác
Vị trí
Sự kiện kích hoạt
+ Thêm bữa ăn mới
Góc trên bên phải
Click chuột
Bộ lọc khoảng ngày
Thanh lọc
Chọn khoảng ngày
Bộ lọc loại bữa
Thanh lọc
Chọn loại bữa
Xem chi tiết
Trên từng Meal Card
Click chuột
Sửa
Trên từng Meal Card
Click chuột
Xóa (icon thùng rác)
Trên từng Meal Card
Click chuột
Trang trước / sau
Phân trang phía dưới
Click chuột

4. Màn hình báo cáo và xuất file
Màn hình phân tích chuyên sâu các chỉ số dinh dưỡng theo chu kỳ tuần/tháng, so sánh với mục tiêu cá nhân và cho phép trích xuất báo cáo dữ liệu.
4.1. Bản phác thảo giao diện 

4.2. Khối thông tin hiển thị
Bộ chuyển đổi mốc thời gian: Tab chuyển nhanh giữa 7 ngày qua, 30 ngày qua hoặc Chọn khoảng ngày tùy chọn.
Thẻ tóm tắt chỉ số trung bình (KPI Cards):
Calo trung bình/ngày kèm mức độ % so với mục tiêu TDEE.
Lượng Đạm (Protein), Đường (Carbs), Béo (Fat) trung bình mỗi ngày kèm nhãn trạng thái (Đạt chuẩn / Thấp / Cao).
Biểu đồ cột xu hướng năng lượng (Bar Chart): Trục hoành là các ngày trong chu kỳ, trục tung là lượng calo; có vạch kẻ ngang đỏ thể hiện mức Calo khuyến nghị (Target Calories) để trực quan hóa những ngày ăn vượt hoặc thiếu calo.
Biểu đồ cơ cấu nhóm chất (Donut Chart): Tỷ lệ phân bổ năng lượng % giữa Carbs / Protein / Fat.
Hộp khuyến nghị thông minh: Tự động đưa ra nhận xét dựa trên dữ liệu thực tế (vd: số ngày vượt mục tiêu, thiếu chất xơ).
4.3. Danh sách các nút thao tác
Tên nút / Thao tác
Vị trí
Sự kiện kích hoạt
7 ngày / 30 ngày / Tùy chỉnh
Thanh chọn thời gian
Click chuột vào Tab
Tương tác trên biểu đồ (Tooltip)
Trên cột biểu đồ
Di chuột (Hover)
XUẤT BÁO CÁO PDF
Dưới cùng bên trái
Click chuột
XUẤT DỮ LIỆU CSV
Dưới cùng bên phải
Click chuột



