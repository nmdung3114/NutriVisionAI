"use client";

import Link from "next/link";
import { Camera, PlusCircle, Sparkles, UserCog } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { PageHeader } from "@/components/shared/page-header";
import { EmptyState } from "@/components/shared/empty-state";
import { ErrorState } from "@/components/shared/error-state";
import { useDashboard } from "@/hooks/use-api";
import { formatDate } from "@/lib/date";
import { CalorieBarChart } from "./week-chart";
import { HeroCard } from "./hero-card";
import { TodayMeals } from "./today-meals";
import { Suggestions } from "./suggestions";

function DashboardSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-9 w-56" />
      <Skeleton className="h-72 rounded-3xl" />
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {[0, 1, 2, 3].map((i) => (
          <Skeleton key={i} className="h-36 rounded-2xl" />
        ))}
      </div>
      <Skeleton className="h-72 rounded-2xl" />
    </div>
  );
}

export function DashboardPage() {
  const { data, isLoading, error, refetch } = useDashboard();
  if (isLoading) return <DashboardSkeleton />;
  if (error || !data) return <ErrorState error={error} onRetry={() => void refetch()} />;

  return (
    <div className="space-y-6">
      <PageHeader
        title="Tổng quan hôm nay"
        description={`Ngày ${formatDate(data.ngay)} · Mục tiêu ${data.mucTieuCalo.toLocaleString("vi-VN")} kcal`}
        actions={
          <Button asChild className="font-bold">
            <Link href="/meals/new">
              <PlusCircle /> Thêm bữa ăn
            </Link>
          </Button>
        }
      />

      {!data.hoSoDayDu && (
        <div className="flex flex-col gap-3 rounded-2xl border border-yellow/60 bg-yellow/15 p-4 sm:flex-row sm:items-center" role="note">
          <span className="flex size-11 shrink-0 items-center justify-center rounded-xl bg-yellow/40 text-[#8a5a00]">
            <UserCog className="size-5" />
          </span>
          <div className="flex-1">
            <p className="font-bold">Hoàn thiện hồ sơ để có mục tiêu calo chính xác</p>
            <p className="text-sm text-muted-foreground">Bạn đang dùng mục tiêu mặc định. Nhập chiều cao, cân nặng, mức độ vận động để tính TDEE cá nhân.</p>
          </div>
          <Button asChild variant="outline" className="border-yellow bg-white">
            <Link href="/profile">Cập nhật hồ sơ</Link>
          </Button>
        </div>
      )}

      {!data.coDuLieu ? (
        <EmptyState
          icon={<span className="flex size-20 items-center justify-center rounded-3xl bg-brand-50 text-primary"><Sparkles className="size-10" /></span>}
          title="Chào mừng đến với NutriVision AI!"
          description="Bạn chưa ghi nhận bữa ăn nào. Chụp ảnh món ăn đầu tiên để AI giúp bạn tính calo và dinh dưỡng."
          action={
            <Button asChild size="lg" className="font-bold">
              <Link href="/meals/new">
                <Camera /> Thêm bữa ăn đầu tiên
              </Link>
            </Button>
          }
        />
      ) : (
        <>
          <HeroCard data={data} />
          <TodayMeals data={data} />
        </>
      )}

      <div className="grid items-start gap-6 lg:grid-cols-[1.6fr_1fr]">
        <section className="rounded-2xl border bg-card p-5 shadow-soft" aria-label="Calo 7 ngày gần nhất">
          <h2 className="mb-1 text-lg font-extrabold">7 ngày gần nhất</h2>
          <p className="mb-3 text-xs text-muted-foreground">Cột là calo mỗi ngày, vạch đỏ nét đứt là mục tiêu của bạn.</p>
          <CalorieBarChart data={data.bayNgay} />
        </section>
        <Suggestions />
      </div>
    </div>
  );
}
