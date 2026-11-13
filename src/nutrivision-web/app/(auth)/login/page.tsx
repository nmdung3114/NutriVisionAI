import type { Metadata } from "next";
import Link from "next/link";
import { AuthShell } from "@/components/layout/auth-shell";
import { LoginForm } from "@/features/auth/login-form";

export const metadata: Metadata = { title: "Đăng nhập" };

export default function LoginPage() {
  return (
    <AuthShell
      title="Chào mừng trở lại 👋"
      subtitle="Đăng nhập để tiếp tục theo dõi dinh dưỡng của bạn."
      footer={
        <>
          Chưa có tài khoản?{" "}
          <Link href="/register" className="font-semibold text-primary hover:underline">
            Đăng ký miễn phí
          </Link>
        </>
      }
    >
      <LoginForm />
    </AuthShell>
  );
}
