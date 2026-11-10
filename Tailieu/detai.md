Tên đề tài: Xây dựng nền tảng dữ liệu cho hệ thống NutriVisionAI hỗ trợ theo dõi và phân tích dinh dưỡng
Nội dung chính:
1. Mục tiêu của đề tài

Đồ án hướng đến xây dựng một Nền tảng dữ liệu nhằm đơn giản hóa và chuẩn hóa quá trình xây dựng, triển khai và vận hành các pipeline dữ liệu trong môi trường doanh nghiệp. Nền tảng cung cấp các thành phần dùng chung, giúp người phát triển tập trung vào logic xử lý dữ liệu thay vì phải tự chuẩn bị môi trường thực thi, cấu hình kết nối và quyền truy cập cho từng pipeline.

Đồ án đồng thời xây dựng NutriVisionAIlàm ứng dụng minh họa nhằm kiểm chứng hoạt động của nền tảng trên một luồng dữ liệu hoàn chỉnh. Trọng tâm của đề tài là Nền tảng dữ liệu; việc nghiên cứu hoặc huấn luyện mô hình AI không thuộc phạm vi chính.

2. Công cụ và môi trường sử dụng

Các công cụ dưới đây là phương án dự kiến. Ngoại trừ bộ service cốt lõi dbt, Astronomer Cosmos và Apache Airflow, các công nghệ hỗ trợ có thể được điều chỉnh trong quá trình thực hiện dựa trên kết quả thử nghiệm, khả năng tích hợp và chi phí.

• Xây dựng và điều phối pipeline: dbt, Astronomer Cosmos, Apache Airflow.

• Authenticationvà quản lý secrets: dự kiến sử dụng HashiCorp Vault và OIDC.

• Phân tích và trực quan hóa dữ liệu: dự kiến sử dụng Apache Superset.

• Môi trường thực thi:dự kiến sử dụng Docker và Kubernetes.

• Lưu trữ dữ liệu:dự kiến sử dụng cơ sở dữ liệu quan hệ và object storage; service cụ thể sẽ được lựa chọn sau khi khảo sát, triển khai thử nghiệm.

• Triển khai hệ thống: dự kiến sử dụng dịch vụ cloud phù hợp với yêu cầu kỹ thuật và kinh phí.

• Phân tích và thiết kế: Draw.io hoặc công cụ mô hình hóa tương đương.

• Quản lý mã nguồn: Git và nền tảng quản lý Git phù hợp.

3. Nội dung chính của đồ án

3.1. Phân tích và xác định yêu cầu

• Xác định yêu cầu đối với nền tảng hỗ trợ phát triển và vận hành data pipeline.

• Xác định yêu cầu xác thực người dùng cho Airflow và Superset.

• Xác định các chức năng của ứng dụng NutriVisionAI.

• Xác định các pipeline dữ lệu phục vụ hoạt động của NutriVisionAI.

• Xác định yêu cầu về lưu trữ, kiểm tra chất lượng dữ liệu và triển khai hệ thống.

3.2. Thiết kế hệ thống

• Thiết kế kiến trúc tổng thể và các thành phần của Nền tảng dữ liệu.

• Thiết kế cơ chế xây dựng và vận hành pipeline dựa trên dbt, Astronomer Cosmos và Apache Airflow.

• Thiết kế luồng xác thực OIDC cho Airflow và Superset, trong đó Vault đóng vai trò OIDCProvider.

• Thiết kế chức năng, cơ sở dữ liệu và giao diện của NutriVisionAI.

• Thiết kế các pipeline thu thập, xử lý và tổng hợp dữ liệu phục vụ NutriVisionAI.

3.3. Cài đặt các chức năng chính

• Nền tảng hỗ trợ phát triển data pipeline: xây dựng bộ khung và quy trình dùng chung giúp developer đưa một pipeline mới vào hoạt động bằng cách viết logic xử lý trong dbt, khai báo DAG và cấu hình kết nối.Nền tảng cung cấp sẵn cơ chế lập lịch, thực thi, theo dõi, kiểm tra chất lượng, xử lý lỗi và chạy lại pipeline, giúp giảm các bước thiết lập lặp lại và không yêu cầu người phát triển trực tiếp thao tác trên Kubernetes.

• Xác thực cho Airflow và Superset: cấu hình Vault làm OIDC Provider và tích hợp Airflow, Superset với Vault để xác thực người dùng. Các Secret đều được lưu trong Vault.   

• Ứng dụng NutriVisionAI: xây dựng chức năng ghi nhận bữa ăn bằng hình ảnh; sử dụng mô hìnhAI đa phương thức để đề xuất món ăn, thành phần và khẩu phần; cho phép người dùng xác nhận hoặc hiệu chỉnh kết quả trước khi tính toán giá trị dinh dưỡng. Ứng dụng lưu lịch sử bữa ăn, tổng hợp lượng dinh dưỡng theo thời gian và cung cấp báo cáo giúp người dùng theo dõi chế độ ăn uống.

• Các pipeline phục vụ NutriVisionAI: xây dựng pipeline thu thập, làm sạch và chuẩn hóa dữ liệu thực phẩm – dinh dưỡng tham chiếu; pipeline xử lý dữ liệu bữa ăn, kết quả AI và phản hồi của người dùng. Dữ liệu được kiểm tra chất lượng và tổng hợp thành các bảng phục vụ ứng dụng, báo cáo dinh dưỡng, dashboard phân tích và đánh giá mức độ chính xác của kết quả AI.

3.4. Kiểm thử và triển khai

• Kiểm thử việc tạo, lập lịch, theo dõi và chạy lại pipeline.

• Kiểm thử đăng nhập Airflow và Superset thông qua OIDC.

• Kiểm thử các chức năng và luồng dữ liệu của NutriVisionAI.

• Triển khai và chạy thử toàn bộ hệ thống trong môi trường thử nghiệm.

• Đánh giá khả năng hoạt động tích hợp của Nền tảng dữ liệu, các pipeline và ứng dụng minh họa.

.

Cơ sở dữ liệu ban đầu:
Dữ liệu không phải là nội dung/vai trò nghiên cứu trọng tâm của đồ án mà được sử dụng để kiểm thử pipeline và minh họa hoạt động của NutriVisionAI. Hiện tại, đồ án chưa lựa chọn bộ dữ liệu cụ thể.

Dữ liệu dự kiến được thu thập từ các nguồn công khai trên Internet có điều kiện sử dụng phù hợp, gồm:

1. Hình ảnh món ăn: phục vụ thử nghiệm/kiểm thử khả năng đề xuất tên món ăn của mô hình AI có sẵn.

2. Dữ liệu thực phẩm và dinh dưỡng: gồm tên thực phẩm, đơn vị, khối lượng tham chiếu, năng lượng và một số thành phần dinh dưỡng, dùng để tính toán thông tin dinh dưỡng.   

3. Dữ liệu phát sinh từ NutriVisionAI: gồm thông tin bữa ăn, kết quả của AI, nội dung xác nhận hoặc hiệu chỉnh và dữ liệu phục vụ báo cáo.

Nguồn dữ liệu, quy mô và các trường dữ liệu cụ thể sẽ được xác định sau khi khảo sát, triển khai