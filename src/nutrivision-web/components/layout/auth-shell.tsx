import Link from "next/link";
import { Camera, Flame, LineChart, Sparkles } from "lucide-react";
import { Logo } from "@/components/shared/logo";

const POINTS = [
  { icon: Camera, text: "Chụp ảnh món ăn, AI nhận diện trong vài giây" },
  { icon: Flame, text: "Tính calo và dinh dưỡng chính xác theo khẩu phần" },
  { icon: LineChart, text: "Báo cáo tuần/tháng và gợi ý món ăn phù hợp" },
];

export function AuthShell({ title, subtitle, children, footer }: { title: string; subtitle?: string; children: React.ReactNode; footer?: React.ReactNode }) {
  return (
    <div className="grid min-h-dvh lg:grid-cols-[1.05fr_1fr]">
      <aside className="relative hidden overflow-hidden bg-gradient-to-br from-[#1fb857] via-[#17a04e] to-[#0f7a3b] p-12 text-white lg:flex lg:flex-col lg:justify-between">
        <div className="absolute -right-24 -top-24 size-96 rounded-full bg-white/10" />
        <div className="absolute -bottom-32 -left-16 size-96 rounded-full bg-yellow/20" />
        <div className="relative">
          <Link href="/" className="inline-flex items-center gap-2 text-xl font-extrabold">
            <Sparkles className="size-6 text-yellow" /> NutriVision AI
          </Link>
        </div>
        <div className="relative max-w-md">
          <h2 className="text-4xl font-extrabold leading-tight">Ăn ngon, sống khỏe, hiểu rõ từng bữa ăn.</h2>
          <ul className="mt-8 space-y-4">
            {POINTS.map((p) => (
              <li key={p.text} className="flex items-center gap-3">
                <span className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-white/20">
                  <p.icon className="size-5" />
                </span>
                <span className="text-white/90">{p.text}</span>
              </li>
            ))}
          </ul>
        </div>
        <p className="relative text-sm text-white/70">© NutriVision AI – Đồ án tốt nghiệp</p>
      </aside>
      <main className="flex items-center justify-center bg-background px-4 py-10">
        <div className="w-full max-w-md">
          <div className="mb-6 lg:hidden">
            <Logo />
          </div>
          <div className="rounded-3xl border bg-card p-6 shadow-soft sm:p-8">
            <h1 className="text-2xl font-extrabold tracking-tight">{title}</h1>
            {subtitle && <p className="mt-1 text-sm text-muted-foreground">{subtitle}</p>}
            <div className="mt-6">{children}</div>
          </div>
          {footer && <div className="mt-5 text-center text-sm text-muted-foreground">{footer}</div>}
        </div>
      </main>
    </div>
  );
}
