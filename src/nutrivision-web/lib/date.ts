const pad = (n: number) => String(n).padStart(2, "0");

/** Date -> "yyyy-MM-dd" theo giờ địa phương. */
export function toDateInput(d: Date): string {
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
}

export const todayStr = () => toDateInput(new Date());

/** Parse "yyyy-MM-dd" thành Date địa phương (tránh lệch múi giờ). */
export function parseDateOnly(s: string): Date {
  const [y, m, d] = s.split("-").map(Number);
  return new Date(y, (m || 1) - 1, d || 1);
}

export function addDays(s: string, n: number): string {
  const d = parseDateOnly(s);
  d.setDate(d.getDate() + n);
  return toDateInput(d);
}

/** Khoảng N ngày kết thúc hôm nay (bao gồm hôm nay). */
export function lastNDays(n: number, today: string = todayStr()): { from: string; to: string } {
  return { from: addDays(today, -(n - 1)), to: today };
}

/** yyyy-MM-dd -> dd/MM/yyyy */
export function formatDate(s: string | null | undefined): string {
  if (!s) return "";
  const [y, m, d] = s.slice(0, 10).split("-");
  return `${d}/${m}/${y}`;
}

/** yyyy-MM-dd -> dd/MM */
export function formatDayMonth(s: string): string {
  const [, m, d] = s.slice(0, 10).split("-");
  return `${d}/${m}`;
}

const WEEKDAYS = ["Chủ nhật", "Thứ hai", "Thứ ba", "Thứ tư", "Thứ năm", "Thứ sáu", "Thứ bảy"];

export function weekdayName(s: string): string {
  return WEEKDAYS[parseDateOnly(s).getDay()];
}

/** Thứ viết tắt: CN, T2 … T7. */
export function weekdayShort(s: string): string {
  const d = parseDateOnly(s).getDay();
  return d === 0 ? "CN" : `T${d + 1}`;
}

/** "Hôm nay" / "Hôm qua" / "Thứ ba, 12/05/2026" */
export function relativeDayLabel(s: string, today: string = todayStr()): string {
  if (s === today) return "Hôm nay";
  if (s === addDays(today, -1)) return "Hôm qua";
  return `${weekdayName(s)}, ${formatDate(s)}`;
}

export function formatTime(iso: string): string {
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return "";
  return `${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

export function formatDateTime(iso: string): string {
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return "";
  return `${pad(d.getDate())}/${pad(d.getMonth() + 1)}/${d.getFullYear()} ${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

export function greetingByHour(hour: number): string {
  if (hour < 11) return "Chào buổi sáng";
  if (hour < 14) return "Chào buổi trưa";
  if (hour < 18) return "Chào buổi chiều";
  return "Chào buổi tối";
}

export function calcAge(birth: string, today: Date = new Date()): number {
  const b = parseDateOnly(birth);
  let age = today.getFullYear() - b.getFullYear();
  const m = today.getMonth() - b.getMonth();
  if (m < 0 || (m === 0 && today.getDate() < b.getDate())) age--;
  return age;
}
