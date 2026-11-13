import { api } from "./client";
import type { Food, FoodMatch, Suggestion, UnitConversion } from "@/types/api";

export const foodsApi = {
  search: (q: string, take = 20, nhom?: string, signal?: AbortSignal) =>
    api<Food[]>("/api/foods", { query: { q, take, nhom }, signal }),
  match: (name: string) => api<FoodMatch[]>("/api/foods/match", { query: { name } }),
  units: () => api<UnitConversion[]>("/api/foods/units"),
  groups: () => api<string[]>("/api/foods/groups"),
  suggestions: () => api<Suggestion[]>("/api/suggestions"),
};
