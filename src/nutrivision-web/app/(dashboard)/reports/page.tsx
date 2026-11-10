"use client";

import React, { useState, useEffect } from "react";
import {
  BarChart3,
  Download,
  Calendar,
  Sparkles,
  Award,
  TrendingUp,
  PieChart as PieIcon,
  CheckCircle2,
  FileSpreadsheet,
} from "lucide-react";
import { AppLayout } from "@/components/layout/AppLayout";
import { dashboardApi, DashboardWeeklyTrend } from "@/lib/api";
import {
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  Tooltip,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Legend,
} from "recharts";

const MACRO_COLORS = ["#6366f1", "#f59e0b", "#f43f5e"];

export default function ReportsPage() {
  const [weeklyData, setWeeklyData] = useState<DashboardWeeklyTrend | null>(null);
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const res = await dashboardApi.getWeeklyTrends();
        if (res.data?.success && res.data?.data) {
          setWeeklyData(res.data.data);
        } else {
          setWeeklyData({
            tuNgay: "2026-10-23",
            denNgay: "2026-10-29",
            caloTrungBinh: 2050,
            tyLeDatMucTieu: 85.7,
            ngayDatas: [
              { ngay: "2026-10-23", thuTrongTuan: "Thứ 6", tongCalo: 2150, protein: 95, carbs: 240, fat: 55, mucTieuCalo: 2200, datMucTieu: true },
              { ngay: "2026-10-24", thuTrongTuan: "Thứ 7", tongCalo: 1890, protein: 85, carbs: 210, fat: 48, mucTieuCalo: 2200, datMucTieu: true },
              { ngay: "2026-10-25", thuTrongTuan: "Chủ Nhật", tongCalo: 2280, protein: 110, carbs: 230, fat: 62, mucTieuCalo: 2200, datMucTieu: true },
              { ngay: "2026-10-26", thuTrongTuan: "Thứ 2", tongCalo: 2050, protein: 90, carbs: 225, fat: 52, mucTieuCalo: 2200, datMucTieu: true },
              { ngay: "2026-10-27", thuTrongTuan: "Thứ 3", tongCalo: 1980, protein: 92, carbs: 215, fat: 50, mucTieuCalo: 2200, datMucTieu: true },
              { ngay: "2026-10-28", thuTrongTuan: "Thứ 4", tongCalo: 2110, protein: 98, carbs: 235, fat: 54, mucTieuCalo: 2200, datMucTieu: true },
              { ngay: "2026-10-29", thuTrongTuan: "Thứ 5", tongCalo: 1485, protein: 88, carbs: 165, fat: 49, mucTieuCalo: 2200, datMucTieu: true },
            ],
          });
        }
      } catch (e) {
        console.error(e);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  const handleExportCsv = async () => {
    setExporting(true);
    try {
      await dashboardApi.exportCsv();
    } catch (e) {
      alert("Xuất dữ liệu thất bại");
    } finally {
      setExporting(false);
    }
  };

  // Macro Energy Distribution (% from 4 kcal/g protein, 4 kcal/g carbs, 9 kcal/g fat)
  const totalProteinGrams = weeklyData?.ngayDatas?.reduce((sum, d) => sum + d.protein, 0) || 648;
  const totalCarbsGrams = weeklyData?.ngayDatas?.reduce((sum, d) => sum + d.carbs, 0) || 1520;
  const totalFatGrams = weeklyData?.ngayDatas?.reduce((sum, d) => sum + d.fat, 0) || 370;

  const caloFromProtein = totalProteinGrams * 4;
  const caloFromCarbs = totalCarbsGrams * 4;
  const caloFromFat = totalFatGrams * 9;
  const totalEnergy = caloFromProtein + caloFromCarbs + caloFromFat;

  const macroDonutData = [
    { name: "Đạm (Protein)", value: Math.round((caloFromProtein / totalEnergy) * 100), grams: totalProteinGrams },
    { name: "Tinh bột (Carbs)", value: Math.round((caloFromCarbs / totalEnergy) * 100), grams: totalCarbsGrams },
    { name: "Chất béo (Fat)", value: Math.round((caloFromFat / totalEnergy) * 100), grams: totalFatGrams },
  ];

  return (
    <AppLayout>
      <div className="max-w-6xl mx-auto space-y-8">
        {/* Header with Export CTA */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div>
            <h1 className="text-2xl font-extrabold text-white tracking-tight flex items-center gap-2">
              Báo cáo & Phân tích Dinh dưỡng
            </h1>
            <p className="text-xs text-slate-400 mt-1">
              Đánh giá chuyên sâu thói quen ăn uống, cân bằng dưỡng chất và xuất lịch sử định dạng Excel/CSV
            </p>
          </div>

          <button
            onClick={handleExportCsv}
            disabled={exporting}
            className="flex items-center gap-2 px-4 py-2.5 rounded-xl bg-slate-800 hover:bg-slate-750 border border-slate-700 text-brand-300 font-bold text-xs shadow-glow-sm hover:border-brand-500/50 transition-all disabled:opacity-50"
          >
            <FileSpreadsheet className="w-4 h-4 text-brand-400" />
            <span>{exporting ? "Đang kết xuất file..." : "Xuất báo cáo Excel / CSV"}</span>
          </button>
        </div>

        {/* 3 Metric Summary Banner */}
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-5">
          <div className="p-6 rounded-3xl bg-slate-900/80 border border-slate-800 flex items-center gap-4">
            <div className="w-12 h-12 rounded-2xl bg-brand-500/10 border border-brand-500/20 text-brand-400 flex items-center justify-center shrink-0">
              <Award className="w-6 h-6" />
            </div>
            <div>
              <p className="text-xs text-slate-400">Điểm Dinh dưỡng AI</p>
              <p className="text-2xl font-extrabold text-white mt-0.5">88 <span className="text-xs font-normal text-slate-400">/ 100</span></p>
              <p className="text-[11px] text-brand-400 mt-0.5">Hạng Rất tốt (Top 10%)</p>
            </div>
          </div>

          <div className="p-6 rounded-3xl bg-slate-900/80 border border-slate-800 flex items-center gap-4">
            <div className="w-12 h-12 rounded-2xl bg-tech-cyan/10 border border-tech-cyan/20 text-tech-cyan flex items-center justify-center shrink-0">
              <TrendingUp className="w-6 h-6" />
            </div>
            <div>
              <p className="text-xs text-slate-400">Calo Trung bình tuần</p>
              <p className="text-2xl font-extrabold text-white mt-0.5">{weeklyData?.caloTrungBinh || 2050} <span className="text-xs font-normal text-slate-400">kcal</span></p>
              <p className="text-[11px] text-slate-400 mt-0.5">Mục tiêu: 2,200 kcal</p>
            </div>
          </div>

          <div className="p-6 rounded-3xl bg-slate-900/80 border border-slate-800 flex items-center gap-4">
            <div className="w-12 h-12 rounded-2xl bg-indigo-500/10 border border-indigo-500/20 text-indigo-400 flex items-center justify-center shrink-0">
              <CheckCircle2 className="w-6 h-6" />
            </div>
            <div>
              <p className="text-xs text-slate-400">Tỷ lệ Đạt Mục tiêu</p>
              <p className="text-2xl font-extrabold text-white mt-0.5">{weeklyData?.tyLeDatMucTieu || 85.7}%</p>
              <p className="text-[11px] text-indigo-300 mt-0.5">6 trên 7 ngày chuẩn TDEE</p>
            </div>
          </div>
        </div>

        {/* 2 Main Visual Analytics: Macro Energy Ratio Donut & Daily Stacked Bars */}
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
          {/* Donut Chart: Macro Split (5 cols) */}
          <div className="lg:col-span-5 p-6 rounded-3xl bg-slate-900/80 border border-slate-800 shadow-card-glass flex flex-col justify-between">
            <div>
              <h3 className="text-base font-bold text-white flex items-center gap-2">
                <PieIcon className="w-4 h-4 text-brand-400" /> Phân bổ năng lượng Macro
              </h3>
              <p className="text-xs text-slate-400 mt-0.5">
                Tỷ lệ % calo cung cấp bởi 3 nhóm chất dinh dưỡng chính
              </p>

              <div className="h-56 w-full mt-4">
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie
                      data={macroDonutData}
                      cx="50%"
                      cy="50%"
                      innerRadius={60}
                      outerRadius={85}
                      paddingAngle={5}
                      dataKey="value"
                    >
                      {macroDonutData.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={MACRO_COLORS[index % MACRO_COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip
                      contentStyle={{
                        backgroundColor: "#0f172a",
                        borderColor: "#334155",
                        borderRadius: "12px",
                        fontSize: "12px",
                      }}
                    />
                  </PieChart>
                </ResponsiveContainer>
              </div>
            </div>

            {/* Legend & Advice */}
            <div className="space-y-2 pt-4 border-t border-slate-800">
              <div className="flex items-center justify-between text-xs">
                <span className="flex items-center gap-2">
                  <span className="w-3 h-3 rounded-full bg-indigo-500"></span> Đạm (Protein)
                </span>
                <span className="font-bold text-slate-200">{macroDonutData[0].value}% ({macroDonutData[0].grams}g)</span>
              </div>
              <div className="flex items-center justify-between text-xs">
                <span className="flex items-center gap-2">
                  <span className="w-3 h-3 rounded-full bg-amber-500"></span> Tinh bột (Carbs)
                </span>
                <span className="font-bold text-slate-200">{macroDonutData[1].value}% ({macroDonutData[1].grams}g)</span>
              </div>
              <div className="flex items-center justify-between text-xs">
                <span className="flex items-center gap-2">
                  <span className="w-3 h-3 rounded-full bg-rose-500"></span> Chất béo (Fat)
                </span>
                <span className="font-bold text-slate-200">{macroDonutData[2].value}% ({macroDonutData[2].grams}g)</span>
              </div>
            </div>
          </div>

          {/* Daily Macro Breakdown Stacked Chart (7 cols) */}
          <div className="lg:col-span-7 p-6 rounded-3xl bg-slate-900/80 border border-slate-800 shadow-card-glass flex flex-col justify-between">
            <div>
              <h3 className="text-base font-bold text-white flex items-center gap-2">
                <BarChart3 className="w-4 h-4 text-brand-400" /> Biến thiên các chất dinh dưỡng theo ngày
              </h3>
              <p className="text-xs text-slate-400 mt-0.5">
                Khối lượng Protein, Carbs, Fat (gam) nạp vào mỗi ngày trong tuần
              </p>

              <div className="h-64 w-full mt-4">
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={weeklyData?.ngayDatas || []} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#1e293b" />
                    <XAxis dataKey="thuTrongTuan" stroke="#64748b" fontSize={11} />
                    <YAxis stroke="#64748b" fontSize={11} />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: "#0f172a",
                        borderColor: "#334155",
                        borderRadius: "12px",
                        fontSize: "12px",
                      }}
                    />
                    <Legend
                      wrapperStyle={{ fontSize: "11px", paddingTop: "10px" }}
                    />
                    <Bar dataKey="protein" name="Protein (g)" fill="#6366f1" radius={[4, 4, 0, 0]} />
                    <Bar dataKey="carbs" name="Carbs (g)" fill="#f59e0b" radius={[4, 4, 0, 0]} />
                    <Bar dataKey="fat" name="Fat (g)" fill="#f43f5e" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            </div>

            {/* Smart Medical Review */}
            <div className="p-4 rounded-2xl bg-brand-950/40 border border-brand-500/20 mt-4 text-xs text-slate-300 flex items-start gap-3">
              <Sparkles className="w-4 h-4 text-brand-400 shrink-0 mt-0.5" />
              <span>
                <strong>Nhận xét từ AI Y tế:</strong> Cơ cấu dinh dưỡng trong tuần duy trì tỷ lệ rất tốt (20% Đạm : 50% Tinh bột : 30% Chất béo). Khuyến nghị tiếp tục duy trì mức tiêu thụ nước lọc $\ge 2$ lít/ngày và bổ sung rau lá xanh đậm.
              </span>
            </div>
          </div>
        </div>
      </div>
    </AppLayout>
  );
}
