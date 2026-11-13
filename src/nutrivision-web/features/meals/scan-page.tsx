"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { AlertCircle, Camera, ImagePlus, Loader2, PenLine, ScanLine, Trash2, UploadCloud, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { PageHeader } from "@/components/shared/page-header";
import { mealsApi } from "@/lib/api/meals";
import { ApiError, errorHint, errorMessage } from "@/lib/api/errors";
import { formatBytes, validateMealImage } from "@/lib/image";
import { MEAL_THEME, MEAL_TYPES, isMealType, mealTypeByHour } from "@/lib/meal";
import { QuickLog } from "./quick/quick-log";
import { ScanOverlay, ScanSteps } from "./scan-overlay";
import { cn } from "@/lib/utils";
import type { LoaiBuaAn } from "@/types/api";
import { CameraDialog } from "./camera-dialog";

interface Picked {
  file: File;
  url: string;
  width: number;
  height: number;
}

export const scanNoticeKey = (id: string) => `nv_scan_notice_${id}`;

export function ScanPage() {
  const router = useRouter();
  const qc = useQueryClient();
  const params = useSearchParams();
  const [loai, setLoai] = useState<LoaiBuaAn>("Trua");
  const [picked, setPicked] = useState<Picked | null>(null);
  const [error, setError] = useState<{ title: string; hint?: string } | null>(null);
  const [drag, setDrag] = useState(false);
  const [camera, setCamera] = useState(false);
  const [scanning, setScanning] = useState(false);
  const [progress, setProgress] = useState(0);
  const [done, setDone] = useState(false);
  const abortRef = useRef<AbortController | null>(null);
  const fileRef = useRef<HTMLInputElement>(null);
  const captureRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    const q = params.get("loai");
    setLoai(isMealType(q) ? q : mealTypeByHour(new Date().getHours()));
  }, [params]);

  useEffect(() => {
    return () => {
      if (picked) URL.revokeObjectURL(picked.url);
    };
  }, [picked]);

  // Tiến trình giả lập tiệm cận 95% trong khi chờ AI.
  useEffect(() => {
    if (!scanning) return;
    setProgress(4);
    const t = setInterval(() => setProgress((p) => (p < 95 ? p + (95 - p) * 0.04 : p)), 400);
    return () => clearInterval(t);
  }, [scanning]);

  const accept = useCallback(async (file: File | undefined) => {
    if (!file) return;
    const res = await validateMealImage(file);
    if ("error" in res) {
      setPicked(null);
      setError({ title: res.error });
      return;
    }
    setError(null);
    setPicked({ file, url: URL.createObjectURL(file), width: res.width, height: res.height });
  }, []);

  const startScan = async () => {
    if (!picked) return;
    const ctrl = new AbortController();
    abortRef.current = ctrl;
    setScanning(true);
    setError(null);
    try {
      const meal = await mealsApi.scan(picked.file, loai, undefined, ctrl.signal);
      setProgress(100);
      setDone(true);
      void qc.invalidateQueries({ queryKey: ["meals"] });
      if (meal.aiThatBai) {
        try {
          sessionStorage.setItem(scanNoticeKey(meal.id), meal.thongBaoAI ?? "AI không nhận diện được món ăn. Hãy nhập thủ công.");
        } catch {
          /* bỏ qua */
        }
        toast.warning("AI chưa nhận diện được món ăn – hãy nhập thủ công.");
      }
      await new Promise((r) => setTimeout(r, 600)); // cho người dùng thấy dấu ✓ hoàn tất
      router.push(`/meals/review/${meal.id}`);
    } catch (e) {
      setScanning(false);
      if (e instanceof ApiError && e.code === "ABORTED") {
        toast.info("Đã hủy nhận diện.");
        return;
      }
      setError({ title: errorMessage(e), hint: e instanceof ApiError ? errorHint(e.code) : undefined });
    }
  };

  return (
    <div className="mx-auto max-w-3xl">
      <PageHeader title="Thêm bữa ăn" description="Bước 1/2 · Chọn loại bữa và tải ảnh để AI nhận diện món ăn." />

      <fieldset className="mb-5">
        <legend className="mb-2 text-sm font-bold">Loại bữa ăn</legend>
        <div className="grid grid-cols-2 gap-2 sm:grid-cols-4" role="radiogroup">
          {MEAL_TYPES.map((t) => (
            <button
              key={t.value}
              type="button"
              role="radio"
              aria-checked={loai === t.value}
              onClick={() => setLoai(t.value)}
              className={cn(
                "flex items-center justify-center gap-2 rounded-xl border px-3 py-3 text-sm font-bold transition-all outline-none focus-visible:ring-2 focus-visible:ring-ring",
                loai === t.value ? cn("scale-[1.02] border-transparent shadow-md", MEAL_THEME[t.value].solid) : "bg-card hover:-translate-y-0.5 hover:border-primary/50",
              )}
            >
              <span aria-hidden>{t.emoji}</span>
              {t.label}
            </button>
          ))}
        </div>
      </fieldset>

      {error && (
        <Alert variant="destructive" className="mb-4" role="alert">
          <AlertCircle />
          <AlertTitle>{error.title}</AlertTitle>
          {error.hint && error.hint !== error.title && <AlertDescription>{error.hint}</AlertDescription>}
        </Alert>
      )}

      {!picked ? (
        <div
          onDragOver={(e) => {
            e.preventDefault();
            setDrag(true);
          }}
          onDragLeave={() => setDrag(false)}
          onDrop={(e) => {
            e.preventDefault();
            setDrag(false);
            void accept(e.dataTransfer.files?.[0]);
          }}
          className={cn("rounded-3xl border-2 border-dashed bg-card p-8 text-center transition-colors sm:p-12", drag ? "border-primary bg-brand-50" : "border-input")}
        >
          <span className="mx-auto flex size-16 items-center justify-center rounded-2xl bg-brand-50 text-primary">
            <UploadCloud className="size-8" />
          </span>
          <p className="mt-4 text-lg font-extrabold">Kéo thả ảnh món ăn vào đây</p>
          <p className="mt-1 text-sm text-muted-foreground">JPG, JPEG, PNG hoặc WEBP · tối đa 10MB · tối thiểu 224×224 px</p>
          <div className="mt-5 flex flex-wrap justify-center gap-3">
            <Button type="button" onClick={() => fileRef.current?.click()}>
              <ImagePlus /> Chọn ảnh từ máy
            </Button>
            <Button
              type="button"
              variant="outline"
              onClick={() => {
                const coarse = window.matchMedia("(pointer: coarse)").matches;
                if (!coarse && "mediaDevices" in navigator) setCamera(true);
                else captureRef.current?.click();
              }}
            >
              <Camera /> Chụp ảnh
            </Button>
          </div>
          <input ref={fileRef} type="file" accept="image/jpeg,image/png,image/webp" className="sr-only" tabIndex={-1} aria-label="Chọn tệp ảnh" onChange={(e) => { void accept(e.target.files?.[0]); e.target.value = ""; }} />
          <input ref={captureRef} type="file" accept="image/*" capture="environment" className="sr-only" tabIndex={-1} aria-label="Chụp ảnh bằng camera" onChange={(e) => { void accept(e.target.files?.[0]); e.target.value = ""; }} />
        </div>
      ) : (
        <div className="overflow-hidden rounded-3xl border bg-card shadow-soft">
          <div className="relative bg-black/5">
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={picked.url} alt="Ảnh bữa ăn đã chọn" className="mx-auto max-h-[420px] w-full object-contain" />
            {scanning && <ScanOverlay done={done} />}
          </div>
          <div className="flex items-center justify-between gap-3 p-4">
            <div className="min-w-0">
              <p className="truncate font-semibold">{picked.file.name}</p>
              <p className="text-xs text-muted-foreground">
                {formatBytes(picked.file.size)} · {picked.width}×{picked.height} px
              </p>
            </div>
            <Button type="button" variant="ghost" size="sm" onClick={() => setPicked(null)} disabled={scanning} className="text-destructive">
              <Trash2 /> Xóa ảnh
            </Button>
          </div>
        </div>
      )}

      {scanning && (
        <div className="mt-4 rounded-2xl border bg-card p-4" role="status" aria-live="polite">
          <div className="mb-2 flex items-center justify-between text-sm">
            <span className="flex items-center gap-2 font-semibold">
              <ScanLine className="size-4 text-primary" /> {done ? "Hoàn tất! Đang mở kết quả…" : "AI đang phân tích món ăn…"}
            </span>
            <span className="font-bold text-primary">{Math.round(progress)}%</span>
          </div>
          <div className="h-2 overflow-hidden rounded-full bg-muted" role="progressbar" aria-valuenow={Math.round(progress)} aria-valuemin={0} aria-valuemax={100}>
            <div className="h-full rounded-full bg-primary transition-all duration-500" style={{ width: `${progress}%` }} />
          </div>
          <ScanSteps done={done} />
          <div className="mt-3 flex items-center justify-between">
            <p className="text-xs text-muted-foreground">Có thể mất đến vài chục giây, vui lòng không đóng trang.</p>
            {!done && (
              <Button type="button" variant="ghost" size="sm" onClick={() => abortRef.current?.abort()}>
                <X /> Hủy
              </Button>
            )}
          </div>
        </div>
      )}

      <div className="mt-6 flex flex-col-reverse gap-3 sm:flex-row sm:items-center sm:justify-between">
        <Button type="button" variant="outline" size="lg" disabled={scanning} onClick={() => router.push(`/meals/review/new?mode=manual&loai=${loai}`)}>
          <PenLine /> Nhập tay món ăn
        </Button>
        <Button type="button" size="lg" className="font-extrabold" disabled={!picked || scanning} onClick={() => void startScan()}>
          {scanning ? <Loader2 className="animate-spin" /> : <ScanLine />} TIẾP TỤC NHẬN DIỆN AI
        </Button>
      </div>
      {!scanning && (
        <div className="mt-8">
          <QuickLog title="Hoặc ghi lại món hay ăn" compact loai={loai} />
        </div>
      )}
      <CameraDialog open={camera} onOpenChange={setCamera} onCapture={(f) => void accept(f)} />
    </div>
  );
}
