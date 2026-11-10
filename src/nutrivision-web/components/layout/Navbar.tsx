"use client";

import React, { useState, useEffect } from "react";
import Link from "next/link";
import { Camera, Calendar, Bell, CheckCircle2, ChevronDown } from "lucide-react";
import { useAuth } from "@/context/AuthContext";

export const Navbar: React.FC = () => {
  const { user } = useAuth();
  const [greeting, setGreeting] = useState("Xin chào");
  const [currentDateStr, setCurrentDateStr] = useState("");

  useEffect(() => {
    const hour = new Date().getHours();
    if (hour >= 5 && hour < 11) setGreeting("Chào buổi sáng năng lượng ☀️");
    else if (hour >= 11 && hour < 14) setGreeting("Bữa trưa ngon miệng & đủ chất 🥗");
    else if (hour >= 14 && hour < 18) setGreeting("Nạp năng lượng cho buổi chiều ⚡");
    else setGreeting("Buổi tối thư thái & lành mạnh 🌙");

    const today = new Date();
    const days = ["Chủ Nhật", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy"];
    setCurrentDateStr(`${days[today.getDay()]}, ${today.getDate()} thg ${today.getMonth() + 1}, ${today.getFullYear()}`);
  }, []);

  return (
    <header className="h-16 fixed top-0 right-0 left-64 z-30 bg-[#090d16]/80 backdrop-blur-xl border-b border-slate-800/80 px-6 flex items-center justify-between">
      {/* Realtime Greeting & Date */}
      <div>
        <h2 className="text-sm font-semibold text-white flex items-center gap-2">
          {greeting}, <span className="text-brand-400">{user?.hoTen?.split(" ").pop() || "bạn"}</span>!
        </h2>
        <p className="text-xs text-slate-400 flex items-center gap-1.5 mt-0.5">
          <Calendar className="w-3.5 h-3.5 text-slate-500" /> {currentDateStr}
        </p>
      </div>

      {/* Action Buttons & Status */}
      <div className="flex items-center gap-3.5">
        {/* Backend Connected Badge */}
        <div className="hidden md:flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-emerald-500/10 border border-emerald-500/20 text-emerald-400 text-xs font-medium">
          <span className="w-2 h-2 rounded-full bg-emerald-400 animate-pulse"></span>
          AI Vision Engine: Sẵn sàng
        </div>

        {/* CTA Scan Button */}
        <Link
          href="/meal/scan"
          className="flex items-center gap-2 px-3.5 py-1.5 rounded-xl bg-gradient-to-r from-brand-500 to-tech-teal hover:from-brand-600 hover:to-tech-teal/90 text-white text-xs font-semibold shadow-glow-sm hover:shadow-glow-md transition-all duration-200"
        >
          <Camera className="w-3.5 h-3.5" />
          <span>Quét món ngay</span>
        </Link>

        {/* Notification Bell */}
        <button
          title="Thông báo dinh dưỡng"
          className="relative p-2 rounded-xl text-slate-400 hover:text-white hover:bg-slate-800/70 transition-colors"
        >
          <Bell className="w-4 h-4" />
          <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-brand-500 rounded-full ring-2 ring-[#090d16]"></span>
        </button>

        {/* User Mini Profile dropdown trigger */}
        <Link
          href="/profile"
          className="flex items-center gap-2 pl-2 border-l border-slate-800 hover:opacity-90 transition-opacity"
        >
          <div className="w-8 h-8 rounded-full bg-gradient-to-tr from-brand-600 to-tech-cyan text-white flex items-center justify-center font-bold text-xs ring-2 ring-brand-500/30">
            {user?.hoTen?.charAt(0) || "U"}
          </div>
          <div className="hidden lg:block text-left">
            <p className="text-xs font-medium text-slate-200 leading-none">{user?.hoTen || "Người dùng"}</p>
            <p className="text-[10px] text-brand-400 font-semibold mt-1 uppercase">{user?.vaiTro || "USER"}</p>
          </div>
          <ChevronDown className="w-3 h-3 text-slate-400 hidden lg:block" />
        </Link>
      </div>
    </header>
  );
};
