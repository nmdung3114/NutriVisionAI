# Tích hợp dịch vụ AI nhận diện món ăn

Web gọi AI qua interface `IVisionAiClient` (tầng Application). Có 3 chế độ, chọn bằng cấu hình (biến môi trường), **không cần sửa code web**:

| `Ai__Provider` | Dùng khi | Cấu hình cần |
|---|---|---|
| `None` (mặc định) | Chưa có AI → web tự chuyển sang nhập tay | – |
| `Gemini` | Dùng Google Gemini | `Ai__ApiKey`, tùy chọn `Ai__Model` (mặc định `gemini-2.5-flash`) |
| `Http` | **Dịch vụ AI riêng của nhóm** (Python/FastAPI, …) | `Ai__BaseUrl`, tùy chọn `Ai__Path` (mặc định `/analyze`), `Ai__ApiKey` |

Chung cho mọi chế độ: `Ai__TimeoutSeconds` (mặc định 30), `Ai__MaxRetries` (mặc định 2).

## Hợp đồng cho dịch vụ AI riêng (`Provider=Http`)

### Request web gửi đi
```
POST {BaseUrl}{Path}
Content-Type: application/json
Authorization: Bearer {ApiKey}        ← chỉ có khi đặt Ai__ApiKey
```
```json
{ "image": "<ảnh mã hóa base64>", "contentType": "image/jpeg", "ngonNgu": "vi" }
```
- `contentType` ∈ `image/jpeg`, `image/png`, `image/webp`. Web đã kiểm tra ảnh trước: tối đa 10 MB, tối thiểu 224×224 px.
- Body có `Content-Length` (không chunked).

### Response dịch vụ AI phải trả (HTTP 200)
```json
{
  "isFood": true,
  "tenMonAn": "Phở bò tái nạm",
  "doTinCay": 0.91,
  "thanhPhan": [
    { "ten": "Phở bò tái nạm", "gram": 480 },
    { "ten": "Rau sống các loại", "gram": 30 }
  ]
}
```
| Trường | Kiểu | Ý nghĩa |
|---|---|---|
| `isFood` | bool | `false` nếu ảnh không phải đồ ăn/thức uống → web báo và cho nhập tay |
| `tenMonAn` | string | Tên món tiếng Việt, ngắn gọn |
| `doTinCay` | number 0–1 | Độ tin cậy (tùy chọn) |
| `thanhPhan[].ten` | string | Tên thành phần tiếng Việt. **Đặt trùng tên trong bảng `ThucPhamThamChieu` thì web tự khớp dinh dưỡng**; không khớp → người dùng chọn món gần đúng hoặc nhập tay |
| `thanhPhan[].gram` | number > 0 | Khối lượng ước lượng trong ảnh (gram) |

Thành phần có tên rỗng hoặc `gram ≤ 0` bị bỏ qua. Web **không bao giờ tự bịa** giá trị dinh dưỡng từ AI: dinh dưỡng luôn lấy từ bảng tham chiếu.

### Mã lỗi
| Mã | Web xử lý |
|---|---|
| 200 + JSON sai khuôn | Báo "AI trả về kết quả không hợp lệ", thử lại |
| 429, 5xx, lỗi mạng, quá `TimeoutSeconds` | Lỗi tạm thời → **thử lại** tối đa `MaxRetries` lần, rồi chuyển nhập tay |
| 4xx khác (401 sai key, 400 …) | Không thử lại, chuyển nhập tay ngay |

Mọi phản hồi gốc của AI được lưu ở bảng `LichSuNhanDienAI` (`PhanHoiAIGoc`, `TenMonAIDoan`, `DoTinCay`, `ThoiGianXuLyMs`), cạnh kết quả người dùng chốt (`TenMonNguoiDungChot`, `NguoiDungCoSua`), dùng để pipeline đánh giá độ chính xác AI.

## Chạy thử với mock AI (không cần mô hình thật)

`tools/mock-ai-server/server.py` là server mẫu đúng hợp đồng trên (chỉ dùng thư viện chuẩn Python). Nó **không nhận diện thật**, chỉ trả 1 trong 5 kết quả mẫu theo hash ảnh. Đây cũng là code mẫu để nhóm AI thay phần "chọn kết quả mẫu" bằng mô hình thật.

```bash
python3 tools/mock-ai-server/server.py --key thu-nghiem          # http://localhost:8000/analyze
cd src/NutriVision.Api
Ai__Provider=Http Ai__BaseUrl=http://localhost:8000 Ai__ApiKey=thu-nghiem dotnet run --launch-profile http
```
Vào web → *Thêm bữa ăn* → tải ảnh bất kỳ → nhận kết quả AI (mẫu), các thành phần đã tự khớp dinh dưỡng.

Kiểm tra nhanh dịch vụ AI của nhóm bằng curl:
```bash
curl -s http://localhost:8000/analyze -H "Authorization: Bearer thu-nghiem" -H "Content-Type: application/json" \
  -d "{\"image\":\"$(base64 -w0 anh.jpg)\",\"contentType\":\"image/jpeg\",\"ngonNgu\":\"vi\"}"
```
