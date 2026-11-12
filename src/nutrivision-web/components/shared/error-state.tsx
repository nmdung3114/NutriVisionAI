"use client";

import { AlertTriangle, RefreshCw, ShieldAlert, SearchX, WifiOff } from "lucide-react";
import { Button } from "@/components/ui/button";
import { isApiError, errorHint, errorMessage, errorStatusTitle } from "@/lib/api/errors";
import { cn } from "@/lib/utils";

export function ErrorState({
  error,
  onRetry,
  className,
  action,
}: {
  error: unknown;
  onRetry?: () => void;
  className?: string;
  action?: React.ReactNode;
}) {
  const status = isApiError(error) ? error.status : 500;
  const Icon = status === 403 ? ShieldAlert : status === 404 ? SearchX : status === 0 ? WifiOff : AlertTriangle;
  const hint = isApiError(error) ? errorHint(error.code) : undefined;
  return (
    <div
      role="alert"
      className={cn("flex flex-col items-center rounded-2xl border bg-card px-6 py-12 text-center shadow-soft", className)}
    >
      <span className="flex size-14 items-center justify-center rounded-2xl bg-orange/15 text-orange">
        <Icon className="size-7" />
      </span>
      <h3 className="mt-4 text-lg font-bold">{errorStatusTitle(error)}</h3>
      <p className="mt-1 max-w-md text-sm text-muted-foreground">{errorMessage(error)}</p>
      {hint && hint !== errorMessage(error) && <p className="mt-1 max-w-md text-xs text-muted-foreground">{hint}</p>}
      <div className="mt-5 flex gap-2">
        {onRetry && status !== 403 && status !== 404 && (
          <Button variant="outline" onClick={onRetry}>
            <RefreshCw /> Thử lại
          </Button>
        )}
        {action}
      </div>
    </div>
  );
}
