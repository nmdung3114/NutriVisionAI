import type { Metadata } from "next";
import { Suspense } from "react";
import { MealDetailPage } from "@/features/meals/detail-page";

export const metadata: Metadata = { title: "Chi tiết bữa ăn" };

export default async function Page({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  return (
    <Suspense>
      <MealDetailPage id={id} />
    </Suspense>
  );
}
