"use client";

import Link from "next/link";
import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { ArrowRight, Camera, CheckCircle2, FileDown, LineChart, Sparkles, Target, Zap } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Logo } from "@/components/shared/logo";
import { ThemeToggle } from "@/components/theme/theme-toggle";
import { FullPageLoader } from "@/components/layout/app-shell";
import { useAuth } from "@/providers/auth-provider";

const FEATURES = [
  { icon: Camera, title: "Nhận diện bằng AI", text: "Chụp ảnh món ăn, AI tách thành phần và ước tính khẩu phần trong vài giây.", tone: "bg-brand-50 text-brand-700" },
  { icon: Target, title: "Mục tiêu cá nhân hóa", text: "Tự động tính BMI, BMR, TDEE và mục tiêu calo phù hợp với thể trạng của bạn.", tone: "bg-orange/15 text-danger-strong" },
  { icon: LineChart, title: "Báo cáo trực quan", text: "Theo dõi calo, đạm, tinh bột, béo theo ngày, tuần, tháng cùng khuyến nghị thông minh.", tone: "bg-blue/15 text-info" },
  { icon: FileDown, title: "Xuất PDF / CSV", text: "Tải báo cáo dinh dưỡng của bạn để lưu trữ hoặc chia sẻ với chuyên gia.", tone: "bg-purple/15 text-violet" },
];

const STEPS = [
  { n: 1, title: "Chụp ảnh bữa ăn", text: "Chọn loại bữa và tải ảnh lên." },
  { n: 2, title: "AI nhận diện", text: "Xem kết quả, chỉnh sửa tên món, thành phần, khẩu phần." },
  { n: 3, title: "Lưu và theo dõi", text: "Dinh dưỡng được tính và cập nhật vào dashboard của bạn." },
];

export function Landing() {
  const { status } = useAuth();
  const router = useRouter();
  useEffect(() => {
    if (status === "authenticated") router.replace("/dashboard");
  }, [status, router]);
  if (status !== "anonymous") return <FullPageLoader />;

  return (
    <div className="bg-background">
      <header className="mx-auto flex max-w-6xl items-center justify-between px-4 py-4 lg:px-8">
        <Logo />
        <nav className="flex items-center gap-2" aria-label="Tài khoản">
          <Button asChild variant="ghost"><Link href="/login">Đăng nhập</Link></Button>
          <Button asChild className="font-bold"><Link href="/register">Đăng ký</Link></Button>
          <ThemeToggle />
        </nav>
      </header>

      <main>
        <section className="relative mx-auto max-w-6xl overflow-hidden px-4 pb-16 pt-10 lg:px-8 lg:pt-16">
          <div className="absolute -right-24 -top-10 -z-10 size-96 rounded-full bg-brand-100/70 blur-3xl" />
          <div className="absolute -left-24 top-40 -z-10 size-72 rounded-full bg-yellow/20 blur-3xl" />
          <div className="grid items-center gap-10 lg:grid-cols-2">
            <div>
              <span className="inline-flex items-center gap-1.5 rounded-full bg-brand-50 px-3 py-1 text-sm font-semibold text-brand-700">
                <Sparkles className="size-4" /> Theo dõi dinh dưỡng thông minh
              </span>
              <h1 className="mt-5 text-4xl font-extrabold leading-tight tracking-tight sm:text-5xl">
                Chụp một tấm ảnh, <span className="text-primary">biết ngay</span> bữa ăn của bạn có gì.
              </h1>
              <p className="mt-4 max-w-xl text-lg text-muted-foreground">
                NutriVision AI nhận diện món ăn từ hình ảnh, tính calo và dinh dưỡng, giúp bạn đạt mục tiêu sức khỏe mỗi ngày.
              </p>
              <div className="mt-7 flex flex-wrap gap-3">
                <Button asChild size="lg" className="font-bold"><Link href="/register">Bắt đầu miễn phí <ArrowRight /></Link></Button>
                <Button asChild size="lg" variant="outline"><Link href="/login">Tôi đã có tài khoản</Link></Button>
              </div>
              <ul className="mt-6 flex flex-wrap gap-x-5 gap-y-2 text-sm text-muted-foreground">
                {["Giao diện tiếng Việt", "Bảng thực phẩm Việt Nam", "Bảo mật dữ liệu cá nhân"].map((t) => (
                  <li key={t} className="flex items-center gap-1.5"><CheckCircle2 className="size-4 text-primary" />{t}</li>
                ))}
              </ul>
            </div>
            <div className="relative mx-auto w-full max-w-md" aria-hidden>
              <div className="rounded-3xl border bg-card p-6 shadow-soft">
                <div className="flex items-center gap-5">
                  <div className="relative size-32 shrink-0">
                    <svg viewBox="0 0 120 120" className="-rotate-90">
                      <circle cx="60" cy="60" r="50" fill="none" stroke="var(--track)" strokeWidth="12" />
                      <circle cx="60" cy="60" r="50" fill="none" stroke="#21b45a" strokeWidth="12" strokeLinecap="round" strokeDasharray="314" strokeDashoffset="90" />
                    </svg>
                    <div className="absolute inset-0 flex flex-col items-center justify-center"><b className="text-2xl">1.420</b><span className="text-[11px] text-muted-foreground">/ 2.000 kcal</span></div>
                  </div>
                  <div className="flex-1 space-y-3">
                    {[["Đạm", "#ff795e", "70%"], ["Tinh bột", "#ffc934", "55%"], ["Béo", "#4b9cf5", "40%"]].map(([l, c, w]) => (
                      <div key={l}>
                        <p className="mb-1 text-xs font-semibold">{l}</p>
                        <div className="h-2 rounded-full bg-muted"><div className="h-full rounded-full" style={{ width: w, background: c }} /></div>
                      </div>
                    ))}
                  </div>
                </div>
                <div className="mt-5 flex items-center gap-3 rounded-2xl bg-brand-50 p-3">
                  <span className="flex size-11 items-center justify-center rounded-xl bg-card text-2xl">🍜</span>
                  <div className="flex-1"><p className="text-sm font-bold">Phở bò tái</p><p className="text-xs text-muted-foreground">AI tin cậy 92%</p></div>
                  <b className="text-primary">450 kcal</b>
                </div>
              </div>
              <span className="absolute -left-4 -top-4 flex items-center gap-1.5 rounded-full bg-yellow px-3 py-1.5 text-xs font-bold text-[#3b2a00] shadow-md"><Zap className="size-3.5" /> Nhận diện 5 giây</span>
            </div>
          </div>
        </section>

        <section className="mx-auto max-w-6xl px-4 py-12 lg:px-8" aria-labelledby="features">
          <h2 id="features" className="text-center text-3xl font-extrabold">Mọi thứ bạn cần để ăn uống khoa học</h2>
          <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            {FEATURES.map((f) => (
              <div key={f.title} className="rounded-2xl border bg-card p-5 shadow-soft transition-transform hover:-translate-y-1">
                <span className={`flex size-12 items-center justify-center rounded-2xl ${f.tone}`}><f.icon className="size-6" /></span>
                <h3 className="mt-4 font-extrabold">{f.title}</h3>
                <p className="mt-1 text-sm text-muted-foreground">{f.text}</p>
              </div>
            ))}
          </div>
        </section>

        <section className="mx-auto max-w-6xl px-4 py-12 lg:px-8" aria-labelledby="how">
          <h2 id="how" className="text-center text-3xl font-extrabold">Hoạt động như thế nào?</h2>
          <ol className="mt-8 grid gap-4 md:grid-cols-3">
            {STEPS.map((s) => (
              <li key={s.n} className="rounded-2xl bg-card p-6 text-center shadow-soft">
                <span className="mx-auto flex size-12 items-center justify-center rounded-full bg-primary text-xl font-extrabold text-primary-foreground">{s.n}</span>
                <h3 className="mt-4 font-extrabold">{s.title}</h3>
                <p className="mt-1 text-sm text-muted-foreground">{s.text}</p>
              </li>
            ))}
          </ol>
        </section>

        <section className="mx-auto max-w-6xl px-4 pb-16 lg:px-8">
          <div className="rounded-3xl bg-gradient-to-br from-[#1fb857] to-[#0f7a3b] p-8 text-center text-white sm:p-12">
            <h2 className="text-3xl font-extrabold">Sẵn sàng bắt đầu hành trình khỏe mạnh?</h2>
            <p className="mx-auto mt-2 max-w-xl text-white/85">Đăng ký chỉ mất một phút. Bữa ăn đầu tiên của bạn đang chờ được ghi nhận.</p>
            <Button asChild size="lg" className="mt-6 bg-white font-bold text-[#168d47] hover:bg-white/90"><Link href="/register">Tạo tài khoản miễn phí</Link></Button>
          </div>
        </section>
      </main>
      <footer className="border-t py-6 text-center text-sm text-muted-foreground">© NutriVision AI – Đồ án tốt nghiệp</footer>
    </div>
  );
}
