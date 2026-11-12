"use client";

import { PageHeader } from "@/components/shared/page-header";
import { WeightCard } from "./weight-card";

export function TrackingPage() {
  return (
    <div className="space-y-6">
      <PageHeader title="Cân nặng & nước uống" description="Theo dõi cân nặng theo thời gian và lượng nước uống mỗi ngày." />
      <WeightCard />
    </div>
  );
}
