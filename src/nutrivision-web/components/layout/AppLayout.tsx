"use client";

import React from "react";
import { Sidebar } from "./Sidebar";
import { Navbar } from "./Navbar";
import { AuthProvider } from "@/context/AuthContext";

interface AppLayoutProps {
  children: React.ReactNode;
}

export const AppLayout: React.FC<AppLayoutProps> = ({ children }) => {
  return (
    <AuthProvider>
      <div className="min-h-screen bg-[#090d16] text-slate-100 flex">
        {/* Fixed Left Sidebar */}
        <Sidebar />

        {/* Main Content Area */}
        <div className="flex-1 ml-64 flex flex-col min-h-screen">
          <Navbar />
          <main className="flex-1 pt-20 px-8 pb-12 overflow-y-auto">
            {children}
          </main>
        </div>
      </div>
    </AuthProvider>
  );
};
