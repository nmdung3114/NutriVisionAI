import { cn } from "@/lib/utils";

export type Illustration = "plate" | "chart" | "search" | "welcome";

/** Hình minh họa SVG dùng biến màu theme nên tự hợp với chế độ sáng / tối. */
function Art({ kind }: { kind: Illustration }) {
  const common = { viewBox: "0 0 200 150", className: "h-32 w-44", fill: "none", "aria-hidden": true } as const;
  const shadow = <ellipse cx="100" cy="136" rx="58" ry="7" fill="var(--track)" />;
  switch (kind) {
    case "chart":
      return (
        <svg {...common}>
          {shadow}
          <rect x="38" y="20" width="124" height="104" rx="14" fill="var(--card)" stroke="var(--border)" strokeWidth="3" />
          <path d="M56 104h88" stroke="var(--track)" strokeWidth="3" strokeLinecap="round" />
          <rect x="62" y="74" width="14" height="30" rx="4" fill="var(--brand-100)" />
          <rect x="84" y="58" width="14" height="46" rx="4" fill="#ffc934" opacity=".7" />
          <rect x="106" y="80" width="14" height="24" rx="4" fill="var(--brand-100)" />
          <rect x="128" y="46" width="14" height="58" rx="4" fill="#21b45a" />
          <path d="M60 60l26-16 22 12 32-22" stroke="#ff795e" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round" strokeDasharray="4 6" />
          <circle cx="164" cy="22" r="7" fill="#4b9cf5" />
        </svg>
      );
    case "search":
      return (
        <svg {...common}>
          {shadow}
          <circle cx="92" cy="66" r="40" fill="var(--card)" stroke="var(--border)" strokeWidth="6" />
          <circle cx="92" cy="66" r="26" fill="var(--brand-50)" />
          <path d="M121 96l26 26" stroke="var(--muted-foreground)" strokeWidth="10" strokeLinecap="round" />
          <path d="M80 60q12-12 24 0" stroke="#21b45a" strokeWidth="4" strokeLinecap="round" />
          <circle cx="152" cy="34" r="6" fill="#ffc934" />
          <circle cx="44" cy="40" r="4" fill="#ff795e" />
        </svg>
      );
    case "welcome":
      return (
        <svg {...common}>
          {shadow}
          <rect x="52" y="34" width="96" height="74" rx="18" fill="#21b45a" />
          <rect x="82" y="24" width="36" height="16" rx="6" fill="#168d47" />
          <circle cx="100" cy="71" r="25" fill="var(--card)" />
          <circle cx="100" cy="71" r="15" fill="var(--brand-100)" />
          <circle cx="135" cy="48" r="4" fill="#ffc934" />
          <path d="M160 30l4 9 9 4-9 4-4 9-4-9-9-4 9-4z" fill="#ffc934" />
          <path d="M40 64l3 6 6 3-6 3-3 6-3-6-6-3 6-3z" fill="#ff795e" />
        </svg>
      );
    default:
      return (
        <svg {...common}>
          {shadow}
          <circle cx="100" cy="72" r="52" fill="var(--card)" stroke="var(--brand-100)" strokeWidth="7" />
          <circle cx="100" cy="72" r="34" fill="var(--brand-50)" stroke="var(--brand-100)" strokeWidth="2" />
          <path d="M34 30v24M40 30v24M46 30v24M40 54v44" stroke="var(--muted-foreground)" strokeWidth="3.5" strokeLinecap="round" opacity=".6" />
          <path d="M166 30c-7 9-8 23-2 34v34" stroke="var(--muted-foreground)" strokeWidth="3.5" strokeLinecap="round" opacity=".6" />
          <circle cx="100" cy="72" r="6" fill="#21b45a" />
          <circle cx="150" cy="24" r="6" fill="#ffc934" />
          <circle cx="56" cy="18" r="4" fill="#ff795e" />
        </svg>
      );
  }
}

export function EmptyState({
  title,
  description,
  action,
  icon,
  illustration = "plate",
  className,
}: {
  title: string;
  description?: React.ReactNode;
  action?: React.ReactNode;
  icon?: React.ReactNode;
  illustration?: Illustration;
  className?: string;
}) {
  return (
    <div className={cn("flex flex-col items-center justify-center rounded-2xl border border-dashed bg-card px-6 py-12 text-center", className)}>
      <div className="animate-float">{icon ?? <Art kind={illustration} />}</div>
      <h3 className="mt-4 text-lg font-bold text-foreground">{title}</h3>
      {description && <p className="mt-1 max-w-md text-sm text-muted-foreground">{description}</p>}
      {action && <div className="mt-5">{action}</div>}
    </div>
  );
}
