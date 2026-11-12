"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { Plus } from "lucide-react";
import { USER_NAV, isActive } from "./nav-items";
import { cn } from "@/lib/utils";

export function BottomNav() {
  const pathname = usePathname();
  const byHref = Object.fromEntries(USER_NAV.map((n) => [n.href, n]));
  const left = [byHref["/dashboard"], byHref["/meals"]];
  const right = [byHref["/reports"], byHref["/profile"]];
  const renderItem = (item: (typeof USER_NAV)[number]) => {
    const active = isActive(pathname, item);
    return (
      <li key={item.href} className="flex-1">
        <Link
          href={item.href}
          aria-current={active ? "page" : undefined}
          className={cn(
            "flex flex-col items-center gap-0.5 py-2 text-[11px] font-semibold outline-none focus-visible:ring-2 focus-visible:ring-ring",
            active ? "text-primary" : "text-muted-foreground",
          )}
        >
          <item.icon className="size-5" />
          {item.label}
        </Link>
      </li>
    );
  };
  return (
    <nav
      aria-label="Điều hướng nhanh"
      className="fixed inset-x-0 bottom-0 z-30 border-t bg-card/95 pb-[env(safe-area-inset-bottom)] backdrop-blur lg:hidden"
    >
      <ul className="mx-auto flex max-w-lg items-end">
        {left.map(renderItem)}
        <li className="flex flex-1 justify-center">
          <Link
            href="/meals/new"
            aria-label="Thêm bữa ăn"
            className="-mt-6 flex size-14 items-center justify-center rounded-full bg-gradient-to-br from-[#2ed16f] to-[#168d47] text-white shadow-lg ring-4 ring-background outline-none transition-transform active:scale-95 focus-visible:ring-ring"
          >
            <Plus className="size-7" strokeWidth={3} />
          </Link>
        </li>
        {right.map(renderItem)}
      </ul>
    </nav>
  );
}
