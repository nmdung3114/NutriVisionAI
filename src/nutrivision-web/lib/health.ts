import type { ActivityLevel, Gender } from "@/types/api";

export const ACTIVITY_LEVELS: { value: ActivityLevel; label: string; factor: number; desc: string }[] = [
  { value: "ItVanDong", label: "Ít vận động", factor: 1.2, desc: "Làm việc văn phòng, hầu như không tập thể dục" },
  { value: "VanDongNhe", label: "Vận động nhẹ", factor: 1.375, desc: "Tập nhẹ 1–3 buổi/tuần" },
  { value: "VuaPhai", label: "Vận động vừa phải", factor: 1.55, desc: "Tập vừa 3–5 buổi/tuần" },
  { value: "NangDong", label: "Năng động", factor: 1.725, desc: "Tập nặng 6–7 buổi/tuần" },
  { value: "RatNangDong", label: "Rất năng động", factor: 1.9, desc: "Lao động nặng hoặc tập luyện 2 lần/ngày" },
];

export function bmi(heightCm: number, weightKg: number): number {
  const m = heightCm / 100;
  return Math.round((weightKg / (m * m)) * 10) / 10;
}

export function bmiClass(v: number): { label: string; color: string; bg: string } {
  if (v < 18.5) return { label: "Gầy", color: "#2a6fc4", bg: "bg-blue/15 text-[#2a6fc4]" };
  if (v < 25) return { label: "Bình thường", color: "#1a9a4c", bg: "bg-brand-100 text-brand-700" };
  if (v < 30) return { label: "Thừa cân", color: "#c98a00", bg: "bg-yellow/30 text-[#8a5a00]" };
  return { label: "Béo phì", color: "#e5484d", bg: "bg-red-100 text-red-700" };
}

/** Mifflin–St Jeor (chỉ dùng để xem trước; server là nguồn chính thức). */
export function bmr(gender: Gender, age: number, heightCm: number, weightKg: number): number {
  const base = 10 * weightKg + 6.25 * heightCm - 5 * age;
  return Math.round(gender === "Nam" ? base + 5 : base - 161);
}

export function tdee(bmrValue: number, level: ActivityLevel): number {
  const f = ACTIVITY_LEVELS.find((a) => a.value === level)?.factor ?? 1.2;
  return Math.round(bmrValue * f);
}
