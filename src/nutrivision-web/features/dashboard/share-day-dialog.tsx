"use client";

import { useEffect, useRef, useState } from "react";
import { Download, Loader2, Share2 } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { useStreak } from "@/hooks/use-api";
import { useAuth } from "@/providers/auth-provider";
import type { Dashboard } from "@/types/api";
import { drawShareCard } from "./share-card";

/** Nút + hộp thoại xem trước thẻ tổng kết ngày, tải PNG hoặc chia sẻ qua Web Share API (nếu thiết bị hỗ trợ). */
export function ShareDayDialog({ data }: { data: Dashboard }) {
  const [open, setOpen] = useState(false);
  const [url, setUrl] = useState<string | null>(null);
  const [blob, setBlob] = useState<Blob | null>(null);
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const { data: streak } = useStreak();
  const { user } = useAuth();
  const fileName = `nutrivision-${data.ngay}.png`;

  useEffect(() => {
    if (!open) return;
    let revoked: string | null = null;
    const canvas = canvasRef.current ?? document.createElement("canvas");
    canvasRef.current = canvas;
    const font = getComputedStyle(document.body).fontFamily || "sans-serif";
    // Đợi font web tải xong để chữ trên ảnh đúng font.
    void document.fonts.ready.then(() => {
      drawShareCard(canvas, data, { streak: streak?.chuoiHienTai, name: user?.hoTen, font });
      canvas.toBlob((b) => {
        if (!b) return;
        revoked = URL.createObjectURL(b);
        setBlob(b);
        setUrl(revoked);
      }, "image/png");
    });
    return () => {
      if (revoked) URL.revokeObjectURL(revoked);
      setUrl(null);
      setBlob(null);
    };
  }, [open, data, streak?.chuoiHienTai, user?.hoTen]);

  const file = blob ? new File([blob], fileName, { type: "image/png" }) : null;
  const canShare = typeof navigator !== "undefined" && !!file && !!navigator.canShare?.({ files: [file] });

  const share = async () => {
    if (!file) return;
    try {
      await navigator.share({ files: [file], title: "Tổng kết dinh dưỡng hôm nay", text: `Hôm nay mình đã nạp ${Math.round(data.homNay.calo)} kcal 🥗` });
    } catch (e) {
      if ((e as DOMException).name !== "AbortError") toast.error("Không chia sẻ được, hãy tải ảnh về.");
    }
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant="outline">
          <Share2 /> Chia sẻ
        </Button>
      </DialogTrigger>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>Thẻ tổng kết hôm nay</DialogTitle>
          <DialogDescription>Tải ảnh về hoặc chia sẻ thành tích ăn uống của bạn.</DialogDescription>
        </DialogHeader>
        <div className="flex aspect-[4/5] items-center justify-center overflow-hidden rounded-2xl border bg-muted">
          {url ? (
            // eslint-disable-next-line @next/next/no-img-element
            <img src={url} alt="Xem trước thẻ tổng kết dinh dưỡng hôm nay" className="size-full object-contain" />
          ) : (
            <Loader2 className="size-8 animate-spin text-muted-foreground" />
          )}
        </div>
        <DialogFooter className="gap-2 sm:gap-2">
          {canShare && (
            <Button variant="outline" onClick={() => void share()}>
              <Share2 /> Chia sẻ
            </Button>
          )}
          <Button asChild disabled={!url}>
            <a href={url ?? undefined} download={fileName} aria-disabled={!url}>
              <Download /> Tải ảnh PNG
            </a>
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
