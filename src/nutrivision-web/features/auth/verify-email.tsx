"use client";

import Link from "next/link";
import { useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { CheckCircle2, Info, Loader2, XCircle, Clock } from "lucide-react";
import { Button } from "@/components/ui/button";
import { authApi } from "@/lib/api/auth";
import { ApiError, errorMessage } from "@/lib/api/errors";
import { ResendVerificationForm } from "./resend-verification-form";

type State =
  | { kind: "loading" }
  | { kind: "success" }
  | { kind: "already" }
  | { kind: "expired" }
  | { kind: "invalid"; message: string };

export function VerifyEmail({ token }: { token: string }) {
  const router = useRouter();
  const [state, setState] = useState<State>({ kind: token ? "loading" : "invalid" } as State);
  const started = useRef(false);

  useEffect(() => {
    if (!token) {
      setState({ kind: "invalid", message: "Liên kết xác thực thiếu mã token." });
      return;
    }
    if (started.current) return; // tránh POST hai lần (StrictMode)
    started.current = true;
    authApi
      .verifyEmail(token)
      .then((r) => setState({ kind: r?.daXacThucTruocDo ? "already" : "success" }))
      .catch((e) => {
        if (e instanceof ApiError && e.code === "TOKEN_EXPIRED") setState({ kind: "expired" });
        else if (e instanceof ApiError && e.code === "TOKEN_USED") setState({ kind: "already" });
        else setState({ kind: "invalid", message: errorMessage(e) });
      });
  }, [token]);

  useEffect(() => {
    if (state.kind !== "success") return;
    const t = setTimeout(() => router.replace("/login"), 3000);
    return () => clearTimeout(t);
  }, [state, router]);

  const icon = (Icon: typeof Info, cls: string) => (
    <span className={`mx-auto flex size-16 items-center justify-center rounded-2xl ${cls}`}>
      <Icon className="size-8" />
    </span>
  );

  return (
    <div className="space-y-4 text-center" role="status" aria-live="polite">
      {state.kind === "loading" && (
        <>
          {icon(Loader2, "bg-brand-50 text-primary [&_svg]:animate-spin")}
          <p className="text-sm text-muted-foreground">Đang xác thực email của bạn…</p>
        </>
      )}
      {state.kind === "success" && (
        <>
          {icon(CheckCircle2, "bg-brand-50 text-primary")}
          <h2 className="text-xl font-extrabold">Xác thực email thành công!</h2>
          <p className="text-sm text-muted-foreground">Tài khoản đã được kích hoạt. Bạn sẽ được chuyển tới trang đăng nhập sau 3 giây…</p>
          <Button asChild className="w-full">
            <Link href="/login">Đăng nhập ngay</Link>
          </Button>
        </>
      )}
      {state.kind === "already" && (
        <>
          {icon(Info, "bg-blue/15 text-blue")}
          <h2 className="text-xl font-extrabold">Email đã được xác thực trước đó</h2>
          <p className="text-sm text-muted-foreground">Bạn có thể đăng nhập bình thường.</p>
          <Button asChild className="w-full">
            <Link href="/login">Đến trang đăng nhập</Link>
          </Button>
        </>
      )}
      {state.kind === "expired" && (
        <>
          {icon(Clock, "bg-yellow/25 text-[#9a6b00]")}
          <h2 className="text-xl font-extrabold">Liên kết đã hết hạn</h2>
          <p className="text-sm text-muted-foreground">Nhập email để nhận liên kết xác thực mới.</p>
          <div className="rounded-xl border bg-muted/50 p-4 text-left">
            <ResendVerificationForm />
          </div>
        </>
      )}
      {state.kind === "invalid" && (
        <>
          {icon(XCircle, "bg-red-100 text-red-600")}
          <h2 className="text-xl font-extrabold">Liên kết không hợp lệ</h2>
          <p className="text-sm text-muted-foreground">{state.message}</p>
          <div className="rounded-xl border bg-muted/50 p-4 text-left">
            <ResendVerificationForm />
          </div>
          <Link href="/login" className="inline-block text-sm font-semibold text-primary hover:underline">
            Quay lại đăng nhập
          </Link>
        </>
      )}
    </div>
  );
}
