import { api } from "./client";
import type { AuthResult, UserInfo } from "@/types/api";

export const authApi = {
  register: (b: { hoTen: string; email: string; matKhau: string; xacNhanMatKhau: string; dongYDieuKhoan: boolean }) =>
    api<{ email: string; emailDaGui: boolean }>("/api/auth/register", { method: "POST", body: b, auth: false }),
  verifyEmail: (token: string) =>
    api<{ daXacThucTruocDo: boolean }>("/api/auth/verify-email", { method: "POST", body: { token }, auth: false }),
  resendVerification: (email: string) =>
    api<null>("/api/auth/resend-verification", { method: "POST", body: { email }, auth: false }),
  login: (b: { email: string; matKhau: string }) =>
    api<AuthResult>("/api/auth/login", { method: "POST", body: b, auth: false }),
  logout: (refreshToken: string) => api<null>("/api/auth/logout", { method: "POST", body: { refreshToken } }),
  forgotPassword: (email: string) =>
    api<null>("/api/auth/forgot-password", { method: "POST", body: { email }, auth: false }),
  resetPassword: (b: { token: string; matKhauMoi: string; xacNhanMatKhau: string }) =>
    api<null>("/api/auth/reset-password", { method: "POST", body: b, auth: false }),
  changePassword: (b: { matKhauHienTai: string; matKhauMoi: string; xacNhanMatKhau: string }) =>
    api<AuthResult>("/api/auth/change-password", { method: "POST", body: b }),
  me: () => api<UserInfo>("/api/auth/me"),
};
