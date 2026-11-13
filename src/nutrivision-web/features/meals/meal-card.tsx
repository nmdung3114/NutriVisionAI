"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { motion, useAnimationControls, useMotionValue, useTransform } from "framer-motion";
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

const SWIPE_DELETE = -96;

/** Trên thiết bị cảm ứng: vuốt thẻ sang trái để xóa (mở hộp thoại xác nhận như nút Xóa). */
export function MealCard({ meal, onDelete }: { meal: MealSummary; onDelete?: (m: MealSummary) => void }) {
  const [touch, setTouch] = useState(false);
  useEffect(() => setTouch(window.matchMedia("(pointer: coarse)").matches), []);
  const x = useMotionValue(0);
  const controls = useAnimationControls();
  const hint = useTransform(x, [0, SWIPE_DELETE], [0, 1]);
  const swipe = !!onDelete && touch;

  const card = <MealCardBody meal={meal} onDelete={onDelete} />;
  if (!swipe) return card;
  return (
    <div className="relative overflow-hidden rounded-2xl">
      <motion.div aria-hidden style={{ opacity: hint }} className="absolute inset-0 flex items-center justify-end rounded-2xl bg-destructive pr-6 text-white">
        <Trash2 className="size-6" />
      </motion.div>
      <motion.div
        drag="x"
        dragDirectionLock
        dragConstraints={{ left: SWIPE_DELETE * 1.4, right: 0 }}
        dragElastic={0.1}
        style={{ x }}
        animate={controls}
        onDragEnd={() => {
          if (x.get() <= SWIPE_DELETE) onDelete?.(meal);
          void controls.start({ x: 0, transition: { type: "spring", stiffness: 400, damping: 32 } });
        }}
        className="relative touch-pan-y"
      >
        {card}
      </motion.div>
    </div>
  );
}

function MealCardBody({ meal, onDelete }: { meal: MealSummary; onDelete?: (m: MealSummary) => void }) {
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
