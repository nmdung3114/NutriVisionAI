export interface PasswordStrength {
  score: 0 | 1 | 2 | 3 | 4;
  label: string;
  /** Đạt yêu cầu tối thiểu của server: ≥8 ký tự, có chữ và số. */
  valid: boolean;
  checks: { length: boolean; letter: boolean; digit: boolean; mixedCase: boolean; special: boolean };
}

const LABELS = ["Rất yếu", "Yếu", "Trung bình", "Mạnh", "Rất mạnh"] as const;

export function evaluatePassword(pw: string): PasswordStrength {
  const checks = {
    length: pw.length >= 8,
    letter: /[A-Za-zÀ-ỹ]/.test(pw),
    digit: /\d/.test(pw),
    mixedCase: /[a-z]/.test(pw) && /[A-Z]/.test(pw),
    special: /[^A-Za-z0-9]/.test(pw),
  };
  const valid = checks.length && checks.letter && checks.digit;
  if (!pw) return { score: 0, label: "", valid: false, checks };
  let score = 0;
  if (checks.length) score++;
  if (checks.letter && checks.digit) score++;
  if (checks.mixedCase) score++;
  if (checks.special || pw.length >= 12) score++;
  if (!valid) score = Math.min(score, 1);
  const s = Math.min(score, 4) as 0 | 1 | 2 | 3 | 4;
  return { score: s, label: LABELS[s], valid, checks };
}
