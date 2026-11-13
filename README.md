# NutriVision AI

Ứng dụng web hỗ trợ ghi nhận bữa ăn bằng hình ảnh (AI đa phương thức), tính toán dinh dưỡng, theo dõi lịch sử, dashboard và báo cáo — ứng dụng minh họa của đồ án *"Xây dựng nền tảng dữ liệu cho hệ thống NutriVisionAI"*.

| Thành phần | Công nghệ |
|---|---|
| Backend | ASP.NET Core 8 Web API, Clean Architecture 4 tầng, EF Core 8, **SQL Server** (SQLite cho môi trường dev), FluentValidation, JWT + refresh token xoay vòng, BCrypt, Serilog, QuestPDF, MailKit |
| Frontend | Next.js 15 (App Router), TypeScript, Tailwind CSS v4, shadcn/ui, TanStack Query, react-hook-form + zod, Recharts |
| AI | Google Gemini (structured JSON output) qua interface `IVisionAiClient`; timeout 30s, retry 2 lần, fallback nhập tay |

## 1. Kiến trúc

```
src/
├── NutriVision.Domain          Entity theo ERD (9 bảng), hằng số nghiệp vụ, quy tắc thuần:
│                               Mifflin–St Jeor (BMI/BMR/TDEE), quy đổi khẩu phần → gram, chuẩn hóa tiếng Việt
├── NutriVision.Application     Use case theo module: Auth, Profile, Foods, Meals, Dashboard, Reports,
│                               Suggestions, Admin, Audit — DTO, validator, interface cổng (DbContext, AI, Storage, Email…)
├── NutriVision.Infrastructure  EF Core (cấu hình + migration SQL Server), JWT/BCrypt, Gemini client,
│                               lưu file cục bộ + URL ký HMAC, email SMTP/outbox, xuất PDF/CSV, seed, job dọn bản nháp
├── NutriVision.Api             Controllers mỏng, middleware lỗi → envelope chuẩn, security headers, rate limit, CORS, Swagger
└── nutrivision-web             Frontend Next.js
tests/
├── NutriVision.UnitTests        Quy tắc domain, so khớp thực phẩm, chính sách mật khẩu, Gemini client (retry/timeout/parse)
└── NutriVision.IntegrationTests Toàn bộ API qua WebApplicationFactory: kiểm tra các AC trong tài liệu phân tích
database/schema.sql              Script SQL Server sinh từ migration (idempotent)
docs/api-contract.md             Hợp đồng API giữa frontend và backend
docs/ai-integration.md           Hợp đồng kết nối dịch vụ AI riêng (Provider=Http) + mock AI server
docs/pipeline-integration.md     Tích hợp pipeline: quyền, quy ước dữ liệu, nạp tăng dần, đánh giá AI, ghi bảng thực phẩm
database/pipeline-access.sql     Tài khoản SQL Server quyền tối thiểu cho pipeline
tools/mock-ai-server/            Mock AI đúng hợp đồng để phát triển/demo
```

Phụ thuộc một chiều: `Api → Infrastructure → Application → Domain`. Tầng Application chỉ biết các interface; mọi dịch vụ ngoài (AI, email, storage) có thể thay thế mà không đổi nghiệp vụ.

## 2. Chạy nhanh

Yêu cầu: .NET SDK 8 (hoặc mới hơn), Node.js ≥ 20.9, **Docker** (để chạy SQL Server). Không có Docker vẫn chạy được bằng SQLite.

### Cài toàn bộ package (một lệnh)

```bash
./setup.sh            # Linux/macOS  (thêm --verify để build + chạy toàn bộ test)
setup.bat             # Windows      (thêm --verify tương tự)
```

Script kiểm tra phiên bản .NET/Node rồi cài: công cụ `dotnet-ef` (manifest `.config/dotnet-tools.json`), toàn bộ NuGet package (`dotnet restore`), npm package theo `package-lock.json` (`npm ci`), và tạo `src/nutrivision-web/.env.local` nếu chưa có.

### Chạy

```bash
./run_all.sh          # Linux/macOS
run_all.bat           # Windows
```
Script tự bật **SQL Server 2022 trong Docker** (`docker compose up -d --wait`), rồi chạy backend `http://localhost:5000` (Swagger `/swagger`) và frontend `http://localhost:3000`. Lần đầu khởi động, migration tự tạo database `NutriVisionDb` và nạp dữ liệu. Máy không có Docker → script tự chuyển sang SQLite (`App_Data/nutrivision-dev.db`).

Chạy từng phần:
```bash
docker compose up -d --wait                                      # SQL Server
cd src/NutriVision.Api && dotnet run --launch-profile http       # backend + SQL Server
#                         dotnet run --launch-profile http-sqlite  ← backend + SQLite (không cần Docker)
cd src/nutrivision-web && npm run dev                            # frontend
```
Dừng SQL Server: `docker compose stop` · xóa sạch dữ liệu: `docker compose down -v`.

Tài khoản demo (chỉ seed ở Development): `user@nutrivision.ai / User@123` (có hồ sơ + ~30 ngày dữ liệu minh họa), `admin@nutrivision.ai / Admin@123`.

### Xem database bằng DBeaver / SSMS / Azure Data Studio

| Mục | Giá trị |
|---|---|
| Loại kết nối | SQL Server |
| Host / Port | `localhost` / `1433` |
| Database | `NutriVisionDb` |
| User / Password | `sa` / `NutriVision@2026` (chỉ dùng cho máy dev) |
| Driver property | `trustServerCertificate = true` |

Email ở môi trường dev **không gửi thật**: nội dung (kèm link xác thực / đặt lại mật khẩu) được ghi vào `src/NutriVision.Api/App_Data/outbox/*.html` và log.

## 3. Cấu hình cho môi trường thật

Mọi khóa bí mật đặt qua biến môi trường (hoặc `dotnet user-secrets`), **không commit vào repo**.

| Mục đích | Biến môi trường |
|---|---|
| SQL Server | `Database__Provider=SqlServer`, `Database__ConnectionString=Server=...;Database=NutriVisionDb;...` (migration tự áp dụng khi khởi động; hoặc chạy `database/schema.sql`) |
| JWT (bắt buộc, ≥ 32 ký tự) | `Jwt__Secret=...` |
| AI Gemini | `Ai__Provider=Gemini`, `Ai__ApiKey=<khóa từ Google AI Studio>`, tùy chọn `Ai__Model=gemini-2.5-flash` |
| AI riêng của nhóm | `Ai__Provider=Http`, `Ai__BaseUrl=http://<dịch-vụ-ai>`, tùy chọn `Ai__ApiKey`, `Ai__Path=/analyze` (xem `docs/ai-integration.md`) |
| Email SMTP | `Email__Provider=Smtp`, `Email__Host`, `Email__Port`, `Email__User`, `Email__Password`, `Email__FromAddress` |
| Link trong email | `App__FrontendUrl=https://...` |
| CORS | `Cors__Origins__0=https://...` |
| Frontend | `NEXT_PUBLIC_API_URL=https://api...` (file `src/nutrivision-web/.env.local`) |

SQL Server cục bộ bằng Docker: `docker compose up -d` (xem `docker-compose.yml`), rồi
`Database__ConnectionString="Server=localhost,1433;Database=NutriVisionDb;User Id=sa;Password=NutriVision@2026;TrustServerCertificate=True"`.

Khi chưa cấu hình AI, chức năng quét ảnh vẫn chạy trọn luồng: ảnh được kiểm tra và lưu, hệ thống báo "chưa cấu hình AI" và chuyển sang nhập tay trên chính bản nháp đó (đúng luồng ngoại lệ UC-10 / AC-26).

## 4. Kiểm thử

```bash
dotnet test NutriVision.sln                      # unit + integration test (SQLite tạm)
NV_TEST_SQLSERVER="Server=localhost,1433;User Id=sa;Password=NutriVision@2026;TrustServerCertificate=True" \
  dotnet test tests/NutriVision.IntegrationTests  # integration test trên SQL Server thật (DB tạm, tự xóa)
cd src/nutrivision-web && npm run lint && npx tsc --noEmit && npm test && npm run build
```

Kiểm thử tích hợp chạy API thật trên SQLite tạm, thay AI và email bằng test double, và phủ các AC chính: đăng ký/xác thực/token dùng 1 lần/hết hạn (AC-01…06), khóa sau 5 lần sai (AC-09), refresh xoay vòng + phát hiện dùng lại, quên/đổi mật khẩu và hủy phiên (AC-11…14), phân quyền 403 và admin không tự khóa (AC-15, 16), hồ sơ và tính lại chỉ số (AC-17…22), quét ảnh → bản nháp → xác nhận → dashboard (AC-23…39), lịch sử/phân trang/lọc/403 bữa ăn người khác/xóa mềm (AC-40…46), dashboard chỉ tính bữa đã xác nhận (AC-47…49), báo cáo và xuất PDF/CSV (AC-50…52), gợi ý món (AC-53), quản trị và nhật ký (AC-54…58).

Ngoài ra đã chạy E2E trên Chrome với backend + frontend thật: đăng ký → xác thực qua link email → đăng nhập → hồ sơ + avatar → quét ảnh (AI chưa cấu hình → nhập tay) → hiệu chỉnh, xác nhận → dashboard, lịch sử, chi tiết, xóa → báo cáo, tải PDF/CSV → admin khóa tài khoản (người dùng bị đẩy ra ngay).

## 5. Giả định và quyết định thiết kế

Tài liệu có một số điểm mâu thuẫn hoặc chưa chốt. Các lựa chọn đã áp dụng:

1. **Phân trang lịch sử 20 bữa/trang** theo AC-40 (FR-32 ghi 10).
2. **Ảnh bữa ăn chấp nhận JPG/PNG/WEBP ≤ 10 MB, ≥ 224×224** (BR-19). Chưa hỗ trợ HEIC vì không kiểm tra được kích thước phía server.
3. **Trạng thái bữa ăn**: bản nháp (`DaXacNhan=false`) → đã xác nhận → đã xóa mềm. Chỉ bữa đã xác nhận mới vào `TongHopDinhDuongNgay`, dashboard và báo cáo (BR-26, BR-28). Bản nháp bỏ dở quá 24 giờ được job nền dọn, xóa cả ảnh (BR-25).
4. **Hồ sơ chưa nhập** (UC-07 3a): bảng `HoSoNguoiDung` có cột NOT NULL theo ERD nên chỉ tạo bản ghi khi người dùng lưu hồ sơ. Trước đó mục tiêu calo mặc định là 2000 kcal, và phải lưu hồ sơ trước khi đổi ảnh đại diện (`PROFILE_REQUIRED`).
5. **Mục tiêu calo** mặc định = TDEE (làm tròn). Người dùng có thể đặt mục tiêu riêng (800–6000 kcal); mục tiêu thấp/cao hơn TDEE được hiểu là giảm/tăng cân khi gợi ý món (FR-42).
6. **Mục tiêu đa lượng chất**: 20% đạm / 50% tinh bột / 30% béo năng lượng; chất xơ 25 g/ngày.
7. **Không tự bịa dữ liệu dinh dưỡng** (BR-30): thành phần AI trả về được tự khớp với bảng tham chiếu khi đủ tin cậy. Không khớp thì người dùng chọn món gần đúng, nhập tay giá trị /100 g, hoặc xóa thành phần; server từ chối lưu nếu còn thiếu (`NUTRITION_DATA_MISSING`).
8. **Token** (xác thực email, đặt lại mật khẩu, refresh) chỉ lưu **hash SHA-256**. Refresh token xoay vòng; dùng lại token cũ ngoài khoảng ân hạn 30 giây thì thu hồi toàn bộ phiên. Khoảng ân hạn tránh đăng xuất nhầm khi tải lại trang giữa chừng.
9. **Ảnh riêng tư**: URL ảnh ký HMAC, hết hạn sau 60 phút, chỉ cấp cho chủ sở hữu. Admin không có API nào trả bữa ăn, ảnh hay hồ sơ người dùng (BR-45…47).
10. **Dữ liệu thực phẩm tham chiếu**: 110 món/nguyên liệu Việt Nam phổ biến, giá trị gần đúng tổng hợp từ Bảng thành phần thực phẩm Việt Nam/USDA, dùng để minh họa. Trong đề tài, bảng này dự kiến được cung cấp bởi pipeline dbt.
11. Phạm vi tài liệu `detai.md` (Airflow, dbt, Vault, Superset) là phần **nền tảng dữ liệu**, nằm ngoài repo ứng dụng web này. Schema đã được thiết kế sẵn để pipeline đọc `BuaAn`, `LichSuNhanDienAI` (AI gốc so với người dùng chốt) và `TongHopDinhDuongNgay`.
12. **Bảng bổ sung**: `NhatKyCanNang`, `NhatKyNuocUong` ngoài ERD gốc, xem `docs/erd-bo-sung.md`.
13. **Nhắc giờ ăn** chạy trên trình duyệt (không có máy chủ push) nên chỉ nhắc khi ứng dụng đang mở; cài đặt lưu theo thiết bị.
14. **CSDL SQLite dev** tự bổ sung bảng mới mà không mất dữ liệu; nếu đổi cột của bảng cũ thì xóa `App_Data/nutrivision-dev.db` để tạo lại.

## 6. Bảo mật đã áp dụng

BCrypt cho mật khẩu; chính sách mật khẩu ≥ 8 ký tự gồm chữ và số; khóa tài khoản 15 phút sau 5 lần sai; không tiết lộ email tồn tại; rate limit các endpoint xác thực; JWT 30 phút và kiểm tra trạng thái/vai trò tài khoản ở mỗi request (khóa có hiệu lực ngay); kiểm tra quyền sở hữu dữ liệu (403); EF Core tham số hóa chống SQL injection; React tự escape và CSP chặt ở frontend chống XSS; dùng Bearer token thay cookie nên không bị CSRF; kiểm tra file bằng nội dung thật (đọc header JPEG/PNG/WEBP theo đặc tả), không tin phần mở rộng; chống path traversal; chống CSV injection; security headers; HSTS ở môi trường production; lỗi trả thông báo thân thiện, không lộ stack trace.

## 7. Package và bảo mật phụ thuộc

- Phiên bản được khóa: `package-lock.json` (npm) và phiên bản cố định trong từng `.csproj` (NuGet). `dotnet list package --vulnerable --include-transitive` và `npm audit` đều sạch tại thời điểm bàn giao.
- Microsoft.* 8.0.34 (bản vá mới nhất của .NET 8 LTS); ghim `SQLitePCLRaw.bundle_e_sqlite3` 2.1.14 và `System.Security.Cryptography.Pkcs` 8.0.2 để vá lỗ hổng gói phụ thuộc gián tiếp.
- Không dùng ImageSharp: mọi bản đã vá lỗi của nó yêu cầu license key thương mại khi build Release. Ứng dụng chỉ cần định dạng + kích thước ảnh nên tự đọc header (`ImageHeaderInspector`), được kiểm thử bằng ảnh thật trong `tests/NutriVision.UnitTests/Fixtures/Images`.
- Frontend: `overrides` trong `package.json` ép `postcss` ≥ 8.5.32 (bản Next 15 đi kèm có lỗ hổng) và `minimatch` ≥ 11.0.1 cho typescript-eslint. shadcn CLI không phải dependency chạy; CSS nền của nó được chép vào `app/shadcn-base.css`. Thêm component mới bằng `npx shadcn@latest add <tên>`.
