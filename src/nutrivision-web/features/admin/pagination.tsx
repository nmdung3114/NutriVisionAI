import { ChevronLeft, ChevronRight } from "lucide-react";
import { Button } from "@/components/ui/button";

export function Pagination({ page, totalPages, total, onPage }: { page: number; totalPages: number; total: number; onPage: (p: number) => void }) {
  return (
    <nav aria-label="Phân trang" className="mt-4 flex flex-wrap items-center justify-between gap-3">
      <span className="text-sm text-muted-foreground">Tổng {total.toLocaleString("vi-VN")} bản ghi</span>
      <div className="flex items-center gap-3">
        <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => onPage(page - 1)}>
          <ChevronLeft /> Trước
        </Button>
        <span className="text-sm">
          Trang <b>{page}</b> / {Math.max(totalPages, 1)}
        </span>
        <Button variant="outline" size="sm" disabled={page >= totalPages} onClick={() => onPage(page + 1)}>
          Sau <ChevronRight />
        </Button>
      </div>
    </nav>
  );
}
