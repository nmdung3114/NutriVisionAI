"use client";

import React, { useState, useEffect } from "react";
import Link from "next/link";
import {
  Calendar,
  Filter,
  Plus,
  Utensils,
  Trash2,
  Clock,
  Sparkles,
  ChevronRight,
  Eye,
  RotateCcw,
  CheckCircle2,
} from "lucide-react";
import { AppLayout } from "@/components/layout/AppLayout";
import { mealApi, MealDetail } from "@/lib/api";

export default function MealHistoryPage() {
  const [meals, setMeals] = useState<MealDetail[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedFilterMealType, setSelectedFilterMealType] = useState<string>("ALL");
  const [selectedDate, setSelectedDate] = useState<string>(new Date().toISOString().slice(0, 10));
  const [activeModalMeal, setActiveModalMeal] = useState<MealDetail | null>(null);
  const [undoToast, setUndoToast] = useState<{ id: string; name: string } | null>(null);

  const fetchMeals = async () => {
    setLoading(true);
    try {
      const res = await mealApi.getMeals({
        tuNgay: selectedDate,
        denNgay: selectedDate,
        loaiBuaAn: selectedFilterMealType === "ALL" ? undefined : selectedFilterMealType,
      });

      if (res.data?.success && res.data?.data) {
        setMeals(res.data.data);
      } else {
        // Fallback demo data
        setMeals([
          {
            id: "m-101",
            loaiBuaAn: "Bữa sáng",
            thoiGianBuaAn: `${selectedDate}T07:45:00`,
            urlAnh: "https://images.unsplash.com/photo-1582878826629-29b7ad1cdc43?w=500&auto=format&fit=crop&q=60",
            tongCalo: 485,
            tongProtein: 28,
            tongCarbs: 65,
            tongFat: 12,
            nguonNhap: "AI_CAMERA",
            ghiChu: "Phở bò kèm quẩy nóng",
            thanhPhans: [
              { id: "tp-1", tenMon: "Phở bò tái nạm", khoiLuongGam: 450, donVi: "tô", calo: 485, protein: 28, carbs: 65, fat: 12, tiLeTinCay: 0.94 },
            ],
          },
          {
            id: "m-102",
            loaiBuaAn: "Bữa trưa",
            thoiGianBuaAn: `${selectedDate}T12:30:00`,
            urlAnh: "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=500&auto=format&fit=crop&q=60",
            tongCalo: 750,
            tongProtein: 42,
            tongCarbs: 80,
            tongFat: 28,
            nguonNhap: "AI_CAMERA",
            ghiChu: "Cơm tấm bì chả sườn nướng",
            thanhPhans: [
              { id: "tp-2", tenMon: "Cơm tấm sườn", khoiLuongGam: 350, donVi: "phần", calo: 550, protein: 32, carbs: 65, fat: 18, tiLeTinCay: 0.92 },
              { id: "tp-3", tenMon: "Chả trứng hấp", khoiLuongGam: 100, donVi: "miếng", calo: 200, protein: 10, carbs: 15, fat: 10, tiLeTinCay: 0.88 },
            ],
          },
        ]);
      }
    } catch (e) {
      console.error(e);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchMeals();
  }, [selectedDate, selectedFilterMealType]);

  const handleDelete = async (meal: MealDetail) => {
    if (confirm(`Bạn có chắc muốn xóa "${meal.loaiBuaAn}" khỏi nhật ký?`)) {
      try {
        await mealApi.deleteMeal(meal.id);
        setMeals((prev) => prev.filter((m) => m.id !== meal.id));
        setUndoToast({ id: meal.id, name: meal.loaiBuaAn });
        setTimeout(() => setUndoToast(null), 5000);
      } catch (e) {
        alert("Xóa không thành công");
      }
    }
  };

  const handleUndo = async () => {
    if (!undoToast) return;
    try {
      await mealApi.undoMeal(undoToast.id);
      setUndoToast(null);
      fetchMeals();
    } catch (e) {
      alert("Hoàn tác không thành công");
    }
  };

  return (
    <AppLayout>
      <div className="max-w-6xl mx-auto space-y-6">
        {/* Header & Filter Controls */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div>
            <h1 className="text-2xl font-extrabold text-white tracking-tight flex items-center gap-2">
              Nhật ký bữa ăn
            </h1>
            <p className="text-xs text-slate-400 mt-1">
              Tra cứu và quản lý lịch sử các bữa ăn hàng ngày của bạn
            </p>
          </div>

          <div className="flex flex-wrap items-center gap-3">
            {/* Filter by Meal Type */}
            <div className="flex items-center gap-1 p-1 bg-slate-900 border border-slate-800 rounded-xl text-xs">
              {["ALL", "Bữa sáng", "Bữa trưa", "Bữa tối", "Bữa phụ"].map((type) => (
                <button
                  key={type}
                  onClick={() => setSelectedFilterMealType(type)}
                  className={`px-3 py-1.5 rounded-lg font-medium transition-all ${
                    selectedFilterMealType === type
                      ? "bg-brand-500 text-white shadow-glow-sm"
                      : "text-slate-400 hover:text-slate-200"
                  }`}
                >
                  {type === "ALL" ? "Tất cả" : type}
                </button>
              ))}
            </div>

            {/* Date Picker */}
            <input
              type="date"
              value={selectedDate}
              onChange={(e) => setSelectedDate(e.target.value)}
              className="bg-slate-900 border border-slate-800 rounded-xl px-3 py-2 text-xs font-medium text-slate-200 focus:outline-none focus:border-brand-500"
            />

            {/* Quick Add CTA */}
            <Link
              href="/meal/scan"
              className="px-3.5 py-2 rounded-xl bg-gradient-to-r from-brand-500 to-tech-teal hover:from-brand-600 text-white text-xs font-bold flex items-center gap-1.5 shadow-glow-sm"
            >
              <Plus className="w-3.5 h-3.5" /> Thêm bữa ăn
            </Link>
          </div>
        </div>

        {/* Undo Floating Toast Notification (Soft-delete feature BR-07, AC-23) */}
        {undoToast && (
          <div className="p-4 rounded-2xl bg-slate-800 border border-brand-500/40 text-white text-xs flex items-center justify-between shadow-glow-md animate-bounce">
            <span className="flex items-center gap-2">
              <CheckCircle2 className="w-4 h-4 text-brand-400" />
              Đã xóa thành công {undoToast.name}.
            </span>
            <button
              onClick={handleUndo}
              className="px-3 py-1 bg-brand-500 hover:bg-brand-600 rounded-lg text-white font-bold text-xs flex items-center gap-1"
            >
              <RotateCcw className="w-3 h-3" /> Hoàn tác
            </button>
          </div>
        )}

        {/* Meals Cards Grid */}
        {meals.length > 0 ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
            {meals.map((meal) => {
              const timeStr = meal.thoiGianBuaAn
                ? new Date(meal.thoiGianBuaAn).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })
                : "--:--";

              return (
                <div
                  key={meal.id}
                  className="p-5 rounded-3xl bg-slate-900/80 border border-slate-800 hover:border-brand-500/30 transition-all flex flex-col justify-between group shadow-card-glass"
                >
                  <div>
                    {/* Image & Header Tags */}
                    <div className="relative w-full h-44 rounded-2xl overflow-hidden bg-slate-800 mb-4">
                      {meal.urlAnh ? (
                        <img
                          src={meal.urlAnh}
                          alt={meal.loaiBuaAn}
                          className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
                        />
                      ) : (
                        <div className="w-full h-full flex items-center justify-center text-slate-500">
                          <Utensils className="w-8 h-8" />
                        </div>
                      )}
                      <div className="absolute top-2.5 left-2.5 px-2.5 py-1 rounded-xl bg-black/60 backdrop-blur-md text-[10px] font-bold text-white border border-white/10 flex items-center gap-1">
                        <Clock className="w-3 h-3 text-brand-400" /> {timeStr}
                      </div>
                      <div className="absolute top-2.5 right-2.5 px-2.5 py-1 rounded-xl bg-brand-500/80 backdrop-blur-md text-[10px] font-bold text-white">
                        {meal.loaiBuaAn}
                      </div>
                    </div>

                    {/* Meal Main Info */}
                    <div className="space-y-1">
                      <h3 className="text-base font-bold text-white flex items-center justify-between">
                        <span>{meal.thanhPhans?.[0]?.tenMon || meal.loaiBuaAn}</span>
                        <span className="text-brand-400 font-extrabold text-sm">{meal.tongCalo} kcal</span>
                      </h3>
                      {meal.ghiChu && (
                        <p className="text-xs text-slate-400 italic line-clamp-1">{meal.ghiChu}</p>
                      )}
                    </div>

                    {/* Macro Breakdown Chips */}
                    <div className="grid grid-cols-3 gap-2 mt-4 pt-3 border-t border-slate-800/80 text-center text-xs">
                      <div className="p-1.5 rounded-lg bg-slate-800/60">
                        <span className="text-[10px] text-indigo-400 block font-semibold">Protein</span>
                        <span className="font-bold text-slate-200">{meal.tongProtein}g</span>
                      </div>
                      <div className="p-1.5 rounded-lg bg-slate-800/60">
                        <span className="text-[10px] text-amber-400 block font-semibold">Carbs</span>
                        <span className="font-bold text-slate-200">{meal.tongCarbs}g</span>
                      </div>
                      <div className="p-1.5 rounded-lg bg-slate-800/60">
                        <span className="text-[10px] text-rose-400 block font-semibold">Fat</span>
                        <span className="font-bold text-slate-200">{meal.tongFat}g</span>
                      </div>
                    </div>
                  </div>

                  {/* Card Actions */}
                  <div className="mt-5 pt-3 border-t border-slate-800 flex items-center justify-between">
                    <button
                      onClick={() => setActiveModalMeal(meal)}
                      className="text-xs font-semibold text-brand-400 hover:text-brand-300 flex items-center gap-1 transition-colors"
                    >
                      <Eye className="w-3.5 h-3.5" /> Chi tiết ({meal.thanhPhans?.length || 1} món)
                    </button>
                    <button
                      onClick={() => handleDelete(meal)}
                      className="p-1.5 text-slate-500 hover:text-rose-400 hover:bg-rose-500/10 rounded-lg transition-colors"
                      title="Xóa bữa ăn"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                </div>
              );
            })}
          </div>
        ) : (
          /* Empty State */
          <div className="p-12 rounded-3xl bg-slate-900/40 border border-slate-800 text-center space-y-4 max-w-md mx-auto">
            <div className="w-16 h-16 rounded-2xl bg-slate-800/80 text-slate-500 flex items-center justify-center mx-auto">
              <Utensils className="w-8 h-8" />
            </div>
            <h3 className="text-base font-bold text-white">Không có bữa ăn nào</h3>
            <p className="text-xs text-slate-400">
              Bạn chưa ghi nhận bữa ăn nào cho ngày {selectedDate}. Hãy chụp ảnh hoặc quét món để AI hỗ trợ phân tích nhé!
            </p>
            <Link
              href="/meal/scan"
              className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl bg-gradient-to-r from-brand-500 to-tech-teal text-white font-bold text-xs shadow-glow-sm"
            >
              <Plus className="w-4 h-4" /> Quét món ăn ngay
            </Link>
          </div>
        )}

        {/* Meal Detail Modal */}
        {activeModalMeal && (
          <div className="fixed inset-0 z-50 bg-black/70 backdrop-blur-sm flex items-center justify-center p-4">
            <div className="w-full max-w-lg rounded-3xl bg-slate-900 border border-slate-800 p-6 space-y-4 shadow-card-glass">
              <div className="flex items-center justify-between pb-3 border-b border-slate-800">
                <h3 className="text-base font-bold text-white flex items-center gap-2">
                  Chi tiết {activeModalMeal.loaiBuaAn}
                </h3>
                <button
                  onClick={() => setActiveModalMeal(null)}
                  className="p-1 text-slate-400 hover:text-white"
                >
                  ✕
                </button>
              </div>

              {activeModalMeal.urlAnh && (
                <div className="w-full h-44 rounded-2xl overflow-hidden bg-black">
                  <img src={activeModalMeal.urlAnh} alt="Meal" className="w-full h-full object-cover" />
                </div>
              )}

              <div className="space-y-2">
                <p className="text-xs font-bold uppercase tracking-wider text-slate-400">
                  Thành phần chi tiết:
                </p>
                <div className="space-y-2 max-h-48 overflow-y-auto">
                  {activeModalMeal.thanhPhans?.map((tp, idx) => (
                    <div
                      key={idx}
                      className="p-3 rounded-xl bg-slate-800/70 border border-slate-700/60 flex items-center justify-between text-xs"
                    >
                      <div>
                        <p className="font-bold text-white">{tp.tenMon}</p>
                        <p className="text-slate-400 text-[11px]">
                          {tp.khoiLuongGam} {tp.donVi || "gam"} • P:{tp.protein}g C:{tp.carbs}g F:{tp.fat}g
                        </p>
                      </div>
                      <span className="font-extrabold text-brand-400">{tp.calo} kcal</span>
                    </div>
                  ))}
                </div>
              </div>

              <div className="pt-3 border-t border-slate-800 flex justify-end">
                <button
                  onClick={() => setActiveModalMeal(null)}
                  className="px-4 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-200 text-xs font-semibold"
                >
                  Đóng
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </AppLayout>
  );
}
