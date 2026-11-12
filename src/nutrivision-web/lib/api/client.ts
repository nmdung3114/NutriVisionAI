import { API_URL } from "@/lib/config";
import { tokenStore } from "@/lib/auth/token-store";
import { ApiError } from "./errors";
import type { ApiEnvelope, AuthResult } from "@/types/api";

type Query = Record<string, string | number | boolean | null | undefined>;

export interface RequestOptions {
  method?: "GET" | "POST" | "PUT" | "DELETE";
  query?: Query;
  body?: unknown;
  formData?: FormData;
  /** Có gửi Bearer hay không (mặc định: có). */
  auth?: boolean;
  signal?: AbortSignal;
  timeoutMs?: number;
}

const DEFAULT_TIMEOUT = 30_000;

/* ---------- Xử lý mất phiên (AuthProvider đăng ký) ---------- */
let onAuthLost: (() => void) | null = null;
export function setAuthLostHandler(fn: (() => void) | null) {
  onAuthLost = fn;
}

export function buildUrl(path: string, query?: Query): string {
  const url = new URL(path.startsWith("http") ? path : `${API_URL}${path}`);
  if (query) {
    for (const [k, v] of Object.entries(query)) {
      if (v === undefined || v === null || v === "") continue;
      url.searchParams.set(k, String(v));
    }
  }
  return url.toString();
}

/* ---------- Refresh token (single-flight) ---------- */
let refreshPromise: Promise<boolean> | null = null;

export function refreshSession(): Promise<boolean> {
  if (refreshPromise) return refreshPromise;
  const refreshToken = tokenStore.getRefresh();
  if (!refreshToken) return Promise.resolve(false);
  refreshPromise = (async () => {
    try {
      const res = await fetch(buildUrl("/api/auth/refresh"), {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ refreshToken }),
      });
      const json = (await res.json().catch(() => null)) as ApiEnvelope<AuthResult> | null;
      if (!res.ok || !json?.success || !json.data) {
        if (res.status === 401 || res.status === 400 || res.status === 403) tokenStore.clear();
        return false;
      }
      tokenStore.setTokens(json.data.accessToken, json.data.refreshToken);
      sessionListeners.forEach((fn) => fn(json.data!));
      return true;
    } catch {
      return false;
    } finally {
      refreshPromise = null;
    }
  })();
  return refreshPromise;
}

const sessionListeners = new Set<(r: AuthResult) => void>();
/** Được gọi mỗi khi refresh thành công (để cập nhật user trong context). */
export function onSessionRefreshed(fn: (r: AuthResult) => void) {
  sessionListeners.add(fn);
  return () => {
    sessionListeners.delete(fn);
  };
}

/* ---------- Fetch lõi ---------- */
async function rawFetch(path: string, opts: RequestOptions): Promise<Response> {
  const auth = opts.auth ?? true;
  const attempt = async (): Promise<Response> => {
    const headers: Record<string, string> = { Accept: "application/json" };
    let body: BodyInit | undefined;
    if (opts.formData) body = opts.formData;
    else if (opts.body !== undefined) {
      headers["Content-Type"] = "application/json";
      body = JSON.stringify(opts.body);
    }
    const token = tokenStore.getAccess();
    if (auth && token) headers.Authorization = `Bearer ${token}`;

    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(new DOMException("timeout", "TimeoutError")), opts.timeoutMs ?? DEFAULT_TIMEOUT);
    const onAbort = () => controller.abort(opts.signal?.reason);
    opts.signal?.addEventListener("abort", onAbort);
    try {
      return await fetch(buildUrl(path, opts.query), {
        method: opts.method ?? "GET",
        headers,
        body,
        signal: controller.signal,
      });
    } catch (e) {
      if (opts.signal?.aborted) throw new ApiError({ code: "ABORTED", message: "Yêu cầu đã được hủy.", status: 0 });
      if (e instanceof DOMException && (e.name === "TimeoutError" || e.name === "AbortError")) {
        throw new ApiError({ code: "TIMEOUT", message: "Máy chủ phản hồi quá lâu. Vui lòng thử lại.", status: 0 });
      }
      throw new ApiError({
        code: "NETWORK_ERROR",
        message: "Không kết nối được tới máy chủ. Hãy kiểm tra mạng hoặc đảm bảo backend đang chạy.",
        status: 0,
      });
    } finally {
      clearTimeout(timeout);
      opts.signal?.removeEventListener("abort", onAbort);
    }
  };

  let res = await attempt();
  if (res.status === 401 && auth) {
    const ok = await refreshSession();
    if (ok) res = await attempt();
    if (!ok || res.status === 401) {
      tokenStore.clear();
      onAuthLost?.();
    }
  }
  return res;
}

async function toApiError(res: Response): Promise<ApiError> {
  let json: ApiEnvelope<unknown> | null = null;
  try {
    json = (await res.json()) as ApiEnvelope<unknown>;
  } catch {
    /* body rỗng / không phải JSON */
  }
  const data = (json?.data ?? null) as { fieldErrors?: Record<string, string[]> } | null;
  const defaults: Record<number, [string, string]> = {
    400: ["VALIDATION_ERROR", "Dữ liệu không hợp lệ."],
    401: ["UNAUTHORIZED", "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."],
    403: ["FORBIDDEN", "Bạn không có quyền thực hiện thao tác này."],
    404: ["NOT_FOUND", "Không tìm thấy dữ liệu yêu cầu."],
    409: ["CONFLICT", "Dữ liệu đã thay đổi hoặc đã được xử lý."],
    413: ["FILE_TOO_LARGE", "Tệp quá lớn."],
    429: ["RATE_LIMITED", "Bạn thao tác quá nhanh. Vui lòng thử lại sau."],
  };
  const [dCode, dMsg] = defaults[res.status] ?? ["SERVER_ERROR", "Máy chủ gặp sự cố. Vui lòng thử lại sau."];
  return new ApiError({
    code: json?.code ?? dCode,
    message: json?.message || dMsg,
    status: res.status,
    fieldErrors: data?.fieldErrors ?? {},
    errors: json?.errors ?? [],
    data: json?.data,
  });
}

/** Gọi API, bóc envelope, ném ApiError khi lỗi. */
export async function api<T = null>(path: string, opts: RequestOptions = {}): Promise<T> {
  const res = await rawFetch(path, opts);
  if (!res.ok) throw await toApiError(res);
  let json: ApiEnvelope<T> | null = null;
  try {
    json = (await res.json()) as ApiEnvelope<T>;
  } catch {
    return null as T;
  }
  if (json && json.success === false) throw await toApiErrorFromJson(json, res.status);
  return (json?.data ?? null) as T;
}

function toApiErrorFromJson(json: ApiEnvelope<unknown>, status: number): ApiError {
  const data = (json.data ?? null) as { fieldErrors?: Record<string, string[]> } | null;
  return new ApiError({
    code: json.code ?? "SERVER_ERROR",
    message: json.message ?? "Đã có lỗi xảy ra.",
    status,
    fieldErrors: data?.fieldErrors ?? {},
    errors: json.errors ?? [],
    data: json.data,
  });
}

function parseFilename(header: string | null): string | null {
  if (!header) return null;
  const star = /filename\*=UTF-8''([^;]+)/i.exec(header);
  if (star) {
    try {
      return decodeURIComponent(star[1].trim());
    } catch {
      /* bỏ qua */
    }
  }
  const plain = /filename="?([^";]+)"?/i.exec(header);
  return plain ? plain[1].trim() : null;
}

/** Tải file (PDF/CSV) bằng fetch + Bearer rồi kích hoạt tải xuống bằng blob. */
export async function downloadFile(path: string, query: Query, fallbackName: string): Promise<string> {
  const res = await rawFetch(path, { query, timeoutMs: 60_000 });
  if (!res.ok) throw await toApiError(res);
  const blob = await res.blob();
  const name = parseFilename(res.headers.get("Content-Disposition")) ?? fallbackName;
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = name;
  document.body.appendChild(a);
  a.click();
  a.remove();
  setTimeout(() => URL.revokeObjectURL(url), 10_000);
  return name;
}
