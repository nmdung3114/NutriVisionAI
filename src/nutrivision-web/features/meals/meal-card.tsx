"use client";

import Link from "next/link";
import { Eye, Pencil, Sparkles, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { SafeImage } from "@/components/shared/safe-image";
import { MACRO_COLORS, fmt, mealTheme } from "@/lib/meal";
import { MealTypeBadge } from "@/components/shared/meal-type-badge";
import { cn } from "@/lib/utils";
import { formatTime } from "@/lib/date";
import type { MealSummary } from "@/types/api";

function Chip({ label, value, unit, color }: { label: string; value: number; unit: string; color: string }) {
  return (
    <span className="inline-flex items-center gap-1 rounded-full bg-muted px-2.5 py-1 text-xs font-semibold">
      <span className="size-2 rounded-full" style={{ background: color }} />
      {label} {fmt(value, unit === "kcal" ? 0 : 1)}
      {unit}
    </span>
  );
}

export function MealCard({ meal, onDelete }: { meal: MealSummary; onDelete?: (m: MealSummary) => void }) {
  const n = meal.tongDinhDuong;
  return (
    <article className="relative flex gap-3 overflow-hidden rounded-2xl border bg-card p-3 pl-4 shadow-soft transition-shadow hover:shadow-md sm:gap-4 sm:p-4 sm:pl-5">
      <span aria-hidden className={cn("absolute inset-y-0 left-0 w-1.5", mealTheme(meal.loaiBuaAn).dot)} />
      <Link href={`/meals/${meal.id}`} className="shrink-0 overflow-hidden rounded-xl" aria-label={`Xem chi tiết ${meal.tenMonAn}`}>
        <SafeImage src={meal.hinhAnhUrl} alt={meal.tenMonAn} className="size-20 sm:size-24" />
      </Link>
      <div className="min-w-0 flex-1">
        <p className="flex items-center gap-1.5 text-xs font-semibold text-muted-foreground">
          <MealTypeBadge loai={meal.loaiBuaAn} short /> {formatTime(meal.thoiGianGhiNhan)}
          {meal.taoTuAI && (
            <span className="inline-flex items-center gap-0.5 text-primary">
              <Sparkles className="size-3" /> AI
            </span>
          )}
        </p>
        <h3 className="truncate text-base font-extrabold">
          <Link href={`/meals/${meal.id}`} className="hover:text-primary">
            {meal.tenMonAn}
          </Link>
        </h3>
        <div className="mt-2 flex flex-wrap gap-1.5">
          <Chip label="" value={n.calo} unit="kcal" color={MACRO_COLORS.calo} />
          <Chip label="Đạm" value={n.chatDam} unit="g" color={MACRO_COLORS.chatDam} />
          <Chip label="Tinh bột" value={n.tinhBot} unit="g" color={MACRO_COLORS.tinhBot} />
          <Chip label="Béo" value={n.chatBeo} unit="g" color={MACRO_COLORS.chatBeo} />
        </div>
      </div>
      <div className="flex shrink-0 flex-col gap-1 sm:flex-row sm:items-start">
        <Button asChild variant="ghost" size="icon" aria-label="Xem chi tiết">
          <Link href={`/meals/${meal.id}`}>
            <Eye className="size-4" />
          </Link>
        </Button>
        <Button asChild variant="ghost" size="icon" aria-label="Sửa bữa ăn">
          <Link href={`/meals/${meal.id}?edit=1`}>
            <Pencil className="size-4" />
          </Link>
        </Button>
        {onDelete && (
          <Button variant="ghost" size="icon" aria-label="Xóa bữa ăn" className="text-destructive hover:bg-destructive/10" onClick={() => onDelete(meal)}>
            <Trash2 className="size-4" />
          </Button>
        )}
      </div>
    </article>
  );
}
