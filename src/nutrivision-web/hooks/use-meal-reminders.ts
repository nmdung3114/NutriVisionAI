"use client";

import { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { dashboardApi } from "@/lib/api/dashboard";
import { qk } from "@/hooks/use-api";
import { mealTypeLabel } from "@/lib/meal";
import { dayKey, dueReminders, loadFired, loadReminders, markFired, type ReminderSettings } from "@/lib/reminders";
import type { LoaiBuaAn } from "@/types/api";

export async function showNotification(title: string, body: string, url: string) {
  const opts: NotificationOptions = { body, icon: "/icon-192.png", badge: "/icon-192.png", tag: url, data: { url } };
  const reg = "serviceWorker" in navigator ? await navigator.serviceWorker.getRegistration() : undefined;
  if (reg) await reg.showNotification(title, opts);
  else new Notification(title, opts);
}

/**
 * Nhắc giờ ăn khi ứng dụng đang mở (không có máy chủ push): mỗi 30 giây kiểm tra
 * bữa đến giờ mà hôm nay chưa ghi, rồi hiện thông báo của hệ điều hành.
 */
export function useMealReminders(active: boolean) {
  const [settings, setSettings] = useState<ReminderSettings | null>(null);
  useEffect(() => {
    const sync = () => setSettings(loadReminders());
    sync();
    window.addEventListener("nv-reminders-changed", sync);
    window.addEventListener("storage", sync);
    return () => {
      window.removeEventListener("nv-reminders-changed", sync);
      window.removeEventListener("storage", sync);
    };
  }, []);

  const enabled = active && !!settings?.enabled && typeof Notification !== "undefined" && Notification.permission === "granted";
  const { data } = useQuery({ queryKey: qk.dashboard, queryFn: dashboardApi.get, enabled, refetchInterval: enabled ? 5 * 60_000 : false });

  useEffect(() => {
    if (!enabled || !settings) return;
    const tick = () => {
      const now = new Date();
      if (data && data.ngay !== dayKey(now)) return; // dữ liệu của ngày khác → chờ tải lại
      const logged = new Set<LoaiBuaAn>((data?.buaAnHomNay ?? []).map((m) => m.loaiBuaAn));
      const due = dueReminders(now, settings, loadFired(now), logged);
      if (!due.length) return;
      markFired(due.map((m) => `${dayKey(now)}:${m}`), now);
      for (const m of due)
        void showNotification(`Đến giờ ${mealTypeLabel(m).toLowerCase()} rồi! 🍽️`, "Chụp ảnh món ăn để NutriVision AI ghi lại dinh dưỡng giúp bạn.", `/meals/new?loai=${m}`);
    };
    tick();
    const id = setInterval(tick, 30_000);
    return () => clearInterval(id);
  }, [enabled, settings, data]);
}
