import type { Metadata } from "next";
import { ReportsPage } from "@/features/reports/reports-page";

export const metadata: Metadata = { title: "Báo cáo dinh dưỡng" };

export default function Page() {
  return <ReportsPage />;
}
