import type { Food, Nutrition, Per100g, UnitConversion } from "@/types/api";

export const ZERO_NUTRITION: Nutrition = { calo: 0, chatDam: 0, tinhBot: 0, chatBeo: 0, chatXo: 0 };

export const round2 = (n: number) => Math.round((n + Number.EPSILON) * 100) / 100;

/** Bỏ dấu tiếng Việt + lower-case (dùng để so khớp đơn vị tính). */
export function normalizeText(s: string): string {
  return s
    .normalize("NFD")
    .replace(/[̀-ͯ]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D")
    .toLowerCase()
    .trim();
}

const FIXED_UNITS: Record<string, number> = { g: 1, gram: 1, ml: 1, kg: 1000, lit: 1000 };

/**
 * Quy đổi 1 đơn vị -> gram (contract §4):
 * g/gram = 1, ml = 1, kg/lít = 1000; trùng donViChuan của thực phẩm => khoiLuongChuan; còn lại tra bảng units.
 * Trả null nếu không biết đơn vị.
 */
export function gramPerUnit(
  donViTinh: string,
  food: Pick<Food, "donViChuan" | "khoiLuongChuan"> | null | undefined,
  units: UnitConversion[],
): number | null {
  const u = normalizeText(donViTinh);
  if (u in FIXED_UNITS) return FIXED_UNITS[u];
  if (food && normalizeText(food.donViChuan) === u) return food.khoiLuongChuan;
  const hit = units.find((x) => normalizeText(x.donVi) === u);
  return hit ? hit.gram : null;
}

export function computeGram(
  khauPhan: number,
  donViTinh: string,
  food: Pick<Food, "donViChuan" | "khoiLuongChuan"> | null | undefined,
  units: UnitConversion[],
): number {
  if (!Number.isFinite(khauPhan) || khauPhan <= 0) return 0;
  const per = gramPerUnit(donViTinh, food, units) ?? 1;
  return round2(khauPhan * per);
}

export function foodToPer100g(f: Food): Per100g {
  return {
    caloTren100g: f.caloTren100g,
    chatDamTren100g: f.chatDamTren100g,
    tinhBotTren100g: f.tinhBotTren100g,
    chatBeoTren100g: f.chatBeoTren100g,
    chatXoTren100g: f.chatXoTren100g,
  };
}

export function per100gToNutrition(p: Per100g): Nutrition {
  return {
    calo: p.caloTren100g,
    chatDam: p.chatDamTren100g,
    tinhBot: p.tinhBotTren100g,
    chatBeo: p.chatBeoTren100g,
    chatXo: p.chatXoTren100g,
  };
}

export function nutritionToPer100g(n: Nutrition): Per100g {
  return {
    caloTren100g: n.calo,
    chatDamTren100g: n.chatDam,
    tinhBotTren100g: n.tinhBot,
    chatBeoTren100g: n.chatBeo,
    chatXoTren100g: n.chatXo,
  };
}

/** Dinh dưỡng = tren100g × gram / 100, làm tròn 2 chữ số. */
export function nutritionForGram(per100: Nutrition, gram: number): Nutrition {
  const k = gram / 100;
  return {
    calo: round2(per100.calo * k),
    chatDam: round2(per100.chatDam * k),
    tinhBot: round2(per100.tinhBot * k),
    chatBeo: round2(per100.chatBeo * k),
    chatXo: round2(per100.chatXo * k),
  };
}

export function sumNutrition(items: Nutrition[]): Nutrition {
  const t = items.reduce(
    (a, n) => ({
      calo: a.calo + n.calo,
      chatDam: a.chatDam + n.chatDam,
      tinhBot: a.tinhBot + n.tinhBot,
      chatBeo: a.chatBeo + n.chatBeo,
      chatXo: a.chatXo + n.chatXo,
    }),
    { ...ZERO_NUTRITION },
  );
  return {
    calo: round2(t.calo),
    chatDam: round2(t.chatDam),
    tinhBot: round2(t.tinhBot),
    chatBeo: round2(t.chatBeo),
    chatXo: round2(t.chatXo),
  };
}

/** Năng lượng từ macro (4/4/9 kcal/g) -> % năng lượng. */
export function macroEnergyShare(n: Pick<Nutrition, "chatDam" | "tinhBot" | "chatBeo">) {
  const p = n.chatDam * 4;
  const c = n.tinhBot * 4;
  const f = n.chatBeo * 9;
  const total = p + c + f;
  if (total <= 0) return { chatDam: 0, tinhBot: 0, chatBeo: 0 };
  return {
    chatDam: Math.round((p / total) * 100),
    tinhBot: Math.round((c / total) * 100),
    chatBeo: Math.round((f / total) * 100),
  };
}

export function percentOf(value: number, target: number): number {
  if (!target || target <= 0) return 0;
  return Math.round((value / target) * 100);
}

/** Mục tiêu macro/ngày (gram) theo cơ cấu năng lượng 20% đạm / 50% tinh bột / 30% béo. */
export function dailyMacroTargets(targetCalo: number) {
  return {
    chatDam: round2((targetCalo * 0.2) / 4),
    tinhBot: round2((targetCalo * 0.5) / 4),
    chatBeo: round2((targetCalo * 0.3) / 9),
    chatXo: 25,
  };
}
