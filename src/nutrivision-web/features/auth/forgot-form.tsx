"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { MailCheck } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Field } from "@/components/shared/field";
import { authApi } from "@/lib/api/auth";
import { errorMessage } from "@/lib/api/errors";
import { applyFieldErrors } from "@/lib/forms";
import { emailOnlySchema, type EmailOnlyValues } from "./schemas";

export function ForgotForm() {
  const [sent, setSent] = useState(false);
  const form = useForm<EmailOnlyValues>({ resolver: zodResolver(emailOnlySchema), defaultValues: { email: "" } });
  const { register, handleSubmit, formState } = form;
  const onSubmit = handleSubmit(async ({ email }) => {
    try {
      await authApi.forgotPassword(email.trim());
      setSent(true);
    } catch (e) {
      if (!applyFieldErrors(form, e)) toast.error(errorMessage(e));
    }
  });
  if (sent)
    return (
      <div className="space-y-3 text-center" role="status">
        <span className="mx-auto flex size-16 items-center justify-center rounded-2xl bg-brand-50 text-primary">
          <MailCheck className="size-8" />
        </span>
        <p className="text-sm text-muted-foreground">
          Nếu email tồn tại trong hệ thống, chúng tôi đã gửi hướng dẫn đặt lại mật khẩu. Liên kết có hạn sử dụng, hãy kiểm tra cả thư rác.
        </p>
      </div>
    );
  return (
    <form onSubmit={onSubmit} className="space-y-4" noValidate>
      <Field label="Email" htmlFor="email" error={formState.errors.email?.message}>
        <Input id="email" type="email" autoComplete="email" placeholder="ban@example.com" aria-invalid={!!formState.errors.email} {...register("email")} />
      </Field>
      <Button type="submit" size="lg" className="w-full font-bold" disabled={formState.isSubmitting}>
        {formState.isSubmitting ? "Đang gửi…" : "Gửi hướng dẫn đặt lại"}
      </Button>
    </form>
  );
}
