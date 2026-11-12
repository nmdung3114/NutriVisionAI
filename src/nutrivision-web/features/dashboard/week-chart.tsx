"use client";

import { Bar, BarChart, CartesianGrid, Cell, ReferenceLine, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { formatDayMonth, todayStr } from "@/lib/date";
import { fmt } from "@/lib/meal";

export interface DayPoint {
  ngay: string;
  calo: number;
  mucTieuCalo: number;
}

/** Biểu đồ cột calo theo ngày + đường mục tiêu đỏ nét đứt (dùng cho dashboard & báo cáo). */
export function CalorieBarChart({ data, height = 260 }: { data: DayPoint[]; height?: number }) {
  const target = data.length ? data[data.length - 1].mucTieuCalo : 0;
  const today = todayStr();
  const rows = data.map((d) => ({ ...d, label: formatDayMonth(d.ngay) }));
  const sparse = rows.length > 14;
  return (
    <div style={{ height }} className="w-full" role="img" aria-label="Biểu đồ calo theo ngày so với mục tiêu">
      <ResponsiveContainer width="100%" height="100%">
        <BarChart data={rows} margin={{ top: 12, right: 8, left: -12, bottom: 0 }}>
          <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#e8efeb" />
          <XAxis dataKey="label" tickLine={false} axisLine={false} fontSize={12} interval={sparse ? "preserveStartEnd" : 0} minTickGap={sparse ? 16 : 0} />
          <YAxis tickLine={false} axisLine={false} fontSize={12} width={48} />
          <Tooltip
            cursor={{ fill: "rgba(33,180,90,0.08)" }}
            content={({ active, payload }) => {
              if (!active || !payload?.length) return null;
              const p = payload[0].payload as DayPoint & { label: string };
              return (
                <div className="rounded-xl border bg-card px-3 py-2 text-xs shadow-soft">
                  <p className="font-bold">{p.label}</p>
                  <p>
                    Đã nạp: <b>{fmt(p.calo)} kcal</b>
                  </p>
                  <p className="text-muted-foreground">Mục tiêu: {fmt(p.mucTieuCalo)} kcal</p>
                </div>
              );
            }}
          />
          {target > 0 && <ReferenceLine y={target} stroke="#ef4d4d" strokeDasharray="6 4" strokeWidth={2} label={{ value: `Mục tiêu ${fmt(target)}`, position: "insideTopRight", fill: "#ef4d4d", fontSize: 11 }} />}
          <Bar dataKey="calo" radius={[8, 8, 0, 0]} maxBarSize={44}>
            {rows.map((r) => (
              <Cell key={r.ngay} fill={r.calo > r.mucTieuCalo * 1.1 ? "#ff795e" : r.ngay === today ? "#21b45a" : "#8fdcae"} />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
