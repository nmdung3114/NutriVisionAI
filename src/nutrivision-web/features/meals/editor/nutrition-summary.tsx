"use client";

import { Flame } from "lucide-react";
import { NumberTicker } from "@/components/shared/number-ticker";
import { dailyMacroTargets, percentOf } from "@/lib/nutrition";
import { MACRO_COLORS } from "@/lib/meal";
import type { Nutrition } from "@/types/api";

export function NutritionSummary({ total, target }: { total: Nutrition; target: number }) {
  const macro = dailyMacroTargets(target);
  const items = [
    { label: "Chất đạm", v: total.chatDam, pct: percentOf(total.chatDam, macro.chatDam), color: MACRO_COLORS.chatDam },
    { label: "Tinh bột", v: total.tinhBot, pct: percentOf(total.tinhBot, macro.tinhBot), color: MACRO_COLORS.tinhBot },
    { label: "Chất béo", v: total.chatBeo, pct: percentOf(total.chatBeo, macro.chatBeo), color: MACRO_COLORS.chatBeo },
    { label: "Chất xơ", v: total.chatXo, pct: percentOf(total.chatXo, macro.chatXo), color: MACRO_COLORS.chatXo },
  ];
  const caloPct = percentOf(total.calo, target);
  return (
    <section className="rounded-2xl border bg-card p-5 shadow-soft" aria-label="Tổng dinh dưỡng tạm tính">
      <p className="text-sm font-semibold text-muted-foreground">Tổng dinh dưỡng tạm tính</p>
      <div className="mt-1 flex items-end gap-2">
        <Flame className="mb-1.5 size-6 text-orange" />
        <NumberTicker value={total.calo} className="text-4xl font-extrabold tracking-tight" />
        <span className="mb-1 text-sm font-semibold text-muted-foreground">kcal</span>
        <span className="mb-1 ml-auto rounded-full bg-brand-50 px-2 py-0.5 text-xs font-bold text-brand-700">{caloPct}% nhu cầu ngày</span>
      </div>
      <div className="mt-2 h-2 overflow-hidden rounded-full bg-muted" aria-hidden>
        <div className="h-full rounded-full bg-primary transition-all duration-500" style={{ width: `${Math.min(caloPct, 100)}%` }} />
      </div>
      <dl className="mt-4 grid grid-cols-2 gap-3">
        {items.map((i) => (
          <div key={i.label} className="rounded-xl bg-muted/60 p-3">
            <dt className="flex items-center gap-1.5 text-xs text-muted-foreground">
              <span className="size-2 rounded-full" style={{ background: i.color }} />
              {i.label}
            </dt>
            <dd className="mt-0.5 flex items-baseline gap-1">
              <NumberTicker value={i.v} digits={1} className="text-lg font-extrabold" />
              <span className="text-xs text-muted-foreground">g</span>
              <span className="ml-auto text-xs font-semibold text-muted-foreground">{i.pct}%</span>
            </dd>
          </div>
        ))}
      </dl>
      <p className="mt-3 text-[11px] text-muted-foreground">% so với nhu cầu cả ngày ({Math.round(target).toLocaleString("vi-VN")} kcal). Số liệu chính thức do máy chủ tính khi lưu.</p>
    </section>
  );
}
