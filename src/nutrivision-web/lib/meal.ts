import type { LoaiBuaAn } from "@/types/api";

export const MEAL_TYPES: { value: LoaiBuaAn; label: string; short: string; emoji: string }[] = [
  { value: "Sang", label: "Bữa sáng", short: "Sáng", emoji: "🌅" },
  { value: "Trua", label: "Bữa trưa", short: "Trưa", emoji: "☀️" },
  { value: "Toi", label: "Bữa tối", short: "Tối", emoji: "🌙" },
  { value: "Phu", label: "Bữa phụ", short: "Phụ", emoji: "🍎" },
];

export function mealTypeLabel(v: LoaiBuaAn | string): string {
  return MEAL_TYPES.find((m) => m.value === v)?.label ?? String(v);
}

/** Gợi ý loại bữa theo giờ: 6–9h Sáng, 11–13h Trưa, 18–20h Tối, còn lại Phụ. */
export function mealTypeByHour(hour: number): LoaiBuaAn {
  if (hour >= 6 && hour < 10) return "Sang";
  if (hour >= 11 && hour < 14) return "Trua";
  if (hour >= 18 && hour < 21) return "Toi";
  return "Phu";
}

export function isMealType(v: string | null | undefined): v is LoaiBuaAn {
  return v === "Sang" || v === "Trua" || v === "Toi" || v === "Phu";
}

export const MACRO_COLORS = {
  calo: "#21b45a",
  chatDam: "#ff795e",
  tinhBot: "#ffc934",
  chatBeo: "#4b9cf5",
  chatXo: "#9d68ed",
} as const;

export const fmt = (n: number | null | undefined, digits = 0): string =>
  n === null || n === undefined || Number.isNaN(n)
    ? "–"
    : n.toLocaleString("vi-VN", { maximumFractionDigits: digits, minimumFractionDigits: 0 });
