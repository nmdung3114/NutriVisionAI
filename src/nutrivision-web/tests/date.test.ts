import { describe, expect, it } from "vitest";
import { addDays, weekdayName, weekdayShort } from "@/lib/date";

describe("weekdayShort", () => {
  it("viết tắt đúng cả tuần (02/11/2026 là Thứ hai)", () => {
    const week = Array.from({ length: 7 }, (_, i) => addDays("2026-11-02", i));
    expect(week.map(weekdayShort)).toEqual(["T2", "T3", "T4", "T5", "T6", "T7", "CN"]);
    expect(week.map(weekdayName)).toEqual(["Thứ hai", "Thứ ba", "Thứ tư", "Thứ năm", "Thứ sáu", "Thứ bảy", "Chủ nhật"]);
  });
});
