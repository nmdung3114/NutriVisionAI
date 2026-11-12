"use client";

import Link from "next/link";
import { useMemo, useState } from "react";
import { AlertTriangle, CheckCircle2, Download, FileSpreadsheet, FileText, Flame, Info, Minus, TrendingDown, TrendingUp, Wheat, Beef, Droplet } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { PageHeader } from "@/components/shared/page-header";
import { EmptyState } from "@/components/shared/empty-state";
import { ErrorState } from "@/components/shared/error-state";
import { StatCard } from "@/components/shared/stat-card";
import { DonutLegend, MacroDonut, macroSlices } from "@/components/shared/macro-donut";
import { CalorieBarChart } from "@/features/dashboard/week-chart";
import { useReport } from "@/hooks/use-api";
import { reportsApi } from "@/lib/api/dashboard";
import { errorMessage } from "@/lib/api/errors";
import { formatDate, lastNDays, todayStr } from "@/lib/date";
import { fmt } from "@/lib/meal";
import type { Rating, Report, Trend } from "@/types/api";

type Preset = "7" | "30" | "custom";

const RATING: Record<Rating, { label: string; cls: string }> = {
  Thap: { label: "Thấp", cls: "bg-yellow/30 text-[#8a5a00]" },
  DatChuan: { label: "Đạt chuẩn", cls: "bg-brand-100 text-brand-700" },
  Cao: { label: "Cao", cls: "bg-orange/20 text-[#a8341c]" },
};
const TREND: Record<Trend, { label: string; icon: typeof Minus; cls: string }> = {
  Tang: { label: "Xu hướng tăng", icon: TrendingUp, cls: "bg-orange/20 text-[#a8341c]" },
  Giam: { label: "Xu hướng giảm", icon: TrendingDown, cls: "bg-blue/15 text-[#2a6fc4]" },
  OnDinh: { label: "Ổn định", icon: Minus, cls: "bg-brand-100 text-brand-700" },
  KhongDuDuLieu: { label: "Chưa đủ dữ liệu xu hướng", icon: Minus, cls: "bg-muted text-muted-foreground" },
};

function RatingBadge({ r }: { r: Rating }) {
  return <Badge className={`rounded-full ${RATING[r].cls}`}>{RATING[r].label}</Badge>;
}

function Recommendations({ items }: { items: Report["khuyenNghi"] }) {
  const icon = { info: Info, warning: AlertTriangle, success: CheckCircle2 } as const;
  const cls = { info: "bg-blue/10 text-[#2a6fc4]", warning: "bg-yellow/20 text-[#8a5a00]", success: "bg-brand-50 text-brand-700" } as const;
  return (
    <section className="rounded-2xl border bg-card p-5 shadow-soft" aria-label="Khuyến nghị thông minh">
      <h2 className="mb-3 text-lg font-extrabold">Khuyến nghị thông minh</h2>
      {items.length === 0 ? (
        <p className="text-sm text-muted-foreground">Chưa có khuyến nghị cho giai đoạn này.</p>
      ) : (
        <ul className="space-y-2.5">
          {items.map((k, i) => {
            const Icon = icon[k.muc] ?? Info;
            return (
              <li key={i} className={`flex items-start gap-3 rounded-xl p-3 text-sm ${cls[k.muc] ?? cls.info}`}>
                <Icon className="mt-0.5 size-4 shrink-0" />
                <span className="text-foreground">{k.noiDung}</span>
              </li>
            );
          })}
        </ul>
      )}
    </section>
  );
}

export function ReportsPage() {
  const [preset, setPreset] = useState<Preset>("7");
  const [custom, setCustom] = useState(lastNDays(14));
  const [busy, setBusy] = useState<"pdf" | "csv" | null>(null);

  const range = useMemo(() => (preset === "7" ? lastNDays(7) : preset === "30" ? lastNDays(30) : custom), [preset, custom]);
  const spanDays = custom.from && custom.to ? (new Date(custom.to).getTime() - new Date(custom.from).getTime()) / 86400000 + 1 : 0;
  const customError =
    preset !== "custom" ? null : !custom.from || !custom.to ? "Vui lòng chọn đủ ngày bắt đầu và kết thúc." : custom.from > custom.to ? "Ngày bắt đầu phải trước ngày kết thúc." : spanDays > 366 ? "Khoảng thời gian tối đa 366 ngày." : null;

  const { data, error, isLoading, refetch } = useReport(range.from, range.to, !customError);

  const doExport = async (format: "pdf" | "csv") => {
    setBusy(format);
    try {
      const name = await reportsApi.export(range.from, range.to, format);
      toast.success(`Đã tải ${name}`);
    } catch (e) {
      toast.error(errorMessage(e));
    } finally {
      setBusy(null);
    }
  };

  const trend = data ? TREND[data.xuHuong] : null;
  const hasData = !!data && data.soNgayCoDuLieu > 0;

  return (
    <div>
      <PageHeader title="Báo cáo dinh dưỡng" description={`Từ ${formatDate(range.from)} đến ${formatDate(range.to)}`} />

      <div className="mb-5 flex flex-col gap-3 rounded-2xl border bg-card p-3 shadow-soft sm:flex-row sm:items-center">
        <Tabs value={preset} onValueChange={(v) => setPreset(v as Preset)}>
          <TabsList>
            <TabsTrigger value="7">7 ngày</TabsTrigger>
            <TabsTrigger value="30">30 ngày</TabsTrigger>
            <TabsTrigger value="custom">Tùy chọn</TabsTrigger>
          </TabsList>
        </Tabs>
        {preset === "custom" && (
          <div className="flex items-center gap-2">
            <Input type="date" aria-label="Từ ngày" max={todayStr()} value={custom.from} onChange={(e) => setCustom((c) => ({ ...c, from: e.target.value }))} className="h-9 w-40" />
            <span className="text-muted-foreground">→</span>
            <Input type="date" aria-label="Đến ngày" max={todayStr()} value={custom.to} onChange={(e) => setCustom((c) => ({ ...c, to: e.target.value }))} className="h-9 w-40" />
          </div>
        )}
      </div>
      {customError && <p role="alert" className="mb-4 text-sm font-medium text-destructive">{customError}</p>}

      {isLoading ? (
        <div className="space-y-4">
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            {[0, 1, 2, 3].map((i) => (
              <Skeleton key={i} className="h-28 rounded-2xl" />
            ))}
          </div>
          <Skeleton className="h-72 rounded-2xl" />
        </div>
      ) : error ? (
        <ErrorState error={error} onRetry={() => void refetch()} />
      ) : !data || !hasData ? (
        <EmptyState
          title="Chưa có dữ liệu trong khoảng thời gian này"
          description="Hãy ghi nhận bữa ăn để xem báo cáo dinh dưỡng và nhận khuyến nghị."
          action={
            <Button asChild>
              <Link href="/meals/new">+ Thêm bữa ăn ngay</Link>
            </Button>
          }
        />
      ) : (
        <div className="space-y-6">
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <StatCard
              label="Calo trung bình/ngày"
              value={fmt(data.trungBinhNgay.calo)}
              unit="kcal"
              icon={<Flame />}
              tone="green"
              hint={`${fmt(data.phanTramMucTieu)}% mục tiêu (${fmt(data.mucTieuCaloTrungBinh)} kcal) · ${data.soNgayCoDuLieu}/${data.soNgay} ngày có dữ liệu`}
              badge={<RatingBadge r={data.danhGia.calo} />}
            />
            <StatCard label="Đạm trung bình" value={fmt(data.trungBinhNgay.chatDam, 1)} unit="g/ngày" icon={<Beef />} tone="orange" badge={<RatingBadge r={data.danhGia.chatDam} />} hint={`${data.tyLeNangLuong.chatDam}% năng lượng`} />
            <StatCard label="Tinh bột trung bình" value={fmt(data.trungBinhNgay.tinhBot, 1)} unit="g/ngày" icon={<Wheat />} tone="yellow" badge={<RatingBadge r={data.danhGia.tinhBot} />} hint={`${data.tyLeNangLuong.tinhBot}% năng lượng`} />
            <StatCard label="Chất béo trung bình" value={fmt(data.trungBinhNgay.chatBeo, 1)} unit="g/ngày" icon={<Droplet />} tone="blue" badge={<RatingBadge r={data.danhGia.chatBeo} />} hint={`${data.tyLeNangLuong.chatBeo}% năng lượng`} />
          </div>

          <div className="grid gap-6 lg:grid-cols-[1.7fr_1fr]">
            <section className="rounded-2xl border bg-card p-5 shadow-soft" aria-label="Biểu đồ năng lượng">
              <div className="mb-2 flex flex-wrap items-center justify-between gap-2">
                <h2 className="text-lg font-extrabold">Năng lượng theo ngày</h2>
                {trend && (
                  <Badge className={`gap-1 rounded-full ${trend.cls}`}>
                    <trend.icon className="size-3.5" /> {trend.label}
                  </Badge>
                )}
              </div>
              <CalorieBarChart data={data.theoNgay} height={290} />
              <p className="mt-2 text-xs text-muted-foreground">
                {data.soNgayVuotMucTieu} ngày vượt mục tiêu · {data.soNgayThieuNangLuong} ngày thiếu năng lượng · {data.tongSoBuaAn} bữa ăn
              </p>
            </section>
            <section className="rounded-2xl border bg-card p-5 shadow-soft" aria-label="Cơ cấu năng lượng">
              <h2 className="mb-2 text-lg font-extrabold">Cơ cấu nhóm chất</h2>
              <MacroDonut slices={macroSlices(data.tyLeNangLuong)} centerLabel={`${fmt(data.trungBinhNgay.calo)}`} centerSub="kcal/ngày" unit="%" />
              <DonutLegend slices={macroSlices(data.tyLeNangLuong)} />
            </section>
          </div>

          <Recommendations items={data.khuyenNghi} />

          <div className="flex flex-col gap-3 sm:flex-row sm:justify-between">
            <Button variant="outline" size="lg" disabled={!!busy} onClick={() => void doExport("pdf")} className="font-bold">
              {busy === "pdf" ? <Download className="animate-bounce" /> : <FileText />} XUẤT BÁO CÁO PDF
            </Button>
            <Button size="lg" disabled={!!busy} onClick={() => void doExport("csv")} className="font-bold">
              {busy === "csv" ? <Download className="animate-bounce" /> : <FileSpreadsheet />} XUẤT DỮ LIỆU CSV
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}
