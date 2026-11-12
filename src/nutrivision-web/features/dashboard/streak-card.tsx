"use client";

import Link from "next/link";
import { motion, useReducedMotion } from "framer-motion";
import { Flame, Trophy } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
import { useStreak } from "@/hooks/use-api";
import { cn } from "@/lib/utils";
import type { Badge } from "@/types/api";

/** Chuỗi ngày ghi nhận liên tiếp + huy hiệu (đạt được thì có màu, chưa đạt thì xám kèm tiến độ). */
export function StreakCard() {
  const { data, isLoading } = useStreak();
  const reduce = useReducedMotion();
  if (isLoading) return <Skeleton className="h-64 rounded-2xl" />;
  if (!data) return null;

  const earned = data.huyHieu.filter((b) => b.datDuoc).length;
  const atRisk = data.chuoiHienTai > 0 && !data.homNayDaGhi;

  return (
    <section className="relative overflow-hidden rounded-2xl border bg-card p-5 shadow-soft" aria-label="Chuỗi ngày và huy hiệu">
      <div aria-hidden className="pointer-events-none absolute -right-10 -top-10 size-40 rounded-full bg-orange/15 blur-2xl" />
      <div className="relative flex items-center gap-4">
        <motion.div
          className="flex size-16 shrink-0 flex-col items-center justify-center rounded-2xl bg-gradient-to-br from-[#ffb347] to-[#ff5e3a] text-white shadow-md"
          animate={reduce || data.chuoiHienTai === 0 ? undefined : { scale: [1, 1.06, 1] }}
          transition={{ duration: 1.6, repeat: Infinity, ease: "easeInOut" }}
        >
          <Flame className="size-6" fill="currentColor" />
          <span className="text-lg font-extrabold leading-none">{data.chuoiHienTai}</span>
        </motion.div>
        <div className="min-w-0 flex-1">
          <h2 className="text-lg font-extrabold leading-tight">
            {data.chuoiHienTai > 0 ? `Chuỗi ${data.chuoiHienTai} ngày liên tiếp` : "Bắt đầu chuỗi ngày mới"}
          </h2>
          <p className="text-xs text-muted-foreground">
            Kỷ lục {data.chuoiDaiNhat} ngày · {data.tongBuaAn} bữa đã ghi · {earned}/{data.huyHieu.length} huy hiệu
          </p>
        </div>
      </div>

      {atRisk && (
        <div className="relative mt-3 flex items-center gap-2 rounded-xl bg-orange/10 px-3 py-2 text-sm">
          <span className="flex-1 font-medium">Hôm nay chưa ghi bữa nào, ghi ngay để giữ chuỗi 🔥</span>
          <Button asChild size="sm" className="rounded-full">
            <Link href="/meals/new">Ghi ngay</Link>
          </Button>
        </div>
      )}

      <h3 className="relative mt-4 mb-2 flex items-center gap-1.5 text-sm font-bold">
        <Trophy className="size-4 text-warn" /> Huy hiệu
      </h3>
      <ul className="relative grid grid-cols-3 gap-2 sm:grid-cols-5 lg:grid-cols-3">
        {data.huyHieu.map((b) => (
          <BadgeTile key={b.ma} badge={b} />
        ))}
      </ul>
    </section>
  );
}

function BadgeTile({ badge: b }: { badge: Badge }) {
  const pct = Math.round((b.tienDo / b.mucTieu) * 100);
  return (
    <li>
      <Tooltip>
        <TooltipTrigger asChild>
          <div
            tabIndex={0}
            aria-label={`${b.ten}: ${b.moTa}. ${b.datDuoc ? "Đã đạt" : `Tiến độ ${b.tienDo}/${b.mucTieu}`}`}
            className={cn(
              "flex flex-col items-center gap-1 rounded-xl border p-2 text-center outline-none transition-transform hover:-translate-y-0.5 focus-visible:ring-2 focus-visible:ring-ring",
              b.datDuoc ? "border-yellow/60 bg-gradient-to-b from-yellow/20 to-transparent" : "bg-muted/40",
            )}
          >
            <span aria-hidden className={cn("text-2xl", !b.datDuoc && "opacity-40 grayscale")}>{b.bieuTuong}</span>
            <span className={cn("line-clamp-2 min-h-[2lh] text-[11px] font-bold leading-tight", !b.datDuoc && "text-muted-foreground")}>{b.ten}</span>
            {!b.datDuoc && (
              <span className="h-1 w-full overflow-hidden rounded-full bg-track">
                <span className="block h-full rounded-full bg-primary" style={{ width: `${pct}%` }} />
              </span>
            )}
          </div>
        </TooltipTrigger>
        <TooltipContent>
          <p className="font-bold">{b.ten}</p>
          <p>{b.moTa}</p>
          <p>{b.datDuoc ? "✅ Đã đạt" : `Tiến độ ${b.tienDo}/${b.mucTieu}`}</p>
        </TooltipContent>
      </Tooltip>
    </li>
  );
}
