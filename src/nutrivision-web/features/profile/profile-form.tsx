"use client";

import { useEffect, useMemo } from "react";
import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Save } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Field } from "@/components/shared/field";
import { profileApi } from "@/lib/api/profile";
import { errorMessage } from "@/lib/api/errors";
import { applyFieldErrors } from "@/lib/forms";
import { calcAge, todayStr } from "@/lib/date";
import { ACTIVITY_LEVELS, bmi as calcBmi, bmr as calcBmr, tdee as calcTdee } from "@/lib/health";
import { cn } from "@/lib/utils";
import { useAuth } from "@/providers/auth-provider";
import type { Profile } from "@/types/api";
import { profileSchema, type ProfileValues } from "./schemas";
import { MetricsCards } from "./metrics-cards";

const PRESETS = [1500, 1800, 2000, 2200, 2500];

function toValues(p: Profile): ProfileValues {
  const custom = p.mucTieuCalo && p.chiSoTDEE !== null && Math.round(p.mucTieuCalo) !== Math.round(p.chiSoTDEE);
  return {
    hoTen: p.hoTen,
    gioiTinh: (p.gioiTinh ?? "Nam") as ProfileValues["gioiTinh"],
    ngaySinh: p.ngaySinh?.slice(0, 10) ?? "",
    chieuCao: (p.chieuCao ?? NaN) as number,
    canNang: (p.canNang ?? NaN) as number,
    mucDoVanDong: (p.mucDoVanDong ?? "VanDongNhe") as ProfileValues["mucDoVanDong"],
    mucTieuMode: custom ? "custom" : "tdee",
    mucTieuCalo: custom ? p.mucTieuCalo : null,
  };
}

export function ProfileForm({ profile }: { profile: Profile }) {
  const qc = useQueryClient();
  const { patchUser } = useAuth();
  const form = useForm<ProfileValues>({ resolver: zodResolver(profileSchema), defaultValues: toValues(profile) });
  const { register, handleSubmit, control, watch, formState, reset, setValue } = form;
  const e = formState.errors;

  useEffect(() => reset(toValues(profile)), [profile, reset]);

  const v = watch();
  const live = useMemo(() => {
    const ok = v.ngaySinh && Number.isFinite(v.chieuCao) && Number.isFinite(v.canNang) && v.chieuCao >= 50 && v.canNang >= 10;
    if (!ok) return { bmi: null, bmr: null, tdee: null };
    const age = calcAge(v.ngaySinh);
    if (!(age >= 10 && age <= 100)) return { bmi: null, bmr: null, tdee: null };
    const b = calcBmr(v.gioiTinh, age, v.chieuCao, v.canNang);
    return { bmi: calcBmi(v.chieuCao, v.canNang), bmr: b, tdee: calcTdee(b, v.mucDoVanDong) };
  }, [v.ngaySinh, v.chieuCao, v.canNang, v.gioiTinh, v.mucDoVanDong]);

  const save = useMutation({
    mutationFn: (x: ProfileValues) =>
      profileApi.update({
        hoTen: x.hoTen.trim(),
        ngaySinh: x.ngaySinh,
        gioiTinh: x.gioiTinh,
        chieuCao: x.chieuCao,
        canNang: x.canNang,
        mucDoVanDong: x.mucDoVanDong,
        mucTieuCalo: x.mucTieuMode === "custom" ? x.mucTieuCalo : null,
      }),
    onSuccess: (p) => {
      qc.setQueryData(["profile"], p);
      for (const k of ["dashboard", "report", "suggestions"]) void qc.invalidateQueries({ queryKey: [k] });
      patchUser({ hoTen: p.hoTen });
      toast.success("Đã lưu hồ sơ");
    },
    onError: (err) => {
      if (!applyFieldErrors(form, err)) toast.error(errorMessage(err));
    },
  });

  // Giá trị hiển thị: server sau khi lưu (không có thay đổi) hoặc ước tính khi đang chỉnh.
  const dirty = formState.isDirty;
  const showServer = !dirty && profile.daDayDu;
  const cards = showServer ? { bmi: profile.chiSoBMI, bmr: profile.chiSoBMR, tdee: profile.chiSoTDEE } : live;

  return (
    <form onSubmit={handleSubmit((x) => save.mutate(x))} className="space-y-6" noValidate>
      <MetricsCards bmi={cards.bmi} bmr={cards.bmr} tdee={cards.tdee} preview={!showServer && cards.tdee !== null} />

      <section className="rounded-2xl border bg-card p-5 shadow-soft">
        <h2 className="mb-4 text-lg font-extrabold">Thông tin cá nhân</h2>
        <div className="grid gap-4 sm:grid-cols-2">
          <Field label="Họ và tên" htmlFor="hoTen" error={e.hoTen?.message} required className="sm:col-span-2">
            <Input id="hoTen" aria-invalid={!!e.hoTen} {...register("hoTen")} />
          </Field>
          <Field label="Giới tính" htmlFor="gioiTinh" error={e.gioiTinh?.message} required>
            <Controller
              control={control}
              name="gioiTinh"
              render={({ field }) => (
                <Select value={field.value} onValueChange={field.onChange}>
                  <SelectTrigger id="gioiTinh" className="w-full">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Nam">Nam</SelectItem>
                    <SelectItem value="Nu">Nữ</SelectItem>
                  </SelectContent>
                </Select>
              )}
            />
          </Field>
          <Field label="Ngày sinh" htmlFor="ngaySinh" error={e.ngaySinh?.message} required>
            <Input id="ngaySinh" type="date" max={todayStr()} aria-invalid={!!e.ngaySinh} {...register("ngaySinh")} />
          </Field>
          <Field label="Chiều cao (cm)" htmlFor="chieuCao" error={e.chieuCao?.message} required>
            <Input id="chieuCao" type="number" inputMode="decimal" step="0.1" aria-invalid={!!e.chieuCao} {...register("chieuCao", { valueAsNumber: true })} />
          </Field>
          <Field label="Cân nặng (kg)" htmlFor="canNang" error={e.canNang?.message} required>
            <Input id="canNang" type="number" inputMode="decimal" step="0.1" aria-invalid={!!e.canNang} {...register("canNang", { valueAsNumber: true })} />
          </Field>
          <Field label="Mức độ vận động" htmlFor="mucDoVanDong" error={e.mucDoVanDong?.message} required className="sm:col-span-2">
            <Controller
              control={control}
              name="mucDoVanDong"
              render={({ field }) => (
                <Select value={field.value} onValueChange={field.onChange}>
                  <SelectTrigger id="mucDoVanDong" className="h-auto w-full py-2 text-left">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {ACTIVITY_LEVELS.map((a) => (
                      <SelectItem key={a.value} value={a.value}>
                        <span className="flex flex-col items-start">
                          <span className="font-semibold">{a.label} (×{a.factor})</span>
                          <span className="text-xs text-muted-foreground">{a.desc}</span>
                        </span>
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              )}
            />
          </Field>
        </div>
      </section>

      <section className="rounded-2xl border bg-card p-5 shadow-soft">
        <h2 className="mb-1 text-lg font-extrabold">Mục tiêu calo mỗi ngày</h2>
        <p className="mb-4 text-sm text-muted-foreground">Mục tiêu hiện tại: <b className="text-foreground">{profile.mucTieuCalo.toLocaleString("vi-VN")} kcal</b></p>
        <Controller
          control={control}
          name="mucTieuMode"
          render={({ field }) => (
            <div className="grid gap-3 sm:grid-cols-2" role="radiogroup" aria-label="Chế độ mục tiêu calo">
              {[
                { v: "tdee", t: "Dùng TDEE (khuyến nghị)", d: "Tự động theo thể trạng và mức vận động của bạn" },
                { v: "custom", t: "Tùy chỉnh", d: "Tự đặt mục tiêu calo (800 – 6000 kcal)" },
              ].map((o) => (
                <button
                  key={o.v}
                  type="button"
                  role="radio"
                  aria-checked={field.value === o.v}
                  onClick={() => {
                    field.onChange(o.v);
                    if (o.v === "tdee") setValue("mucTieuCalo", null, { shouldDirty: true });
                    else if (v.mucTieuCalo === null) setValue("mucTieuCalo", profile.mucTieuCalo, { shouldDirty: true });
                  }}
                  className={cn("rounded-xl border p-4 text-left outline-none transition-colors focus-visible:ring-2 focus-visible:ring-ring", field.value === o.v ? "border-primary bg-brand-50" : "hover:border-primary/50")}
                >
                  <p className="font-bold">{o.t}</p>
                  <p className="text-xs text-muted-foreground">{o.d}</p>
                </button>
              ))}
            </div>
          )}
        />
        {v.mucTieuMode === "custom" && (
          <div className="mt-4 space-y-3">
            <Field label="Mục tiêu calo (kcal/ngày)" htmlFor="mucTieuCalo" error={e.mucTieuCalo?.message}>
              <Controller
                control={control}
                name="mucTieuCalo"
                render={({ field }) => (
                  <Input
                    id="mucTieuCalo"
                    type="number"
                    inputMode="numeric"
                    value={field.value ?? ""}
                    onChange={(ev) => field.onChange(ev.target.value === "" ? null : Number(ev.target.value))}
                    aria-invalid={!!e.mucTieuCalo}
                    className="max-w-xs"
                  />
                )}
              />
            </Field>
            <div className="flex flex-wrap gap-2" aria-label="Mức gợi ý nhanh">
              {PRESETS.map((p) => (
                <Button key={p} type="button" size="sm" variant={v.mucTieuCalo === p ? "default" : "outline"} className="rounded-full" onClick={() => setValue("mucTieuCalo", p, { shouldDirty: true, shouldValidate: true })}>
                  {p.toLocaleString("vi-VN")}
                </Button>
              ))}
            </div>
          </div>
        )}
      </section>

      <div className="flex justify-end">
        <Button type="submit" size="lg" className="font-bold" disabled={save.isPending}>
          <Save /> {save.isPending ? "Đang lưu…" : "Lưu hồ sơ"}
        </Button>
      </div>
    </form>
  );
}
