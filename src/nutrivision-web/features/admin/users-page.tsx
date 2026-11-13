"use client";

import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Lock, Search, ShieldCheck, Unlock, UserCog } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from "@/components/ui/alert-dialog";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { PageHeader } from "@/components/shared/page-header";
import { EmptyState } from "@/components/shared/empty-state";
import { ErrorState } from "@/components/shared/error-state";
import { adminApi } from "@/lib/api/admin";
import { errorMessage } from "@/lib/api/errors";
import { useDebounced } from "@/hooks/use-debounced";
import { formatDate } from "@/lib/date";
import { useAuth } from "@/providers/auth-provider";
import type { AdminUser, Role } from "@/types/api";
import { Pagination } from "./pagination";

const STATUS: Record<string, { label: string; cls: string }> = {
  ACTIVE: { label: "Hoạt động", cls: "bg-brand-100 text-brand-700" },
  PENDING: { label: "Chờ xác thực", cls: "bg-yellow/30 text-warn" },
  LOCKED: { label: "Đã khóa", cls: "bg-red-100 text-red-700" },
};

export function UsersPage() {
  const { user: me } = useAuth();
  const qc = useQueryClient();
  const [q, setQ] = useState("");
  const [trangThai, setTrangThai] = useState("all");
  const [vaiTro, setVaiTro] = useState("all");
  const [page, setPage] = useState(1);
  const [lockTarget, setLockTarget] = useState<AdminUser | null>(null);
  const [roleTarget, setRoleTarget] = useState<AdminUser | null>(null);
  const [newRole, setNewRole] = useState<Role>("USER");
  const dq = useDebounced(q.trim(), 350);

  const params = { q: dq || undefined, trangThai: trangThai === "all" ? undefined : trangThai, vaiTro: vaiTro === "all" ? undefined : vaiTro, page, pageSize: 20 };
  const { data, error, isLoading, isFetching, refetch } = useQuery({ queryKey: ["admin", "users", params], queryFn: () => adminApi.users(params), placeholderData: (p) => p });

  const done = (msg: string) => {
    void qc.invalidateQueries({ queryKey: ["admin"] });
    toast.success(msg);
    setLockTarget(null);
    setRoleTarget(null);
  };
  const status = useMutation({
    mutationFn: (u: AdminUser) => adminApi.setStatus(u.id, u.trangThai === "LOCKED" ? "ACTIVE" : "LOCKED"),
    onSuccess: (u) => done(u.trangThai === "LOCKED" ? "Đã khóa tài khoản" : "Đã mở khóa tài khoản"),
    onError: (e) => { toast.error(errorMessage(e)); setLockTarget(null); },
  });
  const role = useMutation({
    mutationFn: (v: { u: AdminUser; r: Role }) => adminApi.setRole(v.u.id, v.r),
    onSuccess: () => done("Đã đổi vai trò"),
    onError: (e) => { toast.error(errorMessage(e)); setRoleTarget(null); },
  });

  const selfHint = "Bạn không thể thao tác trên chính tài khoản của mình";

  return (
    <div>
      <PageHeader title="Quản lý người dùng" description="Tìm kiếm, khóa/mở khóa tài khoản và phân quyền." />
      <div className="mb-4 flex flex-col gap-3 rounded-2xl border bg-card p-3 shadow-soft sm:flex-row">
        <div className="relative flex-1">
          <Search className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
          <Input value={q} onChange={(e) => { setQ(e.target.value); setPage(1); }} placeholder="Tìm theo tên hoặc email…" className="h-9 pl-9" aria-label="Tìm người dùng" />
        </div>
        <Select value={trangThai} onValueChange={(v) => { setTrangThai(v); setPage(1); }}>
          <SelectTrigger className="h-9 w-full sm:w-44" aria-label="Lọc trạng thái"><SelectValue /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Mọi trạng thái</SelectItem>
            <SelectItem value="ACTIVE">Hoạt động</SelectItem>
            <SelectItem value="PENDING">Chờ xác thực</SelectItem>
            <SelectItem value="LOCKED">Đã khóa</SelectItem>
          </SelectContent>
        </Select>
        <Select value={vaiTro} onValueChange={(v) => { setVaiTro(v); setPage(1); }}>
          <SelectTrigger className="h-9 w-full sm:w-40" aria-label="Lọc vai trò"><SelectValue /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Mọi vai trò</SelectItem>
            <SelectItem value="USER">Người dùng</SelectItem>
            <SelectItem value="ADMIN">Quản trị viên</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {isLoading ? (
        <Skeleton className="h-72 rounded-2xl" />
      ) : error ? (
        <ErrorState error={error} onRetry={() => void refetch()} />
      ) : !data?.items.length ? (
        <EmptyState illustration="search" title="Không tìm thấy người dùng" description="Hãy thử thay đổi từ khóa hoặc bộ lọc." />
      ) : (
        <div className={isFetching ? "opacity-70" : ""}>
          <div className="overflow-x-auto rounded-2xl border bg-card shadow-soft">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Người dùng</TableHead>
                  <TableHead>Vai trò</TableHead>
                  <TableHead>Trạng thái</TableHead>
                  <TableHead>Ngày tạo</TableHead>
                  <TableHead className="text-right">Thao tác</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.items.map((u) => {
                  const self = u.id === me?.id;
                  const st = STATUS[u.trangThai] ?? STATUS.PENDING;
                  const locked = u.trangThai === "LOCKED";
                  return (
                    <TableRow key={u.id}>
                      <TableCell>
                        <p className="font-semibold">{u.hoTen} {self && <span className="text-xs text-primary">(Bạn)</span>}</p>
                        <p className="text-xs text-muted-foreground">{u.email}</p>
                      </TableCell>
                      <TableCell>
                        <Badge variant="outline" className="gap-1 rounded-full">{u.vaiTro === "ADMIN" && <ShieldCheck className="size-3" />}{u.vaiTro === "ADMIN" ? "Quản trị" : "Người dùng"}</Badge>
                      </TableCell>
                      <TableCell><Badge className={`rounded-full ${st.cls}`}>{st.label}</Badge></TableCell>
                      <TableCell className="text-muted-foreground">{formatDate(u.ngayTao)}</TableCell>
                      <TableCell>
                        <div className="flex justify-end gap-2">
                          <Tooltip>
                            <TooltipTrigger asChild>
                              <span tabIndex={self ? 0 : -1}>
                                <Button size="sm" variant="outline" disabled={self || u.trangThai === "PENDING"} onClick={() => setLockTarget(u)}>
                                  {locked ? <Unlock /> : <Lock />} {locked ? "Mở khóa" : "Khóa"}
                                </Button>
                              </span>
                            </TooltipTrigger>
                            {(self || u.trangThai === "PENDING") && <TooltipContent>{self ? selfHint : "Tài khoản chưa xác thực email"}</TooltipContent>}
                          </Tooltip>
                          <Tooltip>
                            <TooltipTrigger asChild>
                              <span tabIndex={self ? 0 : -1}>
                                <Button size="sm" variant="outline" disabled={self} onClick={() => { setRoleTarget(u); setNewRole(u.vaiTro === "ADMIN" ? "USER" : "ADMIN"); }}>
                                  <UserCog /> Đổi vai trò
                                </Button>
                              </span>
                            </TooltipTrigger>
                            {self && <TooltipContent>{selfHint}</TooltipContent>}
                          </Tooltip>
                        </div>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </div>
          <Pagination page={data.page} totalPages={data.totalPages} total={data.totalCount} onPage={setPage} />
        </div>
      )}

      <AlertDialog open={!!lockTarget} onOpenChange={(o) => !o && setLockTarget(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{lockTarget?.trangThai === "LOCKED" ? "Mở khóa tài khoản?" : "Khóa tài khoản?"}</AlertDialogTitle>
            <AlertDialogDescription>
              {lockTarget?.trangThai === "LOCKED" ? `${lockTarget?.email} sẽ có thể đăng nhập trở lại.` : `${lockTarget?.email} sẽ bị đăng xuất và không thể đăng nhập cho tới khi được mở khóa.`}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Hủy</AlertDialogCancel>
            <AlertDialogAction onClick={(e) => { e.preventDefault(); if (lockTarget) status.mutate(lockTarget); }}>{status.isPending ? "Đang xử lý…" : "Xác nhận"}</AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <Dialog open={!!roleTarget} onOpenChange={(o) => !o && setRoleTarget(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Đổi vai trò</DialogTitle>
            <DialogDescription>{roleTarget?.hoTen} ({roleTarget?.email})</DialogDescription>
          </DialogHeader>
          <Select value={newRole} onValueChange={(v) => setNewRole(v as Role)}>
            <SelectTrigger aria-label="Vai trò mới" className="w-full"><SelectValue /></SelectTrigger>
            <SelectContent>
              <SelectItem value="USER">Người dùng</SelectItem>
              <SelectItem value="ADMIN">Quản trị viên</SelectItem>
            </SelectContent>
          </Select>
          <DialogFooter>
            <Button variant="outline" onClick={() => setRoleTarget(null)}>Hủy</Button>
            <Button disabled={role.isPending || newRole === roleTarget?.vaiTro} onClick={() => roleTarget && role.mutate({ u: roleTarget, r: newRole })}>
              {role.isPending ? "Đang lưu…" : "Lưu vai trò"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
