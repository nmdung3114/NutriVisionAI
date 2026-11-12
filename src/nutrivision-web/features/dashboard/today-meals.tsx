"use client";

import Link from "next/link";
import { Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { MEAL_TYPES } from "@/lib/meal";
import { fmt } from "@/lib/meal";
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
          return (
            <div key={t.value} className="flex flex-col rounded-2xl border bg-card p-4 shadow-soft">
              <div className="flex items-center justify-between">
                <span className="flex items-center gap-2 font-bold">
                  <span aria-hidden className="text-xl">{t.emoji}</span>
                  {t.label}
                </span>
                {meals.length > 0 && <span className="text-sm font-extrabold text-primary">{fmt(calo)} kcal</span>}
              </div>
              {meals.length === 0 ? (
                <div className="mt-4 flex flex-1 flex-col items-start justify-end gap-2">
                  <p className="text-sm text-muted-foreground">Chưa có bữa ăn nào</p>
                  <Button asChild size="sm" variant="outline" className="rounded-full border-primary/40 text-primary">
                    <Link href={`/meals/new?loai=${t.value}`}>
                      <Plus /> Thêm ngay
                    </Link>
                  </Button>
                </div>
              ) : (
                <ul className="mt-3 space-y-2">
                  {meals.map((m) => (
                    <li key={m.id}>
                      <Link href={`/meals/${m.id}`} className="block rounded-xl bg-muted/60 px-3 py-2 transition-colors hover:bg-accent focus-visible:ring-2 focus-visible:ring-ring">
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
