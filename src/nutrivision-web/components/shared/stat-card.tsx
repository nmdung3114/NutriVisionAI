import { cn } from "@/lib/utils";

const TONES = {
  green: "bg-brand-50 text-brand-700",
  orange: "bg-orange/15 text-[#d4472b]",
  yellow: "bg-yellow/25 text-[#9a6b00]",
  blue: "bg-blue/15 text-[#2a6fc4]",
  purple: "bg-purple/15 text-[#6f3fc4]",
  gray: "bg-muted text-muted-foreground",
} as const;

export type Tone = keyof typeof TONES;

export function StatCard({
  label,
  value,
  unit,
  hint,
  icon,
  tone = "green",
  badge,
  className,
}: {
  label: string;
  value: React.ReactNode;
  unit?: string;
  hint?: React.ReactNode;
  icon?: React.ReactNode;
  tone?: Tone;
  badge?: React.ReactNode;
  className?: string;
}) {
  return (
    <div className={cn("rounded-2xl border bg-card p-4 shadow-soft sm:p-5", className)}>
      <div className="flex items-start justify-between gap-2">
        <p className="text-sm font-medium text-muted-foreground">{label}</p>
        {icon && <span className={cn("flex size-9 shrink-0 items-center justify-center rounded-xl [&_svg]:size-4.5", TONES[tone])}>{icon}</span>}
      </div>
      <div className="mt-2 flex items-baseline gap-1.5">
        <span className="text-2xl font-extrabold tracking-tight sm:text-3xl">{value}</span>
        {unit && <span className="text-sm font-medium text-muted-foreground">{unit}</span>}
        {badge && <span className="ml-auto">{badge}</span>}
      </div>
      {hint && <p className="mt-1 text-xs text-muted-foreground">{hint}</p>}
    </div>
  );
}
