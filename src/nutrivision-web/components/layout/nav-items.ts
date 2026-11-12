import { BarChart3, ClipboardList, LayoutDashboard, PlusCircle, ShieldCheck, ScrollText, UserCircle, Users, type LucideIcon } from "lucide-react";

export interface NavItem {
  href: string;
  label: string;
  icon: LucideIcon;
  /** Chỉ active khi khớp chính xác đường dẫn. */
  exact?: boolean;
}

export const USER_NAV: NavItem[] = [
  { href: "/dashboard", label: "Dashboard", icon: LayoutDashboard },
  { href: "/meals/new", label: "Thêm bữa ăn", icon: PlusCircle },
  { href: "/meals", label: "Lịch sử", icon: ClipboardList, exact: true },
  { href: "/reports", label: "Báo cáo", icon: BarChart3 },
  { href: "/profile", label: "Hồ sơ", icon: UserCircle },
];

export const ADMIN_NAV: NavItem[] = [
  { href: "/admin", label: "Thống kê hệ thống", icon: ShieldCheck, exact: true },
  { href: "/admin/users", label: "Người dùng", icon: Users },
  { href: "/admin/logs", label: "Nhật ký hệ thống", icon: ScrollText },
];

export function isActive(pathname: string, item: NavItem): boolean {
  if (item.exact) return pathname === item.href || (item.href === "/meals" && /^\/meals\/(?!new|review)[^/]+/.test(pathname));
  return pathname === item.href || pathname.startsWith(item.href + "/");
}
