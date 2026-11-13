"use client";

import { Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { fmt } from "@/lib/meal";
import { normalizeText } from "@/lib/nutrition";
import { cn } from "@/lib/utils";
import type { Food, Nutrition, UnitConversion } from "@/types/api";
import { rowGram, rowNeedsData, rowNutrition, type EditorRow } from "./editor-state";
import { ResolverPanel } from "./resolver-panel";

export function ComponentRow({
  row,
  units,
  error,
  flagged,
  autoOpenManual,
  index,
  onChange,
  onRemove,
  onPickFood,
  onManual,
}: {
  row: EditorRow;
  units: UnitConversion[];
  error?: string;
  flagged?: boolean;
  autoOpenManual?: boolean;
  index: number;
  onChange: (patch: Partial<EditorRow>) => void;
  onRemove: () => void;
  onPickFood: (f: Food) => void;
  onManual: (n: Nutrition) => void;
}) {
  const nut = rowNutrition(row, units);
  const needsData = rowNeedsData(row);
  const gram = rowGram(row, units);
  const names = new Set(units.map((u) => u.donVi));
  const options = [...units.map((u) => u.donVi)];
  for (const extra of [row.thamChieu?.donViChuan, row.donViTinh]) if (extra && !names.has(extra) && !options.some((o) => normalizeText(o) === normalizeText(extra))) options.push(extra);
  const selectedValue = options.find((o) => normalizeText(o) === normalizeText(row.donViTinh)) ?? row.donViTinh;
  const idBase = `row-${row.key}`;

  return (
    <li
      id={idBase}
      className={cn(
        "rounded-2xl border bg-card p-3 shadow-soft transition-colors sm:p-4",
        (error || flagged) && "border-destructive/60 bg-red-50/50 ring-1 ring-destructive/20",
      )}
    >
      <div className="grid gap-3 sm:grid-cols-[1fr_auto] sm:items-start">
        <div className="min-w-0">
          <label htmlFor={`${idBase}-name`} className="sr-only">
            Tên thành phần {index + 1}
          </label>
          <Input
            id={`${idBase}-name`}
            value={row.tenThanhPhan}
            onChange={(e) => onChange({ tenThanhPhan: e.target.value })}
            maxLength={200}
            className="h-10 border-transparent bg-transparent px-2 text-base font-bold shadow-none hover:border-input focus-visible:border-ring"
            aria-invalid={!!error}
            aria-describedby={error ? `${idBase}-err` : undefined}
          />
          {row.thamChieu && !needsData && (
            <p className="px-2 text-xs text-muted-foreground">
              Tham chiếu: {row.thamChieu.tenThucPham} · {fmt(row.thamChieu.caloTren100g)} kcal/100g
            </p>
          )}
          {row.nhapTay && <p className="px-2 text-xs text-muted-foreground">Dinh dưỡng nhập tay ({fmt(row.per100?.calo)} kcal/100g)</p>}
        </div>

        <div className="flex flex-wrap items-center gap-2 sm:justify-end">
          <Input
            type="number"
            inputMode="decimal"
            min={0}
            step="any"
            value={row.khauPhan}
            onChange={(e) => onChange({ khauPhan: e.target.value })}
            className="h-10 w-24 text-right font-semibold"
            aria-label={`Khẩu phần của ${row.tenThanhPhan || `thành phần ${index + 1}`}`}
            aria-invalid={!!error}
          />
          <Select value={selectedValue} onValueChange={(v) => onChange({ donViTinh: v })}>
            <SelectTrigger className="h-10 w-28" aria-label="Đơn vị tính">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {options.map((o) => (
                <SelectItem key={o} value={o}>
                  {o}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <div className="w-24 text-right" aria-live="polite">
            <p className={cn("text-lg font-extrabold leading-none", needsData ? "text-muted-foreground" : "text-primary")}>{nut ? fmt(nut.calo) : "–"}</p>
            <p className="text-[11px] text-muted-foreground">kcal{gram > 0 && nut ? ` · ${fmt(gram, 1)}g` : ""}</p>
          </div>
          <Button type="button" variant="ghost" size="icon" onClick={onRemove} aria-label={`Xóa thành phần ${row.tenThanhPhan}`} className="text-muted-foreground hover:text-destructive">
            <Trash2 className="size-4" />
          </Button>
        </div>
      </div>

      {error && (
        <p id={`${idBase}-err`} role="alert" className="mt-2 px-2 text-xs font-medium text-destructive">
          {error}
        </p>
      )}

      {needsData && (
        <ResolverPanel name={row.tenThanhPhan} initialMode={autoOpenManual ? "manual" : "idle"} onPickFood={onPickFood} onManual={onManual} onRemove={onRemove} />
      )}
    </li>
  );
}
