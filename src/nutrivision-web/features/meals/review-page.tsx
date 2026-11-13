"use client";

import { useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { PageHeader } from "@/components/shared/page-header";
import { PageSkeleton } from "@/components/layout/app-shell";
import { ErrorState } from "@/components/shared/error-state";
import { useMeal } from "@/hooks/use-api";
import { isMealType } from "@/lib/meal";
import { MealEditor } from "./editor/meal-editor";
import { scanNoticeKey } from "./scan-page";

function ManualMode() {
  const q = useSearchParams().get("loai");
  return (
    <>
      <PageHeader title="Nhập tay món ăn" description="Nhập tên món, thành phần và khẩu phần – hệ thống sẽ tính dinh dưỡng giúp bạn." />
      <MealEditor mode="manual" initialLoai={isMealType(q) ? q : "Trua"} />
    </>
  );
}

function DraftMode({ id }: { id: string }) {
  const router = useRouter();
  const { data, error, isLoading, refetch } = useMeal(id);
  const [notice, setNotice] = useState<string | null | undefined>(undefined);

  useEffect(() => {
    try {
      setNotice(sessionStorage.getItem(scanNoticeKey(id)));
    } catch {
      setNotice(null);
    }
  }, [id]);

  useEffect(() => {
    if (data?.daXacNhan) router.replace(`/meals/${id}`);
  }, [data, id, router]);

  if (isLoading || notice === undefined) return <PageSkeleton />;
  if (error || !data) return <ErrorState error={error} onRetry={() => void refetch()} />;
  if (data.daXacNhan) return <PageSkeleton />;
  return (
    <>
      <PageHeader title="Kiểm tra và hiệu chỉnh" description="Bước 2/2 · Xem lại kết quả nhận diện, chỉnh sửa trước khi lưu chính thức." />
      <MealEditor mode="draft" meal={data} failedNotice={data.aiThatBai ? (data.thongBaoAI ?? notice ?? "AI không nhận diện được món ăn.") : notice} />
    </>
  );
}

export function ReviewPage({ id }: { id: string }) {
  return id === "new" ? <ManualMode /> : <DraftMode id={id} />;
}
