import { Label } from "@/components/ui/label";
import { cn } from "@/lib/utils";

/** Nhãn + control + thông báo lỗi, dùng chung cho mọi form. */
export function Field({
  label,
  htmlFor,
  error,
  hint,
  children,
  className,
  required,
}: {
  label: string;
  htmlFor: string;
  error?: string;
  hint?: React.ReactNode;
  children: React.ReactNode;
  className?: string;
  required?: boolean;
}) {
  return (
    <div className={cn("space-y-1.5", className)}>
      <Label htmlFor={htmlFor} className="text-sm font-semibold">
        {label}
        {required && <span className="text-destructive"> *</span>}
      </Label>
      {children}
      {hint && !error && <p className="text-xs text-muted-foreground">{hint}</p>}
      {error && (
        <p id={`${htmlFor}-error`} role="alert" className="text-xs font-medium text-destructive">
          {error}
        </p>
      )}
    </div>
  );
}
