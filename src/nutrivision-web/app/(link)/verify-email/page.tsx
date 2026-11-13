"use client";

import { Suspense } from "react";
import { useSearchParams } from "next/navigation";
import { AuthShell } from "@/components/layout/auth-shell";
import { VerifyEmail } from "@/features/auth/verify-email";

function Inner() {
  const token = useSearchParams().get("token") ?? "";
  return <VerifyEmail token={token} />;
}

export default function VerifyEmailPage() {
  return (
    <AuthShell title="Xác thực email">
      <Suspense>
        <Inner />
      </Suspense>
    </AuthShell>
  );
}
