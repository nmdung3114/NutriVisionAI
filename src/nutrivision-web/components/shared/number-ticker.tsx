"use client";

import { useEffect, useState } from "react";
import { animate, useMotionValue } from "framer-motion";
import { fmt } from "@/lib/meal";

/** Số nhảy mượt khi giá trị thay đổi. */
export function NumberTicker({ value, digits = 0, className }: { value: number; digits?: number; className?: string }) {
  const mv = useMotionValue(value);
  const [text, setText] = useState(fmt(value, digits));
  useEffect(() => {
    const unsub = mv.on("change", (v) => setText(fmt(v, digits)));
    const controls = animate(mv, value, { duration: 0.5, ease: "easeOut" });
    return () => {
      unsub();
      controls.stop();
    };
  }, [value, digits, mv]);
  return <span className={className}>{text}</span>;
}
