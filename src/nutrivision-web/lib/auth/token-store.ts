/**
 * Access token giữ trong bộ nhớ (không lưu đĩa), refresh token lưu localStorage.
 * Không bao giờ log token.
 */
const REFRESH_KEY = "nv_refresh_token";

let accessToken: string | null = null;

export const tokenStore = {
  getAccess: () => accessToken,
  setAccess: (t: string | null) => {
    accessToken = t;
  },
  getRefresh(): string | null {
    try {
      return typeof window === "undefined" ? null : window.localStorage.getItem(REFRESH_KEY);
    } catch {
      return null;
    }
  },
  setRefresh(t: string | null) {
    try {
      if (typeof window === "undefined") return;
      if (t) window.localStorage.setItem(REFRESH_KEY, t);
      else window.localStorage.removeItem(REFRESH_KEY);
    } catch {
      /* localStorage không khả dụng */
    }
  },
  setTokens(access: string, refresh: string) {
    accessToken = access;
    this.setRefresh(refresh);
  },
  clear() {
    accessToken = null;
    this.setRefresh(null);
  },
};
