"use client";

import { useMealReminders } from "@/hooks/use-meal-reminders";
import { useAuth } from "@/providers/auth-provider";

export function MealReminderRunner() {
  const { user } = useAuth();
  useMealReminders(user?.vaiTro === "USER");
  return null;
}
