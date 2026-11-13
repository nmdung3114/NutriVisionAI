import type { Metadata } from "next";
import { UsersPage } from "@/features/admin/users-page";

export const metadata: Metadata = { title: "Quản trị – Người dùng" };

export default function Page() {
  return <UsersPage />;
}
