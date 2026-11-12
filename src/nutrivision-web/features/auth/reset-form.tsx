"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { AlertCircle } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Field } from "@/components/shared/field";
import { PasswordInput } from "@/components/shared/password-input";
import { PasswordStrength } from "@/components/shared/password-strength";
import { authApi } from "@/lib/api/auth";
import { ApiError, errorHint, errorMessage } from "@/lib/api/errors";
import { applyFieldErrors } from "@/lib/forms";
import { resetSchema, type ResetValues } from "./schemas";

export function ResetForm({ token }: { token: string }) {
  const router = useRouter();
  const [tokenError, setTokenError] = useState<ApiError | null>(null);
  const form = useForm<ResetValues>({ resolver: zodResolver(resetSchema), defaultValues: { matKhauMoi: "", xacNhanMatKhau: "" } });
  const { register, handleSubmit, formState, watch } = form;

  const onSubmit = handleSubmit(async (v) => {
    setTokenError(null);
    try {
      await authApi.resetPassword({ token, ...v });
      toast.success("Đặt lại mật khẩu thành công. Hãy đăng nhập bằng mật khẩu mới.");
      router.replace("/login");
    } catch (e) {
      if (applyFieldErrors(form, e)) return;
      if (e instanceof ApiError && ["TOKEN_INVALID", "TOKEN_EXPIRED", "TOKEN_USED"].includes(e.code)) setTokenError(e);
      else toast.error(errorMessage(e));
    }
  });

  if (!token || tokenError)
    return (
      <div className="space-y-4">
        <Alert variant="destructive">
          <AlertCircle />
          <AlertTitle>{tokenError ? errorMessage(tokenError) : "Liên kết không hợp lệ"}</AlertTitle>
          <AlertDescription>{tokenError ? errorHint(tokenError.code) : "Thiếu mã đặt lại mật khẩu trong liên kết."}</AlertDescription>
        </Alert>
        <Button asChild className="w-full">
          <Link href="/forgot-password">Yêu cầu liên kết mới</Link>
        </Button>
      </div>
    );

  const e = formState.errors;
  return (
    <form onSubmit={onSubmit} className="space-y-4" noValidate>
      <Field label="Mật khẩu mới" htmlFor="matKhauMoi" error={e.matKhauMoi?.message}>
        <PasswordInput id="matKhauMoi" autoComplete="new-password" aria-invalid={!!e.matKhauMoi} {...register("matKhauMoi")} />
        <PasswordStrength password={watch("matKhauMoi")} />
      </Field>
      <Field label="Xác nhận mật khẩu mới" htmlFor="xacNhanMatKhau" error={e.xacNhanMatKhau?.message}>
        <PasswordInput id="xacNhanMatKhau" autoComplete="new-password" aria-invalid={!!e.xacNhanMatKhau} {...register("xacNhanMatKhau")} />
      </Field>
      <Button type="submit" size="lg" className="w-full font-bold" disabled={formState.isSubmitting}>
        {formState.isSubmitting ? "Đang lưu…" : "Đặt lại mật khẩu"}
      </Button>
    </form>
  );
}
