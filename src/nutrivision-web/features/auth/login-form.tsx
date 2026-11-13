"use client";

import Link from "next/link";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { AlertCircle } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Field } from "@/components/shared/field";
import { PasswordInput } from "@/components/shared/password-input";
import { useAuth } from "@/providers/auth-provider";
import { ApiError, errorHint, errorMessage } from "@/lib/api/errors";
import { applyFieldErrors } from "@/lib/forms";
import { formatDateTime } from "@/lib/date";
import { loginSchema, type LoginValues } from "./schemas";
import { ResendVerificationForm } from "./resend-verification-form";

export function LoginForm() {
  const { login } = useAuth();
  const [failure, setFailure] = useState<{ code: string; message: string; unlockAt?: string } | null>(null);
  const form = useForm<LoginValues>({ resolver: zodResolver(loginSchema), defaultValues: { email: "", matKhau: "" } });
  const { register, handleSubmit, formState, getValues } = form;

  const onSubmit = handleSubmit(async (v) => {
    setFailure(null);
    try {
      const user = await login(v.email.trim(), v.matKhau);
      toast.success(`Chào mừng ${user.hoTen} quay trở lại!`);
      // GuestGuard sẽ chuyển hướng tới ?next= hoặc /dashboard
    } catch (e) {
      if (applyFieldErrors(form, e)) return;
      if (e instanceof ApiError) {
        const unlock = (e.data as { moKhoaLuc?: string } | null)?.moKhoaLuc;
        setFailure({ code: e.code, message: errorMessage(e), unlockAt: unlock });
      } else setFailure({ code: "UNKNOWN", message: errorMessage(e) });
    }
  });

  return (
    <form onSubmit={onSubmit} className="space-y-4" noValidate>
      {failure && (
        <Alert variant="destructive" role="alert">
          <AlertCircle />
          <AlertTitle>{failure.message}</AlertTitle>
          <AlertDescription>
            {errorHint(failure.code)}
            {failure.unlockAt && <span className="mt-1 block">Mở khóa lúc: {formatDateTime(failure.unlockAt)}</span>}
          </AlertDescription>
        </Alert>
      )}
      <Field label="Email" htmlFor="email" error={formState.errors.email?.message}>
        <Input id="email" type="email" autoComplete="email" placeholder="ban@example.com" aria-invalid={!!formState.errors.email} {...register("email")} />
      </Field>
      <Field label="Mật khẩu" htmlFor="matKhau" error={formState.errors.matKhau?.message}>
        <PasswordInput id="matKhau" autoComplete="current-password" placeholder="Nhập mật khẩu" aria-invalid={!!formState.errors.matKhau} {...register("matKhau")} />
      </Field>
      <div className="flex justify-end">
        <Link href="/forgot-password" className="text-sm font-semibold text-primary hover:underline">
          Quên mật khẩu?
        </Link>
      </div>
      <Button type="submit" size="lg" className="w-full font-bold" disabled={formState.isSubmitting}>
        {formState.isSubmitting ? "Đang đăng nhập…" : "Đăng nhập"}
      </Button>
      {failure?.code === "EMAIL_NOT_VERIFIED" && (
        <div className="rounded-xl border bg-muted/50 p-4">
          <p className="mb-3 text-sm font-semibold">Chưa nhận được email xác thực?</p>
          <ResendVerificationForm defaultEmail={getValues("email")} />
        </div>
      )}
    </form>
  );
}
