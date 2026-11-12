"use client";

import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Area, AreaChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { toast } from "sonner";
import { Loader2, Scale, Trash2, TrendingDown, TrendingUp } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { useWeight } from "@/hooks/use-api";
import { weightApi } from "@/lib/api/tracking";
import { errorMessage } from "@/lib/api/errors";
import { formatDate, formatDayMonth, todayStr } from "@/lib/date";
import { fmt } from "@/lib/meal";
import { cn } from "@/lib/utils";

const RANGES = [
  { value: "30", label: "30 ngày" },
  { value: "90", label: "90 ngày" },
  { value: "365", label: "1 năm" },
];

function Delta({ label, value }: { label: string; value: number | null }) {
  const Icon = value !== null && value > 0 ? TrendingUp : TrendingDown;
  return (
    <div className="rounded-xl bg-muted/60 px-3 py-2">
      <p className="text-xs font-semibold text-muted-foreground">{label}</p>
      {value === null ? (
        <p className="text-lg font-extrabold text-muted-foreground">–</p>
      ) : (
        <p className="flex items-center gap-1 text-lg font-extrabold">
          {value !== 0 && <Icon className={cn("size-4", value > 0 ? "text-danger-strong" : "text-brand-700")} />}
          {value > 0 ? "+" : ""}
          {fmt(value, 1)} kg
        </p>
      )}
    </div>
  );
}

/** Cân nặng hiện tại, thay đổi 7/30 ngày, BMI, biểu đồ theo thời gian và form ghi cân nặng. */
export function WeightCard() {
  const [range, setRange] = useState("90");
  const { data, isLoading } = useWeight(Number(range));
  const qc = useQueryClient();
  const [kg, setKg] = useState("");
  const [ngay, setNgay] = useState(todayStr());
  const [ghiChu, setGhiChu] = useState("");
  const [error, setError] = useState<string | null>(null);

  const refresh = () => {
    for (const k of ["weight", "profile", "dashboard", "suggestions"]) void qc.invalidateQueries({ queryKey: [k] });
  };
  const log = useMutation({
    mutationFn: () => weightApi.log({ canNang: Number(kg.replace(",", ".")), ngay, ghiChu: ghiChu || undefined }),
    onSuccess: () => {
      refresh();
      toast.success(`Đã ghi ${kg} kg`);
      setKg("");
      setGhiChu("");
    },
    onError: (e) => setError(errorMessage(e)),
  });
  const remove = useMutation({
    mutationFn: (id: string) => weightApi.remove(id),
    onSuccess: () => {
      refresh();
      toast.success("Đã xóa bản ghi");
    },
    onError: (e) => toast.error(errorMessage(e)),
  });

  const submit = (e: React.FormEvent) => {
    e.preventDefault();
    const v = Number(kg.replace(",", "."));
    if (!kg || Number.isNaN(v) || v < 10 || v > 150) return setError("Cân nặng phải trong khoảng 10–150 kg.");
    setError(null);
    log.mutate();
  };

  if (isLoading) return <Skeleton className="h-96 rounded-2xl" />;
  const rows = (data?.danhSach ?? []).map((w) => ({ ...w, label: formatDayMonth(w.ngay) }));
  const values = rows.map((r) => r.canNang);
  const min = values.length ? Math.floor(Math.min(...values) - 1) : 0;
  const max = values.length ? Math.ceil(Math.max(...values) + 1) : 0;

  return (
    <section className="rounded-2xl border bg-card p-5 shadow-soft" aria-label="Theo dõi cân nặng">
      <div className="mb-4 flex flex-wrap items-center gap-3">
        <span className="flex size-10 items-center justify-center rounded-xl bg-purple/15 text-violet">
          <Scale className="size-5" />
        </span>
        <div className="min-w-0 flex-1">
          <h2 className="text-lg font-extrabold leading-tight">Cân nặng</h2>
          <p className="text-xs text-muted-foreground">Bản ghi mới nhất được cập nhật vào hồ sơ và tính lại BMI, TDEE.</p>
        </div>
        <Tabs value={range} onValueChange={setRange}>
          <TabsList>
            {RANGES.map((r) => (
              <TabsTrigger key={r.value} value={r.value}>
                {r.label}
              </TabsTrigger>
            ))}
          </TabsList>
        </Tabs>
      </div>

      <div className="grid gap-6 lg:grid-cols-[1fr_320px]">
        <div className="min-w-0">
          <div className="mb-4 grid grid-cols-2 gap-2 sm:grid-cols-4">
            <div className="rounded-xl bg-gradient-to-br from-purple/20 to-blue/10 px-3 py-2">
              <p className="text-xs font-semibold text-muted-foreground">Hiện tại</p>
              <p className="text-2xl font-extrabold">{data?.hienTai != null ? `${fmt(data.hienTai, 1)} kg` : "–"}</p>
            </div>
            <Delta label="7 ngày" value={data?.thayDoi7Ngay ?? null} />
            <Delta label="30 ngày" value={data?.thayDoi30Ngay ?? null} />
            <div className="rounded-xl bg-muted/60 px-3 py-2">
              <p className="text-xs font-semibold text-muted-foreground">BMI</p>
              <p className="text-lg font-extrabold">
                {data?.bmi != null ? fmt(data.bmi, 1) : "–"}{" "}
                {data?.phanLoaiBmi && <span className="text-xs font-semibold text-muted-foreground">{data.phanLoaiBmi}</span>}
              </p>
            </div>
          </div>

          {rows.length < 2 ? (
            <div className="flex h-56 flex-col items-center justify-center rounded-xl border border-dashed text-center text-sm text-muted-foreground">
              <Scale className="mb-2 size-8 opacity-50" />
              Ghi ít nhất 2 lần để xem biểu đồ cân nặng.
            </div>
          ) : (
            <div className="h-56 w-full" role="img" aria-label={`Biểu đồ cân nặng ${range} ngày`}>
              <ResponsiveContainer width="100%" height="100%">
                <AreaChart data={rows} margin={{ top: 8, right: 8, left: -16, bottom: 0 }}>
                  <defs>
                    <linearGradient id="weight-fill" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stopColor="#9d68ed" stopOpacity={0.35} />
                      <stop offset="100%" stopColor="#9d68ed" stopOpacity={0} />
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="var(--track)" />
                  <XAxis dataKey="label" tickLine={false} axisLine={false} fontSize={12} minTickGap={20} />
                  <YAxis domain={[min, max]} tickLine={false} axisLine={false} fontSize={12} width={44} />
                  <Tooltip
                    content={({ active, payload }) => {
                      if (!active || !payload?.length) return null;
                      const p = payload[0].payload as (typeof rows)[number];
                      return (
                        <div className="rounded-xl border bg-card px-3 py-2 text-xs shadow-soft">
                          <p className="font-bold">{formatDate(p.ngay)}</p>
                          <p>
                            <b>{fmt(p.canNang, 1)} kg</b>
                          </p>
                          {p.ghiChu && <p className="text-muted-foreground">{p.ghiChu}</p>}
                        </div>
                      );
                    }}
                  />
                  <Area type="monotone" dataKey="canNang" stroke="#9d68ed" strokeWidth={3} fill="url(#weight-fill)" dot={{ r: 3, fill: "#9d68ed" }} activeDot={{ r: 5 }} />
                </AreaChart>
              </ResponsiveContainer>
            </div>
          )}
        </div>

        <div className="space-y-4">
          <form onSubmit={submit} className="space-y-3 rounded-xl border bg-background/60 p-4" noValidate>
            <h3 className="font-bold">Ghi cân nặng</h3>
            <div className="grid grid-cols-2 gap-2">
              <label className="text-xs font-semibold text-muted-foreground">
                Cân nặng (kg)
                <Input inputMode="decimal" placeholder="VD: 62,5" value={kg} onChange={(e) => setKg(e.target.value)} className="mt-1" aria-invalid={!!error} />
              </label>
              <label className="text-xs font-semibold text-muted-foreground">
                Ngày
                <Input type="date" max={todayStr()} value={ngay} onChange={(e) => setNgay(e.target.value)} className="mt-1" />
              </label>
            </div>
            <label className="block text-xs font-semibold text-muted-foreground">
              Ghi chú (tùy chọn)
              <Input maxLength={200} placeholder="VD: sau khi tập" value={ghiChu} onChange={(e) => setGhiChu(e.target.value)} className="mt-1" />
            </label>
            {error && (
              <p role="alert" className="text-sm font-medium text-destructive">
                {error}
              </p>
            )}
            <Button type="submit" className="w-full font-bold" disabled={log.isPending}>
              {log.isPending && <Loader2 className="animate-spin" />} Lưu cân nặng
            </Button>
          </form>

          {rows.length > 0 && (
            <div>
              <h3 className="mb-2 text-sm font-bold">Lần ghi gần đây</h3>
              <ul className="max-h-56 space-y-1 overflow-y-auto pr-1">
                {[...rows].reverse().slice(0, 15).map((w) => (
                  <li key={w.id} className="flex items-center gap-2 rounded-lg px-2 py-1.5 text-sm hover:bg-muted/60">
                    <span className="w-20 shrink-0 text-muted-foreground">{formatDate(w.ngay)}</span>
                    <span className="font-bold">{fmt(w.canNang, 1)} kg</span>
                    <span className="min-w-0 flex-1 truncate text-xs text-muted-foreground">{w.ghiChu}</span>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="size-7 text-destructive hover:bg-destructive/10"
                      aria-label={`Xóa bản ghi ngày ${formatDate(w.ngay)}`}
                      disabled={remove.isPending}
                      onClick={() => remove.mutate(w.id)}
                    >
                      <Trash2 className="size-3.5" />
                    </Button>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </div>
      </div>
    </section>
  );
}
