"use client";

import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { assetUrl } from "@/lib/config";
import { cn } from "@/lib/utils";

export function initials(name: string): string {
  const parts = name.trim().split(/\s+/).filter(Boolean);
  if (!parts.length) return "?";
  const last = parts[parts.length - 1];
  const first = parts.length > 1 ? parts[0] : "";
  return ((first[0] ?? "") + (last[0] ?? "")).toUpperCase();
}

export function UserAvatar({ name, url, className }: { name: string; url?: string | null; className?: string }) {
  return (
    <Avatar className={cn("size-9", className)}>
      {url && <AvatarImage src={assetUrl(url) ?? undefined} alt={name} />}
      <AvatarFallback className="bg-brand-100 font-bold text-brand-700">{initials(name)}</AvatarFallback>
    </Avatar>
  );
}
