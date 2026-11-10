"use client";

import React, { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Sparkles, Mail, Lock, ArrowRight, ShieldCheck, AlertCircle, Eye, EyeOff } from "lucide-react";
import { authApi } from "@/lib/api";

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage("");
    setLoading(true);

    try {
      const res = await authApi.login({ email, matKhau: password });
      if (res.data?.success) {
        const { accessToken, refreshToken, user } = res.data.data;
        localStorage.setItem("nv_access_token", accessToken);
        localStorage.setItem("nv_refresh_token", refreshToken);
        localStorage.setItem("nv_user", JSON.stringify(user));

        if (user.vaiTro === "ADMIN") {
          router.push("/admin");
        } else {
          router.push("/dashboard");
        }
      } else {
        setErrorMessage(res.data?.message || "Đăng nhập thất bại");
      }
    } catch (err: any) {
      const msg = err.response?.data?.message || "Email hoặc mật khẩu không chính xác.";
      setErrorMessage(msg);
    } finally {
      setLoading(false);
    }
  };

  const handleQuickDemo = (role: "admin" | "user") => {
    if (role === "admin") {
      setEmail("admin@nutrivision.ai");
      setPassword("Admin@123");
    } else {
      setEmail("user@nutrivision.ai");
      setPassword("User@123");
    }
  };

  return (
    <div className="min-h-screen bg-[#090d16] text-slate-100 flex items-stretch">
      {/* Left Column: Visual Showcase & Brand Value */}
      <div className="hidden lg:flex flex-1 relative bg-gradient-to-br from-slate-950 via-[#0c1424] to-slate-900 flex-col justify-between p-12 overflow-hidden border-r border-slate-800/80">
        {/* Background glow orbs */}
        <div className="absolute top-1/4 -left-20 w-96 h-96 bg-brand-500/10 rounded-full blur-3xl pointer-events-none"></div>
        <div className="absolute bottom-1/4 right-0 w-96 h-96 bg-tech-cyan/10 rounded-full blur-3xl pointer-events-none"></div>

        {/* Brand */}
        <div className="relative z-10 flex items-center gap-3">
          <div className="w-12 h-12 rounded-2xl bg-gradient-to-tr from-brand-600 to-tech-cyan flex items-center justify-center text-white shadow-glow-md">
            <Sparkles className="w-7 h-7" />
          </div>
          <div>
            <h1 className="text-xl font-bold tracking-tight text-white flex items-center gap-2">
              NutriVision<span className="text-brand-400 text-xs px-2 py-0.5 rounded bg-brand-500/10 border border-brand-500/30">AI</span>
            </h1>
            <p className="text-xs text-slate-400">Smart Health & Diet Vision Platform</p>
          </div>
        </div>

        {/* Hero Illustration / Showcase Card */}
        <div className="relative z-10 max-w-lg my-auto space-y-6">
          <div className="inline-flex items-center gap-2 px-3 py-1.5 rounded-full bg-brand-500/10 border border-brand-500/30 text-brand-400 text-xs font-semibold">
            <span className="w-2 h-2 rounded-full bg-brand-400 animate-pulse"></span>
            Gemini Vision 1.5 Pro AI Engine
          </div>

          <h2 className="text-4xl font-extrabold tracking-tight text-white leading-tight">
            Quét bữa ăn thông minh. <br />
            <span className="text-gradient-emerald">Thấu hiểu dinh dưỡng</span> tức thì.
          </h2>

          <p className="text-slate-400 text-sm leading-relaxed">
            Hệ thống nhận diện tự động thành phần món ăn Việt Nam, bóc tách hàm lượng Calo, Protein, Carbs, Fat
            và tính toán lộ trình TDEE chuẩn y khoa chỉ trong vòng dưới 3 giây.
          </p>

          {/* Interactive Feature Pills */}
          <div className="grid grid-cols-2 gap-3 pt-2">
            <div className="p-3.5 rounded-xl bg-slate-900/80 border border-slate-800">
              <p className="text-xs font-bold text-slate-200">Độ chính xác {">"} 92%</p>
              <p className="text-[11px] text-slate-400 mt-0.5">Mô hình AI huấn luyện đặc thù cho ẩm thực Việt</p>
            </div>
            <div className="p-3.5 rounded-xl bg-slate-900/80 border border-slate-800">
              <p className="text-xs font-bold text-slate-200">Chuẩn Mifflin–St Jeor</p>
              <p className="text-[11px] text-slate-400 mt-0.5">Khuyến nghị BMR/TDEE cá nhân hóa chính xác</p>
            </div>
          </div>
        </div>

        {/* Footer Guarantee */}
        <div className="relative z-10 flex items-center gap-2 text-xs text-slate-400">
          <ShieldCheck className="w-4 h-4 text-brand-400" />
          <span>Bảo mật dữ liệu sức khỏe người dùng & hỗ trợ xuất báo cáo Excel/CSV</span>
        </div>
      </div>

      {/* Right Column: Login Card */}
      <div className="w-full lg:w-[480px] p-8 lg:p-12 flex flex-col justify-center bg-[#090d16] relative">
        <div className="max-w-md w-full mx-auto space-y-6">
          {/* Header */}
          <div>
            <h2 className="text-2xl font-bold text-white tracking-tight">Chào mừng trở lại</h2>
            <p className="text-sm text-slate-400 mt-1">Đăng nhập để theo dõi mục tiêu dinh dưỡng hôm nay</p>
          </div>

          {/* Quick Demo Credentials Switcher */}
          <div className="p-3.5 rounded-2xl bg-slate-900/90 border border-slate-800 space-y-2">
            <p className="text-xs font-medium text-slate-300">Tài khoản demo sẵn sàng (Bấm để điền nhanh):</p>
            <div className="flex gap-2">
              <button
                type="button"
                onClick={() => handleQuickDemo("user")}
                className="flex-1 py-1.5 px-2.5 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-200 text-xs font-medium transition-colors border border-slate-700 text-center"
              >
                👤 Người dùng (user)
              </button>
              <button
                type="button"
                onClick={() => handleQuickDemo("admin")}
                className="flex-1 py-1.5 px-2.5 rounded-xl bg-brand-950/60 hover:bg-brand-900/70 text-brand-300 text-xs font-medium transition-colors border border-brand-800/80 text-center"
              >
                🛡️ Quản trị viên (admin)
              </button>
            </div>
          </div>

          {/* Error Message */}
          {errorMessage && (
            <div className="p-3.5 rounded-xl bg-rose-500/10 border border-rose-500/30 text-rose-300 text-xs flex items-center gap-2">
              <AlertCircle className="w-4 h-4 shrink-0" />
              <span>{errorMessage}</span>
            </div>
          )}

          {/* Form */}
          <form onSubmit={handleLogin} className="space-y-4">
            <div className="space-y-1.5">
              <label className="text-xs font-medium text-slate-300">Email</label>
              <div className="relative">
                <Mail className="w-4 h-4 text-slate-500 absolute left-3.5 top-1/2 -translate-y-1/2" />
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="name@example.com"
                  required
                  className="w-full bg-slate-900/90 border border-slate-800 rounded-xl pl-10 pr-4 py-2.5 text-sm text-slate-100 placeholder:text-slate-600 focus:outline-none focus:border-brand-500 focus:ring-1 focus:ring-brand-500 transition-colors"
                />
              </div>
            </div>

            <div className="space-y-1.5">
              <div className="flex items-center justify-between">
                <label className="text-xs font-medium text-slate-300">Mật khẩu</label>
                <a href="#" className="text-xs text-brand-400 hover:underline">
                  Quên mật khẩu?
                </a>
              </div>
              <div className="relative">
                <Lock className="w-4 h-4 text-slate-500 absolute left-3.5 top-1/2 -translate-y-1/2" />
                <input
                  type={showPassword ? "text" : "password"}
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="••••••••"
                  required
                  className="w-full bg-slate-900/90 border border-slate-800 rounded-xl pl-10 pr-10 py-2.5 text-sm text-slate-100 placeholder:text-slate-600 focus:outline-none focus:border-brand-500 focus:ring-1 focus:ring-brand-500 transition-colors"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3.5 top-1/2 -translate-y-1/2 text-slate-500 hover:text-slate-300"
                >
                  {showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                </button>
              </div>
            </div>

            {/* Submit Button */}
            <button
              type="submit"
              disabled={loading}
              className="w-full py-2.5 px-4 rounded-xl bg-gradient-to-r from-brand-500 to-tech-teal hover:from-brand-600 hover:to-tech-teal/90 text-white font-semibold text-sm shadow-glow-sm hover:shadow-glow-md transition-all duration-200 flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loading ? (
                <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin"></div>
              ) : (
                <>
                  <span>Đăng nhập</span>
                  <ArrowRight className="w-4 h-4" />
                </>
              )}
            </button>
          </form>

          {/* Register Link */}
          <p className="text-center text-xs text-slate-400">
            Chưa có tài khoản?{" "}
            <Link href="/register" className="text-brand-400 font-semibold hover:underline">
              Đăng ký ngay
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}
