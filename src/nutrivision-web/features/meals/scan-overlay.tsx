"use client";

import { motion, useReducedMotion } from "framer-motion";
import { Check, Loader2, Sparkles } from "lucide-react";
import { cn } from "@/lib/utils";

/** Lớp phủ khi AI đang quét ảnh: khung ngắm 4 góc, lưới mờ, tia quét, các đốm sáng. */
export function ScanOverlay({ done }: { done: boolean }) {
  const reduce = useReducedMotion();
  const corners = ["left-4 top-4 border-l-4 border-t-4 rounded-tl-2xl", "right-4 top-4 border-r-4 border-t-4 rounded-tr-2xl", "left-4 bottom-4 border-l-4 border-b-4 rounded-bl-2xl", "right-4 bottom-4 border-r-4 border-b-4 rounded-br-2xl"];
  return (
    <div className="pointer-events-none absolute inset-0 overflow-hidden" aria-hidden>
      <div className={cn("absolute inset-0 transition-colors duration-500", done ? "bg-brand-500/25" : "bg-brand-500/10")} />
      <div
        className="absolute inset-0 opacity-30"
        style={{ backgroundImage: "linear-gradient(rgba(255,255,255,.35) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,.35) 1px, transparent 1px)", backgroundSize: "32px 32px" }}
      />
      {corners.map((c) => (
        <motion.span
          key={c}
          className={cn("absolute size-10 border-white drop-shadow-[0_0_6px_rgba(33,180,90,.9)]", c)}
          animate={reduce || done ? undefined : { scale: [1, 0.9, 1] }}
          transition={{ duration: 1.4, repeat: Infinity, ease: "easeInOut" }}
        />
      ))}
      {!done && <div className="animate-scan-line absolute inset-x-0 h-[3px] bg-gradient-to-r from-transparent via-[#21b45a] to-transparent shadow-[0_0_24px_6px_rgba(33,180,90,0.7)]" />}
      {!reduce &&
        !done &&
        [12, 30, 52, 70, 84].map((left, i) => (
          <motion.span
            key={left}
            className="absolute text-white drop-shadow"
            style={{ left: `${left}%`, top: `${20 + ((i * 37) % 60)}%` }}
            animate={{ opacity: [0, 1, 0], scale: [0.6, 1.1, 0.6] }}
            transition={{ duration: 1.8, repeat: Infinity, delay: i * 0.35 }}
          >
            <Sparkles className="size-4" />
          </motion.span>
        ))}
      {done && (
        <motion.div initial={{ scale: 0.4, opacity: 0 }} animate={{ scale: 1, opacity: 1 }} className="absolute inset-0 flex items-center justify-center">
          <span className="flex size-20 items-center justify-center rounded-full bg-primary text-primary-foreground shadow-xl">
            <Check className="size-10" strokeWidth={3} />
          </span>
        </motion.div>
      )}
    </div>
  );
}

const STEPS = ["Gửi ảnh lên máy chủ", "AI nhận diện món & thành phần", "Đối chiếu bảng dinh dưỡng, tạo bản nháp"];

/** Các bước thật của yêu cầu: bước 1 xong khi đã gửi, bước 2–3 do server làm trong cùng một request → xong khi có kết quả. */
export function ScanSteps({ done }: { done: boolean }) {
  return (
    <ol className="mt-3 space-y-1.5 text-sm">
      {STEPS.map((s, i) => {
        const complete = i === 0 || done;
        const active = !done && i === 1;
        return (
          <li key={s} className={cn("flex items-center gap-2", !complete && !active && "text-muted-foreground")}>
            <span className={cn("flex size-5 items-center justify-center rounded-full text-[10px] font-bold", complete ? "bg-primary text-primary-foreground" : active ? "bg-primary/15 text-primary" : "bg-muted")}>
              {complete ? <Check className="size-3" strokeWidth={3} /> : active ? <Loader2 className="size-3 animate-spin" /> : i + 1}
            </span>
            {s}
          </li>
        );
      })}
    </ol>
  );
}
