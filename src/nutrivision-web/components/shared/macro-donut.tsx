"use client";

import { Cell, Pie, PieChart, ResponsiveContainer, Tooltip } from "recharts";
import { MACRO_COLORS } from "@/lib/meal";

export interface DonutSlice {
  name: string;
  value: number;
  color: string;
}

export function macroSlices(m: { chatDam: number; tinhBot: number; chatBeo: number }): DonutSlice[] {
  return [
    { name: "Đạm", value: m.chatDam, color: MACRO_COLORS.chatDam },
    { name: "Tinh bột", value: m.tinhBot, color: MACRO_COLORS.tinhBot },
    { name: "Béo", value: m.chatBeo, color: MACRO_COLORS.chatBeo },
  ];
}

export function MacroDonut({
  slices,
  centerLabel,
  centerSub,
  height = 200,
  unit = "",
}: {
  slices: DonutSlice[];
  centerLabel?: string;
  centerSub?: string;
  height?: number;
  unit?: string;
}) {
  const total = slices.reduce((s, x) => s + x.value, 0);
  const data = total > 0 ? slices : [{ name: "Chưa có", value: 1, color: "var(--track)" }];
  return (
    <div className="relative w-full" style={{ height }}>
      <ResponsiveContainer width="100%" height="100%">
        <PieChart>
          <Pie
            data={data}
            dataKey="value"
            nameKey="name"
            innerRadius="62%"
            outerRadius="92%"
            paddingAngle={total > 0 ? 3 : 0}
            stroke="none"
            cornerRadius={6}
            isAnimationActive
          >
            {data.map((s) => (
              <Cell key={s.name} fill={s.color} />
            ))}
          </Pie>
          {total > 0 && (
            <Tooltip
              formatter={(v, n) => [`${Number(v).toLocaleString("vi-VN", { maximumFractionDigits: 1 })}${unit}`, String(n)]}
              contentStyle={{ borderRadius: 12, border: "1px solid var(--border)", background: "var(--popover)", color: "var(--popover-foreground)", fontSize: 12 }}
            />
          )}
        </PieChart>
      </ResponsiveContainer>
      {(centerLabel || centerSub) && (
        <div className="pointer-events-none absolute inset-0 flex flex-col items-center justify-center text-center">
          {centerLabel && <span className="text-xl font-extrabold">{centerLabel}</span>}
          {centerSub && <span className="text-xs text-muted-foreground">{centerSub}</span>}
        </div>
      )}
    </div>
  );
}

export function DonutLegend({ slices, unit = "%" }: { slices: DonutSlice[]; unit?: string }) {
  return (
    <ul className="flex flex-wrap justify-center gap-x-4 gap-y-1 text-sm">
      {slices.map((s) => (
        <li key={s.name} className="flex items-center gap-1.5">
          <span className="size-2.5 rounded-full" style={{ background: s.color }} />
          <span className="text-muted-foreground">{s.name}</span>
          <span className="font-semibold">
            {s.value.toLocaleString("vi-VN", { maximumFractionDigits: 1 })}
            {unit}
          </span>
        </li>
      ))}
    </ul>
  );
}
