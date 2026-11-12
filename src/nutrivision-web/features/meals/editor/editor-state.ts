import type { AiOriginal, ComponentInput, Food, LoaiBuaAn, Meal, MealComponent, MealUpsert, Nutrition, Per100g, UnitConversion } from "@/types/api";
import {
  computeGram,
  foodToPer100g,
  normalizeText,
  nutritionForGram,
  nutritionToPer100g,
  per100gToNutrition,
  sumNutrition,
  ZERO_NUTRITION,
} from "@/lib/nutrition";

/** Một dòng thành phần trong trình chỉnh sửa (trạng thái cục bộ, tính toán tức thời). */
export interface EditorRow {
  key: string;
  id: string | null;
  tenThanhPhan: string;
  /** Giữ dạng chuỗi để người dùng gõ tự do; parse khi tính toán. */
  khauPhan: string;
  donViTinh: string;
  thamChieu: Food | null;
  thucPhamThamChieuId: string | null;
  /** Dinh dưỡng trên 100g đang áp dụng (từ thực phẩm tham chiếu / nhập tay / server). */
  per100: Nutrition | null;
  /** true nếu dữ liệu là do người dùng nhập tay -> gửi `nhapTay`. */
  nhapTay: boolean;
}

export interface EditorState {
  tenMonAn: string;
  loaiBuaAn: LoaiBuaAn;
  ngayAn: string;
  rows: EditorRow[];
}

let seq = 0;
export const newKey = () => `r${Date.now().toString(36)}${(seq++).toString(36)}`;

export function rowFromComponent(c: MealComponent): EditorRow {
  const hasRef = !!(c.thamChieu || c.thucPhamThamChieuId);
  let per100: Nutrition | null = null;
  if (c.thamChieu) per100 = per100gToNutrition(foodToPer100g(c.thamChieu));
  else if (c.dinhDuongTren100g && !c.thieuDuLieuDinhDuong) per100 = c.dinhDuongTren100g;
  return {
    key: newKey(),
    id: c.id,
    tenThanhPhan: c.tenThanhPhan,
    khauPhan: String(c.khauPhan),
    donViTinh: c.donViTinh,
    thamChieu: c.thamChieu,
    thucPhamThamChieuId: c.thucPhamThamChieuId ?? c.thamChieu?.id ?? null,
    per100,
    nhapTay: !hasRef && !!per100,
  };
}

export function mealToState(m: Meal): EditorState {
  return { tenMonAn: m.tenMonAn, loaiBuaAn: m.loaiBuaAn, ngayAn: m.ngayAn, rows: m.thanhPhan.map(rowFromComponent) };
}

/** Đơn vị mặc định + khẩu phần mặc định khi thêm một thực phẩm từ danh mục. */
export function rowFromFood(f: Food): EditorRow {
  const u = normalizeText(f.donViChuan);
  const isMass = u === "g" || u === "gram" || u === "ml";
  return {
    key: newKey(),
    id: null,
    tenThanhPhan: f.tenThucPham,
    khauPhan: String(isMass ? (f.khoiLuongChuan > 0 ? f.khoiLuongChuan : 100) : 1),
    donViTinh: f.donViChuan,
    thamChieu: f,
    thucPhamThamChieuId: f.id,
    per100: per100gToNutrition(foodToPer100g(f)),
    nhapTay: false,
  };
}

export function blankRow(name = ""): EditorRow {
  return { key: newKey(), id: null, tenThanhPhan: name, khauPhan: "100", donViTinh: "g", thamChieu: null, thucPhamThamChieuId: null, per100: null, nhapTay: false };
}

export function parseAmount(s: string): number {
  const n = Number(String(s).replace(",", "."));
  return Number.isFinite(n) ? n : NaN;
}

export function rowGram(row: EditorRow, units: UnitConversion[]): number {
  const n = parseAmount(row.khauPhan);
  return computeGram(n, row.donViTinh, row.thamChieu, units);
}

export function rowNutrition(row: EditorRow, units: UnitConversion[]): Nutrition | null {
  if (!row.per100) return null;
  return nutritionForGram(row.per100, rowGram(row, units));
}

export const rowNeedsData = (row: EditorRow) => !row.per100;

export function totalNutrition(rows: EditorRow[], units: UnitConversion[]): Nutrition {
  return sumNutrition(rows.map((r) => rowNutrition(r, units) ?? ZERO_NUTRITION));
}

export interface ValidationResult {
  ok: boolean;
  tenMonAn?: string;
  general?: string;
  /** Lỗi theo key dòng. */
  rows: Record<string, string>;
}

export function validateState(s: EditorState): ValidationResult {
  const rows: Record<string, string> = {};
  let ok = true;
  const res: ValidationResult = { ok: true, rows };
  if (!s.tenMonAn.trim()) {
    res.tenMonAn = "Tên món ăn không được để trống";
    ok = false;
  } else if (s.tenMonAn.trim().length > 200) {
    res.tenMonAn = "Tên món ăn tối đa 200 ký tự";
    ok = false;
  }
  if (s.rows.length === 0) {
    res.general = "Bữa ăn cần có ít nhất một thành phần";
    ok = false;
  }
  for (const r of s.rows) {
    const n = parseAmount(r.khauPhan);
    if (!r.tenThanhPhan.trim()) rows[r.key] = "Tên thành phần không được để trống";
    else if (!(n > 0)) rows[r.key] = "Khẩu phần phải lớn hơn 0";
    else if (!r.per100) rows[r.key] = "Chưa có dữ liệu dinh dưỡng – hãy chọn thực phẩm gần đúng, nhập tay hoặc xóa";
    if (rows[r.key]) ok = false;
  }
  res.ok = ok;
  return res;
}

export function rowToInput(r: EditorRow): ComponentInput {
  const input: ComponentInput = {
    tenThanhPhan: r.tenThanhPhan.trim(),
    khauPhan: parseAmount(r.khauPhan),
    donViTinh: r.donViTinh,
    thucPhamThamChieuId: r.thamChieu?.id ?? r.thucPhamThamChieuId ?? null,
    nhapTay: null,
  };
  if (!input.thucPhamThamChieuId && r.per100) input.nhapTay = nutritionToPer100g(r.per100) satisfies Per100g;
  return input;
}

export function toUpsert(s: EditorState): MealUpsert {
  return { loaiBuaAn: s.loaiBuaAn, ngayAn: s.ngayAn, tenMonAn: s.tenMonAn.trim(), thanhPhan: s.rows.map(rowToInput) };
}

/** So sánh hai trạng thái (bỏ qua key) để biết người dùng đã chỉnh sửa chưa. */
export function isDirty(a: EditorState, b: EditorState): boolean {
  const strip = (s: EditorState) =>
    JSON.stringify({
      t: s.tenMonAn,
      l: s.loaiBuaAn,
      n: s.ngayAn,
      r: s.rows.map((r) => [r.tenThanhPhan, r.khauPhan, r.donViTinh, r.thucPhamThamChieuId, r.per100]),
    });
  return strip(a) !== strip(b);
}

export function normalizeConfidence(v: number | null | undefined): number | null {
  if (v === null || v === undefined || Number.isNaN(v)) return null;
  return Math.round(v <= 1 ? v * 100 : v);
}

/** Tóm tắt AI gốc để hiển thị so sánh. */
export function aiOriginalLines(ai: AiOriginal): string[] {
  return ai.thanhPhan.map((t) => `${t.tenThanhPhan} – ${t.khauPhan} ${t.donViTinh}`);
}

export const per100FromNutrition = (n: Nutrition): Nutrition => ({ ...n });
