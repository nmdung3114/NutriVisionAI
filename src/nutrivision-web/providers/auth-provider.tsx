"use client";

import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { useQueryClient } from "@tanstack/react-query";
import { authApi } from "@/lib/api/auth";
import { onSessionRefreshed, refreshSession, setAuthLostHandler } from "@/lib/api/client";
import { tokenStore } from "@/lib/auth/token-store";
import type { AuthResult, UserInfo } from "@/types/api";

type Status = "loading" | "authenticated" | "anonymous";

interface AuthContextValue {
  status: Status;
  user: UserInfo | null;
  login: (email: string, matKhau: string) => Promise<UserInfo>;
  logout: () => Promise<void>;
  setSession: (r: AuthResult) => void;
  patchUser: (p: Partial<UserInfo>) => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [status, setStatus] = useState<Status>("loading");
  const [user, setUser] = useState<UserInfo | null>(null);
  const router = useRouter();
  const qc = useQueryClient();

  const setSession = useCallback((r: AuthResult) => {
    tokenStore.setTokens(r.accessToken, r.refreshToken);
    setUser(r.user);
    setStatus("authenticated");
  }, []);

  // Khôi phục phiên từ refresh token khi tải trang.
  useEffect(() => {
    let cancelled = false;
    (async () => {
      if (!tokenStore.getRefresh()) {
        setStatus("anonymous");
        return;
      }
      const ok = await refreshSession();
      if (cancelled) return;
      if (!ok) {
        setUser(null);
        setStatus("anonymous");
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  // Refresh thành công (ở bất kỳ đâu) -> cập nhật user.
  useEffect(
    () =>
      onSessionRefreshed((r) => {
        setUser(r.user);
        setStatus("authenticated");
      }),
    [],
  );

  // Mất phiên (refresh thất bại) -> về trang đăng nhập.
  useEffect(() => {
    setAuthLostHandler(() => {
      setUser(null);
      setStatus("anonymous");
      qc.clear();
      const next = window.location.pathname + window.location.search;
      router.replace(`/login?next=${encodeURIComponent(next)}`);
    });
    return () => setAuthLostHandler(null);
  }, [qc, router]);

  const login = useCallback(
    async (email: string, matKhau: string) => {
      const r = await authApi.login({ email, matKhau });
      qc.clear();
      setSession(r);
      return r.user;
    },
    [qc, setSession],
  );

  const logout = useCallback(async () => {
    const refresh = tokenStore.getRefresh();
    try {
      if (refresh) await authApi.logout(refresh);
    } catch {
      /* vẫn đăng xuất cục bộ */
    }
    tokenStore.clear();
    setUser(null);
    setStatus("anonymous");
    qc.clear();
    router.replace("/login");
  }, [qc, router]);

  const patchUser = useCallback((p: Partial<UserInfo>) => setUser((u) => (u ? { ...u, ...p } : u)), []);

  const value = useMemo(
    () => ({ status, user, login, logout, setSession, patchUser }),
    [status, user, login, logout, setSession, patchUser],
  );
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth phải nằm trong AuthProvider");
  return ctx;
}
