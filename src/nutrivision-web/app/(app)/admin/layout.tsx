import { AuthGuard } from "@/components/layout/app-shell";

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  return <AuthGuard role="ADMIN">{children}</AuthGuard>;
}
