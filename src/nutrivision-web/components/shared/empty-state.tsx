import { cn } from "@/lib/utils";

export function PlateIllustration({ className }: { className?: string }) {
  return (
    <svg viewBox="0 0 160 120" className={cn("h-28 w-36", className)} fill="none" aria-hidden>
      <ellipse cx="80" cy="104" rx="52" ry="8" fill="#e3e9e6" />
      <circle cx="80" cy="56" r="44" fill="#fff" stroke="#d6f3e1" strokeWidth="6" />
      <circle cx="80" cy="56" r="30" fill="#effaf3" stroke="#d6f3e1" strokeWidth="2" />
      <path d="M62 20v22M68 20v22M74 20v22M68 42v36" stroke="#b9c6c0" strokeWidth="3" strokeLinecap="round" />
      <path d="M108 20c-6 8-7 20-2 30v28" stroke="#b9c6c0" strokeWidth="3" strokeLinecap="round" />
      <circle cx="122" cy="26" r="6" fill="#ffc934" />
      <circle cx="40" cy="30" r="4" fill="#ff795e" />
      <circle cx="132" cy="84" r="3" fill="#4b9cf5" />
    </svg>
  );
}

export function EmptyState({
  title,
  description,
  action,
  icon,
  className,
}: {
  title: string;
  description?: React.ReactNode;
  action?: React.ReactNode;
  icon?: React.ReactNode;
  className?: string;
}) {
  return (
    <div
      className={cn(
        "flex flex-col items-center justify-center rounded-2xl border border-dashed bg-card px-6 py-12 text-center",
        className,
      )}
    >
      {icon ?? <PlateIllustration />}
      <h3 className="mt-4 text-lg font-bold text-foreground">{title}</h3>
      {description && <p className="mt-1 max-w-md text-sm text-muted-foreground">{description}</p>}
      {action && <div className="mt-5">{action}</div>}
    </div>
  );
}
