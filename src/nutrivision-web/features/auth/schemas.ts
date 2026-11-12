import { z } from "zod";
import { evaluatePassword } from "@/lib/password";

const email = z.string().trim().min(1, "Vui lòng nhập email").email("Email không hợp lệ").max(254, "Email quá dài");
const password = z
  .string()
  .min(1, "Vui lòng nhập mật khẩu")
  .refine((v) => evaluatePassword(v).valid, "Mật khẩu tối thiểu 8 ký tự, gồm cả chữ và số");

export const loginSchema = z.object({
  email,
  matKhau: z.string().min(1, "Vui lòng nhập mật khẩu"),
});
export type LoginValues = z.infer<typeof loginSchema>;

export const registerSchema = z
  .object({
    hoTen: z.string().trim().min(2, "Họ tên tối thiểu 2 ký tự").max(100, "Họ tên tối đa 100 ký tự"),
    email,
    matKhau: password,
    xacNhanMatKhau: z.string().min(1, "Vui lòng xác nhận mật khẩu"),
    dongYDieuKhoan: z.boolean().refine((v) => v, "Bạn cần đồng ý với điều khoản sử dụng"),
  })
  .refine((v) => v.matKhau === v.xacNhanMatKhau, { path: ["xacNhanMatKhau"], message: "Mật khẩu xác nhận không khớp" });
export type RegisterValues = z.infer<typeof registerSchema>;

export const emailOnlySchema = z.object({ email });
export type EmailOnlyValues = z.infer<typeof emailOnlySchema>;

export const resetSchema = z
  .object({
    matKhauMoi: password,
    xacNhanMatKhau: z.string().min(1, "Vui lòng xác nhận mật khẩu"),
  })
  .refine((v) => v.matKhauMoi === v.xacNhanMatKhau, { path: ["xacNhanMatKhau"], message: "Mật khẩu xác nhận không khớp" });
export type ResetValues = z.infer<typeof resetSchema>;

export const changePasswordSchema = z
  .object({
    matKhauHienTai: z.string().min(1, "Vui lòng nhập mật khẩu hiện tại"),
    matKhauMoi: password,
    xacNhanMatKhau: z.string().min(1, "Vui lòng xác nhận mật khẩu"),
  })
  .refine((v) => v.matKhauMoi === v.xacNhanMatKhau, { path: ["xacNhanMatKhau"], message: "Mật khẩu xác nhận không khớp" })
  .refine((v) => v.matKhauMoi !== v.matKhauHienTai, { path: ["matKhauMoi"], message: "Mật khẩu mới phải khác mật khẩu hiện tại" });
export type ChangePasswordValues = z.infer<typeof changePasswordSchema>;
