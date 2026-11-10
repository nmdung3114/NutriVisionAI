PHA THU THẬP YÊU CẦU
1. Mục đích
Trong pha thu thập yêu cầu, nhóm tập trung tìm hiểu nhu cầu của những người sẽ sử dụng và quản lý hệ thống NutriVisionAI. Mục đích chính là hiểu người dùng đang muốn giải quyết vấn đề gì, muốn sử dụng hệ thống như thế nào, cần những thông tin gì và có những mong muốn hoặc yêu cầu nào đối với hệ thống.
Ở bước này, nhóm chưa đi sâu vào việc xác định các yêu cầu chức năng FR, yêu cầu phi chức năng NFR hay các quy tắc nghiệp vụ cụ thể. Những nội dung đó sẽ được phân tích và cụ thể hóa ở các bước tiếp theo.
2. Đối tượng tìm hiểu
Trong quá trình thu thập yêu cầu, nhóm tập trung vào hai nhóm đối tượng chính:
Người dùng cuối: người trực tiếp sử dụng NutriVisionAI để ghi nhận và theo dõi các bữa ăn cũng như thông tin dinh dưỡng cá nhân.
Người quản trị: người có nhiệm vụ quản lý tài khoản người dùng và theo dõi một số thông tin tổng quan của hệ thống.
Nhóm cũng tìm hiểu nhu cầu liên quan đến các dịch vụ bên ngoài như dịch vụ AI nhận diện món ăn, dịch vụ gửi email và nơi lưu trữ hình ảnh đây là những thành phần hỗ trợ cho Web chứ không phải đối tượng sử dụng trực tiếp hệ thống.
3. Nội dung thu thập yêu cầu
Nhóm xây dựng các câu hỏi xoay quanh quá trình người dùng sử dụng hệ thống. Các câu hỏi được đặt theo hướng tìm hiểu nhu cầu trước, sau đó từ câu trả lời mới xác định những nội dung hệ thống cần hỗ trợ.
3.1. Nhu cầu chung của người dùng
Câu hỏi
Câu trả lời
Nhu cầu ghi nhận
Khi sử dụng một hệ thống theo dõi dinh dưỡng, người dùng mong muốn hệ thống hỗ trợ những vấn đề gì?
Người dùng mong muốn có một nơi để ghi lại các bữa ăn hằng ngày, biết mình đã ăn những món gì, lượng dinh dưỡng của mỗi bữa và có thể xem lại thông tin trong những ngày trước đó. Ngoài ra, người dùng cũng muốn theo dõi tình trạng dinh dưỡng của bản thân trong một khoảng thời gian thay vì chỉ xem từng bữa ăn riêng lẻ.
Hệ thống cần hỗ trợ việc ghi nhận, quản lý và theo dõi dữ liệu bữa ăn cũng như thông tin dinh dưỡng theo thời gian.
Người dùng mong muốn việc ghi nhận bữa ăn được thực hiện như thế nào để thuận tiện trong quá trình sử dụng?
Người dùng muốn việc ghi nhận bữa ăn đơn giản và nhanh. Nếu có ảnh món ăn thì có thể sử dụng ảnh để nhận diện, nhưng trong trường hợp kết quả nhận diện không chính xác hoặc không muốn sử dụng ảnh thì vẫn cần có cách nhập thông tin bằng tay.
Việc ghi nhận bữa ăn cần hỗ trợ cả hình ảnh và nhập thông tin thủ công.

3.2. Ghi nhận bữa ăn
Câu hỏi
Câu trả lời
Nhu cầu ghi nhận
Khi bắt đầu ghi nhận một bữa ăn, người dùng cần cung cấp những thông tin gì?
Người dùng muốn xác định bữa ăn thuộc loại nào, ví dụ bữa sáng, trưa, tối hoặc bữa phụ. Sau đó có thể cung cấp hình ảnh hoặc nhập thông tin món ăn.
Bữa ăn cần có thông tin về loại bữa và các món ăn được ghi nhận.
Người dùng muốn đưa hình ảnh món ăn vào hệ thống bằng cách nào?
Người dùng muốn có thể chụp ảnh trực tiếp bằng camera của thiết bị hoặc chọn một ảnh đã có sẵn trong thiết bị.
Hệ thống cần hỗ trợ cả chụp ảnh trực tiếp và tải ảnh có sẵn.
Nếu người dùng tải lên một ảnh không phù hợp thì mong muốn hệ thống xử lý như thế nào?
Hệ thống nên kiểm tra ảnh trước khi xử lý. Nếu ảnh sai định dạng, quá lớn hoặc không đạt kích thước tối thiểu thì cần thông báo cho người dùng biết để chọn ảnh khác.
Hệ thống cần kiểm tra tính hợp lệ của ảnh trước khi gửi đến dịch vụ nhận diện.
Sau khi người dùng cung cấp ảnh món ăn, người dùng mong muốn hệ thống hỗ trợ gì?
Người dùng muốn hệ thống nhận diện món ăn trong ảnh và đưa ra thông tin về tên món, thành phần và khẩu phần để người dùng kiểm tra lại.
Hệ thống cần sử dụng dịch vụ nhận diện để đưa ra kết quả ban đầu cho người dùng.
Nếu hệ thống không nhận diện được món ăn hoặc kết quả nhận diện không chính xác thì người dùng muốn xử lý như thế nào?
Người dùng muốn có thể tự nhập món ăn và thông tin liên quan thay vì phải thực hiện lại toàn bộ quá trình. Nếu AI không phản hồi hoặc gặp lỗi thì quá trình ghi nhận vẫn cần có cách tiếp tục.
Người dùng cần có phương án nhập thông tin thủ công khi kết quả AI không sử dụng được.
Nếu đang ghi nhận bữa ăn nhưng người dùng không muốn tiếp tục thì cần xử lý như thế nào?
Người dùng muốn có thể hủy quá trình đang thực hiện và không tạo ra một bữa ăn mới. Nếu đã tải ảnh lên thì ảnh không còn sử dụng cũng nên được xử lý để tránh lưu trữ không cần thiết.
Người dùng cần có khả năng hủy quá trình ghi nhận trước khi lưu chính thức.

3.3. Kiểm tra và chỉnh sửa kết quả nhận diện
Câu hỏi
Câu trả lời
Nhu cầu ghi nhận
Người dùng có muốn kiểm tra lại kết quả do hệ thống nhận diện trước khi lưu bữa ăn không? Vì sao?
Có. Người dùng muốn kiểm tra lại vì kết quả AI có thể không hoàn toàn chính xác. Người dùng cần xác nhận rằng thông tin nhận diện là đúng trước khi hệ thống sử dụng dữ liệu đó để tính toán dinh dưỡng.
Kết quả AI cần được người dùng kiểm tra và xác nhận trước khi trở thành dữ liệu chính thức của bữa ăn.
Nếu AI nhận diện sai tên món ăn thì người dùng muốn làm gì?
Người dùng muốn sửa lại tên món ăn thành thông tin chính xác.
Người dùng cần có khả năng chỉnh sửa tên món sau khi nhận diện.
Nếu AI bỏ sót một thành phần trong món ăn thì người dùng muốn xử lý như thế nào?
Người dùng muốn tự thêm thành phần còn thiếu và nhập khối lượng tương ứng.
Người dùng cần có khả năng thêm thành phần vào kết quả nhận diện.
Nếu AI nhận diện một thành phần không đúng thì người dùng muốn làm gì?
Người dùng muốn xóa thành phần đó khỏi bữa ăn.
Người dùng cần có khả năng xóa thành phần không chính xác.
Người dùng có cần thay đổi khẩu phần của từng thành phần không?
Có. Khẩu phần do AI nhận diện chỉ mang tính tham khảo nên người dùng muốn điều chỉnh lại khối lượng thực tế mình đã ăn.
Người dùng cần có khả năng điều chỉnh khẩu phần trước khi lưu bữa ăn.
Nếu người dùng chỉnh sửa kết quả nhưng sau đó muốn quay lại kết quả ban đầu thì sao?
Người dùng muốn có thể hoàn tác các chỉnh sửa và quay lại kết quả ban đầu do AI đưa ra.
Cần giữ lại kết quả AI ban đầu để người dùng có thể hoàn tác khi cần.
Sau khi người dùng chỉnh sửa xong, thông tin nào cần được lưu lại?
Thông tin cuối cùng sau khi người dùng xác nhận cần được lưu để sử dụng cho việc tính dinh dưỡng và theo dõi lịch sử. Đồng thời, nhóm cũng cần giữ lại kết quả AI ban đầu để có thể so sánh với kết quả sau khi người dùng chỉnh sửa.
Hệ thống cần lưu cả kết quả nhận diện ban đầu và dữ liệu sau khi người dùng hiệu chỉnh.

3.4. Thông tin dinh dưỡng
Câu hỏi
Câu trả lời
Nhu cầu ghi nhận
Sau khi ghi nhận một bữa ăn, người dùng muốn biết những thông tin dinh dưỡng nào?
Người dùng muốn biết lượng calo và một số chất dinh dưỡng chính như protein, carbohydrate/đường, chất béo và chất xơ.
Hệ thống cần cung cấp các thông tin dinh dưỡng cơ bản của từng thành phần và tổng của cả bữa ăn.
Nếu một món ăn không tìm thấy thông tin dinh dưỡng thì hệ thống nên xử lý như thế nào?
Hệ thống không nên tự tạo ra một giá trị dinh dưỡng không có căn cứ. Người dùng có thể chọn một món tương tự trong dữ liệu có sẵn hoặc tự nhập thông tin nếu cần.
Việc tính toán cần dựa trên dữ liệu dinh dưỡng tham chiếu và có phương án xử lý khi không tìm thấy dữ liệu phù hợp.
Người dùng muốn xem tổng dinh dưỡng của bữa ăn như thế nào?
Người dùng muốn xem tổng lượng calo và các chất dinh dưỡng chính dưới dạng dễ hiểu, có thể kết hợp số liệu và biểu đồ.
Thông tin dinh dưỡng cần được tổng hợp và hiển thị trực quan.
Người dùng có muốn biết lượng calo của một bữa ăn chiếm bao nhiêu so với nhu cầu hằng ngày không?
Có. Người dùng muốn biết lượng calo đã nạp so với nhu cầu năng lượng của bản thân để dễ theo dõi việc ăn uống.
Hệ thống cần có thông tin về nhu cầu năng lượng cá nhân và hỗ trợ so sánh với lượng calo đã nạp.

3.5. Thông tin cá nhân
Câu hỏi
Câu trả lời
Nhu cầu ghi nhận
Để theo dõi dinh dưỡng cá nhân, người dùng muốn cung cấp những thông tin nào?
Người dùng cần cung cấp một số thông tin như họ tên, giới tính, ngày sinh, chiều cao và cân nặng. Người dùng cũng muốn có ảnh đại diện để quản lý tài khoản cá nhân.
Hệ thống cần lưu thông tin hồ sơ cá nhân phục vụ việc quản lý và tính toán các chỉ số liên quan.
Khi người dùng thay đổi chiều cao hoặc cân nặng thì mong muốn điều gì xảy ra?
Người dùng mong muốn các chỉ số liên quan được cập nhật theo thông tin mới thay vì phải tự tính toán.
Hệ thống cần tự động cập nhật các chỉ số như BMI, BMR, TDEE và nhu cầu calo khi thông tin đầu vào thay đổi.

3.6. Tài khoản và bảo mật
Câu hỏi
Câu trả lời
Nhu cầu ghi nhận
Người dùng mong muốn quản lý tài khoản của mình như thế nào?
Người dùng muốn có thể đăng ký tài khoản, xác thực email, đăng nhập, đăng xuất, đổi mật khẩu và lấy lại mật khẩu khi quên.
Hệ thống cần hỗ trợ các nhu cầu cơ bản trong quá trình quản lý tài khoản.
Người dùng có cần xác thực email trước khi sử dụng tài khoản không?
Có. Việc xác thực email giúp đảm bảo tài khoản sử dụng một địa chỉ email hợp lệ. Người dùng chưa xác thực email không nên được sử dụng đầy đủ các chức năng của hệ thống.
Tài khoản cần có trạng thái xác thực và có quy trình xác thực thông qua email.
Nếu người dùng nhập sai mật khẩu nhiều lần thì mong muốn hệ thống xử lý như thế nào?
Để tránh việc thử mật khẩu liên tục, tài khoản nên bị khóa tạm thời sau một số lần đăng nhập sai liên tiếp.
Hệ thống cần có cơ chế hạn chế đăng nhập sai và khóa tạm thời tài khoản.
Thông tin về bữa ăn và hồ sơ cá nhân có cần được bảo vệ không?
Có. Người dùng chỉ muốn xem và chỉnh sửa dữ liệu của chính mình. Người khác không được tự ý truy cập vào thông tin cá nhân hoặc lịch sử bữa ăn.
Hệ thống cần kiểm soát quyền truy cập và bảo vệ dữ liệu cá nhân của người dùng.

3.7. Xem lại lịch sử bữa ăn
Câu hỏi
Câu trả lời
Nhu cầu ghi nhận
Người dùng có nhu cầu xem lại những bữa ăn đã ghi nhận trước đây không?
Có. Người dùng muốn xem lại các bữa ăn theo thời gian để biết mình đã ăn gì và lượng dinh dưỡng của từng bữa.
Hệ thống cần lưu lịch sử bữa ăn và cho phép người dùng xem lại.
Khi có nhiều bữa ăn, người dùng muốn tìm kiếm thông tin như thế nào?
Người dùng muốn lọc theo khoảng thời gian và loại bữa như sáng, trưa, tối hoặc bữa phụ để dễ tìm kiếm.
Lịch sử bữa ăn cần hỗ trợ lọc theo thời gian và loại bữa.
Người dùng có muốn chỉnh sửa hoặc xóa một bữa ăn đã lưu không?
Có. Nếu thông tin đã lưu bị sai, người dùng muốn sửa lại. Nếu không còn muốn lưu thì có thể xóa bữa ăn.
Người dùng cần có khả năng chỉnh sửa và xóa dữ liệu bữa ăn của chính mình.

3.8. Theo dõi và báo cáo
Câu hỏi
Câu trả lời
Nhu cầu ghi nhận
Người dùng muốn theo dõi những thông tin nào để biết tình trạng dinh dưỡng của mình?
Người dùng muốn biết lượng calo đã ăn trong ngày, số bữa ăn, lượng protein, đường và chất béo, đồng thời muốn so sánh với mục tiêu hoặc nhu cầu của bản thân.
Hệ thống cần tổng hợp dữ liệu bữa ăn thành các thông tin dễ theo dõi.
Người dùng muốn theo dõi dữ liệu dinh dưỡng trong khoảng thời gian như thế nào?
Người dùng muốn xem theo ngày, tuần, tháng hoặc một khoảng thời gian nhất định để theo dõi xu hướng ăn uống.
Hệ thống cần hỗ trợ tổng hợp dữ liệu theo nhiều khoảng thời gian.
Người dùng có nhu cầu xuất thông tin đã tổng hợp ra ngoài hệ thống không?
Có. Người dùng muốn có thể xuất báo cáo để lưu lại hoặc sử dụng khi cần, chẳng hạn dưới dạng PDF hoặc CSV.
Hệ thống cần hỗ trợ xuất dữ liệu báo cáo theo định dạng phù hợp.
Nếu người dùng chưa có dữ liệu bữa ăn thì dashboard hoặc báo cáo nên hiển thị như thế nào?
Không nên hiển thị lỗi. Hệ thống nên thông báo rằng chưa có dữ liệu và hướng dẫn người dùng thực hiện việc ghi nhận bữa ăn đầu tiên.
Hệ thống cần có cách xử lý phù hợp đối với trường hợp người dùng chưa có dữ liệu.

3.9. Nhu cầu của người quản trị
Câu hỏi
Câu trả lời
Nhu cầu ghi nhận
Người quản trị cần theo dõi và quản lý những thông tin gì trên hệ thống?
Người quản trị cần xem danh sách tài khoản người dùng, tìm kiếm người dùng và quản lý trạng thái tài khoản. Ngoài ra, quản trị viên cần theo dõi một số hoạt động quan trọng của hệ thống.
Hệ thống cần có khu vực dành riêng cho quản trị viên để quản lý người dùng và theo dõi hoạt động.
Trong trường hợp một tài khoản có vấn đề hoặc vi phạm, người quản trị muốn xử lý như thế nào?
Người quản trị muốn có thể khóa hoặc mở lại tài khoản khi cần. Các thao tác này cần được ghi nhận để có thể kiểm tra lại.
Cần có cơ chế quản lý trạng thái tài khoản và ghi nhận thao tác quản trị.
Người quản trị có cần thay đổi quyền của người dùng không?
Có. Trong một số trường hợp cần thiết, quản trị viên có thể thay đổi vai trò của tài khoản. Tuy nhiên, các thao tác liên quan đến quyền cần được kiểm soát và ghi log.
Hệ thống cần hỗ trợ quản lý vai trò và quyền truy cập của tài khoản.
Người quản trị cần xem những loại thông tin nào trong log?
Người quản trị muốn theo dõi các lần đăng nhập và những thao tác quan trọng như thay đổi trạng thái tài khoản hoặc thay đổi quyền.
Hệ thống cần lưu lại các hoạt động quan trọng để phục vụ việc kiểm tra và quản trị.
Người quản trị có cần xem chi tiết bữa ăn và thông tin cá nhân của từng người dùng không?
Không cần thiết. Người quản trị chủ yếu cần các số liệu tổng quan phục vụ quản lý hệ thống, không nên xem các thông tin cá nhân hoặc hình ảnh bữa ăn chi tiết của người dùng nếu không cần thiết.
Dữ liệu quản trị nên được giới hạn ở mức cần thiết và ưu tiên sử dụng số liệu tổng hợp, ẩn danh.

3.10. Các yêu cầu chung về sử dụng hệ thống
Câu hỏi
Câu trả lời
Nhu cầu ghi nhận
Người dùng mong muốn hệ thống có tốc độ phản hồi như thế nào?
Người dùng mong muốn các trang và thao tác thông thường phản hồi nhanh. Đối với nhận diện món ăn bằng AI thì có thể mất nhiều thời gian hơn, nhưng hệ thống cần thông báo rõ nếu quá trình xử lý gặp vấn đề.
Hệ thống cần đảm bảo thời gian phản hồi phù hợp và có cách xử lý khi dịch vụ AI không phản hồi.
Người dùng thường sử dụng hệ thống trên thiết bị nào?
Người dùng có thể sử dụng trên máy tính hoặc điện thoại nên giao diện cần dễ sử dụng trên cả hai loại thiết bị.
Giao diện cần phù hợp với cả desktop và mobile.
Khi xảy ra lỗi, người dùng mong muốn hệ thống thông báo như thế nào?
Thông báo cần dễ hiểu và cho người dùng biết vấn đề nằm ở đâu hoặc cần thực hiện thao tác gì tiếp theo, thay vì chỉ hiển thị lỗi kỹ thuật.
Hệ thống cần có thông báo lỗi rõ ràng và dễ hiểu.


4. Tổng hợp kết quả thu thập
Sau khi tổng hợp các câu trả lời, nhóm nhận thấy nhu cầu của hệ thống tập trung chủ yếu vào việc giúp người dùng ghi nhận bữa ăn, kiểm tra thông tin món ăn, biết giá trị dinh dưỡng và theo dõi dữ liệu theo thời gian.
Đối với quá trình ghi nhận bữa ăn, người dùng mong muốn có thể sử dụng hình ảnh để hỗ trợ nhận diện món ăn nhưng vẫn phải có quyền kiểm tra và chỉnh sửa kết quả. Kết quả AI chỉ được xem là thông tin ban đầu, người dùng cần xác nhận trước khi dữ liệu được sử dụng chính thức cho việc tính toán và báo cáo.
Bên cạnh đó, người dùng muốn quản lý hồ sơ cá nhân và sử dụng các thông tin như chiều cao, cân nặng, tuổi để hỗ trợ theo dõi nhu cầu dinh dưỡng. Người dùng cũng cần xem lại lịch sử, lọc dữ liệu và xem các báo cáo tổng hợp để theo dõi tình trạng ăn uống theo thời gian.
Đối với quản trị viên, nhu cầu chủ yếu là quản lý tài khoản, vai trò, trạng thái người dùng và theo dõi các hoạt động quan trọng của hệ thống. Các thông tin cá nhân và dữ liệu bữa ăn của người dùng cần được giới hạn quyền truy cập.
Các nhu cầu trên là cơ sở để nhóm chuyển sang bước phân tích. Từ đây, nhóm tiến hành xác định các nhóm chức năng, Actor, Use Case, yêu cầu chức năng, yêu cầu phi chức năng, quy tắc nghiệp vụ, yêu cầu dữ liệu và tiêu chí chấp nhận. Những nội dung FR, NFR, BR, Data Requirements và AC được xây dựng ở bước sau nhằm cụ thể hóa các nhu cầu đã thu thập.

5. Xác nhận kết quả
Nhóm đặc biệt kiểm tra lại một số nội dung quan trọng:
Người dùng có thể ghi nhận bữa ăn bằng hình ảnh hoặc nhập thủ công.
Kết quả nhận diện AI cần được người dùng kiểm tra và xác nhận.
Người dùng có thể chỉnh sửa tên món, thành phần và khẩu phần.
Dữ liệu dinh dưỡng chỉ được sử dụng chính thức sau khi bữa ăn được xác nhận.
Người dùng có thể xem lại, chỉnh sửa và xóa lịch sử bữa ăn của mình.
Người dùng có thể xem các thông tin tổng hợp và báo cáo theo thời gian.
Dữ liệu cá nhân và dữ liệu bữa ăn cần được bảo vệ.
Quản trị viên có quyền quản lý tài khoản và theo dõi hoạt động quản trị nhưng không được tùy ý xem dữ liệu cá nhân chi tiết của người dùng.
Những nội dung này được sử dụng làm cơ sở để nhóm tiếp tục phân tích và đặc tả yêu cầu của hệ thống.

