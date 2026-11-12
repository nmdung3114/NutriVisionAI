import Link from "next/link";
import { Button } from "@/components/ui/button";
import { PlateIllustration } from "@/components/shared/empty-state";

export default function NotFound() {
  return (
    <div className="flex min-h-dvh flex-col items-center justify-center px-4 text-center">
      <PlateIllustration className="h-36 w-48" />
      <p className="mt-4 text-6xl font-extrabold text-primary">404</p>
      <h1 className="mt-2 text-2xl font-extrabold">Không tìm thấy trang bạn cần</h1>
      <p className="mt-1 text-muted-foreground">Trang có thể đã bị xóa hoặc đường dẫn không chính xác.</p>
      <Button asChild className="mt-6"><Link href="/">Về trang chủ</Link></Button>
    </div>
  );
}
