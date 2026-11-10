Database Diagram hệ thống



Mô tả chi tiết từng bảng
1. Bảng NguoiDung
Mô tả: Lưu trữ thông tin tài khoản, thông tin đăng nhập, trạng thái xác thực và các thông tin phục vụ quản lý, bảo mật tài khoản người dùng trong hệ thống.

STT
Tên trường vật lý
Tên trường logic
Mô tả
Kiểu dữ liệu
Maxlength
Nullable
1
Id
ID
Khóa chính, định danh duy nhất người dùng trong hệ thống
uniqueidentifier


Không
2
Email
Email đăng nhập
Địa chỉ email được sử dụng để đăng nhập và xác thực tài khoản
nvarchar
256
Không
3
EmailDaXacThuc
Email đã xác thực
Xác định địa chỉ email của người dùng đã được xác thực hay chưa
bit


Không
4
MatKhauHash
Mật khẩu băm
Lưu giá trị băm của mật khẩu nhằm bảo vệ thông tin đăng nhập
nvarchar
max
Không
5
HoTen
Họ và tên
Họ tên đầy đủ của người dùng
nvarchar
100
Không
6
TrangThai
Trạng thái tài khoản
Xác định trạng thái hoạt động của tài khoản người dùng
nvarchar
30
Không
7
SoLanDangNhapSai
Số lần đăng nhập sai
Ghi nhận số lần người dùng nhập sai thông tin đăng nhập để phục vụ kiểm soát bảo mật
int


Không
8
ThoiGianHetHanKhoa
Thời gian hết hạn khóa
Thời điểm tài khoản được mở khóa sau khi bị khóa tạm thời
datetimeoffset


Có
9
NgayTao
Ngày tạo
Thời điểm tài khoản người dùng được tạo trong hệ thống
datetimeoffset


Không
10
NgayCapNhat
Ngày cập nhật
Thời điểm thông tin tài khoản được cập nhật gần nhất
datetimeoffset


Không


2. Bảng HoSoNguoiDung
Mô tả: Lưu trữ thông tin cá nhân, các chỉ số cơ thể, mức độ vận động và mục tiêu năng lượng của người dùng để phục vụ tính toán, theo dõi dinh dưỡng.

STT
Tên trường vật lý
Tên trường logic
Mô tả
Kiểu dữ liệu
Maxlength
Nullable 
1
Id
ID
Khóa chính, định danh duy nhất hồ sơ người dùng
uniqueidentifier


Không
2
NguoiDungId
ID người dùng
Khóa ngoại liên kết tới NguoiDung.Id, xác định người sở hữu hồ sơ
uniqueidentifier


Không
3
NgaySinh
Ngày sinh
Ngày tháng năm sinh của người dùng, phục vụ tính toán nhu cầu năng lượng
date


Không
4
GioiTinh
Giới tính
Giới tính của người dùng, được sử dụng trong các công thức tính chỉ số dinh dưỡng
nvarchar
10
Không
5
ChieuCao
Chiều cao
Chiều cao cơ thể người dùng, tính bằng centimet
decimal(5,2)


Không
6
CanNang
Cân nặng
Cân nặng hiện tại của người dùng, tính bằng kilogram
decimal(5,2)


Không
7
MucDoVanDong
Mức độ vận động
Phân loại mức độ hoạt động thể chất hằng ngày để tính nhu cầu năng lượng
nvarchar
30
Không
8
ChiSoBMI
Chỉ số BMI
Chỉ số khối cơ thể được tính từ cân nặng và chiều cao
decimal(4,2)


Không
9
ChiSoBMR
Chỉ số BMR
Lượng năng lượng cơ thể cần để duy trì các chức năng sống ở trạng thái nghỉ ngơi
decimal(6,2)


Không
10
ChiSoTDEE
Chỉ số TDEE
Tổng năng lượng cơ thể tiêu hao trong một ngày dựa trên BMR và mức độ vận động
decimal(6,2)


Không
11
MucTieuCalo
Mục tiêu calo
Lượng năng lượng người dùng đặt mục tiêu tiêu thụ mỗi ngày
decimal(6,2)


Không
12
DuongDanAnhDaiDien
Đường dẫn ảnh đại diện
Đường dẫn đến hình ảnh đại diện của người dùng
nvarchar
500
Có
13
NgayCapNhat
Ngày cập nhật
Thời điểm thông tin hồ sơ hoặc các chỉ số sức khỏe được cập nhật gần nhất
datetimeoffset


Không


3. Bảng TokenLamMoi
Mô tả: Lưu trữ các refresh token được cấp cho người dùng nhằm duy trì phiên đăng nhập, cấp lại access token và quản lý trạng thái sử dụng, thu hồi token.

STT
Tên trường vật lý
Tên trường logic
Mô tả
Kiểu dữ liệu
Maxlength
Nullable
1
Id
ID
Khóa chính, định danh duy nhất bản ghi refresh token
uniqueidentifier


Không
2
NguoiDungId
ID người dùng
Khóa ngoại liên kết tới NguoiDung.Id, xác định người sở hữu token
uniqueidentifier


Không
3
MaToken
Mã token
Chuỗi refresh token được sử dụng để yêu cầu cấp lại access token
varchar
500
Không
4
ThoiGianHetHan
Thời gian hết hạn
Thời điểm refresh token hết hiệu lực và không còn được phép sử dụng
datetimeoffset


Không
5
DaSuDung
Đã sử dụng
Xác định refresh token đã được sử dụng để cấp lại token hay chưa
bit


Không
6
DaThuHoi
Đã thu hồi
Xác định refresh token đã bị vô hiệu hóa hoặc thu hồi trước thời hạn hay chưa
bit


Không
7
NgayTao
Ngày tạo
Thời điểm refresh token được tạo và cấp cho người dùng
datetimeoffset


Không


4. Bảng BuaAn
Mô tả: Lưu trữ thông tin các bữa ăn được người dùng ghi nhận, bao gồm loại bữa ăn, thời gian, hình ảnh, tổng giá trị dinh dưỡng và trạng thái xác nhận.

STT
Tên trường vật lý
Tên trường logic
Mô tả
Kiểu dữ liệu
Maxlength
Nullable 
1
Id
ID
Khóa chính, định danh duy nhất bữa ăn
uniqueidentifier


Không
2
NguoiDungId
ID người dùng
Khóa ngoại liên kết tới NguoiDung.Id, xác định người ghi nhận bữa ăn
uniqueidentifier


Không
3
LoaiBuaAn
Loại bữa ăn
Phân loại bữa ăn trong ngày như bữa sáng, bữa trưa, bữa tối hoặc bữa phụ
nvarchar
20
Không
4
NgayAn
Ngày ăn
Ngày người dùng thực hiện bữa ăn, phục vụ thống kê dinh dưỡng theo ngày
date


Không
5
ThoiGianGhiNhan
Thời gian ghi nhận
Thời điểm bữa ăn được ghi nhận vào hệ thống
datetimeoffset


Không
6
TenMonAn
Tên món ăn
Tên món ăn được người dùng nhập hoặc được AI nhận diện
nvarchar
200
Không
7
DuongDanHinhAnh
Đường dẫn hình ảnh
Đường dẫn đến hình ảnh món ăn được người dùng tải lên
nvarchar
500
Có
8
TongCalo
Tổng calo
Tổng năng lượng của toàn bộ bữa ăn, tính bằng kcal
decimal(7,2)


Không
9
TongChatDam
Tổng chất đạm
Tổng lượng protein có trong bữa ăn, tính bằng gram
decimal(6,2)


Không
10
TongTinhBot
Tổng tinh bột
Tổng lượng carbohydrate có trong bữa ăn, tính bằng gram
decimal(6,2)


Không
11
TongChatBeo
Tổng chất béo
Tổng lượng chất béo có trong bữa ăn, tính bằng gram
decimal(6,2)


Không
12
TongChatXo
Tổng chất xơ
Tổng lượng chất xơ có trong bữa ăn, tính bằng gram
decimal(6,2)


Không
13
TaoTuAI
Tạo từ AI
Xác định bữa ăn được tạo thông qua chức năng nhận diện món ăn bằng AI hay được nhập thủ công
bit


Không
14
DaXacNhan
Đã xác nhận
Xác định người dùng đã kiểm tra và xác nhận thông tin bữa ăn hay chưa
bit


Không
15
DaXoa
Đã xóa
Trạng thái xóa mềm, cho phép ẩn bữa ăn mà không xóa dữ liệu vật lý
bit


Không
16
NgayTao
Ngày tạo
Thời điểm bản ghi bữa ăn được tạo trong hệ thống
datetimeoffset


Không
17
NgayCapNhat
Ngày cập nhật
Thời điểm thông tin bữa ăn được cập nhật gần nhất
datetimeoffset


Không


5. Bảng ThanhPhanBuaAn
Mô tả: Lưu trữ danh sách các thành phần thực phẩm trong từng bữa ăn, bao gồm khẩu phần, khối lượng quy đổi và giá trị dinh dưỡng của từng thành phần.

STT
Tên trường vật lý
Tên trường logic
Mô tả
Kiểu dữ liệu
Maxlength
Nullable
1
Id
ID
Khóa chính, định danh duy nhất thành phần bữa ăn
uniqueidentifier


Không
2
BuaAnId
ID bữa ăn
Khóa ngoại liên kết tới BuaAn.Id, xác định bữa ăn chứa thành phần thực phẩm
uniqueidentifier


Không
3
ThucPhamThamChieuId
ID thực phẩm tham chiếu
Khóa ngoại liên kết tới ThucPhamThamChieu.Id, xác định thực phẩm chuẩn tương ứng
uniqueidentifier


Có
4
TenThanhPhan
Tên thành phần
Tên nguyên liệu hoặc thực phẩm cấu thành món ăn
nvarchar
200
Không
5
KhauPhan
Khẩu phần
Số lượng khẩu phần thực phẩm được sử dụng trong bữa ăn
decimal(6,2)


Không
6
DonViTinh
Đơn vị tính
Đơn vị đo lường khẩu phần như phần, bát, đĩa hoặc gram
nvarchar
50
Không
7
KhoiLuongQuyDoiGram
Khối lượng quy đổi
Khối lượng thực phẩm sau khi quy đổi về đơn vị gram
decimal(6,2)


Không
8
LuongCalo
Lượng calo
Năng lượng do thành phần thực phẩm cung cấp, tính bằng kcal
decimal(7,2)


Không
9
LuongChatDam
Lượng chất đạm
Lượng protein của thành phần thực phẩm, tính bằng gram
decimal(6,2)


Không
10
LuongTinhBot
Lượng tinh bột
Lượng carbohydrate của thành phần thực phẩm, tính bằng gram
decimal(6,2)


Không
11
LuongChatBeo
Lượng chất béo
Lượng chất béo của thành phần thực phẩm, tính bằng gram
decimal(6,2)


Không
12
LuongChatXo
Lượng chất xơ
Lượng chất xơ của thành phần thực phẩm, tính bằng gram
decimal(6,2)


Không


6. Bảng ThucPhamThamChieu
Mô tả: Lưu trữ danh mục thực phẩm chuẩn và thành phần dinh dưỡng tương ứng, làm cơ sở để tra cứu, tính toán và đối chiếu giá trị dinh dưỡng của các món ăn.

STT
Tên trường vật lý
Tên trường logic
Mô tả
Kiểu dữ liệu
Maxlength
Nullable
1
Id
ID
Khóa chính, định danh duy nhất thực phẩm tham chiếu
uniqueidentifier


Không
2
MaThucPham
Mã thực phẩm
Mã định danh thực phẩm trong danh mục tham chiếu
varchar
50
Không
3
TenThucPham
Tên thực phẩm
Tên gọi của thực phẩm được sử dụng để tra cứu và tính toán dinh dưỡng
nvarchar
200
Không
4
NhomThucPham
Nhóm thực phẩm
Phân loại thực phẩm theo nhóm như ngũ cốc, thịt, cá, rau củ hoặc trái cây
nvarchar
100
Không
5
DonViChuan
Đơn vị chuẩn
Đơn vị đo lường chuẩn được sử dụng cho thực phẩm
nvarchar
50
Không
6
KhoiLuongChuan
Khối lượng chuẩn
Khối lượng thực phẩm được quy định làm cơ sở tham chiếu
decimal(6,2)


Không
7
CaloTren100g
Calo trên 100g
Lượng năng lượng có trong 100 gram thực phẩm, tính bằng kcal
decimal(6,2)


Không
8
ChatDamTren100g
Chất đạm trên 100g
Lượng protein có trong 100 gram thực phẩm
decimal(6,2)


Không
9
TinhBotTren100g
Tinh bột trên 100g
Lượng carbohydrate có trong 100 gram thực phẩm
decimal(6,2)


Không
10
ChatBeoTren100g
Chất béo trên 100g
Lượng chất béo có trong 100 gram thực phẩm
decimal(6,2)


Không
11
ChatXoTren100g
Chất xơ trên 100g
Lượng chất xơ có trong 100 gram thực phẩm
decimal(6,2)


Không
12
DangHoatDong
Đang hoạt động
Xác định thực phẩm còn được phép sử dụng để tra cứu và tính toán dinh dưỡng hay không
bit


Không


7. Bảng TongHopDinhDuongNgay
Mô tả: Lưu trữ số liệu tổng hợp dinh dưỡng của từng người dùng theo ngày, phục vụ theo dõi lượng năng lượng và các chất dinh dưỡng đã tiêu thụ so với mục tiêu đề ra.

STT
Tên trường vật lý
Tên trường logic
Mô tả
Kiểu dữ liệu
Maxlength
Nullable
1
Id
ID
Khóa chính, định danh duy nhất bản ghi tổng hợp dinh dưỡng
uniqueidentifier


Không
2
NguoiDungId
ID người dùng
Khóa ngoại liên kết tới NguoiDung.Id, xác định người dùng được thống kê dinh dưỡng
uniqueidentifier


Không
3
Ngay
Ngày thống kê
Ngày thực hiện tổng hợp các bữa ăn và giá trị dinh dưỡng của người dùng
date


Không
4
TongCalo
Tổng calo
Tổng năng lượng người dùng đã tiêu thụ trong ngày, tính bằng kcal
decimal(7,2)


Không
5
TongChatDam
Tổng chất đạm
Tổng lượng protein người dùng đã tiêu thụ trong ngày, tính bằng gram
decimal(6,2)


Không
6
TongTinhBot
Tổng tinh bột
Tổng lượng carbohydrate người dùng đã tiêu thụ trong ngày, tính bằng gram
decimal(6,2)


Không
7
TongChatBeo
Tổng chất béo
Tổng lượng chất béo người dùng đã tiêu thụ trong ngày, tính bằng gram
decimal(6,2)


Không
8
TongChatXo
Tổng chất xơ
Tổng lượng chất xơ người dùng đã tiêu thụ trong ngày, tính bằng gram
decimal(6,2)


Không
9
SoBuaAnDaGhi
Số bữa ăn đã ghi
Tổng số bữa ăn được người dùng ghi nhận trong ngày
int


Không
10
MucTieuCaloNgay
Mục tiêu calo ngày
Mức năng lượng mục tiêu của người dùng trong ngày, phục vụ so sánh với lượng calo đã tiêu thụ
decimal(6,2)


Không
11
NgayCapNhat
Ngày cập nhật
Thời điểm dữ liệu tổng hợp dinh dưỡng được tính toán hoặc cập nhật gần nhất
datetimeoffset


Không


8. Bảng LichSuNhanDienAI
Mô tả: Lưu trữ lịch sử nhận diện món ăn bằng trí tuệ nhân tạo, bao gồm hình ảnh đầu vào, kết quả dự đoán ban đầu, độ tin cậy và thông tin người dùng chỉnh sửa sau nhận diện.

STT
Tên trường vật lý
Tên trường logic
Mô tả
Kiểu dữ liệu
Maxlength
Nullable
1
Id
ID
Khóa chính, định danh duy nhất bản ghi lịch sử nhận diện AI
uniqueidentifier


Không
2
NguoiDungId
ID người dùng
Khóa ngoại liên kết tới NguoiDung.Id, xác định người thực hiện nhận diện món ăn
uniqueidentifier


Không
3
BuaAnId
ID bữa ăn
Khóa ngoại liên kết tới BuaAn.Id, xác định bữa ăn được tạo hoặc liên kết từ kết quả nhận diện AI
uniqueidentifier


Có
4
DuongDanHinhAnh
Đường dẫn hình ảnh
Đường dẫn đến hình ảnh món ăn được gửi tới hệ thống AI để nhận diện
nvarchar
500
Không
5
PhanHoiAIGoc
Phản hồi AI gốc
Lưu nội dung phản hồi JSON ban đầu của AI, bao gồm thông tin món ăn và các dữ liệu nhận diện
nvarchar
max
Có
6
TenMonAIDoan
Tên món AI dự đoán
Tên món ăn được mô hình AI dự đoán dựa trên hình ảnh đầu vào
nvarchar
200
Có
7
TenMonNguoiDungChot
Tên món người dùng xác nhận
Tên món ăn cuối cùng sau khi người dùng kiểm tra, chỉnh sửa hoặc xác nhận kết quả AI
nvarchar
200
Có
8
DoTinCay
Độ tin cậy
Giá trị biểu thị mức độ tin cậy của kết quả nhận diện do mô hình AI cung cấp
decimal(4,3)


Có
9
NguoiDungCoSua
Người dùng có sửa
Xác định người dùng có thực hiện chỉnh sửa kết quả nhận diện do AI đề xuất hay không
bit


Không
10
ThoiGianXuLyMs
Thời gian xử lý
Thời gian hệ thống AI thực hiện quá trình nhận diện món ăn, tính bằng mili giây
int


Có
11
NgayTao
Ngày tạo
Thời điểm yêu cầu nhận diện món ăn được ghi nhận trong hệ thống
datetimeoffset


Không


9. Bảng NhatKyHeThong
Mô tả: Lưu trữ lịch sử các thao tác quản trị và hoạt động quan trọng của người dùng, phục vụ theo dõi, kiểm tra, truy vết và hỗ trợ bảo đảm an toàn hệ thống.

STT
Tên trường vật lý
Tên trường logic
Mô tả
Kiểu dữ liệu
Maxlength
Nullable
1
Id
ID
Khóa chính, định danh duy nhất bản ghi nhật ký hệ thống
uniqueidentifier


Không
2
NguoiDungThucHienId
ID người thực hiện
Khóa ngoại liên kết tới NguoiDung.Id, xác định tài khoản thực hiện thao tác
uniqueidentifier


Có
3
LoaiHanhDong
Loại hành động
Xác định loại thao tác được thực hiện như thêm, sửa, xóa hoặc thay đổi trạng thái dữ liệu
varchar
50
Không
4
BangTacDong
Bảng tác động
Tên bảng dữ liệu hoặc đối tượng trong hệ thống chịu ảnh hưởng bởi thao tác
varchar
50
Có
5
IdDoiTuong
ID đối tượng
Định danh bản ghi hoặc đối tượng dữ liệu bị tác động bởi thao tác
varchar
100
Có
6
ChiTiet
Chi tiết thao tác
Nội dung mô tả hoạt động hoặc thông tin thay đổi được ghi nhận trong nhật ký
nvarchar
max
Có
7
DiaChiIP
Địa chỉ IP
Địa chỉ IP của thiết bị hoặc nguồn thực hiện thao tác trong hệ thống
varchar
50
Có
8
NgayTao
Ngày tạo
Thời điểm thao tác được thực hiện và ghi nhận vào nhật ký hệ thống
datetimeoffset


Khôn



































