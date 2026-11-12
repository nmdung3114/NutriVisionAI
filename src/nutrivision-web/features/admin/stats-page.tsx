"use client";

import { Area, AreaChart, Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { Bot, ImageIcon, ShieldAlert, Timer, UserCheck, Users, UtensilsCrossed, Target } from "lucide-react";
import { Skeleton } from "@/components/ui/skeleton";
import { PageHeader } from "@/components/shared/page-header";
import { ErrorState } from "@/components/shared/error-state";
import { StatCard } from "@/components/shared/stat-card";
import { useAdminStats } from "@/hooks/use-api";
import { formatDayMonth } from "@/lib/date";
import { fmt } from "@/lib/meal";
import type { DayCount } from "@/types/api";

function CountChart({ title, data, color, kind }: { title: string; data: DayCount[]; color: string; kind: "area" | "bar" }) {
  const rows = data.map((d) => ({ ...d, label: formatDayMonth(d.ngay) }));
  const common = (
    <>
      <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#e8efeb" />
      <XAxis dataKey="label" tickLine={false} axisLine={false} fontSize={11} minTickGap={20} />
      <YAxis tickLine={false} axisLine={false} fontSize={11} width={32} allowDecimals={false} />
      <Tooltip formatter={(v) => [String(v), title]} contentStyle={{ borderRadius: 12, border: "1px solid #e3e9e6", fontSize: 12 }} />
    </>
  );
  return (
    <section className="rounded-2xl border bg-card p-5 shadow-soft" aria-label={title}>
      <h2 className="mb-3 text-base font-extrabold">{title}</h2>
      <div className="h-56" role="img" aria-label={title}>
        <ResponsiveContainer width="100%" height="100%">
          {kind === "area" ? (
            <AreaChart data={rows} margin={{ top: 8, right: 8, left: -12, bottom: 0 }}>
              {common}
              <Area type="monotone" dataKey="soLuong" stroke={color} fill={color} fillOpacity={0.18} strokeWidth={2.5} />
            </AreaChart>
          ) : (
            <BarChart data={rows} margin={{ top: 8, right: 8, left: -12, bottom: 0 }}>
              {common}
              <Bar dataKey="soLuong" fill={color} radius={[6, 6, 0, 0]} maxBarSize={28} />
            </BarChart>
          )}
        </ResponsiveContainer>
      </div>
    </section>
  );
}

export function AdminStatsPage() {
  const { data, error, isLoading, refetch } = useAdminStats();
  return (
    <div>
      <PageHeader title="Thống kê hệ thống" description="Số liệu tổng hợp 30 ngày gần nhất. Quản trị viên không xem được dữ liệu cá nhân hay bữa ăn của người dùng." />
      {isLoading ? (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <Skeleton key={i} className="h-28 rounded-2xl" />
          ))}
        </div>
      ) : error || !data ? (
        <ErrorState error={error} onRetry={() => void refetch()} />
      ) : (
        <div className="space-y-6">
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <StatCard label="Tổng người dùng" value={fmt(data.tongNguoiDung)} icon={<Users />} tone="green" />
            <StatCard label="Đang hoạt động" value={fmt(data.nguoiDungHoatDong)} icon={<UserCheck />} tone="blue" />
            <StatCard label="Bị khóa" value={fmt(data.nguoiDungBiKhoa)} icon={<ShieldAlert />} tone="orange" />
            <StatCard label="Tổng bữa ăn" value={fmt(data.tongBuaAn)} icon={<UtensilsCrossed />} tone="yellow" />
            <StatCard label="Ảnh AI đã xử lý" value={fmt(data.soAnhAIXuLy)} icon={<ImageIcon />} tone="purple" />
            <StatCard label="Lần AI thất bại" value={fmt(data.soLanAIThatBai)} icon={<Bot />} tone="orange" />
            <StatCard label="Tỷ lệ AI chính xác" value={data.tyLeAIChinhXacPhanTram === null ? "–" : fmt(data.tyLeAIChinhXacPhanTram, 1)} unit={data.tyLeAIChinhXacPhanTram === null ? undefined : "%"} icon={<Target />} tone="green" hint="Bữa ăn người dùng không phải sửa" />
            <StatCard label="Thời gian xử lý AI TB" value={data.thoiGianXuLyAITrungBinhMs === null ? "–" : fmt(data.thoiGianXuLyAITrungBinhMs / 1000, 1)} unit={data.thoiGianXuLyAITrungBinhMs === null ? undefined : "giây"} icon={<Timer />} tone="blue" />
          </div>
          <div className="grid gap-6 lg:grid-cols-2">
            <CountChart title="Người dùng mới theo ngày" data={data.nguoiDungMoiTheoNgay} color="#21b45a" kind="area" />
            <CountChart title="Bữa ăn ghi nhận theo ngày" data={data.buaAnTheoNgay} color="#ffc934" kind="bar" />
          </div>
          <CountChart title="Lượt nhận diện AI theo ngày" data={data.aiTheoNgay} color="#9d68ed" kind="area" />
        </div>
      )}
    </div>
  );
}
