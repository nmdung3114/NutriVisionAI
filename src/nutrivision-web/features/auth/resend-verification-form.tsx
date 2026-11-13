"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { MailCheck } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Field } from "@/components/shared/field";
import { authApi } from "@/lib/api/auth";
import { errorMessage } from "@/lib/api/errors";
import { emailOnlySchema, type EmailOnlyValues } from "./schemas";

export function ResendVerificationForm({ defaultEmail = "" }: { defaultEmail?: string }) {
  const [sent, setSent] = useState(false);
  const form = useForm<EmailOnlyValues>({ resolver: zodResolver(emailOnlySchema), defaultValues: { email: defaultEmail } });
  const { register, handleSubmit, formState } = form;

  const onSubmit = handleSubmit(async ({ email }) => {
    try {
      await authApi.resendVerification(email);
      setSent(true);
      toast.success("Nếu email tồn tại và chưa xác thực, chúng tôi đã gửi lại liên kết xác thực.");
    } catch (e) {
      toast.error(errorMessage(e));
    }
  });

  if (sent)
    return (
      <div className="flex items-start gap-3 rounded-xl bg-brand-50 p-4 text-sm text-brand-700" role="status">
        <MailCheck className="mt-0.5 size-5 shrink-0" />
        Đã gửi (nếu email hợp lệ). Hãy kiểm tra hộp thư đến và cả thư rác.
      </div>
    );

  return (
    <form onSubmit={onSubmit} className="space-y-3" noValidate>
      <Field label="Email nhận liên kết xác thực" htmlFor="resend-email" error={formState.errors.email?.message}>
        <Input id="resend-email" type="email" autoComplete="email" placeholder="ban@example.com" aria-invalid={!!formState.errors.email} {...register("email")} />
      </Field>
      <Button type="submit" variant="outline" className="w-full" disabled={formState.isSubmitting}>
        {formState.isSubmitting ? "Đang gửi…" : "Gửi lại email xác thực"}
      </Button>
    </form>
  );
}
