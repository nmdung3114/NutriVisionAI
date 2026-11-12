"use client";

import { useState } from "react";
import { ImageOff } from "lucide-react";
import { assetUrl } from "@/lib/config";
import { cn } from "@/lib/utils";

/** Ảnh từ backend (URL ký, có hạn) với fallback khi lỗi/không có. */
export function SafeImage({ src, alt, className }: { src: string | null | undefined; alt: string; className?: string }) {
  const [failed, setFailed] = useState(false);
  const url = assetUrl(src);
  if (!url || failed) {
    return (
      <div className={cn("flex items-center justify-center bg-brand-50 text-brand-500/60", className)} aria-label={alt} role="img">
        <ImageOff className="size-1/4 min-h-5 min-w-5" />
      </div>
    );
  }
  // eslint-disable-next-line @next/next/no-img-element
  return <img src={url} alt={alt} className={cn("object-cover", className)} onError={() => setFailed(true)} loading="lazy" />;
}
