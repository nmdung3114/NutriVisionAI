"use client";

import { Lightbulb } from "lucide-react";
import { Skeleton } from "@/components/ui/skeleton";
import { useSuggestions } from "@/hooks/use-api";
import { fmt } from "@/lib/meal";

export function Suggestions() {
  const { data, isLoading, isError } = useSuggestions();
  return (
    <section className="rounded-2xl border bg-card p-5 shadow-soft" aria-label="Gợi ý món ăn">
      <h2 className="mb-1 flex items-center gap-2 text-lg font-extrabold">
        <Lightbulb className="size-5 text-yellow" /> Gợi ý cho bạn
      </h2>
      <p className="mb-4 text-xs text-muted-foreground">Dựa trên lịch sử 30 ngày và mục tiêu calo của bạn.</p>
      {isLoading ? (
        <div className="space-y-3">
          {[0, 1, 2].map((i) => (
            <Skeleton key={i} className="h-14 rounded-xl" />
          ))}
        </div>
      ) : isError ? (
        <p className="text-sm text-muted-foreground">Chưa tải được gợi ý lúc này.</p>
      ) : !data?.length ? (
        <p className="text-sm text-muted-foreground">Hãy ghi nhận thêm bữa ăn để nhận gợi ý phù hợp.</p>
      ) : (
        <ul className="space-y-2.5">
          {data.map((s) => (
            <li key={s.food.id} className="flex items-start gap-3 rounded-xl bg-muted/60 p-3">
              <span className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-yellow/30 text-lg" aria-hidden>
                🥗
              </span>
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-bold">{s.food.tenThucPham}</p>
                <p className="text-xs text-muted-foreground">{s.lyDo}</p>
              </div>
              <span className="shrink-0 text-xs font-semibold text-primary">{fmt(s.food.caloTren100g)} kcal/100g</span>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
