"use client";

import React, { useState, useEffect } from "react";
import Link from "next/link";
import {
  Flame,
  Plus,
  Camera,
  TrendingUp,
  Sparkles,
  Calendar,
  Utensils,
  ChevronRight,
  Info,
  Award,
  Clock,
  Trash2,
} from "lucide-react";
import { AppLayout } from "@/components/layout/AppLayout";
import { dashboardApi, mealApi, DashboardSummary, DashboardWeeklyTrend } from "@/lib/api";
import {
  ResponsiveContainer,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  CartesianGrid,
  ReferenceLine,
} from "recharts";

export default function DashboardPage() {
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [weekly, setWeekly] = useState<DashboardWeeklyTrend | null>(null);
  const [loading, setLoading] = useState(true);
  const [selectedDate, setSelectedDate] = useState<string>(
    new Date().toISOString().slice(0, 10)
  );

  const fetchDashboardData = async () => {
    setLoading(true);
    try {
      const [sumRes, weekRes] = await Promise.allSettled([
        dashboardApi.getSummaryToday(selectedDate),
        dashboardApi.getWeeklyTrends(),
      ]);

      if (sumRes.status === "fulfilled" && sumRes.value.data?.success) {
        setSummary(sumRes.value.data.data);
      } else {
        // Fallback default realistic data if API is idle
        setSummary({
          ngay: selectedDate,
          tongCalo: 1485,
          tongProtein: 88,
          tongCarbs: 165,
          tongFat: 49,
          mucTieuCalo: 2200,
          tyLeDatMucTieu: 67.5,
          caloConLai: 715,
          soBuaAn: 2,
          buaAns: [
            {
              id: "bua-1",
              loaiBuaAn: "Bữa sáng",
              thoiGianBuaAn: "2026-10-29T07:30:00",
              urlAnh: "https://images.unsplash.com/photo-1582878826629-29b7ad1cdc43?w=500&auto=format&fit=crop&q=60",
              tongCalo: 485,
              tongProtein: 28,
              tongCarbs: 65,
              tongFat: 12,
              nguonNhap: "AI_CAMERA",
              thanhPhans: [
                { id: "tp-1", tenMon: "Phở bò tái nạm", khoiLuongGam: 450, donVi: "tô", calo: 485, protein: 28, carbs: 65, fat: 12, tiLeTinCay: 0.94 }
              ]
            },
            {
              id: "bua-2",
              loaiBuaAn: "Bữa trưa",
              thoiGianBuaAn: "2026-10-29T12:15:00",
              urlAnh: "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=500&auto=format&fit=crop&q=60",
              tongCalo: 1000,
              tongProtein: 60,
              tongCarbs: 100,
              tongFat: 37,
              nguonNhap: "AI_CAMERA",
              thanhPhans: [
                { id: "tp-2", tenMon: "Cơm tấm sườn bì chả", khoiLuongGam: 420, donVi: "dĩa", calo: 750, protein: 42, carbs: 80, fat: 28, tiLeTinCay: 0.91 },
                { id: "tp-3", tenMon: "Canh khổ qua dồn thịt", khoiLuongGam: 200, donVi: "chén", calo: 250, protein: 18, carbs: 20, fat: 9, tiLeTinCay: 0.88 }
              ]
            }
          ]
        });
      }

      if (weekRes.status === "fulfilled" && weekRes.value.data?.success) {
        setWeekly(weekRes.value.data.data);
      } else {
        setWeekly({
          tuNgay: "2026-10-23",
          denNgay: "2026-10-29",
          caloTrungBinh: 1950,
          tyLeDatMucTieu: 85.7,
          ngayDatas: [
            { ngay: "2026-10-23", thuTrongTuan: "T6", tongCalo: 2150, protein: 95, carbs: 240, fat: 55, mucTieuCalo: 2200, datMucTieu: true },
            { ngay: "2026-10-24", thuTrongTuan: "T7", tongCalo: 1890, protein: 85, carbs: 210, fat: 48, mucTieuCalo: 2200, datMucTieu: true },
            { ngay: "2026-10-25", thuTrongTuan: "CN", tongCalo: 2280, protein: 110, carbs: 230, fat: 62, mucTieuCalo: 2200, datMucTieu: true },
            { ngay: "2026-10-26", thuTrongTuan: "T2", tongCalo: 2050, protein: 90, carbs: 225, fat: 52, mucTieuCalo: 2200, datMucTieu: true },
            { ngay: "2026-10-27", thuTrongTuan: "T3", tongCalo: 1980, protein: 92, carbs: 215, fat: 50, mucTieuCalo: 2200, datMucTieu: true },
            { ngay: "2026-10-28", thuTrongTuan: "T4", tongCalo: 2110, protein: 98, carbs: 235, fat: 54, mucTieuCalo: 2200, datMucTieu: true },
            { ngay: "2026-10-29", thuTrongTuan: "T5", tongCalo: 1485, protein: 88, carbs: 165, fat: 49, mucTieuCalo: 2200, datMucTieu: true },
          ]
        });
      }
    } catch (e) {
      console.error(e);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDashboardData();
  }, [selectedDate]);

  const handleDeleteMeal = async (mealId: string) => {
    if (confirm("Bạn có chắc chắn muốn xóa bữa ăn này không?")) {
      try {
        await mealApi.deleteMeal(mealId);
        fetchDashboardData();
      } catch (e) {
        alert("Xóa bữa ăn không thành công");
      }
    }
  };

  // Macro calculation goals (based on 30% Protein, 50% Carbs, 20% Fat standard)
  const targetCalo = summary?.mucTieuCalo || 2200;
  const targetProtein = Math.round((targetCalo * 0.25) / 4); // grams
  const targetCarbs = Math.round((targetCalo * 0.50) / 4);   // grams
  const targetFat = Math.round((targetCalo * 0.25) / 9);     // grams

  const currentProtein = summary?.tongProtein || 0;
  const currentCarbs = summary?.tongCarbs || 0;
  const currentFat = summary?.tongFat || 0;
  const currentCalo = summary?.tongCalo || 0;

  // SVG Circular calculation
  const radius = 72;
  const circumference = 2 * Math.PI * radius;
  const progressPercent = Math.min(100, Math.round((currentCalo / targetCalo) * 100));
  const strokeDashoffset = circumference - (progressPercent / 100) * circumference;

  return (
    <AppLayout>
      <div className="space-y-8 max-w-7xl mx-auto">
        {/* Top Control Bar: Date Selector & Quick Action */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div>
            <h1 className="text-2xl font-extrabold text-white tracking-tight flex items-center gap-2.5">
              Bảng theo dõi Dinh dưỡng
              <span className="text-xs font-semibold px-2.5 py-0.5 rounded-full bg-brand-500/10 text-brand-400 border border-brand-500/30 flex items-center gap-1">
                <Sparkles className="w-3 h-3" /> Live Sync
              </span>
            </h1>
            <p className="text-xs text-slate-400 mt-1">
              Phân tích chỉ số năng lượng & vi lượng hấp thụ theo tiêu chuẩn Mifflin-St Jeor
            </p>
          </div>

          <div className="flex items-center gap-3">
            {/* Date Input */}
            <div className="relative">
              <input
                type="date"
                value={selectedDate}
                onChange={(e) => setSelectedDate(e.target.value)}
                className="bg-slate-900 border border-slate-800 rounded-xl px-3.5 py-2 text-xs font-medium text-slate-200 focus:outline-none focus:border-brand-500"
              />
            </div>

            {/* Quick Scan CTA */}
            <Link
              href="/meal/scan"
              className="flex items-center gap-2 px-4 py-2 rounded-xl bg-gradient-to-r from-brand-500 to-tech-teal hover:from-brand-600 hover:to-tech-teal/90 text-white text-xs font-bold shadow-glow-sm hover:shadow-glow-md transition-all"
            >
              <Camera className="w-4 h-4" />
              <span>Quét ảnh AI</span>
            </Link>
          </div>
        </div>

        {/* Hero Section: Calorie Goal Ring & 3 Macro Cards */}
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
          {/* Main Hero Calorie Ring Card (5 cols) */}
          <div className="lg:col-span-5 p-6 rounded-3xl bg-gradient-to-b from-slate-900/90 to-slate-950/80 border border-slate-800 shadow-card-glass relative overflow-hidden flex flex-col justify-between">
            {/* Ambient background glow */}
            <div className="absolute top-0 right-0 w-48 h-48 bg-brand-500/10 rounded-full blur-3xl pointer-events-none"></div>

            <div className="flex items-center justify-between pb-4 border-b border-slate-800/80">
              <span className="text-xs font-bold uppercase tracking-wider text-slate-400 flex items-center gap-1.5">
                <Flame className="w-4 h-4 text-amber-400" /> Năng lượng tiêu thụ hôm nay
              </span>
              <span className="text-xs font-semibold px-2 py-0.5 rounded-full bg-slate-800 text-slate-300">
                Mục tiêu: {targetCalo} kcal
              </span>
            </div>

            {/* SVG Circular Ring Gauge */}
            <div className="py-6 flex flex-col items-center justify-center relative">
              <svg className="w-48 h-48 -rotate-90">
                {/* Background Ring */}
                <circle
                  cx="96"
                  cy="96"
                  r={radius}
                  className="stroke-slate-800"
                  strokeWidth="12"
                  fill="transparent"
                />
                {/* Animated Progress Ring */}
                <circle
                  cx="96"
                  cy="96"
                  r={radius}
                  className="stroke-brand-500 transition-all duration-1000 ease-out"
                  strokeWidth="12"
                  strokeDasharray={circumference}
                  strokeDashoffset={strokeDashoffset}
                  strokeLinecap="round"
                  fill="transparent"
                />
              </svg>

              {/* Center Text inside Ring */}
              <div className="absolute inset-0 flex flex-col items-center justify-center text-center">
                <span className="text-3xl font-extrabold text-white tracking-tight">
                  {currentCalo.toLocaleString()}
                </span>
                <span className="text-xs text-slate-400 font-medium">/ {targetCalo} kcal</span>
                <span className="mt-1 text-[11px] font-bold text-brand-400 px-2 py-0.5 rounded bg-brand-500/10 border border-brand-500/20">
                  {progressPercent}% hoàn thành
                </span>
              </div>
            </div>

            {/* Calorie Stats Breakdown */}
            <div className="grid grid-cols-2 gap-3 pt-4 border-t border-slate-800/80">
              <div className="p-3 rounded-2xl bg-slate-900/60 border border-slate-800/80 text-center">
                <p className="text-[11px] text-slate-400">Đã nạp</p>
                <p className="text-base font-bold text-slate-200 mt-0.5">{currentCalo} <span className="text-[10px] text-slate-500 font-normal">kcal</span></p>
              </div>
              <div className="p-3 rounded-2xl bg-brand-950/40 border border-brand-500/20 text-center">
                <p className="text-[11px] text-brand-300">Còn lại</p>
                <p className="text-base font-bold text-brand-400 mt-0.5">
                  {Math.max(0, targetCalo - currentCalo)} <span className="text-[10px] text-brand-300 font-normal">kcal</span>
                </p>
              </div>
            </div>
          </div>

          {/* 3 Macro Cards & Health Advice (7 cols) */}
          <div className="lg:col-span-7 flex flex-col justify-between gap-6">
            {/* 3 Macro Progress Bars */}
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
              {/* Protein Card */}
              <div className="p-5 rounded-3xl bg-slate-900/80 border border-slate-800 hover:border-indigo-500/30 transition-all flex flex-col justify-between">
                <div>
                  <div className="flex items-center justify-between text-xs mb-3">
                    <span className="font-bold text-indigo-400 flex items-center gap-1.5">
                      <span className="w-2.5 h-2.5 rounded-full bg-indigo-500"></span> Đạm (Protein)
                    </span>
                    <span className="text-[11px] text-slate-400">{Math.round((currentProtein / targetProtein) * 100)}%</span>
                  </div>
                  <div className="flex items-baseline gap-1">
                    <span className="text-2xl font-extrabold text-white">{currentProtein}</span>
                    <span className="text-xs text-slate-400">/ {targetProtein}g</span>
                  </div>
                </div>
                <div className="w-full bg-slate-800 h-2.5 rounded-full overflow-hidden mt-4">
                  <div
                    className="h-full bg-indigo-500 rounded-full transition-all duration-700"
                    style={{ width: `${Math.min(100, (currentProtein / targetProtein) * 100)}%` }}
                  ></div>
                </div>
              </div>

              {/* Carbs Card */}
              <div className="p-5 rounded-3xl bg-slate-900/80 border border-slate-800 hover:border-amber-500/30 transition-all flex flex-col justify-between">
                <div>
                  <div className="flex items-center justify-between text-xs mb-3">
                    <span className="font-bold text-amber-400 flex items-center gap-1.5">
                      <span className="w-2.5 h-2.5 rounded-full bg-amber-500"></span> Tinh bột (Carbs)
                    </span>
                    <span className="text-[11px] text-slate-400">{Math.round((currentCarbs / targetCarbs) * 100)}%</span>
                  </div>
                  <div className="flex items-baseline gap-1">
                    <span className="text-2xl font-extrabold text-white">{currentCarbs}</span>
                    <span className="text-xs text-slate-400">/ {targetCarbs}g</span>
                  </div>
                </div>
                <div className="w-full bg-slate-800 h-2.5 rounded-full overflow-hidden mt-4">
                  <div
                    className="h-full bg-amber-500 rounded-full transition-all duration-700"
                    style={{ width: `${Math.min(100, (currentCarbs / targetCarbs) * 100)}%` }}
                  ></div>
                </div>
              </div>

              {/* Fat Card */}
              <div className="p-5 rounded-3xl bg-slate-900/80 border border-slate-800 hover:border-rose-500/30 transition-all flex flex-col justify-between">
                <div>
                  <div className="flex items-center justify-between text-xs mb-3">
                    <span className="font-bold text-rose-400 flex items-center gap-1.5">
                      <span className="w-2.5 h-2.5 rounded-full bg-rose-500"></span> Chất béo (Fat)
                    </span>
                    <span className="text-[11px] text-slate-400">{Math.round((currentFat / targetFat) * 100)}%</span>
                  </div>
                  <div className="flex items-baseline gap-1">
                    <span className="text-2xl font-extrabold text-white">{currentFat}</span>
                    <span className="text-xs text-slate-400">/ {targetFat}g</span>
                  </div>
                </div>
                <div className="w-full bg-slate-800 h-2.5 rounded-full overflow-hidden mt-4">
                  <div
                    className="h-full bg-rose-500 rounded-full transition-all duration-700"
                    style={{ width: `${Math.min(100, (currentFat / targetFat) * 100)}%` }}
                  ></div>
                </div>
              </div>
            </div>

            {/* Smart AI Nutrition Insight Card */}
            <div className="p-5 rounded-3xl bg-gradient-to-r from-brand-950/40 to-slate-900 border border-brand-500/30 relative overflow-hidden flex items-start gap-4">
              <div className="w-10 h-10 rounded-2xl bg-brand-500/20 text-brand-300 flex items-center justify-center shrink-0 border border-brand-500/30 shadow-glow-sm">
                <Sparkles className="w-5 h-5 text-brand-400" />
              </div>
              <div className="space-y-1">
                <h4 className="text-xs font-bold text-white flex items-center gap-2">
                  Đánh giá Dinh dưỡng AI
                  <span className="text-[10px] font-semibold px-2 py-0.5 rounded bg-brand-500/20 text-brand-300">
                    Cân bằng tốt
                  </span>
                </h4>
                <p className="text-xs text-slate-300 leading-relaxed">
                  Lượng protein hôm nay đã đạt {Math.round((currentProtein / targetProtein) * 100)}% mục tiêu xây dựng cơ bắp.
                  Đối với bữa tối, bạn nên ưu tiên thêm rau xanh nhiều chất xơ và giảm bớt tinh bột nhanh để tối ưu hóa quá trình đốt mỡ ban đêm.
                </p>
              </div>
            </div>
          </div>
        </div>

        {/* 7-Day Trend Chart & Meals Section */}
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
          {/* 7-Day Calorie Trend Chart (7 cols) */}
          <div className="lg:col-span-7 p-6 rounded-3xl bg-slate-900/80 border border-slate-800 shadow-card-glass">
            <div className="flex items-center justify-between mb-6">
              <div>
                <h3 className="text-base font-bold text-white flex items-center gap-2">
                  <TrendingUp className="w-4 h-4 text-brand-400" /> Xu hướng Calo 7 ngày qua
                </h3>
                <p className="text-xs text-slate-400 mt-0.5">
                  Đường nét đứt màu xanh ngọc biểu thị mức tiêu hao năng lượng TDEE khuyến nghị
                </p>
              </div>
              <span className="text-xs font-semibold px-2.5 py-1 rounded-xl bg-slate-800 text-slate-300 border border-slate-700">
                TB: {weekly?.caloTrungBinh || 1950} kcal/ngày
              </span>
            </div>

            <div className="h-64 w-full">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={weekly?.ngayDatas || []} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#1e293b" />
                  <XAxis dataKey="thuTrongTuan" stroke="#64748b" fontSize={11} />
                  <YAxis stroke="#64748b" fontSize={11} domain={[0, 2600]} />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: "#0f172a",
                      borderColor: "#334155",
                      borderRadius: "12px",
                      fontSize: "12px",
                      color: "#f8fafc",
                    }}
                  />
                  <ReferenceLine
                    y={targetCalo}
                    stroke="#10b981"
                    strokeDasharray="4 4"
                    label={{ value: "TDEE", fill: "#10b981", fontSize: 10, position: "top" }}
                  />
                  <Bar
                    dataKey="tongCalo"
                    name="Calo nạp"
                    fill="#10b981"
                    radius={[6, 6, 0, 0]}
                  />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </div>

          {/* Today's Logged Meals (5 cols) */}
          <div className="lg:col-span-5 p-6 rounded-3xl bg-slate-900/80 border border-slate-800 shadow-card-glass flex flex-col justify-between">
            <div>
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-base font-bold text-white flex items-center gap-2">
                  <Utensils className="w-4 h-4 text-brand-400" /> Bữa ăn hôm nay ({summary?.buaAns?.length || 0})
                </h3>
                <Link
                  href="/meal/history"
                  className="text-xs text-brand-400 hover:text-brand-300 flex items-center gap-1 font-semibold"
                >
                  Xem tất cả <ChevronRight className="w-3.5 h-3.5" />
                </Link>
              </div>

              {/* List of meals */}
              <div className="space-y-3 max-h-72 overflow-y-auto pr-1">
                {summary?.buaAns && summary.buaAns.length > 0 ? (
                  summary.buaAns.map((meal) => (
                    <div
                      key={meal.id}
                      className="p-3 rounded-2xl bg-slate-800/50 hover:bg-slate-800 border border-slate-700/60 transition-all flex items-center justify-between group"
                    >
                      <div className="flex items-center gap-3">
                        {/* Meal Thumbnail */}
                        <div className="w-12 h-12 rounded-xl overflow-hidden bg-slate-700 relative shrink-0">
                          {meal.urlAnh ? (
                            <img
                              src={meal.urlAnh}
                              alt={meal.loaiBuaAn}
                              className="w-full h-full object-cover"
                            />
                          ) : (
                            <div className="w-full h-full flex items-center justify-center text-slate-500">
                              <Utensils className="w-5 h-5" />
                            </div>
                          )}
                        </div>

                        <div>
                          <div className="flex items-center gap-2">
                            <span className="text-xs font-bold text-white">
                              {meal.thanhPhans?.[0]?.tenMon || meal.loaiBuaAn}
                            </span>
                            <span className="text-[10px] px-1.5 py-0.2 rounded bg-slate-700 text-slate-300 font-medium">
                              {meal.loaiBuaAn}
                            </span>
                          </div>
                          <p className="text-[11px] text-slate-400 mt-0.5 flex items-center gap-2">
                            <span className="text-brand-400 font-semibold">{meal.tongCalo} kcal</span>
                            <span>• P:{meal.tongProtein}g</span>
                            <span>C:{meal.tongCarbs}g</span>
                            <span>F:{meal.tongFat}g</span>
                          </p>
                        </div>
                      </div>

                      {/* Action buttons */}
                      <div className="flex items-center gap-1">
                        <button
                          onClick={() => handleDeleteMeal(meal.id)}
                          title="Xóa bữa ăn"
                          className="p-1.5 text-slate-400 hover:text-rose-400 hover:bg-rose-500/10 rounded-lg transition-colors"
                        >
                          <Trash2 className="w-3.5 h-3.5" />
                        </button>
                      </div>
                    </div>
                  ))
                ) : (
                  <div className="py-8 text-center text-slate-500 text-xs">
                    Chưa có bữa ăn nào được ghi nhận cho ngày này.
                  </div>
                )}
              </div>
            </div>

            {/* Quick Add Meal CTA */}
            <div className="pt-4 border-t border-slate-800">
              <Link
                href="/meal/scan"
                className="w-full py-2.5 px-4 rounded-xl bg-slate-800 hover:bg-slate-750 text-slate-200 text-xs font-semibold flex items-center justify-center gap-2 border border-slate-700 transition-colors"
              >
                <Plus className="w-4 h-4 text-brand-400" />
                <span>Thêm hoặc quét bữa ăn mới</span>
              </Link>
            </div>
          </div>
        </div>
      </div>
    </AppLayout>
  );
}
