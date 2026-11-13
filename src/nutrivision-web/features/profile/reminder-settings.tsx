"use client";

import { useEffect, useState } from "react";
import { BellOff, BellRing, Info } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Switch } from "@/components/ui/switch";
import { showNotification } from "@/hooks/use-meal-reminders";
import { MAIN_MEALS, loadReminders, saveReminders, type ReminderSettings as Settings } from "@/lib/reminders";
import { mealTypeEmoji, mealTypeLabel } from "@/lib/meal";

type Perm = NotificationPermission | "unsupported";

/** Cài đặt nhắc giờ ăn (lưu trên thiết bị này). */
export function ReminderSettings() {
  const [s, setS] = useState<Settings | null>(null);
  const [perm, setPerm] = useState<Perm>("default");
  useEffect(() => {
    setS(loadReminders());
    setPerm(typeof Notification === "undefined" ? "unsupported" : Notification.permission);
  }, []);
  if (!s) return null;

  const update = (next: Settings) => {
    setS(next);
    saveReminders(next);
  };

  const toggle = async (on: boolean) => {
    if (on && perm !== "granted") {
      if (perm === "unsupported") return toast.error("Trình duyệt này không hỗ trợ thông báo.");
      const p = await Notification.requestPermission();
      setPerm(p);
      if (p !== "granted") return toast.error("Bạn chưa cho phép thông báo. Hãy bật trong cài đặt trình duyệt.");
    }
    update({ ...s, enabled: on });
    toast.success(on ? "Đã bật nhắc giờ ăn" : "Đã tắt nhắc giờ ăn");
  };

  return (
    <section className="rounded-2xl border bg-card p-5 shadow-soft" aria-label="Nhắc giờ ăn">
      <div className="flex items-start gap-3">
        <span className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-orange/15 text-danger-strong">
          {s.enabled ? <BellRing className="size-5" /> : <BellOff className="size-5" />}
        </span>
        <div className="min-w-0 flex-1">
          <h2 className="text-lg font-extrabold leading-tight">Nhắc giờ ăn</h2>
          <p className="text-sm text-muted-foreground">Nhận thông báo khi đến giờ ăn mà bạn chưa ghi bữa đó.</p>
        </div>
        <Switch checked={s.enabled} onCheckedChange={(v) => void toggle(v)} aria-label="Bật nhắc giờ ăn" />
      </div>

      <div className="mt-4 grid gap-3 sm:grid-cols-3">
        {MAIN_MEALS.map((m) => (
          <label key={m} className="rounded-xl border bg-background/60 p-3 text-sm font-semibold">
            <span aria-hidden>{mealTypeEmoji(m)}</span> {mealTypeLabel(m)}
            <Input
              type="time"
              value={s.times[m]}
              disabled={!s.enabled}
              onChange={(e) => e.target.value && update({ ...s, times: { ...s.times, [m]: e.target.value } })}
              className="mt-1.5"
            />
          </label>
        ))}
      </div>

      <div className="mt-4 flex flex-wrap items-center gap-3">
        <Button
          variant="outline"
          size="sm"
          disabled={perm !== "granted"}
          onClick={() => void showNotification("Thử thông báo NutriVision 🔔", "Bạn sẽ nhận nhắc nhở như thế này khi đến giờ ăn.", "/meals/new")}
        >
          Gửi thông báo thử
        </Button>
        <p className="flex items-center gap-1.5 text-xs text-muted-foreground">
          <Info className="size-3.5 shrink-0" />
          {perm === "denied"
            ? "Thông báo đang bị chặn, hãy cho phép trong cài đặt trình duyệt."
            : perm === "unsupported"
              ? "Trình duyệt không hỗ trợ thông báo."
              : "Cài đặt lưu trên thiết bị này. Nhắc nhở hoạt động khi NutriVision đang mở (tab hoặc ứng dụng đã cài)."}
        </p>
      </div>
    </section>
  );
}
