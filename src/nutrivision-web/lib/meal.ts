import type { LoaiBuaAn } from "@/types/api";

export const MEAL_TYPES: { value: LoaiBuaAn; label: string; short: string; emoji: string }[] = [
  { value: "Sang", label: "Bữa sáng", short: "Sáng", emoji: "🌅" },
  { value: "Trua", label: "Bữa trưa", short: "Trưa", emoji: "☀️" },
  { value: "Toi", label: "Bữa tối", short: "Tối", emoji: "🌙" },
  { value: "Phu", label: "Bữa phụ", short: "Phụ", emoji: "🍎" },
];

/**
 * Màu nhận diện từng loại bữa (sáng cam-vàng, trưa xanh lá, tối tím-xanh, phụ hồng-cam).
 * Viết đầy đủ tên class để Tailwind quét được; dùng màu có độ trong suốt nên hợp cả nền sáng lẫn tối.
 */
export const MEAL_THEME: Record<LoaiBuaAn, { tint: string; soft: string; text: string; border: string; dot: string; solid: string; hex: string }> = {
  Sang: {
    tint: "bg-gradient-to-br from-yellow/30 via-orange/10 to-transparent",
    soft: "bg-yellow/20",
    text: "text-warn",
    border: "border-yellow/60",
    dot: "bg-yellow",
    solid: "bg-gradient-to-br from-[#ffd25e] to-[#ff9a3c] text-[#3b2a00]",
    hex: "#f5a524",
  },
  Trua: {
    tint: "bg-gradient-to-br from-brand-500/25 via-brand-500/5 to-transparent",
    soft: "bg-brand-500/15",
    text: "text-brand-700",
    border: "border-brand-500/50",
    dot: "bg-brand-500",
    solid: "bg-gradient-to-br from-[#2ed16f] to-[#168d47] text-white",
    hex: "#21b45a",
  },
  Toi: {
    tint: "bg-gradient-to-br from-purple/25 via-blue/10 to-transparent",
    soft: "bg-purple/15",
    text: "text-violet",
    border: "border-purple/50",
    dot: "bg-purple",
    solid: "bg-gradient-to-br from-[#8f6bf0] to-[#4b6cf5] text-white",
    hex: "#9d68ed",
  },
  Phu: {
    tint: "bg-gradient-to-br from-pink/25 via-orange/10 to-transparent",
    soft: "bg-pink/15",
    text: "text-danger-strong",
    border: "border-pink/50",
    dot: "bg-pink",
    solid: "bg-gradient-to-br from-[#ff7aa8] to-[#ff795e] text-white",
    hex: "#ff6fa8",
  },
};

export function mealTheme(v: LoaiBuaAn | string) {
  return isMealType(v) ? MEAL_THEME[v] : MEAL_THEME.Phu;
}

export function mealTypeEmoji(v: LoaiBuaAn | string): string {
  return MEAL_TYPES.find((m) => m.value === v)?.emoji ?? "🍽️";
}

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
