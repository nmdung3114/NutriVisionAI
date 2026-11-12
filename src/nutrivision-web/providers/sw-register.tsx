"use client";

import { useEffect } from "react";

/** Đăng ký service worker (chỉ bản production để tránh cache khi đang phát triển). */
export function ServiceWorkerRegister() {
  useEffect(() => {
    if (process.env.NODE_ENV !== "production" || !("serviceWorker" in navigator)) return;
    navigator.serviceWorker.register("/sw.js").catch(() => {
      /* trình duyệt chặn SW (chế độ riêng tư…) → ứng dụng vẫn chạy bình thường */
    });
  }, []);
  return null;
}
