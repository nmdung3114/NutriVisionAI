"use client";

import { CalorieRing, STATE_META } from "@/components/shared/calorie-ring";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import { MACRO_COLORS, fmt } from "@/lib/meal";
import type { Dashboard } from "@/types/api";

function MacroBar({ label, grams, target, share, color }: { label: string; grams: number; target: number; share: number; color: string }) {
  const pct = target > 0 ? Math.min((grams / target) * 100, 100) : 0;
  return (
    <div>
      <div className="mb-1.5 flex items-baseline justify-between text-sm">
        <span className="font-semibold">{label}</span>
        <span className="text-muted-foreground">
          <b className="text-foreground">{fmt(grams, 1)}g</b> / {fmt(target)}g · {share}% năng lượng
        </span>
      </div>
      <div
        className="h-2.5 overflow-hidden rounded-full bg-muted"
        role="progressbar"
        aria-label={label}
        aria-valuemin={0}
        aria-valuemax={Math.round(target)}
        aria-valuenow={Math.round(grams)}
      >
        <div className="h-full rounded-full transition-all duration-700" style={{ width: `${pct}%`, background: color }} />
      </div>
    </div>
  );
}

export function HeroCard({ data }: { data: Dashboard }) {
  const { homNay: h } = data;
  const meta = STATE_META[h.trangThai];
  const remain = data.mucTieuCalo - h.calo;
  return (
    <section className="rounded-3xl border bg-card p-5 shadow-soft sm:p-7" aria-label="Tổng quan calo hôm nay">
      <div className="grid items-center gap-6 md:grid-cols-[auto_1fr] md:gap-10">
        <div className="flex flex-col items-center gap-3">
          <CalorieRing value={h.calo} target={data.mucTieuCalo} state={h.trangThai} />
          <Badge className={cn("rounded-full px-3 py-1 text-xs font-bold", meta.bg)}>{meta.label}</Badge>
        </div>
        <div className="space-y-5">
          <div>
            <h2 className="text-lg font-extrabold">Hôm nay bạn đã nạp {h.phanTramMucTieu}% mục tiêu</h2>
            <p className="text-sm text-muted-foreground">
              {remain >= 0 ? (
                <>
                  Còn <b className="text-foreground">{fmt(remain)} kcal</b> để đạt mục tiêu · {h.soBuaAn} bữa đã ghi nhận
                </>
              ) : (
                <>
                  Đã vượt <b className="text-red-600">{fmt(-remain)} kcal</b> so với mục tiêu · {h.soBuaAn} bữa đã ghi nhận
                </>
              )}
            </p>
          </div>
          <div className="space-y-3.5">
            <MacroBar label="Chất đạm" grams={h.chatDam} target={data.mucTieuMacro.chatDam} share={data.tyLeMacro.chatDam} color={MACRO_COLORS.chatDam} />
            <MacroBar label="Tinh bột" grams={h.tinhBot} target={data.mucTieuMacro.tinhBot} share={data.tyLeMacro.tinhBot} color={MACRO_COLORS.tinhBot} />
            <MacroBar label="Chất béo" grams={h.chatBeo} target={data.mucTieuMacro.chatBeo} share={data.tyLeMacro.chatBeo} color={MACRO_COLORS.chatBeo} />
          </div>
        </div>
      </div>
    </section>
  );
}
