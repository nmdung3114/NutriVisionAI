"use client";

import { useMemo, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { AlertTriangle, Check, History, Info, PenLine, Sparkles, Undo2, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from "@/components/ui/alert-dialog";
import { EmptyState } from "@/components/shared/empty-state";
import { SafeImage } from "@/components/shared/safe-image";
import { useProfile, useUnits } from "@/hooks/use-api";
import { mealsApi } from "@/lib/api/meals";
import { ApiError, errorMessage } from "@/lib/api/errors";
import { MEAL_TYPES } from "@/lib/meal";
import { todayStr } from "@/lib/date";
import { cn } from "@/lib/utils";
import type { Food, LoaiBuaAn, Meal, Nutrition } from "@/types/api";
import { AddComponent } from "./add-component";
import { ComponentRow } from "./component-row";
import { NutritionSummary } from "./nutrition-summary";
import {
  blankRow,
  isDirty,
  mealToState,
  normalizeConfidence,
  per100FromNutrition,
  rowFromFood,
  toUpsert,
  totalNutrition,
  validateState,
  type EditorRow,
  type EditorState,
  type ValidationResult,
} from "./editor-state";

export type EditorMode = "draft" | "manual" | "edit";

interface Props {
  mode: EditorMode;
  meal?: Meal;
  initialLoai?: LoaiBuaAn;
  /** Banner khi AI thất bại. */
  failedNotice?: string | null;
  /** Chế độ edit: thoát khỏi trạng thái chỉnh sửa. */
  onExit?: () => void;
}

function ConfidenceBadge({ value }: { value: number | null }) {
  if (value === null) return null;
  const tone = value >= 80 ? "bg-brand-100 text-brand-700" : value >= 50 ? "bg-yellow/30 text-warn" : "bg-orange/20 text-danger-strong";
  return (
    <Badge className={cn("gap-1 rounded-full px-2.5 py-1 text-xs font-bold", tone)} title="Độ tin cậy của AI">
      <Sparkles className="size-3.5" /> AI tin cậy {value}%
    </Badge>
  );
}

export function MealEditor({ mode, meal, initialLoai = "Trua", failedNotice, onExit }: Props) {
  const router = useRouter();
  const qc = useQueryClient();
  const units = useUnits();
  const profile = useProfile();

  const initial = useMemo<EditorState>(
    () => (meal ? mealToState(meal) : { tenMonAn: "", loaiBuaAn: initialLoai, ngayAn: todayStr(), rows: [] }),
    // snapshot lúc mở trình chỉnh sửa; không tính lại khi props đổi
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [],
  );
  const [state, setState] = useState<EditorState>(initial);
  const [editingName, setEditingName] = useState(mode !== "edit" && !initial.tenMonAn);
  const [validation, setValidation] = useState<ValidationResult | null>(null);
  const [serverFlagged, setServerFlagged] = useState<Set<string>>(new Set());
  const [manualRow, setManualRow] = useState<string | null>(null);
  const [dialog, setDialog] = useState<null | "revert" | "cancel">(null);
  const nameRef = useRef<HTMLInputElement>(null);

  const unitList = useMemo(() => units.data ?? [], [units.data]);
  const target = meal?.mucTieuCalo ?? profile.data?.mucTieuCalo ?? 2000;
  const total = useMemo(() => totalNutrition(state.rows, unitList), [state.rows, unitList]);
  const dirty = isDirty(state, initial);
  const confidence = normalizeConfidence(meal?.doTinCay ?? meal?.aiGoc?.doTinCay);
  const hasAi = !!meal?.aiGoc;
  const aiFailed = !!failedNotice || (mode === "draft" && !hasAi && state.rows.length === 0 && initial.rows.length === 0);

  const patch = (p: Partial<EditorState>) => {
    setState((s) => ({ ...s, ...p }));
    setValidation(null);
  };
  const patchRow = (key: string, p: Partial<EditorRow>) => {
    setState((s) => ({ ...s, rows: s.rows.map((r) => (r.key === key ? { ...r, ...p } : r)) }));
    setServerFlagged((f) => {
      if (!f.has(key)) return f;
      const n = new Set(f);
      n.delete(key);
      return n;
    });
    setValidation(null);
  };
  const removeRow = (key: string) => {
    setState((s) => ({ ...s, rows: s.rows.filter((r) => r.key !== key) }));
    setValidation(null);
  };
  const addFood = (f: Food) => {
    setState((s) => ({ ...s, rows: [...s.rows, rowFromFood(f)], tenMonAn: s.tenMonAn || f.tenThucPham }));
    setValidation(null);
  };
  const addCustom = (name: string) => {
    const r = blankRow(name);
    setManualRow(r.key);
    setState((s) => ({ ...s, rows: [...s.rows, r], tenMonAn: s.tenMonAn || name }));
  };
  const applyFood = (key: string, f: Food) => {
    const base = rowFromFood(f);
    setState((s) => ({
      ...s,
      rows: s.rows.map((r) =>
        r.key === key ? { ...r, thamChieu: f, thucPhamThamChieuId: f.id, per100: base.per100, nhapTay: false, tenThanhPhan: r.tenThanhPhan || f.tenThucPham } : r,
      ),
    }));
    setServerFlagged(new Set());
    setValidation(null);
  };
  const applyManual = (key: string, n: Nutrition) => patchRow(key, { per100: per100FromNutrition(n), nhapTay: true, thamChieu: null, thucPhamThamChieuId: null });

  const invalidate = () => {
    for (const k of [["dashboard"], ["meals"], ["report"], ["suggestions"]]) void qc.invalidateQueries({ queryKey: k });
    if (meal) void qc.invalidateQueries({ queryKey: ["meal", meal.id] });
  };

  const handleSaveError = (e: unknown) => {
    if (e instanceof ApiError) {
      if (e.status === 409 && meal) {
        toast.info("Bữa ăn này đã được xác nhận trước đó.");
        router.replace(`/meals/${meal.id}`);
        return;
      }
      if (e.code === "NUTRITION_DATA_MISSING") {
        const idx = ((e.data as { thanhPhanThieu?: number[] } | null)?.thanhPhanThieu ?? []).map((i) => state.rows[i]?.key).filter(Boolean) as string[];
        setServerFlagged(new Set(idx));
        toast.error(e.message || "Một số thành phần chưa có dữ liệu dinh dưỡng.");
        if (idx[0]) document.getElementById(`row-${idx[0]}`)?.scrollIntoView({ behavior: "smooth", block: "center" });
        return;
      }
      if (e.code === "VALIDATION_ERROR") {
        const rows: Record<string, string> = {};
        let name: string | undefined;
        for (const [k, msgs] of Object.entries(e.fieldErrors)) {
          const m = /^thanhPhan\[(\d+)\]/.exec(k);
          if (m && state.rows[Number(m[1])]) rows[state.rows[Number(m[1])].key] = msgs[0];
          else if (/tenMonAn/i.test(k)) name = msgs[0];
        }
        setValidation({ ok: false, rows, tenMonAn: name, general: Object.keys(rows).length || name ? undefined : e.message });
        toast.error(e.message || "Dữ liệu chưa hợp lệ");
        return;
      }
    }
    toast.error(errorMessage(e));
  };

  const save = useMutation({
    mutationFn: async () => {
      const body = toUpsert(state);
      if (mode === "draft" && meal) return mealsApi.confirm(meal.id, body);
      if (mode === "edit" && meal) return mealsApi.update(meal.id, body);
      return mealsApi.createManual(body);
    },
    onSuccess: async (saved) => {
      invalidate();
      toast.success(mode === "edit" ? "Đã cập nhật bữa ăn" : "Đã lưu bữa ăn vào nhật ký dinh dưỡng!");
      if (mode === "edit") {
        qc.setQueryData(["meal", saved.id], saved);
        onExit?.();
      } else {
        const confetti = (await import("canvas-confetti")).default;
        void confetti({ particleCount: 120, spread: 75, origin: { y: 0.7 }, colors: ["#21b45a", "#ffc934", "#ff795e", "#4b9cf5"] });
        router.push("/meals");
      }
    },
    onError: handleSaveError,
  });

  const cancel = useMutation({
    mutationFn: async () => {
      if (mode === "draft" && meal) await mealsApi.cancel(meal.id);
    },
    onSuccess: () => {
      toast.success("Đã hủy bỏ bữa ăn");
      router.replace("/meals/new");
    },
    onError: (e) => toast.error(errorMessage(e)),
  });

  const revert = useMutation({
    mutationFn: async () => {
      if (mode === "edit" && meal) return mealsApi.revert(meal.id);
      return null;
    },
    onSuccess: (res) => {
      if (res) {
        setState(mealToState(res));
        qc.setQueryData(["meal", res.id], res);
        invalidate();
      } else {
        setState({ ...initial, rows: initial.rows.map((r) => ({ ...r })) });
      }
      setServerFlagged(new Set());
      setValidation(null);
      setDialog(null);
      toast.success("Đã khôi phục kết quả AI gốc");
    },
    onError: (e) => {
      setDialog(null);
      toast.error(errorMessage(e));
    },
  });

  const onSave = () => {
    const v = validateState(state);
    setValidation(v);
    if (!v.ok) {
      toast.error(v.general ?? v.tenMonAn ?? Object.values(v.rows)[0] ?? "Vui lòng kiểm tra lại thông tin");
      const firstKey = Object.keys(v.rows)[0];
      if (firstKey) document.getElementById(`row-${firstKey}`)?.scrollIntoView({ behavior: "smooth", block: "center" });
      else if (v.tenMonAn) {
        setEditingName(true);
        setTimeout(() => nameRef.current?.focus(), 0);
      }
      return;
    }
    save.mutate();
  };

  const onCancelClick = () => {
    if (mode === "edit") return dirty ? setDialog("cancel") : onExit?.();
    if (mode === "manual") return dirty || state.rows.length ? setDialog("cancel") : router.back();
    setDialog("cancel");
  };

  const busy = save.isPending || cancel.isPending || revert.isPending;
  const canRevert = (mode === "draft" && !!meal && !aiFailed && hasAi) || (mode === "edit" && hasAi);
  const saveLabel = mode === "edit" ? "LƯU THAY ĐỔI" : "XÁC NHẬN VÀ LƯU BỮA ĂN";

  return (
    <div className="space-y-5">
      {aiFailed && (
        <div className="flex gap-3 rounded-2xl border border-orange/50 bg-orange/10 p-4" role="alert">
          <AlertTriangle className="mt-0.5 size-5 shrink-0 text-danger-strong" />
          <div className="text-sm">
            <p className="font-bold text-danger-strong">AI chưa nhận diện được món ăn trong ảnh</p>
            <p className="text-danger-strong/90">{failedNotice || "Bạn hãy nhập tên món và các thành phần thủ công. Ảnh đã tải lên vẫn được giữ cho bữa ăn này."}</p>
          </div>
        </div>
      )}

      <div className="grid gap-5 lg:grid-cols-[minmax(0,360px)_minmax(0,1fr)] lg:items-start">
        {/* Cột trái */}
        <div className="space-y-4 lg:sticky lg:top-20">
          {mode !== "manual" && (
            <div className="overflow-hidden rounded-2xl border bg-card shadow-soft">
              <SafeImage src={meal?.hinhAnhUrl} alt={meal?.tenMonAn || "Ảnh bữa ăn"} className="aspect-[4/3] w-full" />
            </div>
          )}
          <NutritionSummary total={total} target={target} />
        </div>

        {/* Cột phải */}
        <div className="space-y-4">
          <div className="rounded-2xl border bg-card p-4 shadow-soft sm:p-5">
            <div className="flex flex-wrap items-start justify-between gap-2">
              <div className="flex flex-wrap items-center gap-2">
                {meal?.taoTuAI && <ConfidenceBadge value={confidence} />}
                {mode === "manual" && (
                  <Badge variant="secondary" className="gap-1 rounded-full px-2.5 py-1">
                    <PenLine className="size-3.5" /> Nhập tay
                  </Badge>
                )}
                {meal?.nguoiDungCoSua && <Badge variant="outline" className="rounded-full">Đã chỉnh sửa</Badge>}
              </div>
              {canRevert && (
                <Button type="button" variant="outline" size="sm" onClick={() => setDialog("revert")} disabled={busy}>
                  <Undo2 /> Hoàn tác AI gốc
                </Button>
              )}
            </div>

            <div className="mt-3">
              <label htmlFor="tenMonAn" className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                Tên món ăn
              </label>
              {editingName ? (
                <Input
                  id="tenMonAn"
                  ref={nameRef}
                  autoFocus
                  value={state.tenMonAn}
                  maxLength={200}
                  onChange={(e) => patch({ tenMonAn: e.target.value })}
                  onBlur={() => state.tenMonAn.trim() && setEditingName(false)}
                  onKeyDown={(e) => e.key === "Enter" && state.tenMonAn.trim() && setEditingName(false)}
                  placeholder="Ví dụ: Cơm gà xối mỡ"
                  className="mt-1 h-11 text-lg font-bold"
                  aria-invalid={!!validation?.tenMonAn}
                />
              ) : (
                <div className="mt-1 flex items-center gap-2">
                  <h2 id="tenMonAn" className="min-w-0 flex-1 truncate text-2xl font-extrabold">
                    {state.tenMonAn}
                  </h2>
                  <Button type="button" variant="ghost" size="icon" onClick={() => setEditingName(true)} aria-label="Sửa tên món ăn">
                    <PenLine className="size-4" />
                  </Button>
                </div>
              )}
              {validation?.tenMonAn && (
                <p role="alert" className="mt-1 text-xs font-medium text-destructive">
                  {validation.tenMonAn}
                </p>
              )}
            </div>

            <div className="mt-4 grid grid-cols-2 gap-3 sm:max-w-md">
              <div>
                <label className="text-xs font-semibold text-muted-foreground" htmlFor="loaiBuaAn">
                  Loại bữa
                </label>
                <Select value={state.loaiBuaAn} onValueChange={(v) => patch({ loaiBuaAn: v as LoaiBuaAn })}>
                  <SelectTrigger id="loaiBuaAn" className="mt-1 h-10 w-full">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {MEAL_TYPES.map((t) => (
                      <SelectItem key={t.value} value={t.value}>
                        {t.emoji} {t.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div>
                <label className="text-xs font-semibold text-muted-foreground" htmlFor="ngayAn">
                  Ngày ăn
                </label>
                <Input id="ngayAn" type="date" max={todayStr()} value={state.ngayAn} onChange={(e) => patch({ ngayAn: e.target.value || todayStr() })} className="mt-1 h-10" />
              </div>
            </div>
          </div>

          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <h3 className="text-lg font-extrabold">Thành phần ({state.rows.length})</h3>
              {units.isLoading && <Skeleton className="h-5 w-24" />}
            </div>
            <AddComponent onPickFood={addFood} onAddCustom={addCustom} />
            {validation?.general && (
              <p role="alert" className="flex items-center gap-1.5 text-sm font-medium text-destructive">
                <Info className="size-4" /> {validation.general}
              </p>
            )}
            {state.rows.length === 0 ? (
              <EmptyState
                className="py-8"
                title={mode === "manual" || aiFailed ? "Hãy thêm thành phần cho món ăn" : "Chưa có thành phần nào"}
                description="Tìm thực phẩm ở ô phía trên để thêm vào bữa ăn, sau đó nhập khẩu phần và đơn vị."
              />
            ) : (
              <ul className="space-y-3">
                {state.rows.map((r, i) => (
                  <ComponentRow
                    key={r.key}
                    row={r}
                    index={i}
                    units={unitList}
                    error={validation?.rows[r.key]}
                    flagged={serverFlagged.has(r.key)}
                    autoOpenManual={manualRow === r.key}
                    onChange={(p) => patchRow(r.key, p)}
                    onRemove={() => removeRow(r.key)}
                    onPickFood={(f) => applyFood(r.key, f)}
                    onManual={(n) => applyManual(r.key, n)}
                  />
                ))}
              </ul>
            )}
          </div>
        </div>
      </div>

      {/* Thanh hành động */}
      <div className="sticky bottom-[84px] z-20 -mx-1 flex items-center justify-between gap-3 rounded-2xl border bg-card/95 p-3 shadow-lg backdrop-blur lg:bottom-4">
        <Button type="button" variant="outline" onClick={onCancelClick} disabled={busy} className="border-destructive/40 text-destructive hover:bg-destructive/10">
          <X /> {mode === "edit" ? "HỦY CHỈNH SỬA" : "HỦY BỎ"}
        </Button>
        <Button type="button" size="lg" onClick={onSave} disabled={busy} className="flex-1 font-extrabold sm:flex-none sm:px-8">
          {save.isPending ? (
            <>
              <History className="animate-spin" /> Đang lưu…
            </>
          ) : (
            <>
              <Check /> {saveLabel}
            </>
          )}
        </Button>
      </div>

      <AlertDialog open={dialog === "revert"} onOpenChange={(o) => !o && setDialog(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Hoàn tác về kết quả AI gốc?</AlertDialogTitle>
            <AlertDialogDescription>Tên món và các thành phần sẽ được khôi phục như AI nhận diện ban đầu. Mọi chỉnh sửa của bạn sẽ mất.</AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Giữ chỉnh sửa</AlertDialogCancel>
            <AlertDialogAction
              onClick={(e) => {
                e.preventDefault();
                revert.mutate();
              }}
            >
              {revert.isPending ? "Đang khôi phục…" : "Hoàn tác"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <AlertDialog open={dialog === "cancel"} onOpenChange={(o) => !o && setDialog(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{mode === "edit" ? "Bỏ các thay đổi?" : "Hủy bỏ bữa ăn này?"}</AlertDialogTitle>
            <AlertDialogDescription>
              {mode === "draft" ? "Bản nháp và ảnh đã tải lên sẽ bị xóa. Bạn không thể khôi phục." : mode === "edit" ? "Các chỉnh sửa chưa lưu sẽ bị bỏ." : "Dữ liệu bạn đã nhập sẽ không được lưu."}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Tiếp tục chỉnh sửa</AlertDialogCancel>
            <AlertDialogAction
              className="bg-destructive text-white hover:bg-destructive/90"
              onClick={(e) => {
                e.preventDefault();
                if (mode === "edit") {
                  setDialog(null);
                  onExit?.();
                } else if (mode === "manual") router.back();
                else cancel.mutate();
              }}
            >
              {cancel.isPending ? "Đang hủy…" : "Đồng ý hủy"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
