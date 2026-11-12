import type { LoaiBuaAn } from "@/types/api";

export type MainMeal = Extract<LoaiBuaAn, "Sang" | "Trua" | "Toi">;

export interface ReminderSettings {
  enabled: boolean;
  times: Record<MainMeal, string>; // "HH:mm"
}

export const DEFAULT_REMINDERS: ReminderSettings = { enabled: false, times: { Sang: "07:30", Trua: "12:00", Toi: "18:30" } };
export const MAIN_MEALS: MainMeal[] = ["Sang", "Trua", "Toi"];
/** Quá 2 giờ sau giờ nhắc thì bỏ qua (mở app buổi tối không bị nhắc bữa sáng). */
export const REMINDER_WINDOW_MIN = 120;

const KEY = "nv.reminders";
const FIRED_KEY = "nv.reminders.fired";

const minutes = (hhmm: string) => {
  const [h, m] = hhmm.split(":").map(Number);
  return h * 60 + m;
};

export const dayKey = (d: Date) => `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;

/**
 * Các bữa cần nhắc ngay lúc `now`: đã bật, đã tới giờ (trong cửa sổ 2 giờ),
 * hôm nay chưa nhắc và chưa ghi bữa đó.
 */
export function dueReminders(now: Date, s: ReminderSettings, fired: ReadonlySet<string>, logged: ReadonlySet<LoaiBuaAn>): MainMeal[] {
  if (!s.enabled) return [];
  const cur = now.getHours() * 60 + now.getMinutes();
  const today = dayKey(now);
  return MAIN_MEALS.filter((m) => {
    const t = minutes(s.times[m]);
    return cur >= t && cur < t + REMINDER_WINDOW_MIN && !fired.has(`${today}:${m}`) && !logged.has(m);
  });
}

function safeGet<T>(key: string, fallback: T): T {
  try {
    const raw = localStorage.getItem(key);
    return raw ? (JSON.parse(raw) as T) : fallback;
  } catch {
    return fallback;
  }
}

function safeSet(key: string, value: unknown) {
  try {
    localStorage.setItem(key, JSON.stringify(value));
  } catch {
    /* chế độ riêng tư / bộ nhớ đầy → bỏ qua */
  }
}

export function loadReminders(): ReminderSettings {
  const s = safeGet<Partial<ReminderSettings>>(KEY, {});
  return { enabled: !!s.enabled, times: { ...DEFAULT_REMINDERS.times, ...(s.times ?? {}) } };
}

export function saveReminders(s: ReminderSettings) {
  safeSet(KEY, s);
  window.dispatchEvent(new Event("nv-reminders-changed"));
}

/** Các mốc đã nhắc (chỉ giữ của hôm nay). */
export function loadFired(now = new Date()): Set<string> {
  const today = dayKey(now);
  return new Set(safeGet<string[]>(FIRED_KEY, []).filter((k) => k.startsWith(today)));
}

export function markFired(keys: string[], now = new Date()) {
  safeSet(FIRED_KEY, [...loadFired(now), ...keys]);
}
