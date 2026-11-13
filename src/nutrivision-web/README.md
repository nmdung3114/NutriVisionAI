# NutriVision AI – Web (Next.js)

Giao diện web tiếng Việt cho NutriVision AI. Next.js 15 (App Router) · TypeScript strict · Tailwind CSS v4 · shadcn/ui (Radix) · TanStack Query · react-hook-form + zod · Recharts · framer-motion.

## Chạy

```bash
cp .env.local.example .env.local   # NEXT_PUBLIC_API_URL mặc định http://localhost:5000
npm install
npm run dev                        # http://localhost:3000
```

Backend ASP.NET Core cần chạy tại `http://localhost:5000` (kiểm tra `GET /health`) và cho phép CORS từ `http://localhost:3000`
(và `Access-Control-Expose-Headers: Content-Disposition` để tải PDF/CSV đúng tên file).
Tài khoản dev: `admin@nutrivision.ai / Admin@123`, `user@nutrivision.ai / User@123`.

## Lệnh

| Lệnh | Mục đích |
|---|---|
| `npm run dev` | chạy dev |
| `npm run build` / `npm start` | build / chạy production |
| `npm run lint` | ESLint |
| `npm run typecheck` | `tsc --noEmit` |
| `npm test` | vitest (tính toán dinh dưỡng, mật khẩu, ngày, loại bữa, BMI) |

## Cấu trúc

- `app/` – route mỏng: `(auth)` (khách), `(link)` (verify-email / reset-password), `(app)` (đã đăng nhập; `admin/` yêu cầu ADMIN)
- `features/` – auth, dashboard, meals (scan, editor, history, detail), reports, profile, admin, landing
- `components/ui` (shadcn), `components/shared`, `components/layout`
- `lib/api` – client fetch trung tâm (Bearer, refresh single-flight, bóc envelope, `ApiError`), các module API theo contract
- `lib/` – `nutrition` (quy đổi gram), `password`, `date`, `meal`, `health`, `image`; `providers/` – Auth & Query
- Token: access token trong bộ nhớ, refresh token trong localStorage.

Tài liệu API: `docs/api-contract.md`. Ghi chú/câu hỏi cho backend: `docs/frontend-notes.md`.
