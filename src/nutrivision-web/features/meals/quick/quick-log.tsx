"use client";

import { useState } from "react";
import { History, Loader2, Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { SafeImage } from "@/components/shared/safe-image";
import { useFrequentMeals } from "@/hooks/use-api";
import { fmt, mealTheme, mealTypeByHour, mealTypeEmoji, mealTypeLabel } from "@/lib/meal";
import { cn } from "@/lib/utils";
import type { LoaiBuaAn } from "@/types/api";
import { useRepeatMeal } from "./use-repeat-meal";

/**
 * Ghi nhanh từ các món hay ăn: một chạm để ghi lại món vào bữa hiện tại (theo giờ),
 * không cần chụp ảnh. Ẩn khi người dùng chưa có món nào.
 */
export function QuickLog({ title = "Ghi nhanh món hay ăn", compact = false, loai }: { title?: string; compact?: boolean; loai?: LoaiBuaAn }) {
  const { data, isLoading } = useFrequentMeals();
  const repeat = useRepeatMeal();
  const [pending, setPending] = useState<string | null>(null);

  if (isLoading) return <Skeleton className="h-36 rounded-2xl" />;
  if (!data || data.length === 0) return null;
  const loaiNow = loai ?? mealTypeByHour(new Date().getHours());

  return (
    <section className="rounded-2xl border bg-card p-4 shadow-soft sm:p-5" aria-label={title}>
      <div className="mb-3 flex items-center gap-2">
        <span className="flex size-9 items-center justify-center rounded-xl bg-blue/15 text-info">
          <History className="size-5" />
        </span>
        <div>
          <h2 className="text-lg font-extrabold leading-tight">{title}</h2>
          <p className="text-xs text-muted-foreground">
            Một chạm để ghi vào {mealTypeEmoji(loaiNow)} {loai ? mealTypeLabel(loaiNow).toLowerCase() : "bữa hiện tại"}, không cần chụp ảnh.
          </p>
        </div>
      </div>
      <ul className={cn("-mx-1 flex snap-x gap-3 overflow-x-auto px-1 pb-1", !compact && "lg:grid lg:grid-cols-3 lg:overflow-visible")}>
        {data.map((m) => {
          const busy = repeat.isPending && pending === m.mauBuaAnId;
          return (
            <li key={m.mauBuaAnId} className="w-56 shrink-0 snap-start lg:w-auto">
              <div className="group relative flex h-full items-center gap-3 overflow-hidden rounded-xl border bg-background/60 p-2.5 pl-3.5 transition-shadow hover:shadow-md">
                <span aria-hidden className={cn("absolute inset-y-0 left-0 w-1", mealTheme(m.loaiBuaAn).dot)} />
                <SafeImage src={m.hinhAnhUrl} alt={m.tenMonAn} className="size-12 shrink-0 rounded-lg" />
                <div className="min-w-0 flex-1">
                  <p className="truncate text-sm font-bold">{m.tenMonAn}</p>
                  <p className="text-xs text-muted-foreground">
                    {fmt(m.calo)} kcal · {m.soLan} lần
                  </p>
                </div>
                <Button
                  size="icon"
                  className="size-9 shrink-0 rounded-full"
                  aria-label={`Ăn lại ${m.tenMonAn}`}
                  disabled={repeat.isPending}
                  onClick={() => {
                    setPending(m.mauBuaAnId);
                    repeat.mutate({ id: m.mauBuaAnId, loaiBuaAn: loaiNow });
                  }}
                >
                  {busy ? <Loader2 className="animate-spin" /> : <Plus strokeWidth={3} />}
                </Button>
              </div>
            </li>
          );
        })}
      </ul>
    </section>
  );
}
