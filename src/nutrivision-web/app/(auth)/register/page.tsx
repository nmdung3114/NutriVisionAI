import type { Metadata } from "next";
import Link from "next/link";
import { AuthShell } from "@/components/layout/auth-shell";
import { RegisterForm } from "@/features/auth/register-form";

export const metadata: Metadata = { title: "Đăng ký" };

export default function RegisterPage() {
  return (
    <AuthShell
      title="Tạo tài khoản mới"
      subtitle="Bắt đầu hành trình ăn uống khoẻ mạnh cùng NutriVision AI."
      footer={
        <>
          Đã có tài khoản?{" "}
          <Link href="/login" className="font-semibold text-primary hover:underline">
            Đăng nhập
          </Link>
        </>
      }
    >
      <RegisterForm />
    </AuthShell>
  );
}
