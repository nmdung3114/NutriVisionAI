"use client";

import { forwardRef, useState } from "react";
import { Eye, EyeOff } from "lucide-react";
import { Input } from "@/components/ui/input";

export const PasswordInput = forwardRef<HTMLInputElement, React.ComponentProps<"input">>(function PasswordInput(
  { className, ...props },
  ref,
) {
  const [show, setShow] = useState(false);
  return (
    <div className="relative">
      <Input ref={ref} type={show ? "text" : "password"} className={`pr-10 ${className ?? ""}`} {...props} />
      <button
        type="button"
        onClick={() => setShow((s) => !s)}
        aria-label={show ? "Ẩn mật khẩu" : "Hiện mật khẩu"}
        className="absolute inset-y-0 right-0 flex w-10 items-center justify-center rounded-r-md text-muted-foreground hover:text-foreground focus-visible:ring-2 focus-visible:ring-ring"
      >
        {show ? <EyeOff className="size-4" /> : <Eye className="size-4" />}
      </button>
    </div>
  );
});
