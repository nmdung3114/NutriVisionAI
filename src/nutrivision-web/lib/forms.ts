import type { FieldValues, Path, UseFormReturn } from "react-hook-form";
import { isApiError } from "@/lib/api/errors";

/** Gắn fieldErrors của API lên các field react-hook-form; trả về true nếu có ít nhất một lỗi được gắn. */
export function applyFieldErrors<T extends FieldValues>(
  form: UseFormReturn<T>,
  err: unknown,
  allowed?: string[],
): boolean {
  if (!isApiError(err)) return false;
  let applied = false;
  for (const [key, msgs] of Object.entries(err.fieldErrors)) {
    const name = key.charAt(0).toLowerCase() + key.slice(1);
    if (allowed && !allowed.includes(name)) continue;
    if (msgs?.length) {
      form.setError(name as Path<T>, { type: "server", message: msgs[0] });
      applied = true;
    }
  }
  return applied;
}
