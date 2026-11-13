import { api } from "./client";
import type { AdminLog, AdminStatistics, AdminUser, Paged, Role, UserStatus } from "@/types/api";

export const adminApi = {
  users: (q: { q?: string; trangThai?: string; vaiTro?: string; page?: number; pageSize?: number }) =>
    api<Paged<AdminUser>>("/api/admin/users", { query: q }),
  setStatus: (id: string, trangThai: Exclude<UserStatus, "PENDING">, lyDo?: string) =>
    api<AdminUser>(`/api/admin/users/${id}/status`, { method: "PUT", body: { trangThai, lyDo } }),
  setRole: (id: string, vaiTro: Role) => api<AdminUser>(`/api/admin/users/${id}/role`, { method: "PUT", body: { vaiTro } }),
  logs: (q: { userId?: string; loai?: string; from?: string; to?: string; page?: number; pageSize?: number }) =>
    api<Paged<AdminLog>>("/api/admin/logs", { query: q }),
  logTypes: () => api<string[]>("/api/admin/logs/types"),
  statistics: (from?: string, to?: string) => api<AdminStatistics>("/api/admin/statistics", { query: { from, to } }),
};
