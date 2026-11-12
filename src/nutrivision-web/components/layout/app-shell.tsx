"use client";

import { useEffect } from "react";
import { usePathname, useRouter } from "next/navigation";
import { useAuth } from "@/providers/auth-provider";
import { Skeleton } from "@/components/ui/skeleton";
import { Sidebar } from "./sidebar";
import { Topbar } from "./topbar";
import { BottomNav } from "./bottom-nav";
import { MealReminderRunner } from "./meal-reminder-runner";

export function FullPageLoader() {
  return (
    <div className="flex min-h-dvh items-center justify-center bg-background" role="status" aria-label="Đang tải">
      <div className="flex flex-col items-center gap-3">
        <div className="size-10 animate-spin rounded-full border-4 border-brand-100 border-t-primary" />
        <p className="text-sm text-muted-foreground">Đang tải NutriVision AI…</p>
      </div>
    </div>
  );
}

/** Bảo vệ route: phải đăng nhập (và đúng vai trò nếu yêu cầu). */
export function AuthGuard({ children, role }: { children: React.ReactNode; role?: "ADMIN" }) {
  const { status, user } = useAuth();
  const router = useRouter();
  const pathname = usePathname();

  useEffect(() => {
    if (status === "anonymous") {
      router.replace(`/login?next=${encodeURIComponent(pathname)}`);
    } else if (status === "authenticated" && role && user?.vaiTro !== role) {
      router.replace("/dashboard");
    }
  }, [status, user, role, router, pathname]);

  if (status !== "authenticated" || (role && user?.vaiTro !== role)) return <FullPageLoader />;
  return <>{children}</>;
}

/** Trang dành cho khách: đã đăng nhập thì chuyển vào dashboard. */
export function GuestGuard({ children }: { children: React.ReactNode }) {
  const { status } = useAuth();
  const router = useRouter();
  useEffect(() => {
    if (status === "authenticated") {
      const next = new URLSearchParams(window.location.search).get("next");
      router.replace(next && next.startsWith("/") && !next.startsWith("//") ? next : "/dashboard");
    }
  }, [status, router]);
  if (status !== "anonymous") return <FullPageLoader />;
  return <>{children}</>;
}

export function AppShell({ children }: { children: React.ReactNode }) {
  return (
    <AuthGuard>
      <div className="min-h-dvh">
        <Sidebar />
        <div className="lg:pl-64">
          <Topbar />
          <main id="main" className="mx-auto w-full max-w-6xl px-4 py-6 pb-28 lg:px-8 lg:pb-10">
            {children}
          </main>
        </div>
        <BottomNav />
        <MealReminderRunner />
      </div>
    </AuthGuard>
  );
}

export function PageSkeleton() {
  return (
    <div className="space-y-4">
      <Skeleton className="h-9 w-64" />
      <Skeleton className="h-48 w-full rounded-2xl" />
      <Skeleton className="h-48 w-full rounded-2xl" />
    </div>
  );
}
