import type { Metadata } from "next";
import { HistoryPage } from "@/features/meals/history-page";

export const metadata: Metadata = { title: "Lịch sử bữa ăn" };

export default function Page() {
  return <HistoryPage />;
}
