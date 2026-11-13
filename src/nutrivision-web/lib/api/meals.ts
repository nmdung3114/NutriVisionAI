import { api } from "./client";
import type { FrequentMeal, LoaiBuaAn, Meal, MealHistory, MealUpsert } from "@/types/api";

export interface HistoryParams {
  from?: string;
  to?: string;
  loaiBuaAn?: LoaiBuaAn | "";
  page?: number;
  pageSize?: number;
}

export const mealsApi = {
  /** AI có thể mất ~30s + retry → timeout 100s, cho phép hủy. */
  scan: (file: File, loaiBuaAn: LoaiBuaAn, ngayAn: string | undefined, signal?: AbortSignal) => {
    const fd = new FormData();
    fd.append("image", file);
    fd.append("loaiBuaAn", loaiBuaAn);
    if (ngayAn) fd.append("ngayAn", ngayAn);
    return api<Meal>("/api/meals/scan", { method: "POST", formData: fd, timeoutMs: 100_000, signal });
  },
  createManual: (b: MealUpsert) => api<Meal>("/api/meals", { method: "POST", body: b }),
  get: (id: string) => api<Meal>(`/api/meals/${id}`),
  confirm: (id: string, b: MealUpsert) => api<Meal>(`/api/meals/${id}/confirm`, { method: "POST", body: b }),
  update: (id: string, b: MealUpsert) => api<Meal>(`/api/meals/${id}`, { method: "PUT", body: b }),
  revert: (id: string) => api<Meal>(`/api/meals/${id}/revert`, { method: "POST" }),
  cancel: (id: string) => api<null>(`/api/meals/${id}/cancel`, { method: "DELETE" }),
  remove: (id: string) => api<null>(`/api/meals/${id}`, { method: "DELETE" }),
  repeat: (id: string, b: { loaiBuaAn?: LoaiBuaAn; ngayAn?: string } = {}) =>
    api<Meal>(`/api/meals/${id}/repeat`, { method: "POST", body: b }),
  frequent: (limit = 6) => api<FrequentMeal[]>("/api/meals/frequent", { query: { limit } }),
  history: (p: HistoryParams) =>
    api<MealHistory>("/api/meals", { query: { ...p } }),
};
