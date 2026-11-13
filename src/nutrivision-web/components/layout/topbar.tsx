"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { ChevronDown, LogOut, Menu, UserCircle } from "lucide-react";
import { Button } from "@/components/ui/button";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuLabel, DropdownMenuSeparator, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";
import { Sheet, SheetContent, SheetDescription, SheetHeader, SheetTitle, SheetTrigger } from "@/components/ui/sheet";
import { useAuth } from "@/providers/auth-provider";
import { greetingByHour } from "@/lib/date";
import { SidebarContent } from "./sidebar";
import { UserAvatar } from "./user-avatar";
import { ThemeToggle } from "@/components/theme/theme-toggle";

export function Topbar() {
  const { user, logout } = useAuth();
  const [open, setOpen] = useState(false);
  const [greeting, setGreeting] = useState("Xin chào");
  useEffect(() => setGreeting(greetingByHour(new Date().getHours())), []);
  if (!user) return null;
  const firstName = user.hoTen.trim().split(/\s+/).slice(-1)[0];

  return (
    <header className="sticky top-0 z-20 flex h-16 items-center gap-3 border-b bg-background/85 px-4 backdrop-blur lg:px-8">
      <Sheet open={open} onOpenChange={setOpen}>
        <SheetTrigger asChild>
          <Button variant="ghost" size="icon" className="lg:hidden" aria-label="Mở menu">
            <Menu className="size-5" />
          </Button>
        </SheetTrigger>
        <SheetContent side="left" className="w-72 p-0">
          <SheetHeader className="sr-only">
            <SheetTitle>Menu</SheetTitle>
            <SheetDescription>Điều hướng NutriVision AI</SheetDescription>
          </SheetHeader>
          <SidebarContent onNavigate={() => setOpen(false)} />
        </SheetContent>
      </Sheet>

      <div className="min-w-0 flex-1">
        <p className="truncate text-sm text-muted-foreground">{greeting},</p>
        <p className="truncate text-base font-extrabold leading-tight">{firstName} 👋</p>
      </div>

      <ThemeToggle />

      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <button
            className="flex items-center gap-2 rounded-full border bg-card p-1 pr-3 outline-none transition-shadow hover:shadow-soft focus-visible:ring-2 focus-visible:ring-ring"
            aria-label="Menu người dùng"
          >
            <UserAvatar name={user.hoTen} url={user.anhDaiDienUrl} />
            <span className="hidden max-w-[140px] truncate text-sm font-semibold sm:block">{user.hoTen}</span>
            <ChevronDown className="size-4 text-muted-foreground" />
          </button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end" className="w-60">
          <DropdownMenuLabel>
            <p className="truncate font-bold">{user.hoTen}</p>
            <p className="truncate text-xs font-normal text-muted-foreground">{user.email}</p>
            {user.vaiTro === "ADMIN" && <p className="mt-1 text-xs font-semibold text-primary">Quản trị viên</p>}
          </DropdownMenuLabel>
          <DropdownMenuSeparator />
          <DropdownMenuItem asChild>
            <Link href="/profile">
              <UserCircle /> Hồ sơ cá nhân
            </Link>
          </DropdownMenuItem>
          <DropdownMenuItem onSelect={() => void logout()} variant="destructive">
            <LogOut /> Đăng xuất
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
    </header>
  );
}
