"use client";

import Link from "next/link";
import { Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { MEAL_THEME, MEAL_TYPES, fmt } from "@/lib/meal";
import { cn } from "@/lib/utils";
import { formatTime } from "@/lib/date";
import type { Dashboard } from "@/types/api";

export function TodayMeals({ data }: { data: Dashboard }) {
  return (
    <section aria-label="Bữa ăn hôm nay">
      <h2 className="mb-3 text-lg font-extrabold">Bữa ăn hôm nay</h2>
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {MEAL_TYPES.map((t) => {
          const meals = data.buaAnHomNay.filter((m) => m.loaiBuaAn === t.value);
          const calo = data.theoLoaiBuaAn.find((x) => x.loaiBuaAn === t.value)?.calo ?? meals.reduce((s, m) => s + m.tongDinhDuong.calo, 0);
          const th = MEAL_THEME[t.value];
          return (
            <div key={t.value} className="relative flex flex-col overflow-hidden rounded-2xl border bg-card p-4 shadow-soft transition-transform hover:-translate-y-0.5">
              <div aria-hidden className={cn("pointer-events-none absolute inset-0", th.tint)} />
              <div className="relative flex items-center justify-between">
                <span className="flex items-center gap-2 font-bold">
                  <span aria-hidden className={cn("flex size-9 items-center justify-center rounded-xl text-lg shadow-sm", th.solid)}>{t.emoji}</span>
                  {t.label}
                </span>
                {meals.length > 0 && <span className={cn("text-sm font-extrabold", th.text)}>{fmt(calo)} kcal</span>}
              </div>
              {meals.length === 0 ? (
                <div className="relative mt-4 flex flex-1 flex-col items-start justify-end gap-2">
                  <p className="text-sm text-muted-foreground">Chưa có bữa ăn nào</p>
                  <Button asChild size="sm" variant="outline" className={cn("rounded-full bg-card/70", th.border, th.text)}>
                    <Link href={`/meals/new?loai=${t.value}`}>
                      <Plus /> Thêm ngay
                    </Link>
                  </Button>
                </div>
              ) : (
                <ul className="relative mt-3 space-y-2">
                  {meals.map((m) => (
                    <li key={m.id}>
                      <Link href={`/meals/${m.id}`} className={cn("block rounded-xl border-l-4 bg-card/80 px-3 py-2 transition-colors hover:bg-accent focus-visible:ring-2 focus-visible:ring-ring", th.border)}>
                        <p className="truncate text-sm font-semibold">{m.tenMonAn}</p>
                        <p className="text-xs text-muted-foreground">
                          {formatTime(m.thoiGianGhiNhan)} · {fmt(m.tongDinhDuong.calo)} kcal
                        </p>
                      </Link>
                    </li>
                  ))}
                </ul>
              )}
            </div>
          );
        })}
      </div>
    </section>
  );
}
