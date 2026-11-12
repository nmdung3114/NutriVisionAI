"use client";

import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { dashboardApi, reportsApi } from "@/lib/api/dashboard";
import { foodsApi } from "@/lib/api/foods";
import { mealsApi, type HistoryParams } from "@/lib/api/meals";
import { profileApi } from "@/lib/api/profile";
import { adminApi } from "@/lib/api/admin";

export const qk = {
  dashboard: ["dashboard"] as const,
  profile: ["profile"] as const,
  units: ["foods", "units"] as const,
  suggestions: ["suggestions"] as const,
  meal: (id: string) => ["meal", id] as const,
  history: (p: HistoryParams) => ["meals", "history", p] as const,
  report: (from?: string, to?: string) => ["report", from, to] as const,
};

export const useDashboard = () => useQuery({ queryKey: qk.dashboard, queryFn: dashboardApi.get });
export const useProfile = () => useQuery({ queryKey: qk.profile, queryFn: profileApi.get });
export const useSuggestions = () => useQuery({ queryKey: qk.suggestions, queryFn: foodsApi.suggestions, staleTime: 5 * 60_000 });
export const useUnits = () => useQuery({ queryKey: qk.units, queryFn: foodsApi.units, staleTime: Infinity });
export const useMeal = (id: string) => useQuery({ queryKey: qk.meal(id), queryFn: () => mealsApi.get(id), enabled: !!id });
export const useMealHistory = (p: HistoryParams) =>
  useQuery({ queryKey: qk.history(p), queryFn: () => mealsApi.history(p), placeholderData: keepPreviousData });
export const useReport = (from?: string, to?: string, enabled = true) =>
  useQuery({ queryKey: qk.report(from, to), queryFn: () => reportsApi.get(from, to), enabled });

export const useAdminStats = (from?: string, to?: string) =>
  useQuery({ queryKey: ["admin", "stats", from, to], queryFn: () => adminApi.statistics(from, to) });
export const useLogTypes = () => useQuery({ queryKey: ["admin", "logTypes"], queryFn: adminApi.logTypes, staleTime: 10 * 60_000 });
