import type { Metadata } from "next";
import { Suspense } from "react";
import { HistoryPage } from "@/features/meals/history-page";

export const metadata: Metadata = { title: "Lịch sử bữa ăn" };

export default function Page() {
  return (
    <Suspense>
      <HistoryPage />
    </Suspense>
  );
}
