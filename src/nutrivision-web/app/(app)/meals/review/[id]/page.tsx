import type { Metadata } from "next";
import { Suspense } from "react";
import { ReviewPage } from "@/features/meals/review-page";

export const metadata: Metadata = { title: "Hiệu chỉnh bữa ăn" };

export default async function Page({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  return (
    <Suspense>
      <ReviewPage id={id} />
    </Suspense>
  );
}
