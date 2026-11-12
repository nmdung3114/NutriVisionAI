"use client";

import Link from "next/link";
import { useMemo, useState } from "react";
import { useSearchParams } from "next/navigation";
import { ChevronLeft, ChevronRight, PlusCircle } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { PageHeader } from "@/components/shared/page-header";
import { EmptyState } from "@/components/shared/empty-state";
import { ErrorState } from "@/components/shared/error-state";
import { useMealHistory } from "@/hooks/use-api";
import { lastNDays, relativeDayLabel, todayStr } from "@/lib/date";
import { MEAL_TYPES, fmt } from "@/lib/meal";
import type { LoaiBuaAn, MealSummary } from "@/types/api";
import { MealCard } from "./meal-card";
import { DeleteMealDialog } from "./delete-meal-dialog";

type Preset = "today" | "7" | "30" | "custom";
const PAGE_SIZE = 20;

export function HistoryPage() {
  // ?ngay=YYYY-MM-DD (từ lịch tháng) → mở sẵn đúng ngày đó
  const ngayParam = useSearchParams().get("ngay");
  const initialDay = ngayParam && /^\d{4}-\d{2}-\d{2}$/.test(ngayParam) ? ngayParam : null;
  const [preset, setPreset] = useState<Preset>(initialDay ? "custom" : "7");
  const [custom, setCustom] = useState(initialDay ? { from: initialDay, to: initialDay } : { from: lastNDays(7).from, to: todayStr() });
  const [loai, setLoai] = useState<LoaiBuaAn | "all">("all");
  const [page, setPage] = useState(1);
  const [toDelete, setToDelete] = useState<MealSummary | null>(null);

  const range = useMemo(() => {
    if (preset === "today") return { from: todayStr(), to: todayStr() };
    if (preset === "7") return lastNDays(7);
    if (preset === "30") return lastNDays(30);
    return custom;
  }, [preset, custom]);
  const customInvalid = preset === "custom" && (!custom.from || !custom.to || custom.from > custom.to);

  const { data, error, isLoading, isFetching, refetch } = useMealHistory({
    from: range.from,
    to: range.to,
    loaiBuaAn: loai === "all" ? "" : loai,
    page,
    pageSize: PAGE_SIZE,
  });

  const groups = useMemo(() => {
    const map = new Map<string, MealSummary[]>();
    for (const m of data?.items ?? []) map.set(m.ngayAn, [...(map.get(m.ngayAn) ?? []), m]);
    return [...map.entries()].sort((a, b) => b[0].localeCompare(a[0]));
  }, [data]);

  return (
    <div>
      <PageHeader
        title="Lịch sử bữa ăn"
        description="Xem lại, chỉnh sửa hoặc xóa các bữa ăn đã ghi nhận."
        actions={
          <Button asChild className="font-bold">
            <Link href="/meals/new">
              <PlusCircle /> Thêm bữa ăn mới
            </Link>
          </Button>
        }
      />

      <div className="mb-5 flex flex-col gap-3 rounded-2xl border bg-card p-3 shadow-soft sm:flex-row sm:flex-wrap sm:items-center">
        <Tabs value={preset} onValueChange={(v) => { setPreset(v as Preset); setPage(1); }}>
          <TabsList>
            <TabsTrigger value="today">Hôm nay</TabsTrigger>
            <TabsTrigger value="7">7 ngày</TabsTrigger>
            <TabsTrigger value="30">30 ngày</TabsTrigger>
            <TabsTrigger value="custom">Tùy chọn</TabsTrigger>
          </TabsList>
        </Tabs>
        {preset === "custom" && (
          <div className="flex items-center gap-2">
            <Input type="date" aria-label="Từ ngày" max={todayStr()} value={custom.from} onChange={(e) => { setCustom((c) => ({ ...c, from: e.target.value })); setPage(1); }} className="h-9 w-40" />
            <span className="text-muted-foreground">→</span>
            <Input type="date" aria-label="Đến ngày" max={todayStr()} value={custom.to} onChange={(e) => { setCustom((c) => ({ ...c, to: e.target.value })); setPage(1); }} className="h-9 w-40" />
          </div>
        )}
        <div className="sm:ml-auto">
          <Select value={loai} onValueChange={(v) => { setLoai(v as LoaiBuaAn | "all"); setPage(1); }}>
            <SelectTrigger className="h-9 w-full sm:w-44" aria-label="Lọc theo loại bữa">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Tất cả loại bữa</SelectItem>
              {MEAL_TYPES.map((t) => (
                <SelectItem key={t.value} value={t.value}>
                  {t.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>
      {customInvalid && <p role="alert" className="mb-4 text-sm font-medium text-destructive">Khoảng ngày không hợp lệ: ngày bắt đầu phải trước hoặc bằng ngày kết thúc.</p>}

      {isLoading ? (
        <div className="space-y-3">
          {[0, 1, 2, 3].map((i) => (
            <Skeleton key={i} className="h-28 rounded-2xl" />
          ))}
        </div>
      ) : error ? (
        <ErrorState error={error} onRetry={() => void refetch()} />
      ) : !data || data.items.length === 0 ? (
        <EmptyState
          title="Bạn chưa ghi nhận bữa ăn nào trong khoảng thời gian này"
          description="Hãy thử đổi bộ lọc hoặc thêm bữa ăn mới."
          action={
            <Button asChild>
              <Link href="/meals/new">+ Thêm bữa ăn ngay</Link>
            </Button>
          }
        />
      ) : (
        <div className={isFetching ? "opacity-70 transition-opacity" : "transition-opacity"}>
          {groups.map(([day, meals]) => {
            const t = data.tongHopNgay.find((x) => x.ngay === day);
            const pct = t?.phanTram ?? 0;
            return (
              <section key={day} className="mb-6" aria-label={relativeDayLabel(day)}>
                <div className="mb-2 flex flex-wrap items-center justify-between gap-2 px-1">
                  <h2 className="text-base font-extrabold">{relativeDayLabel(day)}</h2>
                  {t && (
                    <div className="flex items-center gap-3 text-sm">
                      <span className="text-muted-foreground">
                        <b className="text-foreground">{fmt(t.tongCalo)}</b> / {fmt(t.mucTieuCalo)} kcal
                      </span>
                      <div className="h-2 w-24 overflow-hidden rounded-full bg-muted" role="progressbar" aria-valuenow={Math.round(pct)} aria-valuemin={0} aria-valuemax={100} aria-label="Tiến độ calo trong ngày">
                        <div className="h-full rounded-full" style={{ width: `${Math.min(pct, 100)}%`, background: pct > 110 ? "#ef4d4d" : "#21b45a" }} />
                      </div>
                      <span className={`font-bold ${pct > 110 ? "text-red-600" : "text-primary"}`}>{Math.round(pct)}%</span>
                    </div>
                  )}
                </div>
                <div className="space-y-3">
                  {meals.map((m) => (
                    <MealCard key={m.id} meal={m} onDelete={setToDelete} />
                  ))}
                </div>
              </section>
            );
          })}

          {data.totalPages > 1 && (
            <nav aria-label="Phân trang" className="mt-4 flex items-center justify-center gap-3">
              <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
                <ChevronLeft /> Trang trước
              </Button>
              <span className="text-sm text-muted-foreground">
                Trang <b className="text-foreground">{data.page}</b> / {data.totalPages}
              </span>
              <Button variant="outline" size="sm" disabled={page >= data.totalPages} onClick={() => setPage((p) => p + 1)}>
                Trang sau <ChevronRight />
              </Button>
            </nav>
          )}
        </div>
      )}

      <DeleteMealDialog meal={toDelete} onClose={() => setToDelete(null)} />
    </div>
  );
}
