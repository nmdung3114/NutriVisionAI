import { api } from "./client";
import type { WaterSummary, WeightOverview } from "@/types/api";

export const weightApi = {
  get: (days = 90) => api<WeightOverview>("/api/weight", { query: { days } }),
  log: (b: { ngay?: string; canNang: number; ghiChu?: string }) => api<WeightOverview>("/api/weight", { method: "POST", body: b }),
  remove: (id: string) => api<null>(`/api/weight/${id}`, { method: "DELETE" }),
};

export const waterApi = {
  get: () => api<WaterSummary>("/api/water"),
  add: (ml: number) => api<WaterSummary>("/api/water", { method: "POST", body: { ml } }),
};
