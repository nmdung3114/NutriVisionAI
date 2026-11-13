"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from "@/components/ui/alert-dialog";
import { mealsApi } from "@/lib/api/meals";
import { errorMessage } from "@/lib/api/errors";

export function DeleteMealDialog({ meal, onClose, onDeleted }: { meal: { id: string; tenMonAn: string } | null; onClose: () => void; onDeleted?: () => void }) {
  const qc = useQueryClient();
  const del = useMutation({
    mutationFn: (id: string) => mealsApi.remove(id),
    onSuccess: () => {
      for (const k of ["meals", "dashboard", "report", "suggestions"]) void qc.invalidateQueries({ queryKey: [k] });
      toast.success("Đã xóa bữa ăn");
      onClose();
      onDeleted?.();
    },
    onError: (e) => {
      toast.error(errorMessage(e));
      onClose();
    },
  });
  return (
    <AlertDialog open={!!meal} onOpenChange={(o) => !o && onClose()}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Xóa bữa ăn này?</AlertDialogTitle>
          <AlertDialogDescription>
            &quot;{meal?.tenMonAn}&quot; sẽ bị xóa khỏi nhật ký và tổng hợp dinh dưỡng của ngày hôm đó được tính lại.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Giữ lại</AlertDialogCancel>
          <AlertDialogAction
            className="bg-destructive text-white hover:bg-destructive/90"
            onClick={(e) => {
              e.preventDefault();
              if (meal) del.mutate(meal.id);
            }}
          >
            {del.isPending ? "Đang xóa…" : "Xóa bữa ăn"}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
