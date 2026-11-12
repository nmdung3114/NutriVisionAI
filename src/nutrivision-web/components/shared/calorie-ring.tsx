"use client";

import { motion } from "framer-motion";
import { NumberTicker } from "./number-ticker";
import type { DayState } from "@/types/api";

export const STATE_META: Record<DayState, { label: string; color: string; bg: string }> = {
  Thieu: { label: "Thiếu", color: "#f5a524", bg: "bg-yellow/30 text-[#8a5a00]" },
  DatChuan: { label: "Đạt chuẩn", color: "#21b45a", bg: "bg-brand-100 text-brand-700" },
  Vuot: { label: "Vượt", color: "#ef4d4d", bg: "bg-red-100 text-red-700" },
};

export function CalorieRing({
  value,
  target,
  state,
  size = 200,
}: {
  value: number;
  target: number;
  state: DayState;
  size?: number;
}) {
  const stroke = 16;
  const r = (size - stroke) / 2;
  const c = 2 * Math.PI * r;
  const pct = target > 0 ? Math.min(value / target, 1) : 0;
  const color = STATE_META[state].color;
  return (
    <div
      className="relative shrink-0"
      style={{ width: size, height: size }}
      role="img"
      aria-label={`Đã nạp ${Math.round(value)} trên ${Math.round(target)} kcal`}
    >
      <svg width={size} height={size} className="-rotate-90">
        <circle cx={size / 2} cy={size / 2} r={r} fill="none" stroke="#e8efeb" strokeWidth={stroke} />
        <motion.circle
          cx={size / 2}
          cy={size / 2}
          r={r}
          fill="none"
          stroke={color}
          strokeWidth={stroke}
          strokeLinecap="round"
          strokeDasharray={c}
          initial={{ strokeDashoffset: c }}
          animate={{ strokeDashoffset: c * (1 - pct) }}
          transition={{ duration: 0.9, ease: "easeOut" }}
        />
      </svg>
      <div className="absolute inset-0 flex flex-col items-center justify-center text-center">
        <NumberTicker value={value} className="text-4xl font-extrabold tracking-tight" />
        <span className="text-xs font-medium text-muted-foreground">/ {Math.round(target).toLocaleString("vi-VN")} kcal</span>
      </div>
    </div>
  );
}
