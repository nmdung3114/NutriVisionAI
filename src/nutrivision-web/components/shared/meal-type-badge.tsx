import { cn } from "@/lib/utils";
import { MEAL_TYPES, mealTheme, mealTypeEmoji, mealTypeLabel } from "@/lib/meal";
import type { LoaiBuaAn } from "@/types/api";

/** Nhãn loại bữa có màu riêng: 🌅 Bữa sáng (cam), ☀️ trưa (xanh), 🌙 tối (tím), 🍎 phụ (hồng). */
export function MealTypeBadge({ loai, className, short }: { loai: LoaiBuaAn | string; className?: string; short?: boolean }) {
  const t = mealTheme(loai);
  const label = mealTypeLabel(loai);
  return (
    <span className={cn("inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-bold", t.soft, t.text, className)}>
      <span aria-hidden>{mealTypeEmoji(loai)}</span>
      {short ? (MEAL_TYPES.find((m) => m.value === loai)?.short ?? label) : label}
    </span>
  );
}
