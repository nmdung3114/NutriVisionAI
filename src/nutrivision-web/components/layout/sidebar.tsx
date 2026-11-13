"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { Logo } from "@/components/shared/logo";
import { useAuth } from "@/providers/auth-provider";
import { ADMIN_NAV, USER_NAV, isActive, type NavItem } from "./nav-items";
import { cn } from "@/lib/utils";

export function NavList({ items, onNavigate }: { items: NavItem[]; onNavigate?: () => void }) {
  const pathname = usePathname();
  return (
    <ul className="space-y-1">
      {items.map((item) => {
        const active = isActive(pathname, item);
        return (
          <li key={item.href}>
            <Link
              href={item.href}
              onClick={onNavigate}
              aria-current={active ? "page" : undefined}
              className={cn(
                "flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-semibold transition-colors outline-none focus-visible:ring-2 focus-visible:ring-ring",
                active ? "bg-primary text-primary-foreground shadow-sm" : "text-muted-foreground hover:bg-accent hover:text-accent-foreground",
              )}
            >
              <item.icon className="size-5" />
              {item.label}
            </Link>
          </li>
        );
      })}
    </ul>
  );
}

export function SidebarContent({ onNavigate }: { onNavigate?: () => void }) {
  const { user } = useAuth();
  return (
    <div className="flex h-full flex-col gap-6 p-4">
      <div className="px-1 pt-1">
        <Logo href="/dashboard" />
      </div>
      <nav aria-label="Điều hướng chính" className="flex-1 space-y-6 overflow-y-auto">
        <NavList items={USER_NAV} onNavigate={onNavigate} />
        {user?.vaiTro === "ADMIN" && (
          <div>
            <p className="mb-2 px-3 text-xs font-bold uppercase tracking-wider text-muted-foreground">Quản trị</p>
            <NavList items={ADMIN_NAV} onNavigate={onNavigate} />
          </div>
        )}
      </nav>
      <div className="rounded-2xl bg-gradient-to-br from-brand-50 to-brand-100 p-4 text-sm">
        <p className="font-bold text-brand-700">Mẹo hôm nay</p>
        <p className="mt-1 text-xs leading-relaxed text-brand-700/80">Chụp ảnh bữa ăn ngay khi dọn ra để AI nhận diện chính xác hơn.</p>
      </div>
    </div>
  );
}

export function Sidebar() {
  return (
    <aside className="fixed inset-y-0 left-0 z-30 hidden w-64 border-r bg-sidebar lg:block">
      <SidebarContent />
    </aside>
  );
}
