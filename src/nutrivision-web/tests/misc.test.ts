import { describe, expect, it } from "vitest";
import { evaluatePassword } from "@/lib/password";
import { addDays, calcAge, formatDate, greetingByHour, lastNDays, relativeDayLabel, toDateInput } from "@/lib/date";
import { mealTypeByHour } from "@/lib/meal";
import { bmi, bmiClass, bmr, tdee } from "@/lib/health";

describe("password strength", () => {
  it("rỗng", () => expect(evaluatePassword("").score).toBe(0));
  it("quá ngắn / thiếu số không hợp lệ", () => {
    expect(evaluatePassword("abc123").valid).toBe(false);
    expect(evaluatePassword("abcdefgh").valid).toBe(false);
  });
  it("hợp lệ tối thiểu", () => expect(evaluatePassword("abcdefg1").valid).toBe(true));
  it("mạnh hơn khi đa dạng", () => {
    expect(evaluatePassword("Abcdefg1!xyz").score).toBeGreaterThan(evaluatePassword("abcdefg1").score);
    expect(evaluatePassword("Abcdefg1!xyz").score).toBe(4);
  });
});

describe("date helpers", () => {
  it("toDateInput/addDays qua ranh giới tháng", () => {
    expect(addDays("2026-03-01", -1)).toBe("2026-02-28");
    expect(addDays("2026-12-31", 1)).toBe("2027-01-01");
    expect(toDateInput(new Date(2026, 0, 5))).toBe("2026-01-05");
  });
  it("lastNDays gồm cả hôm nay", () => expect(lastNDays(7, "2026-05-10")).toEqual({ from: "2026-05-04", to: "2026-05-10" }));
  it("formatDate & nhãn tương đối", () => {
    expect(formatDate("2026-05-04")).toBe("04/05/2026");
    expect(relativeDayLabel("2026-05-10", "2026-05-10")).toBe("Hôm nay");
    expect(relativeDayLabel("2026-05-09", "2026-05-10")).toBe("Hôm qua");
    expect(relativeDayLabel("2026-05-04", "2026-05-10")).toBe("Thứ hai, 04/05/2026");
  });
  it("calcAge", () => {
    expect(calcAge("2000-06-15", new Date(2026, 5, 14))).toBe(25);
    expect(calcAge("2000-06-15", new Date(2026, 5, 15))).toBe(26);
  });
  it("lời chào theo giờ", () => {
    expect(greetingByHour(8)).toBe("Chào buổi sáng");
    expect(greetingByHour(12)).toBe("Chào buổi trưa");
    expect(greetingByHour(15)).toBe("Chào buổi chiều");
    expect(greetingByHour(21)).toBe("Chào buổi tối");
  });
});

describe("meal type by hour", () => {
  it.each([[6, "Sang"], [9, "Sang"], [11, "Trua"], [13, "Trua"], [18, "Toi"], [20, "Toi"], [3, "Phu"], [10, "Phu"], [15, "Phu"], [22, "Phu"]])("%i giờ -> %s", (h, t) => {
    expect(mealTypeByHour(h as number)).toBe(t);
  });
});

describe("health", () => {
  it("BMI, phân loại WHO", () => {
    expect(bmi(170, 65)).toBe(22.5);
    expect(bmiClass(17).label).toBe("Gầy");
    expect(bmiClass(22).label).toBe("Bình thường");
    expect(bmiClass(27).label).toBe("Thừa cân");
    expect(bmiClass(31).label).toBe("Béo phì");
  });
  it("Mifflin–St Jeor", () => {
    expect(bmr("Nam", 30, 175, 70)).toBe(1649);
    expect(bmr("Nu", 30, 160, 55)).toBe(1239);
    expect(tdee(1649, "VuaPhai")).toBe(2556);
  });
});
