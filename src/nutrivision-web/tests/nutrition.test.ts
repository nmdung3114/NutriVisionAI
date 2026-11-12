import { describe, expect, it } from "vitest";
import { computeGram, gramPerUnit, macroEnergyShare, normalizeText, nutritionForGram, sumNutrition } from "@/lib/nutrition";
import { blankRow, rowFromFood, toUpsert, totalNutrition, validateState } from "@/features/meals/editor/editor-state";
import type { Food, UnitConversion } from "@/types/api";

const units: UnitConversion[] = [
  { donVi: "g", gram: 1 },
  { donVi: "bát", gram: 200 },
  { donVi: "quả", gram: 50 },
];
const rice: Food = {
  id: "f1", maThucPham: "CO", tenThucPham: "Cơm trắng", nhomThucPham: "Ngũ cốc", donViChuan: "bát", khoiLuongChuan: 150,
  caloTren100g: 130, chatDamTren100g: 2.7, tinhBotTren100g: 28, chatBeoTren100g: 0.3, chatXoTren100g: 0.4,
};

describe("quy đổi gram (contract §4)", () => {
  it("bỏ dấu và lower-case", () => expect(normalizeText(" Bát ")).toBe("bat"));
  it("g/ml = 1, kg/lít = 1000", () => {
    expect(gramPerUnit("g", null, units)).toBe(1);
    expect(gramPerUnit("ml", null, units)).toBe(1);
    expect(gramPerUnit("kg", null, units)).toBe(1000);
    expect(gramPerUnit("Lít", null, units)).toBe(1000);
  });
  it("đơn vị trùng donViChuan dùng khoiLuongChuan", () => expect(gramPerUnit("Bát", rice, units)).toBe(150));
  it("tra bảng units khi không trùng donViChuan", () => {
    expect(gramPerUnit("bát", null, units)).toBe(200);
    expect(gramPerUnit("quả", rice, units)).toBe(50);
  });
  it("đơn vị lạ trả null", () => expect(gramPerUnit("xyz", null, units)).toBeNull());
  it("computeGram", () => {
    expect(computeGram(2, "bát", rice, units)).toBe(300);
    expect(computeGram(0.5, "quả", null, units)).toBe(25);
    expect(computeGram(-1, "g", null, units)).toBe(0);
  });
});

describe("tính dinh dưỡng", () => {
  it("tren100g × gram / 100, làm tròn 2 chữ số", () => {
    const n = nutritionForGram({ calo: 130, chatDam: 2.7, tinhBot: 28, chatBeo: 0.3, chatXo: 0.4 }, 150);
    expect(n).toEqual({ calo: 195, chatDam: 4.05, tinhBot: 42, chatBeo: 0.45, chatXo: 0.6 });
  });
  it("sumNutrition", () => {
    expect(sumNutrition([{ calo: 1.111, chatDam: 1, tinhBot: 1, chatBeo: 1, chatXo: 1 }, { calo: 2.222, chatDam: 1, tinhBot: 1, chatBeo: 1, chatXo: 1 }]).calo).toBe(3.33);
  });
  it("macroEnergyShare", () => {
    expect(macroEnergyShare({ chatDam: 25, tinhBot: 50, chatBeo: 11.1 })).toEqual({ chatDam: 25, tinhBot: 50, chatBeo: 25 });
    expect(macroEnergyShare({ chatDam: 0, tinhBot: 0, chatBeo: 0 })).toEqual({ chatDam: 0, tinhBot: 0, chatBeo: 0 });
  });
});

describe("editor state", () => {
  it("tính tổng và tạo payload", () => {
    const r = rowFromFood(rice);
    expect(r.khauPhan).toBe("1");
    const state = { tenMonAn: " Cơm ", loaiBuaAn: "Trua" as const, ngayAn: "2026-01-01", rows: [r] };
    expect(totalNutrition(state.rows, units).calo).toBe(195);
    const up = toUpsert(state);
    expect(up.tenMonAn).toBe("Cơm");
    expect(up.thanhPhan[0]).toMatchObject({ thucPhamThamChieuId: "f1", khauPhan: 1, nhapTay: null });
  });
  it("validate: tên trống, không thành phần, khẩu phần ≤0, thiếu dữ liệu", () => {
    expect(validateState({ tenMonAn: "", loaiBuaAn: "Sang", ngayAn: "x", rows: [] })).toMatchObject({ ok: false, general: expect.any(String), tenMonAn: expect.any(String) });
    const bad = blankRow("Gì đó");
    const v = validateState({ tenMonAn: "A", loaiBuaAn: "Sang", ngayAn: "x", rows: [bad] });
    expect(v.ok).toBe(false);
    expect(v.rows[bad.key]).toContain("dữ liệu dinh dưỡng");
    const zero = { ...rowFromFood(rice), khauPhan: "0" };
    expect(validateState({ tenMonAn: "A", loaiBuaAn: "Sang", ngayAn: "x", rows: [zero] }).rows[zero.key]).toContain("lớn hơn 0");
  });
  it("nhập tay gửi nhapTay", () => {
    const r = { ...blankRow("Tự nấu"), per100: { calo: 100, chatDam: 1, tinhBot: 2, chatBeo: 3, chatXo: 0 }, nhapTay: true };
    expect(toUpsert({ tenMonAn: "A", loaiBuaAn: "Sang", ngayAn: "x", rows: [r] }).thanhPhan[0].nhapTay).toEqual({ caloTren100g: 100, chatDamTren100g: 1, tinhBotTren100g: 2, chatBeoTren100g: 3, chatXoTren100g: 0 });
  });
});
