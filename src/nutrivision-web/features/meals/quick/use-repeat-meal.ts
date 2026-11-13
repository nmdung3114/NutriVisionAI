"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { mealsApi } from "@/lib/api/meals";
import { errorMessage } from "@/lib/api/errors";
import { mealTypeLabel } from "@/lib/meal";
import type { LoaiBuaAn } from "@/types/api";

const KEYS = ["dashboard", "meals", "report", "suggestions"];

/** "Ăn lại" một bữa: tạo bữa mới hôm nay, toast có nút Hoàn tác (xóa bữa vừa tạo). */
export function useRepeatMeal() {
  const qc = useQueryClient();
  const refresh = () => KEYS.forEach((k) => void qc.invalidateQueries({ queryKey: [k] }));
  return useMutation({
    mutationFn: ({ id, loaiBuaAn }: { id: string; loaiBuaAn?: LoaiBuaAn }) => mealsApi.repeat(id, { loaiBuaAn }),
    onSuccess: (meal) => {
      refresh();
      toast.success(`Đã ghi "${meal.tenMonAn}" vào ${mealTypeLabel(meal.loaiBuaAn).toLowerCase()} hôm nay`, {
        description: `${Math.round(meal.tongDinhDuong.calo).toLocaleString("vi-VN")} kcal`,
        action: {
          label: "Hoàn tác",
          onClick: () =>
            void mealsApi
              .remove(meal.id)
              .then(() => {
                refresh();
                toast.info("Đã hoàn tác");
              })
              .catch((e) => toast.error(errorMessage(e))),
        },
      });
    },
    onError: (e) => toast.error(errorMessage(e)),
  });
}
