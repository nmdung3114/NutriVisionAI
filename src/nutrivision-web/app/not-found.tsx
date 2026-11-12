import Link from "next/link";
import { Button } from "@/components/ui/button";
import { EmptyState } from "@/components/shared/empty-state";

export default function NotFound() {
  return (
    <div className="flex min-h-dvh items-center justify-center px-4">
      <EmptyState
        illustration="search"
        className="w-full max-w-lg border-none bg-transparent"
        title="404 · Không tìm thấy trang bạn cần"
        description="Trang có thể đã bị xóa hoặc đường dẫn không chính xác."
        action={
          <Button asChild>
            <Link href="/">Về trang chủ</Link>
          </Button>
        }
      />
    </div>
  );
}
