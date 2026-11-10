Use Case Diagram
Mã
Tên
Actor chính
Actor phụ
FR liên quan
UC-01
Đăng ký tài khoản
user
Email
FR-01
UC-02
Xác thực email
user
—
FR-02
UC-03
Đăng nhập
user
—
FR-03
UC-04
Đăng xuất
user
—
FR-04
UC-05
Quên mật khẩu
user
Email
FR-05
UC-06
Đổi mật khẩu
user
—
FR-06
UC-07
Xem hồ sơ
user
—
FR-09
UC-08
Cập nhật hồ sơ
user
—
FR-10, FR-11
UC-09
Đổi ảnh đại diện
user
Storage
FR-12
UC-10
Ghi nhận bữa ăn bằng ảnh
user
AI, Storage
FR-13 → FR-20
UC-11
Nhập tay món ăn
user
—
FR-19
UC-12
Xác nhận kết quả AI
user
—
FR-21
UC-13
Hiệu chỉnh kết quả AI
user
—
FR-22 → FR-26
UC-14
Tính dinh dưỡng bữa ăn
user
—
FR-28 → FR-31
UC-15
Xem lịch sử bữa ăn
user
—
FR-32 → FR-34
UC-16
Xem chi tiết bữa ăn
user
—
FR-35
UC-17
Sửa bữa ăn
user
—
FR-36
UC-18
Xóa bữa ăn
user
—
FR-37
UC-19
Xem dashboard
user
—
FR-38
UC-20
Xem báo cáo dinh dưỡng
user
—
FR-39, FR-40
UC-21
Xuất báo cáo
user
—
FR-41
UC-22
Quản lý người dùng
admin
—
FR-43 → FR-46
UC-23
Xem log hệ thống
admin
—
FR-47
UC-24
Xem thống kê tổng quan
admin
—
FR-48




Use Case Specifications- đặc tả use case
MODULE 1 — XÁC THỰC VÀ QUẢN LÝ TÀI KHOẢN
UC-01: Đăng ký tài khoản
Mục
Nội dung
Mã Use Case
UC-01
Tên Use Case
Đăng ký tài khoản
Actor chính
User
Actor phụ
Email Service
Mô tả
Cho phép người dùng tạo tài khoản mới bằng email và mật khẩu.
Tiền điều kiện
Người dùng chưa đăng nhập.
Kích hoạt
Người dùng chọn chức năng Đăng ký.
Hậu điều kiện thành công
Tài khoản mới được tạo ở trạng thái pending; hệ thống gửi email xác thực hoặc ghi nhận trạng thái gửi email thất bại để người dùng có thể yêu cầu gửi lại.
Use Case liên quan
UC-02
Luồng chính
1. Người dùng mở trang đăng ký.
2. Hệ thống hiển thị biểu mẫu đăng ký.
3. Người dùng nhập họ tên, email, mật khẩu, xác nhận mật khẩu và đồng ý với điều khoản sử dụng.
4. Người dùng chọn Đăng ký.
5. Hệ thống kiểm tra tính hợp lệ của dữ liệu.6. Hệ thống kiểm tra email đã được sử dụng hay chưa.
7. Hệ thống tạo tài khoản ở trạng thái chờ
8. Hệ thống tạo thông tin xác thực và yêu cầu Email Service gửi email xác thực.
9. Hệ thống thông báo người dùng kiểm tra email để xác thực tài khoản.
Luồng thay thế
- 3a. Người dùng chọn Đã có tài khoản: hệ thống chuyển sang UC-03 – Đăng nhập.
Luồng ngoại lệ
5a. Dữ liệu không hợp lệ: hệ thống hiển thị thông báo lỗi tương ứng.
6a. Email đã tồn tại: hệ thống thông báo email đã được sử dụng.
5b. Mật khẩu không đáp ứng quy tắc hoặc xác nhận mật khẩu không khớp: hệ thống yêu cầu người dùng nhập lại.
8a. Gửi email thất bại: tài khoản vẫn được tạo theo trạng thái đã xác định; hệ thống thông báo hoặc cung cấp chức năng gửi lại email.
Yêu cầu liên quan
FR-01; BR-01, BR-02, BR-10.





UC-02: Xác thực email
Mục
Nội dung
Mã Use Case
UC-02
Tên Use Case
Xác thực email
Actor chính
User
Actor phụ
—
Mô tả
Cho phép người dùng xác thực địa chỉ email để kích hoạt tài khoản.
Tiền điều kiện
Người dùng có tài khoản chưa xác thực và truy cập được liên kết xác thực.
Kích hoạt
Người dùng mở liên kết xác thực trong email.
Hậu điều kiện thành công
Tài khoản chuyển sang trạng thái hoạt động; thông tin xác thực được đánh dấu đã sử dụng.
Luồng chính
1. Người dùng mở email xác thực.
2. Người dùng chọn liên kết xác thực.
3. Hệ thống kiểm tra tính hợp lệ và thời hạn của thông tin xác thực.
4. Hệ thống xác định tài khoản tương ứng.
5. Hệ thống chuyển trạng thái tài khoản sang ACTIVE.
6. Hệ thống đánh dấu thông tin xác thực đã được sử dụng.
7. Hệ thống thông báo xác thực thành công và chuyển người dùng đến trang đăng nhập.
Luồng thay thế
- 3a. Tài khoản đã được xác thực trước đó: hệ thống thông báo tài khoản đã được xác thực.
Luồng ngoại lệ
3b. Thông tin xác thực không hợp lệ: hệ thống thông báo liên kết không hợp lệ.
3c. Thông tin xác thực đã hết hạn: hệ thống thông báo hết hạn và hướng dẫn người dùng yêu cầu gửi lại email xác thực.
Yêu cầu liên quan
FR-02; BR-04, BR-05.










UC-03: Đăng nhập
Mục
Nội dung
Mã Use Case
UC-03
Tên Use Case
Đăng nhập
Actor chính
User
Actor phụ
—
Mô tả
Cho phép người dùng đăng nhập vào hệ thống bằng email và mật khẩu.
Tiền điều kiện
Người dùng có tài khoản.
Kích hoạt
Người dùng chọn Đăng nhập.
Hậu điều kiện thành công
Người dùng được xác thực và có thể truy cập các chức năng phù hợp với quyền của tài khoản.
Use Case liên quan
UC-05
Luồng chính
1. Người dùng mở trang đăng nhập
.2. Hệ thống hiển thị biểu mẫu đăng nhập.
3. Người dùng nhập email và mật khẩu.
4. Người dùng chọn Đăng nhập
5. Hệ thống kiểm tra thông tin tài khoản và mật khẩu.
6. Hệ thống kiểm tra trạng thái xác thực và trạng thái khóa tài khoản.
7. Nếu thông tin hợp lệ, hệ thống tạo phiên đăng nhập và cấp thông tin xác thực cần thiết.
8. Hệ thống chuyển người dùng đến trang chủ.
Luồng thay thế
- 3a. Người dùng chọn Quên mật khẩu: hệ thống chuyển sang UC-05.
Luồng ngoại lệ
5a. Email hoặc mật khẩu không chính xác: hệ thống hiển thị thông báo chung, không tiết lộ thông tin tài khoản.
6a. Tài khoản chưa xác thực: hệ thống thông báo người dùng cần xác thực email.
6b. Tài khoản đang bị khóa: hệ thống thông báo trạng thái khóa và thời điểm có thể đăng nhập lại nếu xác định được.
5b. Người dùng đăng nhập sai từ 5 lần theo quy tắc hiện tại: hệ thống khóa đăng nhập trong 15 phút.
Yêu cầu liên quan
FR-03; BR-03, BR-07.








UC-04: Đăng xuất
Mục
Nội dung
Mã Use Case
UC-04
Tên Use Case
Đăng xuất
Actor chính
User
Actor phụ
—
Mô tả
Cho phép người dùng kết thúc phiên làm việc hiện tại.
Tiền điều kiện
Người dùng đang đăng nhập.
Kích hoạt
Người dùng chọn Đăng xuất.
Hậu điều kiện thành công
Phiên đăng nhập hiện tại được kết thúc theo cơ chế xác thực của hệ thống; người dùng được chuyển về trang đăng nhập.
Luồng chính
1. Người dùng chọn Đăng xuất.
2. Hệ thống thực hiện việc kết thúc phiên đăng nhập và thu hồi thông tin xác thực có thể thu hồi.
3. Ứng dụng phía người dùng xóa thông tin xác thực được lưu trên thiết bị.
4. Hệ thống chuyển người dùng về trang đăng nhập.
Luồng ngoại lệ
- 2a. Không thể thu hồi thông tin xác thực phía máy chủ: ứng dụng vẫn thực hiện việc xóa thông tin xác thực cục bộ và thông báo hoặc xử lý theo cơ chế lỗi đã xác định. Việc này không đồng nghĩa với việc mọi thông tin xác thực phía máy chủ đã bị thu hồi.
Yêu cầu liên quan
FR-04.

UC-05: Quên mật khẩu
Mục
Nội dung
Mã Use Case
UC-05
Tên Use Case
Quên mật khẩu
Actor chính
User
Actor phụ
Email Service
Mô tả
Cho phép người dùng yêu cầu đặt lại mật khẩu thông qua email.
Tiền điều kiện
Người dùng có thể truy cập chức năng quên mật khẩu và cung cấp địa chỉ email cần kiểm tra.
Kích hoạt
Người dùng chọn Quên mật khẩu.
Hậu điều kiện thành công
Nếu email thuộc tài khoản hợp lệ, hệ thống gửi hướng dẫn đặt lại mật khẩu; mật khẩu chỉ được thay đổi sau khi thông tin xác thực hợp lệ.
Luồng chính
1. Người dùng chọn Quên mật khẩu.
2. Hệ thống hiển thị biểu mẫu nhập email.
3. Người dùng nhập email và gửi yêu cầu.
4. Hệ thống xử lý yêu cầu mà không tiết lộ email có tồn tại trong hệ thống hay không.
5. Nếu tài khoản phù hợp, hệ thống tạo thông tin xác thực đặt lại mật khẩu và yêu cầu Email Service gửi email.
6. Người dùng mở email và truy cập liên kết đặt lại mật khẩu.
7. Hệ thống kiểm tra thông tin xác thực.
8. Người dùng nhập mật khẩu mới và xác nhận mật khẩu.
9. Hệ thống kiểm tra dữ liệu, cập nhật mật khẩu và xử lý các phiên đăng nhập cũ theo quy tắc bảo mật.
10. Hệ thống thông báo kết quả và hướng dẫn người dùng đăng nhập lại.
Luồng thay thế
- 5a. Người dùng không nhận được email: người dùng có thể yêu cầu gửi lại theo giới hạn được quy định.
Luồng ngoại lệ
4a. Email không tồn tại: hệ thống vẫn trả về thông báo chung.
7a. Thông tin xác thực không hợp lệ, đã sử dụng hoặc hết hạn: hệ thống yêu cầu người dùng thực hiện lại quy trình.
8a. Mật khẩu mới không hợp lệ hoặc xác nhận không khớp: hệ thống yêu cầu nhập lại.
5b. Gửi email thất bại: hệ thống thông báo theo cách không làm lộ thông tin tài khoản.
Yêu cầu liên quan
FR-05; BR-06, BR-11.

UC-06: Đổi mật khẩu
Mục
Nội dung
Mã Use Case
UC-06
Tên Use Case
Đổi mật khẩu
Actor chính
User
Actor phụ
—
Mô tả
Cho phép người dùng đang đăng nhập thay đổi mật khẩu tài khoản.
Tiền điều kiện
Người dùng đang đăng nhập.
Kích hoạt
Người dùng chọn chức năng Đổi mật khẩu.
Hậu điều kiện thành công
Mật khẩu mới được cập nhật; các phiên đăng nhập khác được xử lý theo chính sách bảo mật.
Luồng chính
1. Người dùng mở phần Cài đặt tài khoản và chọn Đổi mật khẩu.
2. Hệ thống hiển thị biểu mẫu đổi mật khẩu.
3. Người dùng nhập mật khẩu hiện tại, mật khẩu mới và xác nhận mật khẩu mới.
4. Hệ thống kiểm tra mật khẩu hiện tại.
5. Hệ thống kiểm tra tính hợp lệ của mật khẩu mới.
6. Hệ thống cập nhật mật khẩu.
7. Hệ thống xử lý các phiên đăng nhập khác theo chính sách bảo mật.
8. Hệ thống thông báo đổi mật khẩu thành công.
Luồng ngoại lệ
4a. Mật khẩu hiện tại không chính xác: hệ thống thông báo lỗi.
5a. Mật khẩu mới không đáp ứng quy tắc hoặc xác nhận không khớp: hệ thống yêu cầu nhập lại.
Yêu cầu liên quan
FR-06; BR-08, BR-09, BR-10.

MODULE 2 — HỒ SƠ CÁ NHÂN
UC-07: Xem hồ sơ
Mục
Nội dung
Mã Use Case
UC-07
Tên Use Case
Xem hồ sơ
Actor chính
User
Mô tả
Cho phép người dùng xem thông tin cá nhân và các chỉ số liên quan đến dinh dưỡng.
Tiền điều kiện
Người dùng đang đăng nhập.
Kích hoạt
Người dùng mở trang Hồ sơ.
Hậu điều kiện
Thông tin hồ sơ được hiển thị; dữ liệu không bị thay đổi.
Luồng chính
1. Người dùng mở trang Hồ sơ.
2. Hệ thống truy vấn thông tin hồ sơ của người dùng hiện tại.
3. Hệ thống kiểm tra mức độ đầy đủ của dữ liệu cần thiết để tính các chỉ số.
4. Nếu đủ dữ liệu, hệ thống tính toán hoặc lấy các chỉ số BMI, BMR và TDEE tương ứng.
5. Hệ thống hiển thị thông tin cá nhân và các chỉ số.
6. Hệ thống cung cấp chức năng chỉnh sửa hồ sơ.
Luồng thay thế
- 3a. Người dùng chưa có hồ sơ hoặc chưa cung cấp đủ thông tin: hệ thống hiển thị các thông tin hiện có và hướng dẫn cập nhật hồ sơ.
Luồng ngoại lệ
2a. Phiên đăng nhập không còn hợp lệ: hệ thống yêu cầu người dùng đăng nhập lại.
2b. Không thể truy vấn dữ liệu: hệ thống thông báo lỗi và cho phép thử lại.
Yêu cầu liên quan
FR-09.

UC-08: Cập nhật hồ sơ
Mục
Nội dung
Mã Use Case
UC-08
Tên Use Case
Cập nhật hồ sơ
Actor chính
User
Mô tả
Cho phép người dùng cập nhật thông tin cá nhân.
Tiền điều kiện
Người dùng đang đăng nhập.
Kích hoạt
Người dùng chọn Chỉnh sửa hồ sơ.
Hậu điều kiện thành công
Thông tin hồ sơ được cập nhật; các chỉ số phụ thuộc vào thông tin vừa thay đổi được tính toán lại.
Luồng chính
1. Người dùng mở trang Hồ sơ và chọn Chỉnh sửa.
2. Hệ thống hiển thị biểu mẫu với thông tin hiện tại.
3. Người dùng cập nhật các trường thông tin.
4. Người dùng chọn Lưu.
5. Hệ thống kiểm tra tính hợp lệ của dữ liệu.
6. Hệ thống cập nhật thông tin hồ sơ.
7. Hệ thống tính toán lại các chỉ số BMI, BMR và TDEE khi có đủ dữ liệu cần thiết.
8. Hệ thống thông báo cập nhật thành công và hiển thị thông tin mới.
Luồng thay thế
- 4a. Người dùng chọn Hủy: hệ thống bỏ các thay đổi chưa lưu và quay lại trang hồ sơ.
Luồng ngoại lệ
5a. Dữ liệu không hợp lệ, chẳng hạn ngày sinh không hợp lệ hoặc các giá trị nằm ngoài phạm vi được chấp nhận: hệ thống thông báo lỗi tương ứng.
6a. Không thể cập nhật dữ liệu: hệ thống thông báo lỗi và giữ nguyên dữ liệu đã lưu trước đó.
Yêu cầu liên quan
FR-10, FR-11; BR-14 đến BR-17.

UC-09: Đổi ảnh đại diện
Mục
Nội dung
Mã Use Case
UC-09
Tên Use Case
Đổi ảnh đại diện
Actor chính
User
Actor phụ
Storage Service
Mô tả
Cho phép người dùng tải lên và sử dụng ảnh đại diện mới.
Tiền điều kiện
Người dùng đang đăng nhập.
Kích hoạt
Người dùng chọn chức năng Đổi ảnh đại diện.
Hậu điều kiện thành công
Ảnh mới được lưu trữ và thông tin ảnh đại diện của người dùng được cập nhật.
Luồng chính
1. Người dùng chọn Đổi ảnh đại diện.
2. Hệ thống hiển thị chức năng chọn ảnh.
3. Người dùng chọn tệp ảnh từ thiết bị.
4. Hệ thống kiểm tra định dạng, dung lượng và kích thước ảnh theo quy định.
5. Hệ thống gửi ảnh đến Storage Service.
6. Storage Service trả về thông tin tham chiếu đến ảnh đã lưu.
7. Hệ thống cập nhật thông tin ảnh đại diện trong hồ sơ người dùng.
8. Hệ thống hiển thị ảnh đại diện mới.
Luồng ngoại lệ
4a. Tệp không đúng định dạng, vượt giới hạn dung lượng hoặc không đáp ứng yêu cầu kích thước: hệ thống thông báo lỗi.
5a. Tải ảnh lên thất bại: hệ thống thông báo và cho phép thử lại.
7a. Không thể cập nhật thông tin ảnh trong hồ sơ: hệ thống thông báo lỗi; việc xử lý ảnh đã tải lên nhưng chưa được sử dụng cần tuân theo quy tắc quản lý tệp của hệ thống.
Yêu cầu liên quan
FR-12; BR-18.

MODULE 3 — GHI NHẬN BỮA ĂN VÀ TÍNH DINH DƯỠNG
UC-10: Ghi nhận bữa ăn bằng ảnh
Mục
Nội dung
Mã Use Case
UC-10
Tên Use Case
Ghi nhận bữa ăn bằng ảnh
Actor chính
User
Actor phụ
AI Service, Storage Service
Mô tả
Cho phép người dùng cung cấp ảnh bữa ăn để hệ thống nhận diện món ăn và hỗ trợ ghi nhận bữa ăn.
Tiền điều kiện
Người dùng đang đăng nhập.
Kích hoạt
Người dùng chọn chức năng Thêm bữa ăn bằng ảnh.
Hậu điều kiện thành công
Kết quả nhận diện được người dùng xác nhận và bữa ăn được ghi nhận theo quy trình nghiệp vụ.
Use Case liên quan
UC-11, UC-12, UC-13, UC-14
Luồng chính
1. Người dùng chọn Thêm bữa ăn.
2. Hệ thống cho phép người dùng chọn loại bữa ăn.
3. Người dùng chụp ảnh hoặc tải ảnh từ thiết bị.
4. Hệ thống kiểm tra tính hợp lệ của ảnh.
5. Hệ thống gửi ảnh đến Storage Service để lưu trữ.
6. Hệ thống gửi ảnh hoặc thông tin ảnh cần thiết đến AI Service để nhận diện.
7. AI Service trả về kết quả nhận diện, bao gồm thông tin món ăn và các thành phần mà dịch vụ hỗ trợ nhận diện.
8. Hệ thống hiển thị kết quả nhận diện cùng ảnh bữa ăn.
9. Người dùng kiểm tra kết quả và lựa chọn xác nhận hoặc hiệu chỉnh.
10. Hệ thống tiếp tục xử lý theo UC-12 hoặc UC-13.
11. Sau khi kết quả được xác nhận, hệ thống thực hiện quy trình tính dinh dưỡng theo UC-14
12. Hệ thống hoàn tất việc ghi nhận bữa ăn theo trạng thái xử lý thực tế và thông báo kết quả cho người dùng.
Luồng thay thế
8a. Người dùng chọn Hiệu chỉnh: hệ thống chuyển sang UC-13.
7a. AI Service không nhận diện được món ăn: hệ thống cho phép người dùng nhập tay theo UC-11.
9a. Người dùng chọn Hủy: hệ thống kết thúc quy trình ghi nhận hiện tại mà không tạo bữa ăn chính thức.
Luồng ngoại lệ
4a. Ảnh không đúng định dạng hoặc vượt giới hạn cho phép: hệ thống thông báo lỗi.
5a. Storage Service không thể lưu ảnh: hệ thống thông báo lỗi và cho phép thử lại.
6a. AI Service không phản hồi trong thời gian quy định: hệ thống thông báo và cho phép thử lại hoặc nhập tay.
7b. AI Service trả về kết quả rỗng hoặc không hợp lệ: hệ thống cho phép người dùng nhập tay.
11a. Không thể tính dinh dưỡng: hệ thống thông báo tình trạng xử lý. Cách lưu và trạng thái của bữa ăn trong trường hợp này cần được thống nhất theo quy tắc nghiệp vụ.
Yêu cầu liên quan
FR-13 đến FR-20; BR-19, BR-20, BR-24, BR-25.

UC-11: Nhập tay món ăn
Mục
Nội dung
Mã Use Case
UC-11
Tên Use Case
Nhập tay món ăn
Actor chính
User
Mô tả
Cho phép người dùng tự nhập thông tin món ăn khi không sử dụng được kết quả nhận diện AI hoặc muốn nhập trực tiếp.
Tiền điều kiện
Người dùng đang đăng nhập và đang thực hiện quy trình thêm bữa ăn.
Kích hoạt
Người dùng chọn Nhập tay.
Hậu điều kiện
Thông tin món ăn do người dùng nhập được chuyển sang bước kiểm tra và xác nhận.
Luồng chính
1. Người dùng chọn Nhập tay.
2. Hệ thống hiển thị biểu mẫu nhập thông tin món ăn.
3. Người dùng nhập tên món ăn.
4. Người dùng thêm các thành phần của món ăn và thông tin khẩu phần tương ứng.
5. Người dùng chọn Tiếp tục.
6. Hệ thống kiểm tra tính hợp lệ của thông tin.
7. Hệ thống hiển thị kết quả nhập để người dùng kiểm tra và chuyển sang UC-12.
Luồng ngoại lệ
6a. Tên món ăn hoặc dữ liệu thành phần không hợp lệ: hệ thống thông báo lỗi.
6b. Không có thành phần nào được khai báo: hệ thống yêu cầu người dùng bổ sung thành phần.
6c. Không tìm thấy thông tin dinh dưỡng tương ứng: hệ thống thông báo và xử lý theo quy tắc dữ liệu dinh dưỡng đã xác định.
Yêu cầu liên quan
FR-19; BR-21.












UC-12: Xác nhận kết quả AI
Mục
Nội dung
Mã Use Case
UC-12
Tên Use Case
Xác nhận kết quả AI
Actor chính
User
Mô tả
Cho phép người dùng xác nhận kết quả nhận diện hoặc thông tin món ăn đã nhập tay.
Tiền điều kiện
Người dùng đang xem kết quả nhận diện hoặc thông tin món ăn đã nhập tay.
Kích hoạt
Người dùng chọn Xác nhận.
Hậu điều kiện thành công
Kết quả được đánh dấu đã xác nhận và được chuyển sang bước tính dinh dưỡng.
Luồng chính
1. Hệ thống hiển thị thông tin món ăn và các thành phần.
2. Người dùng kiểm tra kết quả.3. Người dùng chọn Xác nhận.
4. Hệ thống ghi nhận việc xác nhận của người dùng.
5. Hệ thống chuyển dữ liệu sang UC-14 để tính dinh dưỡng.
6. Hệ thống hoàn tất việc ghi nhận bữa ăn theo kết quả xử lý.
Luồng thay thế
3a. Người dùng chọn Hiệu chỉnh: hệ thống chuyển sang UC-13.
3b. Người dùng chọn Hủy: hệ thống kết thúc quy trình ghi nhận hiện tại.
Luồng ngoại lệ
- 5a. Không thể tính dinh dưỡng: hệ thống thông báo trạng thái xử lý. Việc lưu bữa ăn và trạng thái tương ứng cần tuân theo quy tắc nghiệp vụ đã thống nhất.
Yêu cầu liên quan
FR-21; BR-26, BR-27.

UC-13: Hiệu chỉnh kết quả AI
Mục
Nội dung
Mã Use Case
UC-13
Tên Use Case
Hiệu chỉnh kết quả AI
Actor chính
User
Mô tả
Cho phép người dùng chỉnh sửa thông tin món ăn, thành phần và khẩu phần trước khi xác nhận.
Tiền điều kiện
Người dùng đang xem kết quả nhận diện hoặc thông tin món ăn chưa được xác nhận.
Kích hoạt
Người dùng chọn Hiệu chỉnh.
Hậu điều kiện thành công
Thông tin đã chỉnh sửa được cập nhật và sẵn sàng cho bước xác nhận.
Luồng chính
1. Người dùng chọn Hiệu chỉnh.
2. Hệ thống hiển thị biểu mẫu chỉnh sửa.
3. Người dùng chỉnh sửa tên món ăn nếu cần.
4. Người dùng thêm, sửa hoặc xóa thành phần.
5. Người dùng điều chỉnh khẩu phần và đơn vị tương ứng.
6. Người dùng chọn Lưu.
7. Hệ thống kiểm tra tính hợp lệ của dữ liệu.
8. Hệ thống cập nhật kết quả đã chỉnh sửa.
9. Hệ thống hiển thị kết quả mới để người dùng kiểm tra.
Luồng thay thế
6a. Người dùng chọn Hoàn tác: hệ thống khôi phục kết quả nhận diện ban đầu.
6b. Người dùng chọn Hủy: hệ thống không áp dụng các thay đổi chưa lưu và quay lại màn hình kết quả.
Luồng ngoại lệ
7a. Tên món ăn không hợp lệ hoặc không có thành phần theo quy tắc nghiệp vụ: hệ thống thông báo lỗi.
7b. Khẩu phần không hợp lệ, chẳng hạn nhỏ hơn hoặc bằng 0: hệ thống yêu cầu nhập lại.
Yêu cầu liên quan
FR-22 đến FR-26; BR-21, BR-22, BR-23.

UC-14: Tính dinh dưỡng bữa ăn
Mục
Nội dung
Mã Use Case
UC-14
Tên Use Case
Tính dinh dưỡng bữa ăn
Actor chính
User (khởi tạo gián tiếp thông qua thao tác xác nhận)
Actor phụ
—
Mô tả
Tính toán tổng giá trị dinh dưỡng của bữa ăn dựa trên các thành phần và khẩu phần đã được xác nhận.
Tiền điều kiện
Thông tin bữa ăn đã được xác nhận và có dữ liệu thành phần, khẩu phần hợp lệ.
Kích hoạt
Quy trình ghi nhận bữa ăn yêu cầu tính dinh dưỡng.
Hậu điều kiện thành công
Kết quả dinh dưỡng của bữa ăn được tính toán và lưu theo quy tắc nghiệp vụ.
Luồng chính
1. Hệ thống tiếp nhận danh sách thành phần và khẩu phần của bữa ăn đã xác nhận.
2. Hệ thống tra cứu thông tin dinh dưỡng tương ứng với từng thành phần.
3. Hệ thống quy đổi giá trị dinh dưỡng theo khẩu phần và đơn vị đo.
4. Hệ thống tổng hợp giá trị dinh dưỡng của các thành phần.
5. Hệ thống tạo kết quả dinh dưỡng tổng hợp của bữa ăn.
6. Hệ thống lưu hoặc trả kết quả cho quy trình ghi nhận bữa ăn.
Luồng thay thế
- 2a. Không tìm thấy thông tin dinh dưỡng của một thành phần: hệ thống xử lý theo quy tắc dữ liệu thiếu đã xác định.
Luồng ngoại lệ
3a. Khẩu phần hoặc dữ liệu dinh dưỡng không hợp lệ: hệ thống không sử dụng dữ liệu đó để tạo kết quả và thông báo lỗi.
5a. Không thể hoàn tất phép tính hoặc lưu kết quả: hệ thống thông báo trạng thái thất bại để quy trình ghi nhận bữa ăn xử lý tiếp.
Yêu cầu liên quan
FR-28 đến FR-31; BR-26, BR-29, BR-30.

MODULE 4 — LỊCH SỬ BỮA ĂN
UC-15: Xem lịch sử bữa ăn
Mục
Nội dung
Mã Use Case
UC-15
Tên Use Case
Xem lịch sử bữa ăn
Actor chính
User
Mô tả
Cho phép người dùng xem danh sách bữa ăn đã ghi nhận và lọc theo các điều kiện phù hợp.
Tiền điều kiện
Người dùng đang đăng nhập.
Kích hoạt
Người dùng mở trang Lịch sử bữa ăn.
Hậu điều kiện
Danh sách bữa ăn phù hợp được hiển thị.
Luồng chính
1. Người dùng mở trang Lịch sử bữa ăn.
2. Hệ thống truy vấn các bữa ăn thuộc về người dùng hiện tại trong khoảng thời gian mặc định là 7 ngày gần nhất.
3. Hệ thống hiển thị danh sách bữa ăn, bao gồm các thông tin cơ bản như ảnh, tên món ăn, giá trị calo và thời gian ghi nhận.
4. Người dùng có thể chọn khoảng thời gian hoặc loại bữa ăn cần xem.
5. Hệ thống truy vấn lại dữ liệu theo điều kiện lọc
6. Hệ thống hiển thị danh sách kết quả.
Luồng thay thế
- 3a. Người dùng chọn một bữa ăn: hệ thống chuyển sang UC-16 – Xem chi tiết bữa ăn.
Luồng ngoại lệ
2a. Không có bữa ăn phù hợp: hệ thống hiển thị thông báo chưa có dữ liệu.
5a. Không thể truy vấn dữ liệu: hệ thống thông báo lỗi và cho phép thử lại.
Yêu cầu liên quan
FR-32 đến FR-34; BR-28, BR-32, BR-33.

UC-16: Xem chi tiết bữa ăn
Mục
Nội dung
Mã Use Case
UC-16
Tên Use Case
Xem chi tiết bữa ăn
Actor chính
User
Mô tả
Cho phép người dùng xem đầy đủ thông tin của một bữa ăn đã ghi nhận.
Tiền điều kiện
Người dùng đang đăng nhập và bữa ăn được yêu cầu thuộc quyền sở hữu của người dùng.
Kích hoạt
Người dùng chọn một bữa ăn trong lịch sử.
Hậu điều kiện
Thông tin chi tiết của bữa ăn được hiển thị.
Luồng chính
1. Người dùng chọn một bữa ăn.
2. Hệ thống kiểm tra bữa ăn có tồn tại và thuộc quyền sở hữu của người dùng hay không.
3. Hệ thống truy vấn thông tin chi tiết của bữa ăn.
4. Hệ thống hiển thị ảnh, tên món ăn, thành phần, khẩu phần, thông tin dinh dưỡng, thời gian ghi nhận và kết quả nhận diện ban đầu nếu có.
5. Hệ thống cung cấp các chức năng sửa và xóa bữa ăn theo quyền được phép.
Luồng thay thế
5a. Người dùng chọn Sửa: hệ thống chuyển sang UC-17.
5b. Người dùng chọn Xóa: hệ thống chuyển sang UC-18.
Luồng ngoại lệ
- 2a. Bữa ăn không tồn tại hoặc không thuộc quyền sở hữu của người dùng: hệ thống không cung cấp dữ liệu và thông báo phù hợp.
Yêu cầu liên quan
FR-35; BR-32, BR-33.




UC-17: Sửa bữa ăn
Mục
Nội dung
Mã Use Case
UC-17
Tên Use Case
Sửa bữa ăn
Actor chính
User
Mô tả
Cho phép người dùng chỉnh sửa thông tin của bữa ăn đã ghi nhận.
Tiền điều kiện
Người dùng đang đăng nhập; bữa ăn tồn tại và thuộc quyền sở hữu của người dùng.
Kích hoạt
Người dùng chọn Sửa tại trang chi tiết bữa ăn.
Hậu điều kiện thành công
Thông tin bữa ăn được cập nhật; dữ liệu dinh dưỡng liên quan được tính toán lại khi cần thiết.
Luồng chính
1. Người dùng mở chi tiết bữa ăn và chọn Sửa.
2. Hệ thống kiểm tra quyền sở hữu và hiển thị thông tin hiện tại.
3. Người dùng chỉnh sửa các trường được phép, chẳng hạn thành phần và khẩu phần.
4. Người dùng chọn Lưu.
5. Hệ thống kiểm tra tính hợp lệ của dữ liệu.
6. Hệ thống tính toán lại thông tin dinh dưỡng nếu nội dung chỉnh sửa ảnh hưởng đến kết quả.
7. Hệ thống cập nhật thông tin bữa ăn.
8. Hệ thống thông báo cập nhật thành công và hiển thị dữ liệu mới.
Luồng thay thế
- 4a. Người dùng chọn Hủy: hệ thống bỏ các thay đổi chưa lưu.
Luồng ngoại lệ
5a. Dữ liệu không hợp lệ: hệ thống thông báo lỗi và yêu cầu chỉnh sửa.
6a. Không thể tính lại dinh dưỡng: hệ thống thông báo lỗi. Việc giữ dữ liệu cũ hay lưu thay đổi ở trạng thái chờ xử lý cần được xác định theo quy tắc nghiệp vụ.
7a. Không thể cập nhật dữ liệu: hệ thống thông báo lỗi và không thông báo thành công khi thay đổi chưa được lưu.
Yêu cầu liên quan
FR-36; BR-26, BR-32, BR-34.

UC-18: Xóa bữa ăn
Mục
Nội dung
Mã Use Case
UC-18
Tên Use Case
Xóa bữa ăn
Actor chính
User
Mô tả
Cho phép người dùng xóa một bữa ăn khỏi lịch sử của mình.
Tiền điều kiện
Người dùng đang đăng nhập; bữa ăn tồn tại và thuộc quyền sở hữu của người dùng.
Kích hoạt
Người dùng chọn Xóa tại trang chi tiết bữa ăn.
Hậu điều kiện thành công
Bữa ăn được đánh dấu xóa theo cơ chế soft delete và không còn xuất hiện trong dữ liệu hiển thị thông thường.
Luồng chính
1. Người dùng chọn Xóa.
2. Hệ thống hiển thị yêu cầu xác nhận.
3. Người dùng xác nhận xóa.
4. Hệ thống kiểm tra quyền sở hữu của bữa ăn.
5. Hệ thống đánh dấu bữa ăn đã bị xóa.
6. Hệ thống cập nhật dữ liệu hiển thị trong lịch sử, dashboard và báo cáo.
7. Hệ thống thông báo xóa thành công.
Luồng thay thế
- 3a. Người dùng hủy thao tác: hệ thống không xóa bữa ăn.
Luồng ngoại lệ
4a. Bữa ăn không tồn tại hoặc không thuộc quyền sở hữu: hệ thống từ chối thao tác.
5a. Không thể cập nhật trạng thái xóa: hệ thống thông báo lỗi.
Yêu cầu liên quan
FR-37; BR-34, BR-35.

MODULE 5 — DASHBOARD VÀ BÁO CÁO DINH DƯỠNG
UC-19: Xem dashboard
Mục
Nội dung
Mã Use Case
UC-19
Tên Use Case
Xem dashboard
Actor chính
User
Mô tả
Cho phép người dùng theo dõi tổng quan tình hình dinh dưỡng trong ngày và xu hướng gần đây.
Tiền điều kiện
Người dùng đang đăng nhập.
Kích hoạt
Người dùng mở trang Dashboard.
Hậu điều kiện
Các chỉ số tổng quan được hiển thị theo dữ liệu hiện có.
Luồng chính
1. Người dùng mở Dashboard.
2. Hệ thống truy vấn dữ liệu bữa ăn của người dùng trong ngày.
3. Hệ thống chỉ tổng hợp các bữa ăn đáp ứng điều kiện được tính vào báo cáo.
4. Hệ thống tổng hợp lượng calo đã nạp, mục tiêu calo, số bữa ăn và các chỉ số dinh dưỡng liên quan.
5. Hệ thống tổng hợp dữ liệu theo ngày để tạo biểu đồ 7 ngày.
6. Hệ thống hiển thị các chỉ số và biểu đồ trên Dashboard.
Luồng thay thế
- 2a. Không có dữ liệu bữa ăn: hệ thống hiển thị dashboard ở trạng thái chưa có dữ liệu và gợi ý người dùng ghi nhận bữa ăn.
Luồng ngoại lệ
- 2b. Không thể truy vấn hoặc tổng hợp dữ liệu: hệ thống thông báo lỗi và cho phép thử lại.
Yêu cầu liên quan
FR-38; BR-28, BR-38, BR-39, BR-40.

UC-20: Xem báo cáo dinh dưỡng
Mục
Nội dung
Mã Use Case
UC-20
Tên Use Case
Xem báo cáo dinh dưỡng
Actor chính
User
Mô tả
Cho phép người dùng xem báo cáo tổng hợp dinh dưỡng trong một khoảng thời gian.
Tiền điều kiện
Người dùng đang đăng nhập.
Kích hoạt
Người dùng mở trang Báo cáo dinh dưỡng.
Hậu điều kiện
Báo cáo được hiển thị theo khoảng thời gian và điều kiện dữ liệu đã chọn.
Luồng chính
1. Người dùng mở trang Báo cáo dinh dưỡng.
2. Hệ thống hiển thị bộ lọc khoảng thời gian.
3. Người dùng chọn khoảng thời gian cần xem.
4. Hệ thống truy vấn các bữa ăn thuộc người dùng và đáp ứng điều kiện đưa vào báo cáo.
5. Hệ thống tổng hợp lượng calo, các chất dinh dưỡng và dữ liệu theo ngày.
6. Hệ thống tính các chỉ số tổng hợp, chẳng hạn lượng calo trung bình mỗi ngày.
7. Hệ thống hiển thị số liệu và biểu đồ báo cáo.
Luồng thay thế
- 7a. Người dùng chọn Xuất báo cáo: hệ thống chuyển sang UC-21.
Luồng ngoại lệ
4a. Không có dữ liệu phù hợp: hệ thống thông báo chưa có dữ liệu trong khoảng thời gian đã chọn.
5a. Không thể tổng hợp dữ liệu: hệ thống thông báo lỗi.
Yêu cầu liên quan
FR-39, FR-40; BR-28, BR-38.

UC-21: Xuất báo cáo
Mục
Nội dung
Mã Use Case
UC-21
Tên Use Case
Xuất báo cáo
Actor chính
User
Mô tả
Cho phép người dùng xuất báo cáo dinh dưỡng thành tệp để tải về.
Tiền điều kiện
Người dùng đang đăng nhập và có thể truy cập dữ liệu báo cáo.
Kích hoạt
Người dùng chọn chức năng Xuất báo cáo.
Hậu điều kiện thành công
Tệp báo cáo được tạo và cung cấp cho người dùng tải về.
Luồng chính
1. Người dùng chọn Xuất báo cáo tại trang báo cáo dinh dưỡng.
2. Hệ thống hiển thị các định dạng xuất được hỗ trợ.
3. Người dùng chọn định dạng PDF hoặc CSV.
4. Hệ thống tạo tệp từ dữ liệu báo cáo tương ứng.
5. Hệ thống cung cấp tệp để người dùng tải về.
Luồng ngoại lệ
4a. Không thể tạo tệp: hệ thống thông báo lỗi.
4b. Không có dữ liệu phù hợp để xuất: hệ thống xử lý theo quy tắc xuất báo cáo đã xác định.
Yêu cầu liên quan
FR-41; BR-41.

MODULE 6 — QUẢN TRỊ HỆ THỐNG
UC-22: Quản lý người dùng
Mục
Nội dung
Mã Use Case
UC-22
Tên Use Case
Quản lý người dùng
Actor chính
Admin
Mô tả
Cho phép Admin xem, tìm kiếm, khóa/mở khóa và thay đổi vai trò người dùng.
Tiền điều kiện
Admin đã đăng nhập và có quyền quản lý người dùng.
Kích hoạt
Admin mở chức năng Quản lý người dùng.
Hậu điều kiện thành công
Thông tin người dùng được cập nhật theo thao tác; các thao tác quản trị được ghi nhận vào log theo quy định.
Luồng chính
1. Admin mở trang Quản lý người dùng.
2. Hệ thống truy vấn và hiển thị danh sách người dùng.
3. Admin tìm kiếm hoặc lọc danh sách.
4. Admin chọn một người dùng.
5. Hệ thống hiển thị thông tin và các thao tác được phép.
6. Admin thực hiện một trong các thao tác: khóa tài khoản, mở khóa tài khoản hoặc thay đổi vai trò.
7. Hệ thống kiểm tra quyền và tính hợp lệ của thao tác.
8. Hệ thống cập nhật dữ liệu người dùng.
9. Hệ thống ghi nhận thao tác quản trị vào log.
10. Hệ thống thông báo kết quả.
Luồng thay thế
- 5a. Admin chỉ xem thông tin: hệ thống hiển thị thông tin người dùng mà không thay đổi dữ liệu.
Luồng ngoại lệ
1a. Người thực hiện không có quyền quản trị: hệ thống từ chối truy cập.
7a. Thao tác không hợp lệ hoặc vi phạm quy tắc bảo vệ tài khoản quản trị: hệ thống từ chối và thông báo lý do.
8a. Không thể cập nhật dữ liệu: hệ thống thông báo lỗi.
9a. Không thể ghi log: hệ thống xử lý theo chính sách ghi nhận sự kiện quản trị đã xác định.
Yêu cầu liên quan
FR-43 đến FR-46; BR-43, BR-44, BR-45, BR-46.

UC-23: Xem log hệ thống
Mục
Nội dung
Mã Use Case
UC-23
Tên Use Case
Xem log hệ thống
Actor chính
Admin
Mô tả
Cho phép Admin xem và lọc các bản ghi log về đăng nhập và những thao tác quan trọng trong hệ thống.
Tiền điều kiện
Admin đã đăng nhập và có quyền xem log hệ thống.
Kích hoạt
Admin mở chức năng Log hệ thống.
Hậu điều kiện
Danh sách log phù hợp được hiển thị; dữ liệu log không bị thay đổi.
Luồng chính
1. Admin mở trang Log hệ thống.
2. Hệ thống hiển thị danh sách log.
3. Admin chọn điều kiện lọc, chẳng hạn người dùng, khoảng thời gian hoặc loại sự kiện.
4. Hệ thống truy vấn các bản ghi phù hợp.
5. Hệ thống hiển thị danh sách log theo điều kiện đã chọn.
6. Admin xem chi tiết các bản ghi được phép truy cập.
Luồng thay thế
- 3a. Admin không chọn bộ lọc: hệ thống hiển thị danh sách theo điều kiện mặc định.
Luồng ngoại lệ
1a. Người dùng không có quyền xem log: hệ thống từ chối truy cập.
4a. Không có bản ghi phù hợp: hệ thống hiển thị thông báo không có dữ liệu.
4b. Không thể truy vấn log: hệ thống thông báo lỗi.
Yêu cầu liên quan
FR-47; BR-44.

UC-24: Xem thống kê tổng quan
Mục
Nội dung
Mã Use Case
UC-24
Tên Use Case
Xem thống kê tổng quan
Actor chính
Admin
Mô tả
Cho phép Admin theo dõi các số liệu tổng quan về hoạt động của hệ thống.
Tiền điều kiện
Admin đã đăng nhập và có quyền xem thống kê.
Kích hoạt
Admin mở trang Thống kê tổng quan.
Hậu điều kiện
Các số liệu thống kê được hiển thị theo khoảng thời gian đã chọn.
Luồng chính
1. Admin mở trang Thống kê tổng quan.
2. Hệ thống hiển thị các chỉ số thống kê và bộ lọc thời gian.
3. Admin chọn khoảng thời gian cần xem.
4. Hệ thống truy vấn dữ liệu liên quan.
5. Hệ thống tổng hợp số lượng người dùng, số bữa ăn theo ngày và số lượt xử lý ảnh bằng AI.
6. Hệ thống tính các chỉ số khác nếu có đủ dữ liệu và quy tắc tính toán đã được xác định.
7. Hệ thống hiển thị các số liệu và biểu đồ thống kê.
Luồng thay thế
- 3a. Admin không chọn khoảng thời gian: hệ thống sử dụng khoảng thời gian mặc định.
Luồng ngoại lệ
4a. Không có dữ liệu trong khoảng thời gian được chọn: hệ thống hiển thị giá trị 0 hoặc trạng thái không có dữ liệu tùy theo loại chỉ số.
4b. Không thể truy vấn hoặc tổng hợp dữ liệu: hệ thống thông báo lỗi.
Yêu cầu liên quan
FR-48; BR-47.









Activity Diagrams - 5UC phức tạp chính
UC-10: ghi nhận bữa ăn bằng hình ảnh


UC-01: Đăng ký tài khoản 

UC-22: quản lí người dùng ( admin)

UC-13: Hiệu chỉnh kết quả AI

UC-14: tính dinh dưỡng bữa ăn

















Sequence Diagrams -3UC
UC-03:Đăng nhập


UC-10: Ghi nhận bữa ăn bằng hình ảnh	


UC-14: tính dinh dưỡng bữa ăn

ERD sơ bộ




Ma trận quan hệ khóa ngoại và liên kết erd hệ thống
STT
Bảng nguồn 
Khóa ngoại
Bảng đích 
Khóa chính đích
Loại quan hệ
Xóa 
Diễn giải nghiệp vụ liên kết
1
HoSoNguoiDung
NguoiDungId
NguoiDung
Id
1 - 1
Cascade
Mỗi người dùng có một hồ sơ thông tin cá nhân, bao gồm chiều cao, cân nặng, chỉ số BMI, BMR và mục tiêu dinh dưỡng.
2
TokenLamMoi
NguoiDungId
NguoiDung
Id
N - 1
Cascade
Một người dùng có thể có nhiều token làm mới để duy trì phiên đăng nhập và xác thực tài khoản.
3
TongHopDinhDuongNgay
NguoiDungId
NguoiDung
Id
N - 1
Cascade
Một người dùng có nhiều bản ghi tổng hợp dinh dưỡng theo từng ngày, bao gồm tổng calo, protein, tinh bột, chất béo và chất xơ.
4
NhatKyHeThong
NguoiDungThucHienId
NguoiDung
Id
N - 1
Chưa xác định
Lưu lại các thao tác của người dùng trong hệ thống để phục vụ kiểm tra, theo dõi và truy vết hoạt động.
5
BuaAn
NguoiDungId
NguoiDung
Id
N - 1
Cascade
Một người dùng có thể tạo và quản lý nhiều bữa ăn, bao gồm thông tin món ăn, thời gian ăn và giá trị dinh dưỡng.
6
ThanhPhanBuaAn
BuaAnId
BuaAn
Id
N - 1
Cascade
Một bữa ăn có thể bao gồm nhiều thành phần thực phẩm với khối lượng và giá trị dinh dưỡng riêng biệt.
7
ThanhPhanBuaAn
ThucPhamThamChieuId
ThucPhamThamChieu
Id
N - 1
Chưa xác định
Mỗi thành phần bữa ăn có thể tham chiếu đến một thực phẩm trong danh mục để xác định thành phần dinh dưỡng.
8
LichSuNhanDienAI
NguoiDungId
NguoiDung
Id
N - 1
Chưa xác định
Một người dùng có thể có nhiều lịch sử nhận diện món ăn bằng AI, bao gồm kết quả dự đoán và độ tin cậy.
9
LichSuNhanDienAI
BuaAnId
BuaAn
Id
N - 1
Chưa xác định
Liên kết kết quả nhận diện AI với bữa ăn tương ứng để lưu lại thông tin nhận diện và chỉnh sửa món ăn.






Mô tả sơ bộ các bảng
1.NguoiDung
Lưu thông tin tài khoản, đăng nhập và xác thực người dùng.
Tên thuộc tính
Mô tả nghiệp vụ
Id
Định danh người dùng
Email
Email đăng nhập
EmailDaXacThuc
Trạng thái xác thực email
MatKhauHash
Mật khẩu đã được mã hóa dạng băm
HoTen
Họ tên người dùng
TrangThai
Trạng thái tài khoản
SoLanDangNhapSai
Số lần đăng nhập sai
ThoiGianHetHanKhoa
Thời điểm hết hạn khóa tài khoản
NgayTao
Ngày tạo tài khoản
NgayCapNhat
Ngày cập nhật gần nhất

2.HoSoNguoiDung
Lưu thông tin cá nhân và các chỉ số sức khỏe của người dùng.
Tên thuộc tính
Mô tả nghiệp vụ
Id
Định danh hồ sơ
NguoiDungId
Người dùng sở hữu hồ sơ
NgaySinh
Ngày sinh
GioiTinh
Giới tính
ChieuCao
Chiều cao (cm)
CanNang
Cân nặng (kg)
MucDoVanDong
Mức độ vận động
ChiSoBMI
Chỉ số khối cơ thể
ChiSoBMR
Năng lượng tiêu hao cơ bản
ChiSoTDEE
Nhu cầu năng lượng hằng ngày
MucTieuCalo
Mục tiêu calo mỗi ngày
DuongDanAnhDaiDien
Đường dẫn ảnh đại diện
NgayCapNhat
Ngày cập nhật hồ sơ

3.TokenLamMoi
Lưu refresh token để duy trì phiên đăng nhập.
Tên thuộc tính
Mô tả nghiệp vụ
Id
Định danh token
NguoiDungId
Người sở hữu token
MaToken
Chuỗi refresh token
ThoiGianHetHan
Thời điểm token hết hạn
DaSuDung
Trạng thái đã sử dụng
DaThuHoi
Trạng thái thu hồi token
NgayTao
Thời điểm tạo token

4. BuaAn
Lưu thông tin và tổng giá trị dinh dưỡng của từng bữa ăn.
Tên thuộc tính
Mô tả nghiệp vụ
Id
Định danh bữa ăn
NguoiDungId
Người ghi nhận bữa ăn
LoaiBuaAn
Bữa sáng, trưa, tối hoặc phụ
NgayAn
Ngày ăn
ThoiGianGhiNhan
Thời điểm ghi nhận bữa ăn
TenMonAn
Tên món ăn
DuongDanHinhAnh
Đường dẫn ảnh món ăn
TongCalo
Tổng năng lượng (kcal)
TongChatDam
Tổng chất đạm (g)
TongTinhBot
Tổng tinh bột (g)
TongChatBeo
Tổng chất béo (g)
TongChatXo
Tổng chất xơ (g)
TaoTuAI
Xác định bữa ăn được tạo từ AI
DaXacNhan
Trạng thái xác nhận bữa ăn
DaXoa
Trạng thái xóa mềm
NgayTao
Ngày tạo
NgayCapNhat
Ngày cập nhật

5.ThanhPhanBuaAn
Lưu các thành phần thực phẩm và giá trị dinh dưỡng tương ứng trong bữa ăn.
Tên thuộc tính
Mô tả nghiệp vụ
Id
Định danh thành phần
BuaAnId
Bữa ăn chứa thành phần
ThucPhamThamChieuId
Thực phẩm tham chiếu tương ứng
TenThanhPhan
Tên thành phần món ăn
KhauPhan
Số lượng khẩu phần
DonViTinh
Đơn vị đo khẩu phần
KhoiLuongQuyDoiGram
Khối lượng quy đổi sang gram
LuongCalo
Năng lượng của thành phần
LuongChatDam
Lượng chất đạm (g)
LuongTinhBot
Lượng tinh bột (g)
LuongChatBeo
Lượng chất béo (g)
LuongChatXo
Lượng chất xơ (g)

6. ThucPhamThamChieu
Lưu dữ liệu thực phẩm chuẩn phục vụ tính toán dinh dưỡng.
Tên thuộc tính
Mô tả nghiệp vụ
Id
Định danh thực phẩm
MaThucPham
Mã thực phẩm
TenThucPham
Tên thực phẩm
NhomThucPham
Nhóm thực phẩm
DonViChuan
Đơn vị tính chuẩn
KhoiLuongChuan
Khối lượng tham chiếu
CaloTren100g
Năng lượng trên 100g
ChatDamTren100g
Chất đạm trên 100g
TinhBotTren100g
Tinh bột trên 100g
ChatBeoTren100g
Chất béo trên 100g
ChatXoTren100g
Chất xơ trên 100g
DangHoatDong
Trạng thái sử dụng thực phẩm

7.TongHopDinhDuongNgay
Lưu tổng lượng dinh dưỡng người dùng tiêu thụ theo ngày.
Tên thuộc tính
Mô tả nghiệp vụ
Id
Định danh bản tổng hợp
NguoiDungId
Người dùng được thống kê
Ngay
Ngày thống kê
TongCalo
Tổng calo trong ngày
TongChatDam
Tổng chất đạm (g)
TongTinhBot
Tổng tinh bột (g)
TongChatBeo
Tổng chất béo (g)
TongChatXo
Tổng chất xơ (g)
SoBuaAnDaGhi
Số bữa ăn đã ghi nhận
MucTieuCaloNgay
Mục tiêu calo trong ngày
NgayCapNhat
Thời điểm cập nhật thống kê

8.LichSuNhanDienAI
Lưu kết quả nhận diện ban đầu của AI và thông tin người dùng chỉnh sửa.
Tên thuộc tính
Mô tả nghiệp vụ
Id
Định danh lịch sử nhận diện
NguoiDungId
Người thực hiện nhận diện
BuaAnId
Bữa ăn liên quan
DuongDanHinhAnh
Đường dẫn ảnh gửi cho AI
PhanHoiAIGoc
Kết quả JSON ban đầu của AI
TenMonAIDoan
Tên món ăn AI dự đoán
TenMonNguoiDungChot
Tên món ăn sau khi xác nhận
DoTinCay
Độ tin cậy của AI
NguoiDungCoSua
Trạng thái chỉnh sửa kết quả AI
ThoiGianXuLyMs
Thời gian AI xử lý (ms)
NgayTao
Thời điểm nhận diện

9.NhatKyHeThong
Lưu lịch sử thao tác quản trị và các hoạt động quan trọng.
Tên thuộc tính
Mô tả nghiệp vụ
Id
Định danh nhật ký
NguoiDungThucHienId
Người thực hiện thao tác
LoaiHanhDong
Loại thao tác được thực hiện
BangTacDong
Bảng dữ liệu bị tác động
IdDoiTuong
Định danh đối tượng bị tác động
ChiTiet
Nội dung thay đổi
DiaChiIP
Địa chỉ IP thực hiện thao tác
NgayTao
Thời điểm thực hiện




