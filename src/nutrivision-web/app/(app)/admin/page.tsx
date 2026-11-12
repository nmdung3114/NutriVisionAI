import type { Metadata } from "next";
import { AdminStatsPage } from "@/features/admin/stats-page";

export const metadata: Metadata = { title: "Quản trị – Thống kê" };

export default function Page() {
  return <AdminStatsPage />;
}
