import type { DayState } from "@/types/api";

export const STATE_META: Record<DayState, { label: string; color: string; bg: string }> = {
  Thieu: { label: "Thiếu", color: "#f5a524", bg: "bg-yellow/30 text-warn" },
  DatChuan: { label: "Đạt chuẩn", color: "#21b45a", bg: "bg-brand-100 text-brand-700" },
  Vuot: { label: "Vượt", color: "#ef4d4d", bg: "bg-destructive/15 text-destructive" },
};
