"use client";

import { motion, useReducedMotion } from "framer-motion";
import { NumberTicker } from "./number-ticker";

export interface Ring {
  key: string;
  label: string;
  value: number;
  target: number;
  color: string;
}

/**
 * Các vòng tiến độ đồng tâm kiểu Apple Watch: vòng ngoài là calo, các vòng trong là đạm / tinh bột / béo.
 * Vòng vượt 100% chạy tiếp vòng thứ hai với đầu vòng có bóng đổ để vẫn thấy được phần vượt.
 */
export function ActivityRings({ rings, size = 220, center }: { rings: Ring[]; size?: number; center?: { value: number; caption: string } }) {
  const reduce = useReducedMotion();
  const stroke = Math.round(size * 0.075);
  const gap = Math.round(stroke * 0.28);
  const summary = rings.map((r) => `${r.label} ${pct(r)}%`).join(", ");

  return (
    <div className="relative shrink-0" style={{ width: size, height: size }} role="img" aria-label={`Tiến độ hôm nay: ${summary}`}>
      <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`} className="-rotate-90 overflow-visible">
        <defs>
          <filter id="ring-tip-shadow" x="-50%" y="-50%" width="200%" height="200%">
            <feDropShadow dx="0" dy="0" stdDeviation={stroke * 0.18} floodOpacity="0.45" />
          </filter>
        </defs>
        {rings.map((ring, i) => {
          const r = size / 2 - stroke / 2 - i * (stroke + gap);
          const c = 2 * Math.PI * r;
          const ratio = ring.target > 0 ? ring.value / ring.target : 0;
          const shown = Math.min(ratio, 2); // tối đa 2 vòng
          const firstLap = Math.min(shown, 1);
          const secondLap = Math.max(shown - 1, 0);
          return (
            <g key={ring.key}>
              <circle cx={size / 2} cy={size / 2} r={r} fill="none" stroke={ring.color} strokeOpacity={0.16} strokeWidth={stroke} />
              <motion.circle
                cx={size / 2}
                cy={size / 2}
                r={r}
                fill="none"
                stroke={ring.color}
                strokeWidth={stroke}
                strokeLinecap="round"
                strokeDasharray={c}
                initial={{ strokeDashoffset: c }}
                animate={{ strokeDashoffset: c * (1 - firstLap) }}
                transition={{ duration: reduce ? 0 : 1.1, delay: reduce ? 0 : i * 0.12, ease: [0.22, 1, 0.36, 1] }}
              />
              {secondLap > 0 && (
                <motion.circle
                  cx={size / 2}
                  cy={size / 2}
                  r={r}
                  fill="none"
                  stroke={ring.color}
                  strokeWidth={stroke}
                  strokeLinecap="round"
                  strokeDasharray={c}
                  filter="url(#ring-tip-shadow)"
                  initial={{ strokeDashoffset: c }}
                  animate={{ strokeDashoffset: c * (1 - secondLap) }}
                  transition={{ duration: reduce ? 0 : 0.8, delay: reduce ? 0 : 1.1 + i * 0.12, ease: "easeOut" }}
                />
              )}
            </g>
          );
        })}
      </svg>
      {center && (
        <div className="absolute inset-0 flex flex-col items-center justify-center text-center">
          <NumberTicker value={center.value} className="text-3xl font-extrabold tracking-tight" />
          <span className="text-[11px] font-medium text-muted-foreground">{center.caption}</span>
        </div>
      )}
    </div>
  );
}

export function pct(r: Ring): number {
  return r.target > 0 ? Math.round((r.value / r.target) * 100) : 0;
}

/** Chú thích cho từng vòng: chấm màu, tên, giá trị / mục tiêu, phần trăm. */
export function RingLegend({ rings, unit = "g" }: { rings: Ring[]; unit?: string }) {
  return (
    <ul className="grid grid-cols-2 gap-2 sm:grid-cols-4">
      {rings.map((r) => (
        <li key={r.key} className="rounded-2xl bg-muted/60 px-3 py-2.5">
          <p className="flex items-center gap-1.5 text-xs font-semibold text-muted-foreground">
            <span className="size-2.5 rounded-full" style={{ background: r.color }} />
            {r.label}
          </p>
          <p className="mt-0.5 text-lg font-extrabold leading-tight">{pct(r)}%</p>
          <div className="mt-1 h-1 overflow-hidden rounded-full bg-track">
            <div className="h-full rounded-full" style={{ width: `${Math.min(pct(r), 100)}%`, background: r.color }} />
          </div>
          <p className="mt-1 text-xs text-muted-foreground">
            {Math.round(r.value).toLocaleString("vi-VN")} / {Math.round(r.target).toLocaleString("vi-VN")} {r.key === "calo" ? "kcal" : unit}
          </p>
        </li>
      ))}
    </ul>
  );
}
