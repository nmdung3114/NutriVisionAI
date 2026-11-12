import type { Metadata } from "next";
import Link from "next/link";
import { AuthShell } from "@/components/layout/auth-shell";
import { ForgotForm } from "@/features/auth/forgot-form";

export const metadata: Metadata = { title: "Quên mật khẩu" };

export default function ForgotPasswordPage() {
  return (
    <AuthShell
      title="Quên mật khẩu?"
      subtitle="Nhập email đã đăng ký, chúng tôi sẽ gửi liên kết đặt lại mật khẩu."
      footer={
        <Link href="/login" className="font-semibold text-primary hover:underline">
          ← Quay lại đăng nhập
        </Link>
      }
    >
      <ForgotForm />
    </AuthShell>
  );
}
