"use client";

import React from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  LayoutDashboard,
  ScanLine,
  UtensilsCrossed,
  BarChart3,
  UserCheck,
  ShieldCheck,
  LogOut,
  Sparkles,
  Flame,
} from "lucide-react";
import { useAuth } from "@/context/AuthContext";

export const Sidebar: React.FC = () => {
  const pathname = usePathname();
  const { user, logout, isAdmin } = useAuth();

  const navItems = [
    {
      title: "Tổng quan",
      href: "/dashboard",
      icon: LayoutDashboard,
    },
    {
      title: "Quét món AI",
      href: "/meal/scan",
      icon: ScanLine,
      badge: "AI Vision",
    },
    {
      title: "Nhật ký bữa ăn",
      href: "/meal/history",
      icon: UtensilsCrossed,
    },
    {
      title: "Báo cáo phân tích",
      href: "/reports",
      icon: BarChart3,
    },
    {
      title: "Hồ sơ & Mục tiêu",
      href: "/profile",
      icon: UserCheck,
    },
  ];

  if (isAdmin || user?.vaiTro === "ADMIN") {
    navItems.push({
      title: "Quản trị viên",
      href: "/admin",
      icon: ShieldCheck,
      badge: "Admin",
    });
  }

  return (
    <aside className="w-64 h-screen fixed left-0 top-0 z-40 bg-[#0c1220]/95 backdrop-blur-xl border-r border-slate-800/80 flex flex-col justify-between p-4 transition-all duration-300">
      {/* Brand Header */}
      <div>
        <div className="flex items-center gap-3 px-2 py-4 mb-6 border-b border-slate-800/80">
          <div className="relative flex items-center justify-center w-11 h-11 rounded-xl bg-gradient-to-tr from-brand-600 to-tech-cyan text-white shadow-glow-sm">
            <Sparkles className="w-6 h-6 animate-pulse" />
            <div className="absolute -bottom-1 -right-1 w-3.5 h-3.5 bg-brand-400 border-2 border-[#0c1220] rounded-full"></div>
          </div>
          <div>
            <h1 className="text-lg font-bold tracking-tight text-white flex items-center gap-1.5">
              NutriVision<span className="text-brand-400 text-xs px-1.5 py-0.5 rounded bg-brand-500/10 border border-brand-500/30">AI</span>
            </h1>
            <p className="text-xs text-slate-400">Smart Health & Diet</p>
          </div>
        </div>

        {/* Navigation Items */}
        <nav className="space-y-1.5">
          {navItems.map((item) => {
            const isActive = pathname === item.href || (item.href !== "/dashboard" && pathname?.startsWith(item.href));
            const Icon = item.icon;

            return (
              <Link
                key={item.href}
                href={item.href}
                className={`group flex items-center justify-between px-3.5 py-2.5 rounded-xl text-sm font-medium transition-all duration-200 ${
                  isActive
                    ? "bg-gradient-to-r from-brand-600/20 to-tech-cyan/10 text-brand-300 border border-brand-500/30 shadow-glow-sm"
                    : "text-slate-400 hover:text-slate-100 hover:bg-slate-800/50"
                }`}
              >
                <div className="flex items-center gap-3">
                  <Icon
                    className={`w-5 h-5 transition-colors ${
                      isActive ? "text-brand-400" : "text-slate-400 group-hover:text-slate-200"
                    }`}
                  />
                  <span>{item.title}</span>
                </div>
                {item.badge && (
                  <span
                    className={`text-[10px] px-2 py-0.5 rounded-full font-semibold ${
                      item.badge === "Admin"
                        ? "bg-rose-500/20 text-rose-300 border border-rose-500/30"
                        : "bg-brand-500/20 text-brand-300 border border-brand-500/30"
                    }`}
                  >
                    {item.badge}
                  </span>
                )}
              </Link>
            );
          })}
        </nav>
      </div>

      {/* Sidebar Footer: Calorie Target Card & User Profile */}
      <div className="space-y-3">
        {/* Quick Calorie Target Mini Widget */}
        <div className="p-3.5 rounded-2xl bg-gradient-to-br from-slate-900/90 to-slate-800/60 border border-slate-700/50">
          <div className="flex items-center justify-between text-xs text-slate-300 mb-1.5 font-medium">
            <span className="flex items-center gap-1.5">
              <Flame className="w-4 h-4 text-amber-400" /> Mục tiêu hôm nay
            </span>
            <span className="text-brand-400 font-bold">{user?.caloMucTieu || 2200} kcal</span>
          </div>
          <div className="w-full bg-slate-800 h-2 rounded-full overflow-hidden">
            <div
              className="h-full bg-gradient-to-r from-brand-500 to-tech-cyan rounded-full transition-all duration-500"
              style={{ width: "68%" }}
            ></div>
          </div>
          <p className="text-[11px] text-slate-400 mt-1.5 text-right">Đã nạp 1,496 kcal (68%)</p>
        </div>

        {/* User Mini Profile & Logout */}
        <div className="flex items-center justify-between p-2 rounded-xl bg-slate-900/60 border border-slate-800">
          <div className="flex items-center gap-2.5 overflow-hidden">
            <div className="w-8 h-8 rounded-full bg-brand-600/30 border border-brand-500/40 flex items-center justify-center font-bold text-xs text-brand-300">
              {user?.hoTen?.charAt(0) || "U"}
            </div>
            <div className="overflow-hidden">
              <p className="text-xs font-semibold text-slate-200 truncate">{user?.hoTen || "Người dùng"}</p>
              <p className="text-[10px] text-slate-400 truncate">{user?.email || "user@nutrivision.ai"}</p>
            </div>
          </div>
          <button
            onClick={logout}
            title="Đăng xuất"
            className="p-1.5 text-slate-400 hover:text-rose-400 hover:bg-rose-500/10 rounded-lg transition-colors"
          >
            <LogOut className="w-4 h-4" />
          </button>
        </div>
      </div>
    </aside>
  );
};
