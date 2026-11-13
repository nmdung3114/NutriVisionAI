import type { Metadata } from "next";
import { Suspense } from "react";
import { ScanPage } from "@/features/meals/scan-page";

export const metadata: Metadata = { title: "Thêm bữa ăn" };

export default function Page() {
  return (
    <Suspense>
      <ScanPage />
    </Suspense>
  );
}
