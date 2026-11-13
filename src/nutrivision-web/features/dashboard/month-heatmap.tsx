"use client";

import Link from "next/link";
import { useState } from "react";
import { CalendarDays, ChevronLeft, ChevronRight } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
import { useCalendar } from "@/hooks/use-api";
import { fmt } from "@/lib/meal";
import { cn } from "@/lib/utils";
import type { CalendarDay, DayState } from "@/types/api";

const WEEKDAYS = ["T2", "T3", "T4", "T5", "T6", "T7", "CN"];

const CELL: Record<DayState | "none", { cls: string; label: string }> = {
  none: { cls: "bg-muted text-muted-foreground", label: "Chưa ghi nhận" },
  Thieu: { cls: "bg-yellow/45 text-[#5c3d00] dark:bg-yellow/30 dark:text-yellow", label: "Thiếu (< 90%)" },
  DatChuan: { cls: "bg-brand-500 text-white", label: "Đạt chuẩn (90–110%)" },
  Vuot: { cls: "bg-orange text-white", label: "Vượt (> 110%)" },
};

function monthLabel(nam: number, thang: number) {
  return `Tháng ${thang}/${nam}`;
}

/** Lịch nhiệt tháng: mỗi ô là một ngày, tô màu theo mức calo so với mục tiêu. Bấm ô → xem bữa ăn ngày đó. */
export function MonthHeatmap({ today }: { today: string }) {
  const [y0, m0] = today.split("-").map(Number);
  const [cursor, setCursor] = useState({ nam: y0, thang: m0 });
  const { data, isLoading, isFetching } = useCalendar(cursor.nam, cursor.thang);
  const isCurrent = cursor.nam === y0 && cursor.thang === m0;

  const shift = (delta: number) =>
    setCursor((c) => {
      const d = new Date(c.nam, c.thang - 1 + delta, 1);
      return { nam: d.getFullYear(), thang: d.getMonth() + 1 };
    });

  // Ô trống đầu tháng để ngày 1 rơi đúng thứ (tuần bắt đầu từ Thứ Hai).
  const lead = (new Date(cursor.nam, cursor.thang - 1, 1).getDay() + 6) % 7;
  const daysInMonth = new Date(cursor.nam, cursor.thang, 0).getDate();
  const byDay = new Map((data?.ngay ?? []).map((d) => [d.ngay, d]));
  const pad = (n: number) => String(n).padStart(2, "0");

  return (
    <section className="rounded-2xl border bg-card p-5 shadow-soft" aria-label="Lịch dinh dưỡng theo tháng">
      <div className="mb-4 flex items-center gap-2">
        <span className="flex size-9 items-center justify-center rounded-xl bg-brand-50 text-brand-700">
          <CalendarDays className="size-5" />
        </span>
        <div className="min-w-0 flex-1">
          <h2 className="text-lg font-extrabold leading-tight">{monthLabel(cursor.nam, cursor.thang)}</h2>
          <p className="text-xs text-muted-foreground">
            {data ? (
              <>
                <b className="text-foreground">{data.soNgayDatChuan}</b> ngày đạt mục tiêu · {data.soNgayCoDuLieu} ngày có ghi nhận
              </>
            ) : (
              "Đang tải…"
            )}
          </p>
        </div>
        <Button variant="ghost" size="icon" aria-label="Tháng trước" onClick={() => shift(-1)}>
          <ChevronLeft className="size-4" />
        </Button>
        <Button variant="ghost" size="icon" aria-label="Tháng sau" onClick={() => shift(1)} disabled={isCurrent}>
          <ChevronRight className="size-4" />
        </Button>
      </div>

      {isLoading ? (
        <Skeleton className="h-56 rounded-xl" />
      ) : (
        <div className={cn("mx-auto max-w-md transition-opacity", isFetching && "opacity-60")}>
          <div className="mb-1.5 grid grid-cols-7 gap-1.5 text-center text-[11px] font-bold text-muted-foreground">
            {WEEKDAYS.map((w) => (
              <span key={w}>{w}</span>
            ))}
          </div>
          <div className="grid grid-cols-7 gap-1.5">
            {Array.from({ length: lead }).map((_, i) => (
              <span key={`lead-${i}`} aria-hidden />
            ))}
            {Array.from({ length: daysInMonth }).map((_, i) => {
              const iso = `${cursor.nam}-${pad(cursor.thang)}-${pad(i + 1)}`;
              const d = byDay.get(iso);
              const future = !d; // API chỉ trả ngày đã qua
              return <DayCell key={iso} iso={iso} day={i + 1} data={d} future={future} isToday={iso === today} />;
            })}
          </div>
          <ul className="mt-4 flex flex-wrap gap-x-4 gap-y-1.5 text-xs text-muted-foreground" aria-label="Chú thích">
            {(["DatChuan", "Thieu", "Vuot", "none"] as const).map((k) => (
              <li key={k} className="flex items-center gap-1.5">
                <span className={cn("size-3 rounded", CELL[k].cls)} />
                {CELL[k].label}
              </li>
            ))}
          </ul>
        </div>
      )}
    </section>
  );
}

function DayCell({ iso, day, data, future, isToday }: { iso: string; day: number; data?: CalendarDay; future: boolean; isToday: boolean }) {
  const state = data?.trangThai ?? "none";
  const base = cn(
    "flex aspect-square items-center justify-center rounded-lg text-xs font-bold transition-transform outline-none focus-visible:ring-2 focus-visible:ring-ring",
    isToday && "ring-2 ring-foreground/70 ring-offset-1 ring-offset-card",
  );
  if (future) return <span className={cn(base, "border border-dashed text-muted-foreground/50")}>{day}</span>;

  const desc = data?.trangThai
    ? `${fmt(data.calo)} / ${fmt(data.mucTieuCalo)} kcal (${data.phanTramMucTieu}%) · ${data.soBuaAn} bữa`
    : "Chưa ghi nhận bữa ăn";
  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <Link href={`/meals?ngay=${iso}`} aria-label={`Ngày ${day}: ${desc}`} className={cn(base, CELL[state].cls, "hover:scale-110")}>
          {day}
        </Link>
      </TooltipTrigger>
      <TooltipContent>
        <p className="font-bold">Ngày {iso.split("-").reverse().join("/")}</p>
        <p>{desc}</p>
      </TooltipContent>
    </Tooltip>
  );
}
