"use client";

import { motion, useReducedMotion } from "framer-motion";
import { Lightbulb } from "lucide-react";
import { Skeleton } from "@/components/ui/skeleton";
import { useInsights } from "@/hooks/use-api";
import { formatDayMonth } from "@/lib/date";
import { fmt } from "@/lib/meal";
import { cn } from "@/lib/utils";
import type { InsightKind } from "@/types/api";

const KIND: Record<InsightKind, { label: string; card: string; chip: string }> = {
  CanhBao: { label: "Cần chú ý", card: "border-orange/40 bg-orange/[0.07]", chip: "bg-orange/15 text-danger-strong" },
  ThongTin: { label: "Lưu ý", card: "border-blue/30 bg-blue/[0.06]", chip: "bg-blue/15 text-info" },
  TichCuc: { label: "Làm tốt", card: "border-brand-500/30 bg-brand-500/[0.07]", chip: "bg-brand-500/15 text-brand-700" },
};

/** Nhận xét tự động 7 ngày gần nhất (quy tắc ở backend: calo, xơ, đạm, béo, phân bổ bữa, xu hướng). */
export function WeeklyInsights() {
  const { data, isLoading } = useInsights();
  const reduce = useReducedMotion();
  if (isLoading) return <Skeleton className="h-48 rounded-2xl" />;
  if (!data) return null;

  return (
    <section className="rounded-2xl border bg-card p-5 shadow-soft" aria-label="Nhận xét tuần">
      <div className="mb-3 flex items-center gap-2">
        <span className="flex size-9 items-center justify-center rounded-xl bg-yellow/25 text-warn">
          <Lightbulb className="size-5" />
        </span>
        <div>
          <h2 className="text-lg font-extrabold leading-tight">Nhận xét tuần này</h2>
          <p className="text-xs text-muted-foreground">
            {formatDayMonth(data.tuNgay)} – {formatDayMonth(data.denNgay)}
            {data.soNgayCoDuLieu > 0 && <> · {data.soNgayCoDuLieu}/7 ngày · TB {fmt(data.caloTrungBinh)} kcal/ngày</>}
          </p>
        </div>
      </div>
      <ul className="grid gap-2.5 md:grid-cols-2">
        {data.nhanXet.map((i, idx) => (
          <motion.li
            key={i.ma}
            initial={reduce ? false : { opacity: 0, y: 8 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: idx * 0.06 }}
            className={cn("flex gap-3 rounded-xl border p-3", KIND[i.loai].card)}
          >
            <span aria-hidden className="text-2xl leading-none">{i.bieuTuong}</span>
            <div className="min-w-0">
              <p className="flex flex-wrap items-center gap-1.5 text-sm font-bold">
                {i.tieuDe}
                <span className={cn("rounded-full px-1.5 py-0.5 text-[10px] font-bold", KIND[i.loai].chip)}>{KIND[i.loai].label}</span>
              </p>
              <p className="mt-0.5 text-xs text-muted-foreground">{i.noiDung}</p>
            </div>
          </motion.li>
        ))}
      </ul>
    </section>
  );
}
