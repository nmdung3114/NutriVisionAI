"use client";

import Link from "next/link";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { MailCheck } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Checkbox } from "@/components/ui/checkbox";
import { Field } from "@/components/shared/field";
import { PasswordInput } from "@/components/shared/password-input";
import { PasswordStrength } from "@/components/shared/password-strength";
import { authApi } from "@/lib/api/auth";
import { errorMessage } from "@/lib/api/errors";
import { applyFieldErrors } from "@/lib/forms";
import { registerSchema, type RegisterValues } from "./schemas";
import { ResendVerificationForm } from "./resend-verification-form";

export function RegisterForm() {
  const [done, setDone] = useState<{ email: string; emailDaGui: boolean } | null>(null);
  const form = useForm<RegisterValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: { hoTen: "", email: "", matKhau: "", xacNhanMatKhau: "", dongYDieuKhoan: false },
  });
  const { register, handleSubmit, formState, watch, setValue } = form;
  const pw = watch("matKhau");

  const onSubmit = handleSubmit(async (v) => {
    try {
      const r = await authApi.register({ ...v, hoTen: v.hoTen.trim(), email: v.email.trim() });
      setDone(r);
    } catch (e) {
      if (!applyFieldErrors(form, e)) {
        if (typeof e === "object" && e && "code" in e && (e as { code: string }).code === "EMAIL_EXISTS") {
          form.setError("email", { type: "server", message: errorMessage(e) });
        } else toast.error(errorMessage(e));
      }
    }
  });

  if (done)
    return (
      <div className="space-y-4 text-center" role="status">
        <span className="mx-auto flex size-16 items-center justify-center rounded-2xl bg-brand-50 text-primary">
          <MailCheck className="size-8" />
        </span>
        <h2 className="text-xl font-extrabold">Kiểm tra hộp thư của bạn</h2>
        <p className="text-sm text-muted-foreground">
          {done.emailDaGui ? (
            <>
              Chúng tôi đã gửi liên kết xác thực tới <b className="text-foreground">{done.email}</b>. Bấm vào liên kết để kích hoạt tài khoản.
            </>
          ) : (
            <>Tài khoản đã được tạo nhưng chưa gửi được email xác thực. Hãy bấm gửi lại bên dưới.</>
          )}
        </p>
        <div className="rounded-xl border bg-muted/50 p-4 text-left">
          <ResendVerificationForm defaultEmail={done.email} />
        </div>
        <Link href="/login" className="inline-block text-sm font-semibold text-primary hover:underline">
          Quay lại đăng nhập
        </Link>
      </div>
    );

  const e = formState.errors;
  return (
    <form onSubmit={onSubmit} className="space-y-4" noValidate>
      <Field label="Họ và tên" htmlFor="hoTen" error={e.hoTen?.message} required>
        <Input id="hoTen" autoComplete="name" placeholder="Nguyễn Văn A" aria-invalid={!!e.hoTen} {...register("hoTen")} />
      </Field>
      <Field label="Email" htmlFor="email" error={e.email?.message} required>
        <Input id="email" type="email" autoComplete="email" placeholder="ban@example.com" aria-invalid={!!e.email} {...register("email")} />
      </Field>
      <Field label="Mật khẩu" htmlFor="matKhau" error={e.matKhau?.message} required>
        <PasswordInput id="matKhau" autoComplete="new-password" placeholder="Tối thiểu 8 ký tự, có chữ và số" aria-invalid={!!e.matKhau} {...register("matKhau")} />
        <PasswordStrength password={pw} />
      </Field>
      <Field label="Xác nhận mật khẩu" htmlFor="xacNhanMatKhau" error={e.xacNhanMatKhau?.message} required>
        <PasswordInput id="xacNhanMatKhau" autoComplete="new-password" placeholder="Nhập lại mật khẩu" aria-invalid={!!e.xacNhanMatKhau} {...register("xacNhanMatKhau")} />
      </Field>
      <div>
        <div className="flex items-start gap-2">
          <Checkbox
            id="dongYDieuKhoan"
            checked={watch("dongYDieuKhoan")}
            onCheckedChange={(c) => setValue("dongYDieuKhoan", c === true, { shouldValidate: true })}
            aria-invalid={!!e.dongYDieuKhoan}
            className="mt-0.5"
          />
          <label htmlFor="dongYDieuKhoan" className="text-sm leading-snug">
            Tôi đồng ý với <span className="font-semibold text-primary">Điều khoản sử dụng</span> và <span className="font-semibold text-primary">Chính sách bảo mật</span> của NutriVision AI.
          </label>
        </div>
        {e.dongYDieuKhoan && (
          <p role="alert" className="mt-1 text-xs font-medium text-destructive">
            {e.dongYDieuKhoan.message}
          </p>
        )}
      </div>
      <Button type="submit" size="lg" className="w-full font-bold" disabled={formState.isSubmitting}>
        {formState.isSubmitting ? "Đang tạo tài khoản…" : "Tạo tài khoản"}
      </Button>
    </form>
  );
}
