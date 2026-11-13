"use client";

import { Suspense } from "react";
import { useSearchParams } from "next/navigation";
import { AuthShell } from "@/components/layout/auth-shell";
import { ResetForm } from "@/features/auth/reset-form";

function Inner() {
  const token = useSearchParams().get("token") ?? "";
  return <ResetForm token={token} />;
}

export default function ResetPasswordPage() {
  return (
    <AuthShell title="Đặt lại mật khẩu" subtitle="Tạo mật khẩu mới cho tài khoản của bạn.">
      <Suspense>
        <Inner />
      </Suspense>
    </AuthShell>
  );
}
