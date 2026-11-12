import { api, downloadFile } from "./client";
import type { CalendarMonth, Dashboard, Report } from "@/types/api";

export const dashboardApi = {
  get: () => api<Dashboard>("/api/dashboard"),
  calendar: (nam: number, thang: number) => api<CalendarMonth>("/api/dashboard/calendar", { query: { nam, thang } }),
};

export const reportsApi = {
  get: (from?: string, to?: string) => api<Report>("/api/reports", { query: { from, to } }),
  export: (from: string | undefined, to: string | undefined, format: "pdf" | "csv") =>
    downloadFile("/api/reports/export", { from, to, format }, `bao-cao-dinh-duong.${format}`),
};
