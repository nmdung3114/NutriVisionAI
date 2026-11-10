"use client";

import React, { useState, useMemo } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Sparkles, Mail, Lock, User, ArrowRight, Activity, Flame, AlertCircle } from "lucide-react";
import { authApi } from "@/lib/api";

export default function RegisterPage() {
  const router = useRouter();
  const [step, setStep] = useState<1 | 2>(1);
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  // Step 1: Credentials
  const [hoTen, setHoTen] = useState("");
  const [email, setEmail] = useState("");
  const [matKhau, setMatKhau] = useState("");

  // Step 2: Body Profile
  const [chieuCaoCm, setChieuCaoCm] = useState(170);
  const [canNangKg, setCanNangKg] = useState(65);
  const [tuoi, setTuoi] = useState(25);
  const [gioiTinh, setGioiTinh] = useState<"Nam" | "Nu">("Nam");
  const [mucDoHoatDong, setMucDoHoatDong] = useState("VuaPhai");
  const [mucTieu, setMucTieu] = useState("DuyTri");

  // Realtime Mifflin–St Jeor Calculation
  const estimatedMetrics = useMemo(() => {
    let bmr = 10 * canNangKg + 6.25 * chieuCaoCm - 5 * tuoi;
    bmr += gioiTinh === "Nam" ? 5 : -161;

    const activityMultipliers: Record<string, number> = {
      ItVanDong: 1.2,
      NheNhang: 1.375,
      VuaPhai: 1.55,
      NangDong: 1.725,
      RatNangDong: 1.9,
    };
    const mult = activityMultipliers[mucDoHoatDong] || 1.2;
    const tdee = bmr * mult;

    let caloMucTieu = tdee;
    if (mucTieu === "GiamCan") caloMucTieu = tdee - 500;
    else if (mucTieu === "TangCo") caloMucTieu = tdee + 300;

    const bmi = canNangKg / Math.pow(chieuCaoCm / 100, 2);

    return {
      bmr: Math.round(bmr),
      tdee: Math.round(tdee),
      caloMucTieu: Math.max(1200, Math.round(caloMucTieu)),
      bmi: Number(bmi.toFixed(1)),
    };
  }, [chieuCaoCm, canNangKg, tuoi, gioiTinh, mucDoHoatDong, mucTieu]);

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    if (step === 1) {
      if (!hoTen || !email || !matKhau) {
        setErrorMessage("Vui lòng điền đầy đủ họ tên, email và mật khẩu.");
        return;
      }
      setErrorMessage("");
      setStep(2);
      return;
    }

    setLoading(true);
    setErrorMessage("");

    try {
      const res = await authApi.register({
        email,
        matKhau,
        hoTen,
        chieuCaoCm,
        canNangKg,
        tuoi,
        gioiTinh,
        mucDoHoatDong,
        mucTieu,
      });

      if (res.data?.success) {
        const { accessToken, refreshToken, user } = res.data.data;
        localStorage.setItem("nv_access_token", accessToken);
        localStorage.setItem("nv_refresh_token", refreshToken);
        localStorage.setItem("nv_user", JSON.stringify(user));
        router.push("/dashboard");
      } else {
        setErrorMessage(res.data?.message || "Đăng ký thất bại");
      }
    } catch (err: any) {
      const msg = err.response?.data?.message || "Đăng ký không thành công. Email có thể đã tồn tại.";
      setErrorMessage(msg);
      setStep(1);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-[#090d16] text-slate-100 flex items-stretch">
      {/* Left Column: Visual Showcase */}
      <div className="hidden lg:flex flex-1 relative bg-gradient-to-br from-slate-950 via-[#0c1424] to-slate-900 flex-col justify-between p-12 overflow-hidden border-r border-slate-800/80">
        <div className="relative z-10 flex items-center gap-3">
          <div className="w-12 h-12 rounded-2xl bg-gradient-to-tr from-brand-600 to-tech-cyan flex items-center justify-center text-white shadow-glow-md">
            <Sparkles className="w-7 h-7" />
          </div>
          <div>
            <h1 className="text-xl font-bold tracking-tight text-white flex items-center gap-2">
              NutriVision<span className="text-brand-400 text-xs px-2 py-0.5 rounded bg-brand-500/10 border border-brand-500/30">AI</span>
            </h1>
            <p className="text-xs text-slate-400">Thiết lập hồ sơ sức khỏe thông minh</p>
          </div>
        </div>

        {/* Live Calculation Preview Card */}
        <div className="relative z-10 max-w-lg my-auto space-y-6">
          <h2 className="text-3xl font-extrabold tracking-tight text-white leading-tight">
            Cá nhân hóa chế độ ăn <br />
            theo <span className="text-gradient-emerald">thể trạng thực tế</span> của bạn.
          </h2>

          <div className="p-6 rounded-2xl bg-slate-900/90 border border-brand-500/30 shadow-glow-sm space-y-4">
            <div className="flex items-center justify-between pb-3 border-b border-slate-800">
              <span className="text-xs font-semibold text-slate-300 flex items-center gap-2">
                <Activity className="w-4 h-4 text-brand-400" /> Chỉ số cơ thể ước tính (Mifflin–St Jeor)
              </span>
              <span className="text-xs font-bold px-2 py-0.5 rounded bg-brand-500/20 text-brand-300">
                BMI: {estimatedMetrics.bmi}
              </span>
            </div>

            <div className="grid grid-cols-3 gap-3">
              <div className="p-3 rounded-xl bg-slate-800/60 text-center">
                <p className="text-[11px] text-slate-400">Chuyển hóa cơ bản (BMR)</p>
                <p className="text-base font-bold text-slate-200 mt-1">{estimatedMetrics.bmr} <span className="text-[10px] font-normal text-slate-400">kcal</span></p>
              </div>
              <div className="p-3 rounded-xl bg-slate-800/60 text-center">
                <p className="text-[11px] text-slate-400">Tiêu hao hàng ngày (TDEE)</p>
                <p className="text-base font-bold text-tech-cyan mt-1">{estimatedMetrics.tdee} <span className="text-[10px] font-normal text-slate-400">kcal</span></p>
              </div>
              <div className="p-3 rounded-xl bg-brand-950/60 border border-brand-500/30 text-center">
                <p className="text-[11px] text-brand-300 flex items-center justify-center gap-1">
                  <Flame className="w-3.5 h-3.5 text-amber-400" /> Mục tiêu calo
                </p>
                <p className="text-base font-bold text-brand-400 mt-1">{estimatedMetrics.caloMucTieu} <span className="text-[10px] font-normal text-slate-400">kcal</span></p>
              </div>
            </div>
            <p className="text-[11px] text-slate-400 text-center italic">
              * Hệ thống sẽ tự động cân đối tỷ lệ Macro Protein/Carbs/Fat phù hợp với mục tiêu của bạn.
            </p>
          </div>
        </div>

        <div className="relative z-10 text-xs text-slate-400">
          Bước {step}/2: {step === 1 ? "Thông tin tài khoản" : "Chỉ số thể trạng cơ thể"}
        </div>
      </div>

      {/* Right Column: Register Form */}
      <div className="w-full lg:w-[500px] p-8 lg:p-12 flex flex-col justify-center bg-[#090d16] relative overflow-y-auto">
        <div className="max-w-md w-full mx-auto space-y-6">
          <div>
            <h2 className="text-2xl font-bold text-white tracking-tight">Tạo tài khoản mới</h2>
            <p className="text-sm text-slate-400 mt-1">
              {step === 1 ? "Bắt đầu hành trình dinh dưỡng cùng AI" : "Cung cấp chỉ số để AI cá nhân hóa thực đơn"}
            </p>
          </div>

          {errorMessage && (
            <div className="p-3.5 rounded-xl bg-rose-500/10 border border-rose-500/30 text-rose-300 text-xs flex items-center gap-2">
              <AlertCircle className="w-4 h-4 shrink-0" />
              <span>{errorMessage}</span>
            </div>
          )}

          <form onSubmit={handleRegister} className="space-y-4">
            {step === 1 ? (
              <>
                <div className="space-y-1.5">
                  <label className="text-xs font-medium text-slate-300">Họ và tên</label>
                  <div className="relative">
                    <User className="w-4 h-4 text-slate-500 absolute left-3.5 top-1/2 -translate-y-1/2" />
                    <input
                      type="text"
                      value={hoTen}
                      onChange={(e) => setHoTen(e.target.value)}
                      placeholder="Nguyễn Văn A"
                      required
                      className="w-full bg-slate-900/90 border border-slate-800 rounded-xl pl-10 pr-4 py-2.5 text-sm text-slate-100 placeholder:text-slate-600 focus:outline-none focus:border-brand-500 focus:ring-1 focus:ring-brand-500"
                    />
                  </div>
                </div>

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
                      className="w-full bg-slate-900/90 border border-slate-800 rounded-xl pl-10 pr-4 py-2.5 text-sm text-slate-100 placeholder:text-slate-600 focus:outline-none focus:border-brand-500 focus:ring-1 focus:ring-brand-500"
                    />
                  </div>
                </div>

                <div className="space-y-1.5">
                  <label className="text-xs font-medium text-slate-300">Mật khẩu (Tối thiểu 6 ký tự)</label>
                  <div className="relative">
                    <Lock className="w-4 h-4 text-slate-500 absolute left-3.5 top-1/2 -translate-y-1/2" />
                    <input
                      type="password"
                      value={matKhau}
                      onChange={(e) => setMatKhau(e.target.value)}
                      placeholder="••••••••"
                      required
                      minLength={6}
                      className="w-full bg-slate-900/90 border border-slate-800 rounded-xl pl-10 pr-4 py-2.5 text-sm text-slate-100 placeholder:text-slate-600 focus:outline-none focus:border-brand-500 focus:ring-1 focus:ring-brand-500"
                    />
                  </div>
                </div>

                <button
                  type="submit"
                  className="w-full py-2.5 px-4 rounded-xl bg-gradient-to-r from-brand-500 to-tech-teal hover:from-brand-600 hover:to-tech-teal/90 text-white font-semibold text-sm shadow-glow-sm hover:shadow-glow-md transition-all flex items-center justify-center gap-2"
                >
                  <span>Tiếp tục: Nhập chỉ số cơ thể</span>
                  <ArrowRight className="w-4 h-4" />
                </button>
              </>
            ) : (
              <>
                <div className="grid grid-cols-2 gap-3">
                  <div className="space-y-1">
                    <label className="text-xs text-slate-300">Chiều cao (cm)</label>
                    <input
                      type="number"
                      value={chieuCaoCm}
                      onChange={(e) => setChieuCaoCm(Number(e.target.value))}
                      className="w-full bg-slate-900 border border-slate-800 rounded-xl px-3 py-2 text-sm text-slate-100"
                    />
                  </div>
                  <div className="space-y-1">
                    <label className="text-xs text-slate-300">Cân nặng (kg)</label>
                    <input
                      type="number"
                      value={canNangKg}
                      onChange={(e) => setCanNangKg(Number(e.target.value))}
                      className="w-full bg-slate-900 border border-slate-800 rounded-xl px-3 py-2 text-sm text-slate-100"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-3">
                  <div className="space-y-1">
                    <label className="text-xs text-slate-300">Tuổi</label>
                    <input
                      type="number"
                      value={tuoi}
                      onChange={(e) => setTuoi(Number(e.target.value))}
                      className="w-full bg-slate-900 border border-slate-800 rounded-xl px-3 py-2 text-sm text-slate-100"
                    />
                  </div>
                  <div className="space-y-1">
                    <label className="text-xs text-slate-300">Giới tính</label>
                    <select
                      value={gioiTinh}
                      onChange={(e) => setGioiTinh(e.target.value as any)}
                      className="w-full bg-slate-900 border border-slate-800 rounded-xl px-3 py-2 text-sm text-slate-100"
                    >
                      <option value="Nam">Nam</option>
                      <option value="Nu">Nữ</option>
                    </select>
                  </div>
                </div>

                <div className="space-y-1">
                  <label className="text-xs text-slate-300">Mức độ vận động</label>
                  <select
                    value={mucDoHoatDong}
                    onChange={(e) => setMucDoHoatDong(e.target.value)}
                    className="w-full bg-slate-900 border border-slate-800 rounded-xl px-3 py-2 text-sm text-slate-100"
                  >
                    <option value="ItVanDong">Ít vận động (làm việc văn phòng)</option>
                    <option value="NheNhang">Nhẹ nhàng (tập 1-3 ngày/tuần)</option>
                    <option value="VuaPhai">Vừa phải (tập 3-5 ngày/tuần)</option>
                    <option value="NangDong">Năng động (tập 6-7 ngày/tuần)</option>
                  </select>
                </div>

                <div className="space-y-1">
                  <label className="text-xs text-slate-300">Mục tiêu dinh dưỡng</label>
                  <select
                    value={mucTieu}
                    onChange={(e) => setMucTieu(e.target.value)}
                    className="w-full bg-slate-900 border border-slate-800 rounded-xl px-3 py-2 text-sm text-slate-100"
                  >
                    <option value="DuyTri">Duy trì cân nặng khỏe mạnh</option>
                    <option value="GiamCan">Giảm cân (Thâm hụt 500 kcal)</option>
                    <option value="TangCo">Tăng cân / Phát triển cơ bắp (+300 kcal)</option>
                  </select>
                </div>

                <div className="flex gap-2 pt-2">
                  <button
                    type="button"
                    onClick={() => setStep(1)}
                    className="py-2.5 px-4 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-300 text-sm font-medium transition-colors"
                  >
                    Quay lại
                  </button>
                  <button
                    type="submit"
                    disabled={loading}
                    className="flex-1 py-2.5 px-4 rounded-xl bg-gradient-to-r from-brand-500 to-tech-teal hover:from-brand-600 hover:to-tech-teal/90 text-white font-semibold text-sm shadow-glow-sm hover:shadow-glow-md transition-all flex items-center justify-center gap-2 disabled:opacity-50"
                  >
                    {loading ? (
                      <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin"></div>
                    ) : (
                      <>
                        <span>Hoàn tất đăng ký & Bắt đầu</span>
                        <ArrowRight className="w-4 h-4" />
                      </>
                    )}
                  </button>
                </div>
              </>
            )}
          </form>

          <p className="text-center text-xs text-slate-400">
            Đã có tài khoản?{" "}
            <Link href="/login" className="text-brand-400 font-semibold hover:underline">
              Đăng nhập ngay
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}
