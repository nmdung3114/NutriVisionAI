import { describe, expect, it } from "vitest";
import { DEFAULT_REMINDERS, dueReminders } from "@/lib/reminders";

const on = { ...DEFAULT_REMINDERS, enabled: true }; // 07:30 / 12:00 / 18:30
const at = (h: number, m = 0) => new Date(2026, 10, 12, h, m);

describe("dueReminders", () => {
  it("không nhắc khi tắt", () => {
    expect(dueReminders(at(12, 5), DEFAULT_REMINDERS, new Set(), new Set())).toEqual([]);
  });

  it("nhắc đúng bữa khi tới giờ, trong cửa sổ 2 giờ", () => {
    expect(dueReminders(at(7, 29), on, new Set(), new Set())).toEqual([]);
    expect(dueReminders(at(7, 30), on, new Set(), new Set())).toEqual(["Sang"]);
    expect(dueReminders(at(9, 29), on, new Set(), new Set())).toEqual(["Sang"]);
    expect(dueReminders(at(9, 30), on, new Set(), new Set())).toEqual([]); // quá 2 giờ
    expect(dueReminders(at(19, 0), on, new Set(), new Set())).toEqual(["Toi"]);
  });

  it("không nhắc lại bữa đã nhắc hôm nay hoặc đã ghi", () => {
    expect(dueReminders(at(12, 10), on, new Set(["2026-11-12:Trua"]), new Set())).toEqual([]);
    expect(dueReminders(at(12, 10), on, new Set(["2026-11-11:Trua"]), new Set())).toEqual(["Trua"]); // hôm qua không tính
    expect(dueReminders(at(12, 10), on, new Set(), new Set(["Trua"]))).toEqual([]);
  });

  it("dùng giờ tùy chỉnh", () => {
    const custom = { enabled: true, times: { Sang: "06:00", Trua: "11:15", Toi: "20:00" } };
    expect(dueReminders(at(11, 20), custom, new Set(), new Set())).toEqual(["Trua"]);
  });
});
