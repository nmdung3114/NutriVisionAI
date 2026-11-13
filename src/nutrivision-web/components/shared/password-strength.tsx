import { Check, X } from "lucide-react";
import { evaluatePassword } from "@/lib/password";
import { cn } from "@/lib/utils";

const COLORS = ["bg-red-400", "bg-red-400", "bg-yellow", "bg-brand-500", "bg-brand-700"];

export function PasswordStrength({ password }: { password: string }) {
  const s = evaluatePassword(password);
  if (!password) return null;
  const rules = [
    { ok: s.checks.length, text: "Ít nhất 8 ký tự" },
    { ok: s.checks.letter, text: "Có chữ cái" },
    { ok: s.checks.digit, text: "Có chữ số" },
  ];
  return (
    <div className="mt-2 space-y-2" aria-live="polite">
      <div className="flex items-center gap-2">
        <div className="flex flex-1 gap-1" aria-hidden>
          {[1, 2, 3, 4].map((i) => (
            <span key={i} className={cn("h-1.5 flex-1 rounded-full bg-muted transition-colors", s.score >= i && COLORS[s.score])} />
          ))}
        </div>
        <span className="w-20 text-right text-xs font-semibold text-muted-foreground">{s.label}</span>
      </div>
      <ul className="flex flex-wrap gap-x-4 gap-y-1 text-xs">
        {rules.map((r) => (
          <li key={r.text} className={cn("flex items-center gap-1", r.ok ? "text-brand-700" : "text-muted-foreground")}>
            {r.ok ? <Check className="size-3.5" /> : <X className="size-3.5" />}
            {r.text}
          </li>
        ))}
      </ul>
    </div>
  );
}
