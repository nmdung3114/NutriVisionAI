"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { AlertTriangle, Search, Trash2, PenLine } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { foodsApi } from "@/lib/api/foods";
import { fmt } from "@/lib/meal";
import type { Food, Nutrition } from "@/types/api";

type Mode = "idle" | "match" | "manual";

const FIELDS: { key: keyof Nutrition; label: string }[] = [
  { key: "calo", label: "Calo (kcal)" },
  { key: "chatDam", label: "Đạm (g)" },
  { key: "tinhBot", label: "Tinh bột (g)" },
  { key: "chatBeo", label: "Béo (g)" },
  { key: "chatXo", label: "Xơ (g)" },
];

/** Hiển thị khi thành phần chưa có dữ liệu dinh dưỡng: chọn gần đúng / nhập tay / xóa. */
export function ResolverPanel({
  name,
  initialMode = "idle",
  onPickFood,
  onManual,
  onRemove,
}: {
  name: string;
  initialMode?: Mode;
  onPickFood: (f: Food) => void;
  onManual: (n: Nutrition) => void;
  onRemove: () => void;
}) {
  const [mode, setMode] = useState<Mode>(initialMode);
  const [vals, setVals] = useState<Record<keyof Nutrition, string>>({ calo: "", chatDam: "", tinhBot: "", chatBeo: "", chatXo: "" });
  const [err, setErr] = useState<string | null>(null);

  const match = useQuery({
    queryKey: ["foods", "match", name],
    queryFn: () => foodsApi.match(name),
    enabled: mode === "match" && name.trim().length > 0,
    staleTime: 60_000,
  });

  const submitManual = () => {
    const parsed = {} as Nutrition;
    for (const f of FIELDS) {
      const raw = vals[f.key].trim().replace(",", ".");
      const n = raw === "" ? (f.key === "calo" ? NaN : 0) : Number(raw);
      if (!Number.isFinite(n) || n < 0) {
        setErr(f.key === "calo" ? "Vui lòng nhập calo trên 100g (số ≥ 0)" : `Giá trị "${f.label}" không hợp lệ`);
        return;
      }
      parsed[f.key] = n;
    }
    if (parsed.calo > 900) {
      setErr("Calo trên 100g không thể vượt 900 kcal");
      return;
    }
    setErr(null);
    onManual(parsed);
  };

  return (
    <div className="mt-3 rounded-xl border border-orange/40 bg-orange/10 p-3 text-sm" role="group" aria-label={`Bổ sung dữ liệu dinh dưỡng cho ${name}`}>
      <p className="flex items-start gap-2 font-semibold text-danger-strong">
        <AlertTriangle className="mt-0.5 size-4 shrink-0" />
        Chưa có dữ liệu dinh dưỡng cho &quot;{name || "thành phần này"}&quot;
      </p>
      <div className="mt-2 flex flex-wrap gap-2">
        <Button type="button" size="sm" variant={mode === "match" ? "default" : "outline"} onClick={() => setMode(mode === "match" ? "idle" : "match")}>
          <Search /> Chọn thực phẩm gần đúng
        </Button>
        <Button type="button" size="sm" variant={mode === "manual" ? "default" : "outline"} onClick={() => setMode(mode === "manual" ? "idle" : "manual")}>
          <PenLine /> Nhập tay (trên 100g)
        </Button>
        <Button type="button" size="sm" variant="ghost" className="text-destructive" onClick={onRemove}>
          <Trash2 /> Xóa
        </Button>
      </div>

      {mode === "match" && (
        <div className="mt-3">
          {match.isLoading ? (
            <Skeleton className="h-16 rounded-lg" />
          ) : !match.data?.length ? (
            <p className="text-muted-foreground">Không có gợi ý gần đúng. Hãy nhập tay hoặc xóa thành phần này.</p>
          ) : (
            <ul className="space-y-1.5">
              {match.data.map((m) => (
                <li key={m.food.id}>
                  <button
                    type="button"
                    onClick={() => onPickFood(m.food)}
                    className="flex w-full items-center justify-between gap-3 rounded-lg border bg-card px-3 py-2 text-left hover:border-primary hover:bg-accent focus-visible:ring-2 focus-visible:ring-ring"
                  >
                    <span className="min-w-0">
                      <span className="block truncate font-semibold">{m.food.tenThucPham}</span>
                      <span className="block text-xs text-muted-foreground">
                        {fmt(m.food.caloTren100g)} kcal/100g · Độ giống {Math.round(m.diem * 100)}%
                      </span>
                    </span>
                    <span className="shrink-0 text-xs font-bold text-primary">Chọn</span>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}

      {mode === "manual" && (
        <div className="mt-3 space-y-2">
          <div className="grid grid-cols-2 gap-2 sm:grid-cols-5">
            {FIELDS.map((f) => (
              <label key={f.key} className="block text-xs font-medium text-muted-foreground">
                {f.label}
                <Input
                  inputMode="decimal"
                  value={vals[f.key]}
                  onChange={(e) => setVals((v) => ({ ...v, [f.key]: e.target.value }))}
                  className="mt-1 h-9 bg-card"
                  placeholder="0"
                  aria-label={`${f.label} trên 100g`}
                />
              </label>
            ))}
          </div>
          {err && (
            <p role="alert" className="text-xs font-medium text-destructive">
              {err}
            </p>
          )}
          <Button type="button" size="sm" onClick={submitManual}>
            Áp dụng giá trị
          </Button>
        </div>
      )}
    </div>
  );
}
