"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { KeyRound } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Field } from "@/components/shared/field";
import { PasswordInput } from "@/components/shared/password-input";
import { PasswordStrength } from "@/components/shared/password-strength";
import { authApi } from "@/lib/api/auth";
import { ApiError, errorMessage } from "@/lib/api/errors";
import { applyFieldErrors } from "@/lib/forms";
import { useAuth } from "@/providers/auth-provider";
import { changePasswordSchema, type ChangePasswordValues } from "@/features/auth/schemas";

export function ChangePasswordForm() {
  const { setSession } = useAuth();
  const form = useForm<ChangePasswordValues>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: { matKhauHienTai: "", matKhauMoi: "", xacNhanMatKhau: "" },
  });
  const { register, handleSubmit, formState, watch, reset } = form;
  const e = formState.errors;

  const onSubmit = handleSubmit(async (v) => {
    try {
      const r = await authApi.changePassword(v);
      setSession(r); // thay token cũ bằng AuthResult mới
      reset();
      toast.success("Đổi mật khẩu thành công. Các thiết bị khác đã bị đăng xuất.");
    } catch (err) {
      if (applyFieldErrors(form, err)) return;
      if (err instanceof ApiError && err.code === "WRONG_PASSWORD") form.setError("matKhauHienTai", { message: errorMessage(err) });
      else if (err instanceof ApiError && err.code === "SAME_PASSWORD") form.setError("matKhauMoi", { message: errorMessage(err) });
      else toast.error(errorMessage(err));
    }
  });

  return (
    <form onSubmit={onSubmit} noValidate className="max-w-lg space-y-4 rounded-2xl border bg-card p-5 shadow-soft">
      <h2 className="text-lg font-extrabold">Đổi mật khẩu</h2>
      <Field label="Mật khẩu hiện tại" htmlFor="matKhauHienTai" error={e.matKhauHienTai?.message}>
        <PasswordInput id="matKhauHienTai" autoComplete="current-password" aria-invalid={!!e.matKhauHienTai} {...register("matKhauHienTai")} />
      </Field>
      <Field label="Mật khẩu mới" htmlFor="matKhauMoi" error={e.matKhauMoi?.message}>
        <PasswordInput id="matKhauMoi" autoComplete="new-password" aria-invalid={!!e.matKhauMoi} {...register("matKhauMoi")} />
        <PasswordStrength password={watch("matKhauMoi")} />
      </Field>
      <Field label="Xác nhận mật khẩu mới" htmlFor="xacNhanMatKhau" error={e.xacNhanMatKhau?.message}>
        <PasswordInput id="xacNhanMatKhau" autoComplete="new-password" aria-invalid={!!e.xacNhanMatKhau} {...register("xacNhanMatKhau")} />
      </Field>
      <Button type="submit" disabled={formState.isSubmitting} className="font-bold">
        <KeyRound /> {formState.isSubmitting ? "Đang đổi…" : "Đổi mật khẩu"}
      </Button>
    </form>
  );
}
