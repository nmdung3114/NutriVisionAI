"use client";

import React, { useEffect } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import {
  Sparkles,
  ArrowRight,
  ScanLine,
  Activity,
  ShieldCheck,
  Zap,
  BarChart3,
  UtensilsCrossed,
} from "lucide-react";

export default function Home() {
  const router = useRouter();

  useEffect(() => {
    if (typeof window !== "undefined") {
      const token = localStorage.getItem("nv_access_token");
      if (token) {
        router.push("/dashboard");
      }
    }
  }, [router]);

  return (
    <div className="min-h-screen bg-[#090d16] text-slate-100 flex flex-col justify-between selection:bg-brand-500 selection:text-white">
      {/* Top Header */}
      <header className="border-b border-slate-800/80 backdrop-blur-xl bg-[#090d16]/70 sticky top-0 z-50 px-8 py-4 flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-brand-600 to-tech-cyan flex items-center justify-center text-white shadow-glow-sm">
            <Sparkles className="w-5 h-5" />
          </div>
          <div>
            <h1 className="text-lg font-bold tracking-tight text-white flex items-center gap-2">
              NutriVision<span className="text-brand-400 text-xs px-2 py-0.5 rounded bg-brand-500/10 border border-brand-500/30">AI</span>
            </h1>
            <p className="text-[11px] text-slate-400">Health-Tech Nutrition Platform</p>
          </div>
        </div>

        <div className="flex items-center gap-3">
          <Link
            href="/login"
            className="px-4 py-2 rounded-xl text-xs font-semibold text-slate-300 hover:text-white hover:bg-slate-800 transition-colors"
          >
            Đăng nhập
          </Link>
          <Link
            href="/register"
            className="px-4 py-2 rounded-xl text-xs font-semibold bg-gradient-to-r from-brand-500 to-tech-teal hover:from-brand-600 text-white shadow-glow-sm transition-all"
          >
            Bắt đầu miễn phí
          </Link>
        </div>
      </header>

      {/* Hero Section */}
      <main className="flex-1 max-w-6xl mx-auto px-6 py-16 flex flex-col items-center text-center justify-center relative">
        {/* Glow ambient background */}
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[350px] bg-brand-500/10 rounded-full blur-[120px] pointer-events-none"></div>

        {/* Badge */}
        <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-full bg-brand-500/10 border border-brand-500/30 text-brand-300 text-xs font-semibold mb-6 shadow-glow-sm">
          <Zap className="w-3.5 h-3.5 text-brand-400" />
          Nền tảng Dinh dưỡng ứng dụng Gemini Vision 1.5 Pro
        </div>

        {/* Main Heading */}
        <h1 className="text-4xl md:text-6xl font-extrabold tracking-tight text-white max-w-3xl leading-tight md:leading-tight">
          Kiểm soát dinh dưỡng thông minh bằng{" "}
          <span className="text-gradient-emerald">Thị giác Máy tính</span>
        </h1>

        <p className="mt-6 text-base md:text-lg text-slate-400 max-w-2xl leading-relaxed">
          Chỉ cần chụp ảnh món ăn, NutriVision AI tự động phân tách nguyên liệu, ước tính chính xác Calo, Protein, Carbs, Fat và đối chiếu với chỉ số BMR/TDEE cá nhân của bạn trong tích tắc.
        </p>

        {/* CTA Buttons */}
        <div className="mt-8 flex flex-col sm:flex-row items-center gap-4">
          <Link
            href="/dashboard"
            className="w-full sm:w-auto px-7 py-3.5 rounded-2xl bg-gradient-to-r from-brand-500 via-brand-600 to-tech-teal hover:opacity-95 text-white font-bold text-sm shadow-glow-md flex items-center justify-center gap-2.5 transition-all group"
          >
            <span>Trải nghiệm Dashboard ngay</span>
            <ArrowRight className="w-4 h-4 group-hover:translate-x-1 transition-transform" />
          </Link>
          <Link
            href="/meal/scan"
            className="w-full sm:w-auto px-7 py-3.5 rounded-2xl bg-slate-900 hover:bg-slate-800 border border-slate-700/80 text-slate-200 font-semibold text-sm flex items-center justify-center gap-2 transition-colors"
          >
            <ScanLine className="w-4 h-4 text-brand-400" />
            <span>Thử quét ảnh món ăn</span>
          </Link>
        </div>

        {/* Feature Grid */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-5 mt-16 text-left w-full">
          <div className="p-6 rounded-2xl bg-slate-900/60 border border-slate-800/80 hover:border-brand-500/30 transition-colors">
            <div className="w-10 h-10 rounded-xl bg-brand-500/10 border border-brand-500/20 flex items-center justify-center text-brand-400 mb-4">
              <ScanLine className="w-5 h-5" />
            </div>
            <h3 className="text-base font-bold text-white mb-1.5">AI Scan món ăn Việt Nam</h3>
            <p className="text-xs text-slate-400 leading-relaxed">
              Mô hình chuyên biệt nhận diện phở bò, cơm tấm, bún chả, gỏi cuốn... kèm mức độ tin cậy và cho phép người dùng vi chỉnh khẩu phần linh hoạt.
            </p>
          </div>

          <div className="p-6 rounded-2xl bg-slate-900/60 border border-slate-800/80 hover:border-brand-500/30 transition-colors">
            <div className="w-10 h-10 rounded-xl bg-tech-cyan/10 border border-tech-cyan/20 flex items-center justify-center text-tech-cyan mb-4">
              <Activity className="w-5 h-5" />
            </div>
            <h3 className="text-base font-bold text-white mb-1.5">Mifflin–St Jeor Chuẩn Y Khoa</h3>
            <p className="text-xs text-slate-400 leading-relaxed">
              Tính toán tự động BMR, TDEE và mục tiêu Calo thâm hụt/thặng dư theo chỉ số cơ thể thực tế, hỗ trợ giảm cân hay tăng cơ khoa học.
            </p>
          </div>

          <div className="p-6 rounded-2xl bg-slate-900/60 border border-slate-800/80 hover:border-brand-500/30 transition-colors">
            <div className="w-10 h-10 rounded-xl bg-amber-500/10 border border-amber-500/20 flex items-center justify-center text-amber-400 mb-4">
              <BarChart3 className="w-5 h-5" />
            </div>
            <h3 className="text-base font-bold text-white mb-1.5">Biểu đồ & Xuất Dữ liệu</h3>
            <p className="text-xs text-slate-400 leading-relaxed">
              Trực quan hóa xu hướng 7 ngày, phân bổ tỷ lệ Macro (Protein/Carb/Fat) và hỗ trợ trích xuất lịch sử dinh dưỡng ra file Excel/CSV chuẩn quốc tế.
            </p>
          </div>
        </div>
      </main>

      {/* Footer */}
      <footer className="border-t border-slate-800/80 py-6 px-8 text-center text-xs text-slate-500">
        Đồ án Tốt nghiệp CNTT - Hệ thống Dinh dưỡng Thông minh & Phân tích Khẩu phần Tự động qua Thị giác Máy tính (NutriVision AI)
      </footer>
    </div>
  );
}
