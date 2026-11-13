"use client";

import { useEffect } from "react";
import { Button } from "@/components/ui/button";

export default function Error({ error, reset }: { error: Error & { digest?: string }; reset: () => void }) {
  useEffect(() => {
    console.error("Lỗi giao diện:", error.name, error.digest);
  }, [error]);
  return (
    <div className="flex min-h-dvh flex-col items-center justify-center px-4 text-center" role="alert">
      <p className="text-5xl">😵</p>
      <h1 className="mt-4 text-2xl font-extrabold">Đã có lỗi xảy ra</h1>
      <p className="mt-1 max-w-md text-muted-foreground">Rất tiếc, giao diện gặp sự cố ngoài ý muốn. Hãy thử tải lại.</p>
      <Button className="mt-6" onClick={reset}>Thử lại</Button>
    </div>
  );
}
