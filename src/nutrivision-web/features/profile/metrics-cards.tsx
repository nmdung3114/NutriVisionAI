"use client";

import { Activity, Flame, Scale } from "lucide-react";
import { StatCard } from "@/components/shared/stat-card";
import { Badge } from "@/components/ui/badge";
import { bmiClass } from "@/lib/health";
import { fmt } from "@/lib/meal";

export function MetricsCards({ bmi, bmr, tdee, preview }: { bmi: number | null; bmr: number | null; tdee: number | null; preview: boolean }) {
  const cls = bmi !== null ? bmiClass(bmi) : null;
  return (
    <div>
      <div className="grid gap-4 sm:grid-cols-3">
        <StatCard label="Chỉ số BMI" value={bmi !== null ? fmt(bmi, 1) : "–"} icon={<Scale />} tone="blue" badge={cls && <Badge className={`rounded-full ${cls.bg}`}>{cls.label}</Badge>} hint="Chỉ số khối cơ thể (WHO)" />
        <StatCard label="BMR" value={bmr !== null ? fmt(bmr) : "–"} unit="kcal" icon={<Activity />} tone="purple" hint="Năng lượng cơ bản khi nghỉ ngơi" />
        <StatCard label="TDEE" value={tdee !== null ? fmt(tdee) : "–"} unit="kcal" icon={<Flame />} tone="orange" hint="Năng lượng tiêu hao mỗi ngày" />
      </div>
      {preview && <p className="mt-2 text-xs text-muted-foreground">Đang hiển thị giá trị ước tính theo thông tin bạn nhập. Bấm &quot;Lưu hồ sơ&quot; để cập nhật chính thức.</p>}
    </div>
  );
}
