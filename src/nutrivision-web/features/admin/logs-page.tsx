"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { PageHeader } from "@/components/shared/page-header";
import { EmptyState } from "@/components/shared/empty-state";
import { ErrorState } from "@/components/shared/error-state";
import { adminApi } from "@/lib/api/admin";
import { useLogTypes } from "@/hooks/use-api";
import { useDebounced } from "@/hooks/use-debounced";
import { formatDateTime } from "@/lib/date";
import { Pagination } from "./pagination";

const GUID = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

export function LogsPage() {
  const [loai, setLoai] = useState("all");
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [userId, setUserId] = useState("");
  const [page, setPage] = useState(1);
  const types = useLogTypes();
  const duser = useDebounced(userId.trim(), 400);
  const userInvalid = !!duser && !GUID.test(duser);

  const params = { loai: loai === "all" ? undefined : loai, from: from || undefined, to: to || undefined, userId: duser && !userInvalid ? duser : undefined, page, pageSize: 30 };
  const { data, error, isLoading, isFetching, refetch } = useQuery({ queryKey: ["admin", "logs", params], queryFn: () => adminApi.logs(params), placeholderData: (p) => p });

  return (
    <div>
      <PageHeader title="Nhật ký hệ thống" description="Theo dõi các hành động quan trọng của người dùng và quản trị viên." />
      <div className="mb-4 grid gap-3 rounded-2xl border bg-card p-3 shadow-soft sm:grid-cols-2 lg:grid-cols-4">
        <Select value={loai} onValueChange={(v) => { setLoai(v); setPage(1); }}>
          <SelectTrigger className="h-9 w-full" aria-label="Loại hành động"><SelectValue /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Mọi loại hành động</SelectItem>
            {(types.data ?? []).map((t) => <SelectItem key={t} value={t}>{t}</SelectItem>)}
          </SelectContent>
        </Select>
        <Input type="date" aria-label="Từ ngày" value={from} onChange={(e) => { setFrom(e.target.value); setPage(1); }} className="h-9" />
        <Input type="date" aria-label="Đến ngày" value={to} onChange={(e) => { setTo(e.target.value); setPage(1); }} className="h-9" />
        <div>
          <Input value={userId} onChange={(e) => { setUserId(e.target.value); setPage(1); }} placeholder="ID người dùng (GUID)" aria-label="ID người dùng" aria-invalid={userInvalid} className="h-9" />
          {userInvalid && <p className="mt-1 text-xs text-destructive">ID không đúng định dạng GUID</p>}
        </div>
      </div>

      {isLoading ? (
        <Skeleton className="h-80 rounded-2xl" />
      ) : error ? (
        <ErrorState error={error} onRetry={() => void refetch()} />
      ) : !data?.items.length ? (
        <EmptyState title="Không có nhật ký phù hợp" description="Hãy thử nới rộng bộ lọc." />
      ) : (
        <div className={isFetching ? "opacity-70" : ""}>
          <div className="overflow-x-auto rounded-2xl border bg-card shadow-soft">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Thời gian</TableHead>
                  <TableHead>Người thực hiện</TableHead>
                  <TableHead>Hành động</TableHead>
                  <TableHead>Đối tượng</TableHead>
                  <TableHead>Chi tiết</TableHead>
                  <TableHead>IP</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.items.map((l) => (
                  <TableRow key={l.id}>
                    <TableCell className="whitespace-nowrap text-muted-foreground">{formatDateTime(l.ngayTao)}</TableCell>
                    <TableCell>{l.emailNguoiThucHien ?? "Hệ thống"}</TableCell>
                    <TableCell className="font-semibold">{l.loaiHanhDong}</TableCell>
                    <TableCell className="text-muted-foreground">{l.bangTacDong ?? "–"}</TableCell>
                    <TableCell className="max-w-xs truncate" title={l.chiTiet ?? undefined}>{l.chiTiet ?? "–"}</TableCell>
                    <TableCell className="text-muted-foreground">{l.diaChiIP ?? "–"}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
          <Pagination page={data.page} totalPages={data.totalPages} total={data.totalCount} onPage={setPage} />
        </div>
      )}
    </div>
  );
}
