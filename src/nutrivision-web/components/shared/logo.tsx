import Link from "next/link";
import { cn } from "@/lib/utils";

export function LogoMark({ className }: { className?: string }) {
  return (
    <span
      className={cn(
        "inline-flex size-9 items-center justify-center rounded-xl bg-gradient-to-br from-[#2ed16f] to-[#168d47] text-white shadow-sm",
        className,
      )}
      aria-hidden
    >
      <svg viewBox="0 0 64 64" className="size-5" fill="none">
        <path d="M32 14c-8 0-14 6-14 15 0 10 7 21 14 21s14-11 14-21c0-9-6-15-14-15z" fill="#fff" />
        <path d="M32 8c1 5 5 7 9 7-1 5-5 8-9 7z" fill="#ffc934" />
      </svg>
    </span>
  );
}

export function Logo({ href = "/", className }: { href?: string; className?: string }) {
  return (
    <Link href={href} className={cn("inline-flex items-center gap-2.5 rounded-lg focus-visible:ring-2 focus-visible:ring-ring", className)}>
      <LogoMark />
      <span className="text-lg font-extrabold tracking-tight text-foreground">
        Nutri<span className="text-primary">Vision</span> <span className="text-orange">AI</span>
      </span>
    </Link>
  );
}
