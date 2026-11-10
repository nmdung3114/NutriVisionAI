
Actor trong phạm vi Web

Actor 
Loại
vai trò
user
chính
Người dùng cuối - chụp ảnh , xem báo cáo, quản lí bữa ăn ,...
admin
chính
quản trị user, xem log, thống kê
AI 
phụ 
Hệ thống AI bên ngoài - web gọi Api để nhận diện món ăn
Email
phụ
Dịch vụ gửi Email - web gọi để gửi xác thực, reset mật khẩu
Storage
phụ
Dịch vụ lưu trữ ảnh - web gọi để upload/ lấy ảnh
Hệ thống 
-
không phải actor thực sự ( xử lí tự động nội bộ )












Scope- Phạm vi hệ thống 
Mục tiêu hệ thống 
NutriVisionAI là ứng dụng web hỗ trợ người dùng ghi nhận, quản lý và theo dõi thông tin dinh dưỡng thông qua việc nhận diện món ăn từ hình ảnh, xác nhận kết quả nhận diện, tính toán giá trị dinh dưỡng và cung cấp báo cáo theo thời gian.
Ứng dụng cũng cung cấp các chức năng quản lý tài khoản, hồ sơ cá nhân và quản trị người dùng. Trong phạm vi đề tài, NutriVisionAI đóng vai trò là ứng dụng minh họa cho luồng dữ liệu từ đầu vào, xử lý, lưu trữ đến khai thác dữ liệu.
các chức năng trong phạm vi dự án

stt
nhóm chức năng
Nội dung
1
xác thực và tài khoản
Đăng ký, xác thực email, đăng nhập, đăng xuất, quên mật khẩu, đổi mật khẩu và phân quyền 
2
Hồ sơ cá nhân
Xem, cập nhật thông tin cá nhân; tính toán các chỉ số sức khỏe, cơ thể; thay đổi ảnh đại diện 
3
Ghi nhận bữa ăn
Chọn loại bữa ăn, chụp ảnh hoặc tải ảnh lên, gửi ảnh đến dịch vụ AI và nhập món ăn thủ công 
4
xác nhận kết quả AI
Xem, xác nhận, chỉnh sửa và hoàn tác kết quả nhận diện 
5
Tính toán dinh dưỡng
Tra cứu dữ liệu dinh dưỡng thành phần, tính tổng dinh dưỡng của bữa ăn 
6
Lịch sử bữa ăn
Xem, lọc, xem chi tiết, chỉnh sửa và xóa bữa ăn 
7
Báo cáo và dashboard
Xem dashboard, biểu đồ, báo cáo tổng hợp và xuất báo cáo 
8
Quản trị hệ thống
Quản lý người dùng, khóa/mở tài khoản, thay đổi vai trò, xem log và thống kê tổng quan 



Ngoài phạm vi hệ thống

stt
Nội dung
Giải thích
1
Nghiên cứu và huấn luyện mô hình AI 
Ứng dụng sử dụng dịch vụ AI để nhận diện món ăn; việc nghiên cứu, xây dựng và huấn luyện mô hình không phải trọng tâm của ứng dụng web 
2
Quản trị nền tảng Airflow 
Không thuộc chức năng quản trị của ứng dụng NutriVisionAI 
3
Quản trị nền tảng Superset 
Không thuộc chức năng quản trị của ứng dụng NutriVisionAI 
4
Quản trị hạ tầng của nền tảng dữ liệu 
Không thuộc phạm vi chức năng dành cho người dùng và admin của ứng dụng web 







FR - Functional Requirements : Yêu cầu chức năng  
(các số liệu đang ở giá trị dự kiến chưa phải chốt chính thức)
Module 1 : Xác thực và tài khoản người dùng

Mã
Tên 
Mô tả
Actor 
Đầu vào 
Đầu ra 
Ưu tiên 
Ghi chú
FR-01
Đăng kí tài khoản
tạo tài khoản bằng email + mật khẩu , gửi email xác thực
user
Họ tên, email, mật khẩu, xác nhận, tích điều khoản 
user mới, email đã xác thực
cao
mật khẩu hash , email phải là duy nhất
FR-02
xác thực email
bấm link trong email để kích hoạt tài khoản
user
token trong link
trạng thái hoạt động ok
cao
token dùng 1 lân hết hạn sau 24h
FR-03
đăng nhập
Đăng nhập bằng email+mật khẩu
user
email + mật khẩu 
access token
refresh token
cao 
sai 5 lần khóa 15p
FR-04
đăng xuất 
hủy phiên , xóa token
user
refresh token
token bị thu hồi
cao


FR-05
Quên mật khẩu 
Gửi link đặt lại qua email
user, email
email
link reset gửi qua email
cao
token hết hạn 1h, dùng 1 lần
FR-06
đổi mật khẩu
đổi mật khẩu khi đã đăng nhập
user
mật khẩu cũ, mới, xác nhận
mật khẩu mới lưu vào db
trung bình
hủy session cũ sau khi đổi
FR-07
xem thông tin tài khoản
xem email, trạng thái, thông tin cá nhân,...
user


trang thông tin tài khoản 
trung bình
read-only
FR-08
phân quyền 
phân biệt user/ admin  để giới hạn chức năng
Hệ thống
vai trò + enpoint 
cho phép / từ chối ( 403)
cao
user không vào trang admin



Module 2 : Hồ sơ cá nhân
BMI: Chỉ số khối cơ thể, dựa trên chiều cao và cân nặng.
BMR: Lượng calo cơ thể tiêu hao khi nghỉ ngơi.
TDEE: Tổng lượng calo cơ thể tiêu hao trong một ngày
Mã 
Tên 
Mô tả
Actor
Đầu vào
Đầu ra
Ưu tiên 
Ghi chú
FR-09
Xem hồ sơ
Xem họ tên, giới tính,ngày sinh , chiều cao, cân năng, BMI,BMR,TDEE
user


trang hồ sơ
cao
hiển thị read-only
FR-10`
Cập nhật hồ sơ
Sửa thông tin cá nhân 
user
họ tên, giới tính, ngày sinh, chiều cao, cân nặng,....
hồ sơ cập nhật
cao
chiều cao 50-200cm,cân nặng 10-150kg,tuổi 10-100
FR-11
tính BMI,BMR,TDEE
tự động tính khi hồ sơ thay đổi
hệ thống
chiều cao, cân nặng,....


BMI,BRM,TDEE, nhu cầu calo/ngày
trung bình 
công thức Mifflin–St Jeor 
FR-12
đổi avatar
tải ảnh đại diện mới
user, storage 
file ảnh <= 5MB
url ảnh mới
thấp
định dạng : jpg/png/webp

Module 3 : Ghi nhận bữa ăn		

Mã
Tên
Mô tả
actor
Đầu vào 
Đầu ra 
Ưu tiên 
Ghi chú
FR-13
chọn loại bữa ăn
chọn sáng /trưa/tối/snack trước khi thêm món
user
loại bữa
loại bữa gán cho bản ghi
cao


FR-14
chụp ảnh bữa ăn
Dùng camera thiết bị chụp ảnh trực tiếp trong web
user
quyền camera
file ảnh
cao 


FR-15
tải ảnh bữa ăn
tải ảnh có sẵn từ thiết bị 
user, storage
file ảnh
url ảnh trên storage
cao
<=10Mb,jpg/png/webp/heic
FR-16
kiểm tra hợp lệ ảnh
validate định dạng, dung lượng, kích thước
hệ thống
file ảnh
ảnh hợp lệ, lỗi
cao
tối thiếu 224 x 224 px
FR-17
gửi ảnh cho Ai
gọi Api AI đa phương thức nhận điện món , thành phần, khẩu phần
AI
Url, File ảnh
json kết quả từ AI
cao
timeout 30s, retry 2 lần
FR-18
Hiển thị kết quả Ai
hiển thị món ăn, thành phần, khẩu phần mà AI đề xuất
user
json từ AI
giao diện xem kết quả ảnh gốc
cao 


FR-19
nhập tay món ăn
nhập tay khi mà Ai không nhận diện được
user
tên món , thành phần , khẩu phần
danh sách món do user nhập
cao
autocomplete từ bảng dinh dưỡng
FR-20
hủy ghi nhạn bữa ăn
hủy giữa trừng khi đang thêm
user
thao tác hủy
không lưu gì
trung bình
xóa ảnh đã upload nếu có


Module 4: Xác nhận và hiệu chỉnh kết quả AI

Mã 
tên 
Mô tả
actor
Đầu vào
Đầu ra 
ưu tiên
Ghi chú
FR-21
xác nhận kết quả AI
chấp nhận kết quả từ AI là chính xác
user
thao tác xác nhận
bữa ăn đã được confirmed
cao
bắt buộc trước khi tính dinh dưỡng
FR-22
Hiệu chỉnh tên món
sửa tên món do AI nhận diện sai
user
text mới
tên món được cập nhật
cao


FR-23
thêm thành phần
Thêm thành phần mà AI bỏ sót
user
tên + Khối lượng
danh sách có mục mới 
cao 
autocomplete từ bảng dinh dưỡng
FR-24
xóa thành phần
xóa thành phần AI nhận diện sai
uesr 
thao tác xóa
thành phần bị loại
cao 


FR-25
điểu chỉnh khẩu phần
sửa khối lượng từng thành phần 
user
giá trị số ( gram /ml)
khẩu phần mới 
cao 
không cho <=0
FR-26
Hoàn tác hiệu chỉnh
Quay về kết quả AI gốc 
user
thao tác hoàn tác 
kết quả AI ban đầu
thấp
có hộp thoại popup xác nhận lại
FR-27
lưu bữa ăn
lưu bữa ăn sau khi xác nhận
hệ thống
dữ liệu cuối
được lưu vào db
cao
lưu cả kết quả AI gốc để đánh giá độ chính xác





Module 5: Tính dinh dưỡng

Mã
Tên
Mô tả
Actor
Đầu vào
Đầu ra
Ưu tiên 
Ghi chú
FR-28
Tra cứu dinh dưỡng thành phần 
tra calo , đạm, đường, béo, xơ từ bảng tham chiếu 
hệ thống
tên + khối lượng
Giá trị dinh dưỡng thành phần 
cao 
không tìm thấy - cho chọn gần đúng hoặc nhập tay
FR-29
tính tổng dinh dưỡng bữa ăn 
cộng dồn các thành phần 
hệ thống 
danh sách các thành phần 
tổng calo, đạm,đường, béo, xơ
cao 
chỉ tính khi đã confirmed
FR-30
hiển thị dinh dưỡng
hiển thị tổng dinh dưỡng dạng số + biểu đồ 
user
tổng dinh dưỡng
giao diện hiển thị
cao


FR-31
so sánh với nhu cầu calo/ngày
so calo bữa ăn với TDEE
hệ thống
calo bữa ăn + TDEE
% nhu cầu đã nạp
trung bình












Module 6 : Lịch sử bữa ăn

Mã 
Tên 
Mô tả 
Actor 
Đầu vào 
Đầu ra 
Ưu tiên 
Ghi chú
FR-32
xem danh sách bữa ăn
danh sách bữa ăn theo thời gian giảm dần 
user


danh sách ( ảnh , tên món,calo, thời gian)
cao 
phân trang 10 bữa / tràn
FR-33
lọc theo thời gian 
lọc hôm nay , 3 ngày, 7 ngày,...
user
khoảng thời gian
danh sách đã lọc
cao


FR-34
lọc theo loại bũa ăn
lọc sáng/ trưa/tối / snack
user 
loại bữa
danh sách đã lọc
trung bình


FR-35
xem chi tiết bữa ăn 
xem ảnh gốc , kết quả AI , hiệu chỉnh, dinh dưỡng cuối
user
bữa ăn được chọn
trang chi tiết
cao
chỉ chủ sở hưu xem được
FR-36
sửa bữa ăn đã lưu
sửa thành phần, thêm xóa khẩu phần 
user
thao tác sửa
bữa ăn cập nhật, dinh dưỡng tính lại
trung bình


FR-37
xóa bữa ăn 
xóa khỏi lịch sử
user
thao tác xóa + xác nhạn
bữa ăn bị xóa 
trung bình
soft Delete







Module 7: Báo cáo và Dashboard 

Mã
Tên 
Mô tả 
Actor 
Đầu vào 
Đầu ra 
Ưu tiên 
Ghi chú
FR-38
xem dashboard 
calo hôm nay, số bữa, tỉ lệ đạm, đường, béo so vói mục tiêu
user


dashboard với widget
cao 
chỉ tính những bữa ăn confimed
FR-39
biểu đồ dinh dưỡng theo thời gian 
biểu đồ calo + chất theo ngày/tuần /tháng
user
khoảng thời gian 
biều đồ
cao


FR-40
báo cáo tổng hợp 
calo trug bình/ ngày,tổng chất, số bữa,xu hướng
user
khoảng thời gian 
trang báo cáo 
cao


FR-41
xuất báo cáo
xuất pdf, csv
user
thao tác xuất , dịnh dạng
file PDF/CSV
thấp


FR-42
gới ý món ăn
gợi ý món ăn phù hợp dựa trên lịch sử + mục tiêu
Hệ thống
Lịch sử+ mục tiêu
Danh sách món gợi ý 
thấp









Module 8 : Quản trị hệ thống  (admin)-chỉ dành cho admin của NutriVisionAI không liên quan đến Airflow,Superset.

Mã
tên
Mô tả
Actor
Đầu vào
Đầu ra 
Ưu tiên 
Ghi chú
FR-43
xem danh sách người dùng 
xem toàn bộ user web có phân trang
admin
bộ lọc, tìm kiếm 
danh sách user
cao 
chỉ admin có thể truy cập
FR-44
tìm kiếm người dùng 
tìm theo tên , email, trạng thái
admin 
từ khóa 
danh sách kết quả
trung bình


FR-45 
khóa/ mở tài khoản người dùng
khóa hoặc mở tài khoản user vi phạm
admin 
userId, hành động
status user thay đổi
cao 
ghi log thao tác này
FR-46
đổi vai trò người dùng 
gán quyền user ⇔ admin
admin
userId + role mới
role cập nhật
cao 
ghi log thao tác
FR-47
xem log hệ thống 
log đăng nhập, thao tác quan trọng ( thêm sửa xóa ,..)
admin 
bộ lọc( user, thời gian, loại)
danh sách log
trung bình 
read-only
FR-48
xem thống kê tổng quan 
số user, số bữa ăn/ ngày,số ảnh mà AI xử lí,tỉ lệ mà AI nhận diện đúng ,....
admin 
khoảng tgian 
dashboard thống kê
thấp




NFR - Non-Functional Requirements : Yêu cầu phi chức năng 
Mã
Nhóm
Yêu cầu
Chỉ số
NFR-01
Hiệu năng
Thời gian tải trang
các trang thuộc phạm vi hệ thống tải trong <=2s theo điều kiện được đo thống nhất
NFR-02
Hiệu năng
Thời gian nhận diện AI
thời gian xử lý yêu cầu nhận diện <=30s , cần xác định các xử lí khi vượt ngưỡng
NFR-03
Hiệu năng
Thời gian upload ảnh
Ảnh có dung lượng ≤ 10 MB được tải lên trong ≤ 5 giây theo điều kiện mạng thử nghiệm xác định 
NFR-04
Hiệu năng
Thời gian tính dinh dưỡng 
Tính toán dinh dưỡng cho một bữa ăn hoàn tất trong ≤ 1 giây với quy mô dữ liệu đã xác định 
NFR-05
Bảo mật
Bảo vệ thông tin xác thực 
Mật khẩu không được lưu dưới dạng văn bản thuần; phương pháp bảo vệ được xác định trong thiết kế 
NFR-06
Bảo mật
Bảo vệ dữ liệu truyền tải 
Sử dụng HTTPS cho các kết nối thuộc phạm vi yêu cầu 
NFR-07
Bảo mật
Hạn chế đăng nhập sai 
Sau 5 lần đăng nhập sai liên tiếp, tài khoản bị khóa tạm thời trong 15 phút 
NFR-08
Bảo mật
Xác thực và phân quyền 
Mọi yêu cầu truy cập tài nguyên cần được kiểm tra xác thực và quyền phù hợp 
NFR-09
Bảo mật
Bảo vệ dữ liệu cá nhân
Người dùng chỉ được truy cập dữ liệu thuộc phạm vi quyền của mình 
NFR-10
Bảo mật
Chống lỗ hổng phổ biến 
Hệ thống phải có biện pháp phòng chống SQL Injection, XSS, CSRF và các rủi ro phù hợp với kiến trúc 
NFR-11
Khả dụng
Ngôn ngữ
Tiếng Việt
NFR-12
Khả dụng
Responsive
Desktop + Mobile
NFR-13
Khả dụng
Thông báo lỗi
Thông báo dễ hiểu, chỉ rõ vấn đề và hướng xử lý khi phù hợp 
NFR-14
Độ tin cậy
Retry + fallback khi AI lỗi
Retry 2 lần, cho nhập tay
NFR-15
Mở rộng
Kiến trúc
Kiến trúc hỗ trợ mở rộng chức năng và năng lực xử lý khi nhu cầu tăng 
NFR-16
Tương thích
Trình duyệt
Chrome, Edge, Firefox, Safari
NFR-17
Bảo trì
Quản lý mã nguồn
Mã nguồn có cấu trúc rõ ràng, hỗ trợ kiểm thử, sửa lỗi và phát triển chức năng mới 
NFR-18
Pháp lý
Bảo mật dữ liệu cá nhân
Không chia sẻ bên thứ 3





Business Rules - Quy tắc nghiệp vụ 
Đây chỉ là bản dự thảo chưa phải bảng chính thức, có thể cập nhật trong quá trình phân tích và thiết kế ( các số liệu là giá trị dự kiến)

Mã
Quy tắc
Diễn giải
Ví dụ
BR-01
Không lưu mật khẩu dạng rõ 
Mật khẩu người dùng phải được bảo vệ trước khi lưu trữ; thuật toán cụ thể được xác định trong thiết kế kỹ thuật. 
Abc12345 → 2b12$...
BR-02
email duy nhất
mỗi email chỉ 1 tài khoản
trùng email → báo lỗi
BR-03
chưa xác thực không đăng nhập được
bị chặn
chưa bấm link → không login được
BR-04
Token dùng 1 lần
token xác thực/reset chỉ hiệu lực 1 lần
bấm link lần 2 → "đã sử dụng"
BR-05
token xác thực hết hạn 24h
quá 24h phải gửi lại
2 ngày sau bấm → "hết hạn"
BR-06
Token reset hết hạn 1h
quá 1h phải làm lại
1h30 sau bấm → "hết hạn"
BR-07
khóa sau 5 lần sai
Sai 5 lần → khóa 15 phút
nhập sai 5 lần → tạm khóa
BR-08
Hủy session cũ khi đổi mật khẩu
đổi/reset mật khẩu → hủy hết refresh token
dổi MK → thiết bị khác bị logout
BR-09
mật khẩu mới ≠ mật khẩu cũ
không cho trùng
đổi thành MK cũ → từ chối
BR-10
Mật khẩu ≥ 8 ký tự, có chữ + số
chính sách mật khẩu
abc123 → từ chối
BR-11
không tiết lộ email tồn tại
Quên MK → luôn báo chung
Email sai vẫn báo "đã gửi"
BR-12
usser không vào trang admin
Chặn 403
user gõ /admin → 403
BR-13
adminn không tự khóa mình
Chặn tự khóa/tự hạ quyền
admin tự khóa → bị chặn
BR-14
Chiều cao 50–200cm
Giới hạn hợp lệ
30cm → lỗi
BR-15
Cân nặng 10–150 kg
Giới hạn hợp lệ
5kg → lỗi
BR-16
Tuổi 10–100
Giới hạn hợp lệ
Sinh 2025 → lỗi
BR-17
Cập nhật các chỉ số khi hồ sơ thay đổi 
Khi người dùng thay đổi thông tin có ảnh hưởng đến các chỉ số, hệ thống phải cập nhật các chỉ số tương ứng. 
Đổi cân nặng → BMI đổi
BR-18
Ảnh đại diện ≤ 5MB, jpg/png/webp
Định dạng + dung lượng
File 10MB → lỗi
BR-19
Ảnh bữa ăn ≤ 10MB, ≥ 224×224
Validate trước khi gửi AI
File .pdf → lỗi
BR-20
AI timeout 30s, retry 2 lần
Quá 30s → retry; sau 2 lần → nhập tay
AI lỗi 3 lần → nhập tay
BR-21
Bữa ăn ≥ 1 thành phần
không cho lưu rỗng
Xóa hết TP → không lưu
BR-22
Khẩu phần > 0
không cho âm/bằng 0
-50g → lỗi
BR-23
Tên món không rỗng
Phải có ≥ 1 ký tự
Xóa hết tên → lỗi
BR-24
Cho phép hủy quá trình ghi nhận bữa ăn 
Người dùng có thể hủy quá trình tạo bữa ăn trước khi lưu; dữ liệu chưa được xác nhận lưu không được tạo thành bữa ăn chính thức. 
Người dùng bấm Hủy khi đang chỉnh sửa kết quả AI → không tạo bữa ăn mới. 
BR-25
Hủy bữa ăn thì xóa ảnh đã upload
Tiết kiệm dung lượng
Upload rồi hủy → ảnh bị xóa
BR-26
Chỉ sử dụng bữa ăn đã xác nhận để tổng hợp dinh dưỡng 
Bữa ăn chỉ được đưa vào dữ liệu tổng hợp dinh dưỡng sau khi người dùng xác nhận kết quả. 
Bữa ăn đang chờ xác nhận → chưa được tính vào tổng dinh dưỡng trong báo cáo. 
BR-27
Kết quả AI chỉ là gợi ý
không phải dữ liệu chính thức
Bữa chưa confimed không vào báo cáo
BR-28
Bữa chưa xác nhận không vào báo cáo
Chỉ tính confirmed
2 bữa pending → không tính
BR-29
Chỉ dùng dữ liệu tham chiếu đã chuẩn hóa
không dùng dữ liệu thô
Tra "cơm" từ bảng chuẩn
BR-30
không tra được → cho chọn gần đúng / bỏ qua
không tự bịa giá trị
"Món lạ" → gợi ý tương tự
BR-31
Lưu cả kết quả AI gốc + sau hiệu chỉnh
Để đánh giá độ chính xác AI
AI "phở" → user sửa "phở bò" → lưu cả 2
BR-32
Bữa ăn thuộc 1 user
Có user_id xác định
Bữa #123 thuộc user A
BR-33
không xem bữa ăn người khác
Truy cập trái phép → 403
Sửa URL → 403
BR-34
Chỉ chủ sở hữu sửa/xóa bữa ăn
admin không can thiệp
admin xóa bữa → từ chối
BR-35
Xóa bữa ăn dùng soft delete
không xóa vĩnh viễn
Bữa ẩn khỏi UI, còn trong DB
BR-36
Ảnh bữa ăn lưu ≥ 12 tháng
không tự xóa trong 12 tháng
Sau 12 tháng mới archive
BR-37
Bữa ăn phải có trạng thái hợp lệ 
Mỗi bữa ăn chỉ được mang một trạng thái thuộc tập trạng thái được hệ thống quy định. 
Không chấp nhận trạng thái không thuộc danh sách hợp lệ. 
BR-38
Báo cáo chỉ tính confirmed trong khoảng chọn
Filter theo thời gian + trạng thái
Chọn 7 ngày → chỉ 7 ngày
BR-39
Dashboard mặc định hôm nay
Kèm biểu đồ 7 ngày
Mở dashboard → thấy hôm nay
BR-40
không có dữ liệu → hiển thị gợi ý
không hiển thị lỗi
user mới → "Thêm bữa ăn đầu tiên"
BR-41
Xuất báo cáo chỉ gồm dữ liệu user
không lộ dữ liệu người khác
user A xuất → không thấy user B
BR-42
Gợi ý món dựa trên lịch sử + mục tiêu
không bịa món ngoài dữ liệu
Mục tiêu giảm cân → gợi ý món ít calo
BR-43
Chỉ admin vào trang quản trị
user → 403
user gõ /admin → 403
BR-44
Mọi thao tác admin ghi log
Ai làm, làm gì, khi nào, đối tượng
admin khóa user → có log
BR-45
admin không xem mật khẩu user
Chỉ thấy email, không thấy MK
Mở profile → không có MK
BR-46
admin không xem bữa ăn chi tiết user
Chỉ thấy số liệu tổng hợp
Dashboard admin → không có ảnh
BR-47
Thống kê admin dùng dữ liệu ẩn danh
không hiển thị chi tiết cá nhân
"1.000 user, 5.000 bữa"








Data Requirements – Yêu cầu dữ liệu 
Mục đích
Hệ thống cần thu thập, lưu trữ, xử lý và khai thác dữ liệu phục vụ việc quản lý tài khoản, hồ sơ cá nhân, ghi nhận bữa ăn, tính toán dinh dưỡng, lập báo cáo và quản trị ứng dụng.
Dữ liệu cần đảm bảo tính chính xác, nhất quán, có thể truy xuất theo người dùng và được bảo vệ theo quyền truy cập.
Các nhóm dữ liệu
STT
Nhóm dữ liệu
Dữ liệu chính
Mục đích sử dụng
1
Tài khoản
ID người dùng, email, thông tin xác thực, trạng thái tài khoản, vai trò
Đăng nhập, xác thực, phân quyền và quản lý tài khoản
2
Hồ sơ cá nhân
Họ tên, giới tính, ngày sinh, chiều cao, cân nặng, ảnh đại diện
Quản lý hồ sơ và tính toán các chỉ số cá nhân
3
Chỉ số dinh dưỡng cá nhân
BMI, BMR, TDEE, nhu cầu năng lượng hằng ngày
Theo dõi tình trạng và nhu cầu dinh dưỡng
4
Dữ liệu bữa ăn
ID bữa ăn, người sở hữu, loại bữa, thời gian, trạng thái
Ghi nhận, quản lý và tra cứu lịch sử bữa ăn
5
Dữ liệu hình ảnh
Ảnh đại diện, ảnh bữa ăn, đường dẫn lưu trữ
Hiển thị ảnh và phục vụ quá trình nhận diện
6
Kết quả nhận diện AI
Tên món, thành phần, khẩu phần và kết quả nhận diện ban đầu
Hiển thị kết quả nhận diện và hỗ trợ người dùng hiệu chỉnh
7
Dữ liệu dinh dưỡng tham chiếu
Tên thực phẩm, năng lượng, protein, carbohydrate, chất béo, chất xơ
Tra cứu và tính toán dinh dưỡng
8
Dữ liệu dinh dưỡng bữa ăn
Thành phần, khối lượng, giá trị dinh dưỡng từng thành phần và tổng bữa ăn
Tính toán, thống kê và lập báo cáo
9
Dữ liệu báo cáo
Tổng năng lượng, các chất dinh dưỡng, số bữa và dữ liệu theo thời gian
Hiển thị dashboard, biểu đồ và xuất báo cáo
10
Dữ liệu quản trị
Nhật ký đăng nhập, thao tác quản trị, thay đổi trạng thái và vai trò
Kiểm tra hoạt động và hỗ trợ quản trị hệ thống


AC - Acceptance Criteria-Tiêu chí chấp nhận : điều kiện cụ thể để xác nhận chức năng đáp ứng yêu cầu.


Mã
Bối cảnh
Hành động
kết quả
AC-01
Khách chưa có tài khoản
 Đăng ký với email + mật khẩu hợp lệ
Tạo user gửi email xác thực
AC-02
Email đã tồn tại trong hệ thống
 Đăng ký lại bằng email đó
Báo lỗi "Email đã được đăng ký", không tạo user
AC-03
Mật khẩu nhập abc (quá ngắn)
 Bấm "Đăng ký"
Báo lỗi "Mật khẩu ≥ 8 ký tự, gồm chữ và số"
AC-04
user có token xác thực hợp lệ
 Bấm link trong email
status=ACTIVE, token đánh dấu đã dùng, chuyển về đăng nhập
AC-05
Token xác thực đã dùng 1 lần
 Bấm link lần 2
Báo "Link đã được sử dụng"
AC-06
Token xác thực quá 24h
 Bấm link
Báo "Link đã hết hạn", cho gửi lại
AC-07
user active, đúng mật khẩu
 Đăng nhập
Trả access token + refresh token, vào trang chủ
AC-08
user pending
 Đăng nhập
Báo "Vui lòng xác thực email trước"
AC-09
user nhập sai mật khẩu 5 lần
 Đăng nhập lần 6
Tài khoản bị khóa 15 phút
AC-10
user đang đăng nhập
 Bấm "Đăng xuất"
Refresh token bị thu hồi, về trang đăng nhập
AC-11
user quên mật khẩu, nhập email đã đăng ký
 Bấm "Gửi link"
Email reset gửi đi, token hết hạn 1h
AC-12
user nhập email không tồn tại
 Bấm "Gửi link"
Vẫn báo "Nếu email tồn tại, đã gửi link" (không tiết lộ)
AC-13
user đang đăng nhập, nhập đúng MK cũ + MK mới hợp lệ
 Bấm "Đổi mật khẩu"
MK mới lưu DB, session khác bị hủy
AC-14
user nhập MK mới trùng MK cũ
 Bấm "Đổi mật khẩu"
Báo lỗi "MK mới không được trùng MK cũ"
AC-15
user vai trò user
 Truy cập URL /admin/users
Trả về 403 Forbidden
AC-16
admin đang đăng nhập
 Tự bấm "Khóa tài khoản mình"
Hệ thống chặn, báo "không thể tự khóa"
AC-17
user đang đăng nhập
Mở "Hồ sơ"
Hiển thị đầy đủ thông tin + BMI/BMR/TDEE
AC-18
user cập nhật cân nặng 60→65kg
Bấm "Lưu"
Hồ sơ cập nhật, BMI/BMR/TDEE tính lại
AC-19
user nhập chiều cao 30cm
Bấm "Lưu"
Báo lỗi "Chiều cao phải trong khoảng 50–200 cm"
AC-20
user nhập cân nặng 5kg
Bấm "Lưu"
Báo lỗi "Cân nặng phải trong khoảng 10–150 kg"
AC-21
user nhập ngày sinh 01/01/2025
Bấm "Lưu"
Báo lỗi "Độ tuổi phải từ 10 đến 100"
AC-22
user upload ảnh đại diện 8MB
Bấm "Lưu"
Báo lỗi "Ảnh không được vượt quá 5MB"
AC-23
user đang đăng nhập, có ảnh bữa ăn hợp lệ
Upload ảnh + chọn loại bữa
Ảnh upload lên storage, gọi AI, hiển thị kết quả < 30s
AC-24
user upload file .pdf
Hệ thống validate
Báo lỗi "Định dạng không hỗ trợ"
AC-25
user upload ảnh 20MB
Hệ thống validate
Báo lỗi "Ảnh vượt quá 10MB"
AC-26
AI không phản hồi trong 30s
Hệ thống retry 2 lần vẫn lỗi
Hiển thị "không nhận diện được, vui lòng nhập tay"
AC-27
AI trả về danh sách rỗng
Hiển thị kết quả
Mở form nhập tay cho user
AC-28
user đang xem kết quả AI
Bấm "Hủy"
không lưu gì, ảnh đã upload bị xóa, về trang chủ
AC-29
user đang ở màn hình xem kết quả AI
Bấm "Xác nhận"
Bữa ăn cònirmed, dinh dưỡng được tính, lưu DB
AC-30
user chưa bấm "Xác nhận"
Mở dashboard
Bữa ăn đó không xuất hiện, không có calo
AC-31
user sửa tên món từ "phở" → "phở bò"
Bấm "Lưu"
Tên món cập nhật, lưu cả giá trị AI gốc
AC-32
user xóa hết thành phần
Bấm "Lưu"
Báo lỗi "Phải có ít nhất 1 thành phần"
AC-33
user nhập khẩu phần -100g
Bấm "Lưu"
Báo lỗi "Khẩu phần phải > 0"
AC-34
user xóa hết tên món
Bấm "Lưu"
Báo lỗi "Tên món không được để trống"
AC-35
user bấm "Hoàn tác" sau khi sửa
Xác nhận hoàn tác
Kết quả quay về nguyên bản của AI
AC-36
Bữa ăn confirmed có 3 thành phần
Hệ thống tính
Tổng calo = tổng các thành phần, sai số ±1%
AC-37
Thành phần "món lạ" không có trong bảng
Hệ thống tra cứu
Hiển thị gợi ý chọn gần đúng / nhập tay / bỏ qua
AC-38
Bữa ăn đã tính xong
user xem
Hiển thị calo, đạm, đường, béo, xơ dạng số + biểu đồ tròn
AC-39
Bữa ăn 500 kcal, TDEE user 2000 kcal
Hệ thống so sánh
Hiển thị "Đã nạp 25% nhu cầu/ngày"
AC-40
user có 25 bữa ăn
Mở "Lịch sử"
Hiển thị 20 bữa/trang, sắp xếp mới nhất trước
AC-41
user có bữa ăn từ 1/1 → 31/1
Lọc "1/1 – 15/1"
Chỉ hiển thị bữa trong khoảng đó
AC-42
user chưa có bữa ăn nào
Mở "Lịch sử"
Hiển thị "Chưa có bữa ăn nào" (không lỗi)
AC-43
user A bấm vào bữa ăn của mình
Mở chi tiết
Hiển thị ảnh, thành phần, dinh dưỡng, kết quả AI gốc
AC-44
user A cố truy cập bữa ăn của user B (qua URL)
Hệ thống kiểm tra
Trả 403, không hiển thị dữ liệu
AC-45
user sửa khẩu phần cơm 200g → 150g
Bấm "Lưu"
Dinh dưỡng tính lại, DB cập nhật
AC-46
user bấm "Xóa" bữa ăn
Xác nhận trong hộp thoại
Bữa ăn ẩn khỏi UI (soft delete), dashboard cập nhật
AC-47
user có bữa ăn hôm nay confirmed
Mở dashboard
Hiển thị calo hôm nay, calo mục tiêu, số bữa, tỉ lệ đạm/đường/béo
AC-48
user mới, chưa có bữa ăn
Mở dashboard
Hiển thị 0 calo + gợi ý "Thêm bữa ăn đầu tiên"
AC-49
user có 5 bữa confirmed + 2 bữa pending
Mở dashboard
Chỉ tính 5 bữa confirmed
AC-50
user chọn "30 ngày gần nhất"
Xem biểu đồ
Biểu đồ hiển thị calo theo từng ngày trong 30 ngày
AC-51
user chọn khoảng 7 ngày
Xem báo cáo
Hiển thị calo TB/ngày, tổng chất, số bữa, xu hướng
AC-52
user đang xem báo cáo
Bấm "Xuất PDF"
File PDF tạo trong 5s, chỉ chứa dữ liệu của user
AC-53
user có mục tiêu giảm cân + hay ăn sáng bánh mì
Xem gợi ý món
Gợi ý món ít calo từ bảng tham chiếu (không bịa)
AC-54
admin đăng nhập
Mở "Quản lý người dùng"
Hiển thị danh sách user có phân trang
AC-55
admin đang xem danh sách user
Tìm theo email
Hiển thị kết quả đúng
AC-56
admin bấm "Khóa" một user
Xác nhận
user bị khóa, ghi log thao tác
AC-57
admin đổi vai trò user => admin
Xác nhận
Vai trò cập nhật, ghi log
AC-58
admin mở profile user
Xem thông tin
Chỉ thấy email, không thấy mật khẩu
AC-59
admin mở dashboard user
Xem
Chỉ thấy số liệu tổng hợp, không thấy ảnh bữa ăn
AC-60
admin mở "Log hệ thống"
Xem
Hiển thị log đăng nhập + thao tác quan trọng
AC-61
admin mở "Thống kê tổng quan"
Xem
Hiển thị số user, số bữa, số ảnh AI xử lý — dạng ẩn danh
AC-62
user 
Truy cập /admin/logs
Trả 403 Forbidden















