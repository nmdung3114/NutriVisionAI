"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { ArrowLeft, Pencil, Sparkles, Trash2, Undo2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from "@/components/ui/alert-dialog";
import { PageHeader } from "@/components/shared/page-header";
import { PageSkeleton } from "@/components/layout/app-shell";
import { ErrorState } from "@/components/shared/error-state";
import { SafeImage } from "@/components/shared/safe-image";
import { DonutLegend, MacroDonut, macroSlices } from "@/components/shared/macro-donut";
import { useMeal } from "@/hooks/use-api";
import { mealsApi } from "@/lib/api/meals";
import { errorMessage } from "@/lib/api/errors";
import { formatDate, formatTime } from "@/lib/date";
import { fmt, mealTypeLabel } from "@/lib/meal";
import { dailyMacroTargets, percentOf } from "@/lib/nutrition";
import { MealEditor } from "./editor/meal-editor";
import { DeleteMealDialog } from "./delete-meal-dialog";
import { normalizeConfidence } from "./editor/editor-state";
import type { Meal } from "@/types/api";

function AiComparison({ meal }: { meal: Meal }) {
  const ai = meal.aiGoc!;
  const finalNames = new Set(meal.thanhPhan.map((c) => `${c.tenThanhPhan}|${c.khauPhan}|${c.donViTinh}`));
  const aiKeys = new Set(ai.thanhPhan.map((c) => `${c.tenThanhPhan}|${c.khauPhan}|${c.donViTinh}`));
  return (
    <section className="rounded-2xl border bg-card p-5 shadow-soft" aria-label="So sánh AI gốc và kết quả cuối">
      <h2 className="mb-3 flex items-center gap-2 text-lg font-extrabold">
        <Sparkles className="size-5 text-primary" /> So sánh AI gốc và kết quả cuối
      </h2>
      <div className="grid gap-4 md:grid-cols-2">
        <div>
          <p className="mb-2 text-sm font-bold text-muted-foreground">AI nhận diện: {ai.tenMonAn || "(trống)"}</p>
          <ul className="space-y-1.5 text-sm">
            {ai.thanhPhan.map((c, i) => (
              <li key={i} className={`rounded-lg px-3 py-1.5 ${finalNames.has(`${c.tenThanhPhan}|${c.khauPhan}|${c.donViTinh}`) ? "bg-muted/60" : "bg-red-50 text-red-700 line-through"}`}>
                {c.tenThanhPhan} – {fmt(c.khauPhan, 2)} {c.donViTinh}
              </li>
            ))}
          </ul>
        </div>
        <div>
          <p className="mb-2 text-sm font-bold text-muted-foreground">Kết quả cuối: {meal.tenMonAn}</p>
          <ul className="space-y-1.5 text-sm">
            {meal.thanhPhan.map((c, i) => (
              <li key={i} className={`rounded-lg px-3 py-1.5 ${aiKeys.has(`${c.tenThanhPhan}|${c.khauPhan}|${c.donViTinh}`) ? "bg-muted/60" : "bg-brand-50 text-brand-700 font-semibold"}`}>
                {c.tenThanhPhan} – {fmt(c.khauPhan, 2)} {c.donViTinh}
              </li>
            ))}
          </ul>
        </div>
      </div>
      <p className="mt-3 text-xs text-muted-foreground">Mục đỏ gạch ngang: đã bị thay đổi/xóa · mục xanh: do bạn chỉnh sửa hoặc thêm.</p>
    </section>
  );
}

export function MealDetailPage({ id }: { id: string }) {
  const router = useRouter();
  const qc = useQueryClient();
  const sp = useSearchParams();
  const { data: meal, error, isLoading, refetch } = useMeal(id);
  const [editing, setEditing] = useState(sp.get("edit") === "1");
  const [del, setDel] = useState(false);
  const [revertOpen, setRevertOpen] = useState(false);

  useEffect(() => {
    if (meal && !meal.daXacNhan) router.replace(`/meals/review/${meal.id}`);
  }, [meal, router]);

  const revert = useMutation({
    mutationFn: () => mealsApi.revert(id),
    onSuccess: (m) => {
      qc.setQueryData(["meal", id], m);
      for (const k of ["meals", "dashboard", "report"]) void qc.invalidateQueries({ queryKey: [k] });
      toast.success("Đã hoàn tác về kết quả AI gốc");
      setRevertOpen(false);
    },
    onError: (e) => {
      toast.error(errorMessage(e));
      setRevertOpen(false);
    },
  });

  if (isLoading) return <PageSkeleton />;
  if (error || !meal)
    return (
      <ErrorState
        error={error}
        onRetry={() => void refetch()}
        action={
          <Button asChild variant="outline">
            <Link href="/meals">Về lịch sử</Link>
          </Button>
        }
      />
    );
  if (!meal.daXacNhan) return <PageSkeleton />;

  const n = meal.tongDinhDuong;
  const macro = dailyMacroTargets(meal.mucTieuCalo);
  const conf = normalizeConfidence(meal.doTinCay);

  if (editing)
    return (
      <>
        <PageHeader title="Chỉnh sửa bữa ăn" description={`${mealTypeLabel(meal.loaiBuaAn)} · ${formatDate(meal.ngayAn)}`} />
        <MealEditor mode="edit" meal={meal} onExit={() => setEditing(false)} />
      </>
    );

  return (
    <div className="space-y-6">
      <Button asChild variant="ghost" size="sm" className="-ml-2">
        <Link href="/meals">
          <ArrowLeft /> Lịch sử bữa ăn
        </Link>
      </Button>
      <PageHeader
        title={meal.tenMonAn}
        description={
          <span className="flex flex-wrap items-center gap-2">
            {mealTypeLabel(meal.loaiBuaAn)} · {formatDate(meal.ngayAn)} · {formatTime(meal.thoiGianGhiNhan)}
            {meal.taoTuAI && <Badge variant="secondary" className="gap-1 rounded-full"><Sparkles className="size-3" /> AI{conf !== null ? ` ${conf}%` : ""}</Badge>}
            {meal.nguoiDungCoSua && <Badge variant="outline" className="rounded-full">Đã chỉnh sửa</Badge>}
          </span>
        }
        actions={
          <>
            {meal.aiGoc && (
              <Button variant="outline" onClick={() => setRevertOpen(true)}>
                <Undo2 /> Hoàn tác về AI gốc
              </Button>
            )}
            <Button onClick={() => setEditing(true)}>
              <Pencil /> Sửa
            </Button>
            <Button variant="outline" className="border-destructive/40 text-destructive hover:bg-destructive/10" onClick={() => setDel(true)}>
              <Trash2 /> Xóa
            </Button>
          </>
        }
      />

      <div className="grid gap-6 lg:grid-cols-[360px_1fr]">
        <div className="space-y-4">
          <div className="overflow-hidden rounded-2xl border bg-card shadow-soft">
            <SafeImage src={meal.hinhAnhUrl} alt={meal.tenMonAn} className="aspect-[4/3] w-full" />
          </div>
          <section className="rounded-2xl border bg-card p-5 shadow-soft" aria-label="Cơ cấu dinh dưỡng">
            <p className="text-sm font-semibold text-muted-foreground">Tổng năng lượng</p>
            <p className="text-3xl font-extrabold">
              {fmt(n.calo)} <span className="text-base font-semibold text-muted-foreground">kcal · {fmt(meal.phanTramNhuCauNgay)}% nhu cầu ngày</span>
            </p>
            <MacroDonut slices={macroSlices(n)} centerLabel={`${fmt(n.chatDam + n.tinhBot + n.chatBeo, 0)}g`} centerSub="macro" unit="g" height={190} />
            <DonutLegend slices={macroSlices(n)} unit="g" />
            <ul className="mt-4 space-y-1 text-xs text-muted-foreground">
              <li>Đạm: {percentOf(n.chatDam, macro.chatDam)}% nhu cầu ngày</li>
              <li>Tinh bột: {percentOf(n.tinhBot, macro.tinhBot)}% nhu cầu ngày</li>
              <li>Béo: {percentOf(n.chatBeo, macro.chatBeo)}% nhu cầu ngày</li>
              <li>Xơ: {fmt(n.chatXo, 1)}g</li>
            </ul>
          </section>
        </div>

        <section className="rounded-2xl border bg-card p-4 shadow-soft sm:p-5" aria-label="Thành phần">
          <h2 className="mb-3 text-lg font-extrabold">Thành phần ({meal.thanhPhan.length})</h2>
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Thành phần</TableHead>
                  <TableHead className="text-right">Khẩu phần</TableHead>
                  <TableHead className="text-right">Calo</TableHead>
                  <TableHead className="text-right">Đạm</TableHead>
                  <TableHead className="text-right">Tinh bột</TableHead>
                  <TableHead className="text-right">Béo</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {meal.thanhPhan.map((c, i) => (
                  <TableRow key={c.id ?? i}>
                    <TableCell className="font-semibold">{c.tenThanhPhan}</TableCell>
                    <TableCell className="text-right">
                      {fmt(c.khauPhan, 2)} {c.donViTinh}
                      <span className="block text-xs text-muted-foreground">{fmt(c.khoiLuongQuyDoiGram, 1)}g</span>
                    </TableCell>
                    <TableCell className="text-right font-bold text-primary">{fmt(c.luongCalo)}</TableCell>
                    <TableCell className="text-right">{fmt(c.luongChatDam, 1)}g</TableCell>
                    <TableCell className="text-right">{fmt(c.luongTinhBot, 1)}g</TableCell>
                    <TableCell className="text-right">{fmt(c.luongChatBeo, 1)}g</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </section>
      </div>

      {meal.aiGoc && <AiComparison meal={meal} />}

      <DeleteMealDialog meal={del ? meal : null} onClose={() => setDel(false)} onDeleted={() => router.replace("/meals")} />
      <AlertDialog open={revertOpen} onOpenChange={setRevertOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Hoàn tác về kết quả AI gốc?</AlertDialogTitle>
            <AlertDialogDescription>Tên món và thành phần sẽ được khôi phục như AI nhận diện ban đầu, dinh dưỡng được tính lại.</AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Giữ nguyên</AlertDialogCancel>
            <AlertDialogAction onClick={(e) => { e.preventDefault(); revert.mutate(); }}>{revert.isPending ? "Đang khôi phục…" : "Hoàn tác"}</AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
