"use client";

import { STATE_META } from "@/components/shared/day-state";
import { ActivityRings, RingLegend, type Ring } from "@/components/shared/activity-rings";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import { MACRO_COLORS, fmt } from "@/lib/meal";
import type { Dashboard } from "@/types/api";

export function HeroCard({ data }: { data: Dashboard }) {
  const { homNay: h } = data;
  const meta = STATE_META[h.trangThai];
  const remain = data.mucTieuCalo - h.calo;
  const rings: Ring[] = [
    { key: "calo", label: "Calo", value: h.calo, target: data.mucTieuCalo, color: MACRO_COLORS.calo },
    { key: "dam", label: "Chất đạm", value: h.chatDam, target: data.mucTieuMacro.chatDam, color: MACRO_COLORS.chatDam },
    { key: "bot", label: "Tinh bột", value: h.tinhBot, target: data.mucTieuMacro.tinhBot, color: MACRO_COLORS.tinhBot },
    { key: "beo", label: "Chất béo", value: h.chatBeo, target: data.mucTieuMacro.chatBeo, color: MACRO_COLORS.chatBeo },
  ];
  return (
    <section className="relative overflow-hidden rounded-3xl border bg-card p-5 shadow-soft sm:p-7" aria-label="Tổng quan calo hôm nay">
      <div aria-hidden className="pointer-events-none absolute -right-20 -top-24 size-72 rounded-full bg-brand-500/10 blur-3xl" />
      <div aria-hidden className="pointer-events-none absolute -bottom-24 left-40 size-64 rounded-full bg-yellow/15 blur-3xl" />
      <div className="relative grid items-center gap-6 md:grid-cols-[auto_1fr] md:gap-10">
        <div className="flex flex-col items-center gap-3">
          <ActivityRings rings={rings} center={{ value: h.calo, caption: `/ ${fmt(data.mucTieuCalo)} kcal` }} />
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
                  Đã vượt <b className="text-destructive">{fmt(-remain)} kcal</b> so với mục tiêu · {h.soBuaAn} bữa đã ghi nhận
                </>
              )}
            </p>
          </div>
          <RingLegend rings={rings} />
          <p className="text-xs text-muted-foreground">
            Tỷ lệ năng lượng: đạm {data.tyLeMacro.chatDam}% · tinh bột {data.tyLeMacro.tinhBot}% · béo {data.tyLeMacro.chatBeo}% (khuyến nghị 20 / 50 / 30)
          </p>
        </div>
      </div>
    </section>
  );
}
