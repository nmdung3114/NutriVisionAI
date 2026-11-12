"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { motion, useReducedMotion } from "framer-motion";
import { toast } from "sonner";
import { Droplets, GlassWater, Minus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { useWater, qk } from "@/hooks/use-api";
import { waterApi } from "@/lib/api/tracking";
import { errorMessage } from "@/lib/api/errors";
import { weekdayShort } from "@/lib/date";
import { cn } from "@/lib/utils";

const PORTIONS = [
  { ml: 150, label: "Ly nhỏ", emoji: "🥛" },
  { ml: 250, label: "Cốc", emoji: "🥤" },
  { ml: 500, label: "Chai", emoji: "🍶" },
];

/** Ly nước có mực nước + sóng chuyển động theo % mục tiêu. */
function Glass({ pct }: { pct: number }) {
  const reduce = useReducedMotion();
  const level = Math.min(Math.max(pct, 0), 100);
  const y = 112 - (level / 100) * 104; // vùng nước trong ly cao 104px
  const wave = "M0 6 Q 15 0 30 6 T 60 6 T 90 6 T 120 6 T 150 6 V 140 H 0 Z";
  return (
    <svg viewBox="0 0 90 120" className="h-36 w-28 shrink-0" role="img" aria-label={`Đã uống ${Math.round(pct)}% mục tiêu`}>
      <defs>
        <clipPath id="glass-clip">
          <path d="M8 6 L82 6 L74 114 Q73 118 68 118 L22 118 Q17 118 16 114 Z" />
        </clipPath>
        <linearGradient id="water-fill" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor="#6ec3ff" />
          <stop offset="100%" stopColor="#2f80ed" />
        </linearGradient>
      </defs>
      <g clipPath="url(#glass-clip)">
        <rect width="90" height="120" fill="var(--muted)" />
        <motion.g initial={false} animate={{ y }} transition={{ type: "spring", stiffness: 60, damping: 14 }}>
          <motion.path
            d={wave}
            fill="url(#water-fill)"
            opacity={0.55}
            animate={reduce ? undefined : { x: [-60, 0] }}
            transition={{ duration: 3, repeat: Infinity, ease: "linear" }}
          />
          <motion.path
            d={wave}
            fill="url(#water-fill)"
            transform="translate(0 3)"
            animate={reduce ? undefined : { x: [0, -60] }}
            transition={{ duration: 2.2, repeat: Infinity, ease: "linear" }}
          />
        </motion.g>
      </g>
      <path d="M8 6 L82 6 L74 114 Q73 118 68 118 L22 118 Q17 118 16 114 Z" fill="none" stroke="var(--border)" strokeWidth="3" strokeLinejoin="round" />
    </svg>
  );
}

/** Theo dõi nước uống trong ngày: cộng nhanh theo ly/cốc/chai, hoàn tác, 7 ngày gần nhất. */
export function WaterCard({ compact = false }: { compact?: boolean }) {
  const { data, isLoading } = useWater();
  const qc = useQueryClient();
  const add = useMutation({
    mutationFn: (ml: number) => waterApi.add(ml),
    onSuccess: (s, ml) => {
      const reached = data && data.luongMl < data.mucTieuMl && s.luongMl >= s.mucTieuMl;
      qc.setQueryData(qk.water, s);
      if (reached) toast.success("Tuyệt vời! Bạn đã uống đủ nước hôm nay 💧");
      else if (ml > 0) toast.success(`+${ml} ml nước`, { action: { label: "Hoàn tác", onClick: () => add.mutate(-ml) } });
    },
    onError: (e) => toast.error(errorMessage(e)),
  });

  if (isLoading) return <Skeleton className={cn("rounded-2xl", compact ? "h-56" : "h-72")} />;
  if (!data) return null;
  const maxDay = Math.max(data.mucTieuMl, ...data.bayNgay.map((d) => d.luongMl));

  return (
    <section className="relative overflow-hidden rounded-2xl border bg-card p-5 shadow-soft" aria-label="Nước uống hôm nay">
      <div aria-hidden className="pointer-events-none absolute -left-12 -bottom-16 size-48 rounded-full bg-blue/15 blur-3xl" />
      <div className="relative mb-3 flex items-center gap-2">
        <span className="flex size-9 items-center justify-center rounded-xl bg-blue/15 text-info">
          <Droplets className="size-5" />
        </span>
        <div>
          <h2 className="text-lg font-extrabold leading-tight">Nước uống</h2>
          <p className="text-xs text-muted-foreground">Mục tiêu {data.mucTieuMl.toLocaleString("vi-VN")} ml (≈ 35 ml/kg cân nặng)</p>
        </div>
      </div>

      <div className="relative flex items-center gap-5">
        <Glass pct={data.phanTram} />
        <div className="min-w-0 flex-1">
          <p className="text-3xl font-extrabold tracking-tight">
            {data.luongMl.toLocaleString("vi-VN")}
            <span className="text-base font-semibold text-muted-foreground"> / {data.mucTieuMl.toLocaleString("vi-VN")} ml</span>
          </p>
          <p className="mb-3 text-sm text-muted-foreground">
            {data.luongMl >= data.mucTieuMl
              ? "Đã đạt mục tiêu hôm nay 🎉"
              : `Còn ${(data.mucTieuMl - data.luongMl).toLocaleString("vi-VN")} ml · ≈ ${Math.ceil((data.mucTieuMl - data.luongMl) / 250)} cốc`}
          </p>
          <div className="flex flex-wrap gap-2">
            {PORTIONS.map((p) => (
              <Button key={p.ml} variant="outline" size="sm" className="rounded-full border-blue/40 bg-blue/5 hover:bg-blue/15" disabled={add.isPending} onClick={() => add.mutate(p.ml)} aria-label={`Thêm ${p.label} ${p.ml} ml`}>
                <span aria-hidden>{p.emoji}</span> +{p.ml}
              </Button>
            ))}
            <Button variant="ghost" size="sm" className="rounded-full" disabled={add.isPending || data.luongMl === 0} onClick={() => add.mutate(-250)} aria-label="Bớt 250 ml">
              <Minus /> 250
            </Button>
          </div>
        </div>
      </div>

      {!compact && (
        <div className="relative mt-5">
          <p className="mb-2 flex items-center gap-1.5 text-sm font-bold">
            <GlassWater className="size-4 text-info" /> 7 ngày gần nhất · {data.soNgayDatMucTieu7Ngay}/7 ngày đạt
          </p>
          <ul className="flex h-28 items-end gap-2" aria-label="Lượng nước 7 ngày">
            {data.bayNgay.map((d) => {
              const h = maxDay > 0 ? (d.luongMl / maxDay) * 100 : 0;
              const ok = d.luongMl >= data.mucTieuMl;
              return (
                <li key={d.ngay} className="flex h-full flex-1 flex-col items-center justify-end gap-1" title={`${d.luongMl.toLocaleString("vi-VN")} ml`}>
                  <span className="text-[10px] font-semibold text-muted-foreground">{d.luongMl > 0 ? (d.luongMl / 1000).toLocaleString("vi-VN", { maximumFractionDigits: 1 }) + "L" : ""}</span>
                  <span className={cn("w-full rounded-t-md", ok ? "bg-gradient-to-t from-[#2f80ed] to-[#6ec3ff]" : "bg-blue/30")} style={{ height: `${Math.max(h, 3)}%` }} />
                  <span className="text-[11px] font-bold text-muted-foreground">{weekdayShort(d.ngay)}</span>
                </li>
              );
            })}
          </ul>
        </div>
      )}
    </section>
  );
}
