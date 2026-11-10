"use client";

import React, { createContext, useContext, useState, useEffect } from "react";
import { authApi, profileApi, UserProfile } from "@/lib/api";
import { useRouter } from "next/navigation";

interface AuthContextType {
  user: UserProfile | null;
  loading: boolean;
  login: (token: string, refreshToken: string, user: UserProfile) => void;
  logout: () => void;
  refreshUser: () => Promise<void>;
  isAdmin: boolean;
}

const AuthContext = createContext<AuthContextType>({
  user: null,
  loading: true,
  login: () => {},
  logout: () => {},
  refreshUser: async () => {},
  isAdmin: false,
});

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [user, setUser] = useState<UserProfile | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const router = useRouter();

  const refreshUser = async () => {
    try {
      const res = await authApi.getMe();
      if (res.data?.success && res.data?.data) {
        setUser(res.data.data);
        localStorage.setItem("nv_user", JSON.stringify(res.data.data));
      }
    } catch (err) {
      console.warn("Could not fetch user profile", err);
    }
  };

  useEffect(() => {
    const initAuth = async () => {
      if (typeof window !== "undefined") {
        const token = localStorage.getItem("nv_access_token");
        const cachedUser = localStorage.getItem("nv_user");

        if (token && cachedUser) {
          try {
            setUser(JSON.parse(cachedUser));
            await refreshUser();
          } catch (e) {
            console.error("Parse cached user failed", e);
          }
        } else if (!token) {
          // Provide default demo mock user if not logged in to make preview instant & seamless
          const demoUser: UserProfile = {
            id: "demo-user-id",
            email: "demo@nutrivision.ai",
            hoTen: "Nguyễn Minh Anh",
            vaiTro: "USER",
            chieuCaoCm: 172,
            canNangKg: 68,
            tuoi: 24,
            gioiTinh: "Nam",
            mucDoHoatDong: "Vừa phải",
            mucTieu: "Duy trì cân nặng",
            bmr: 1650,
            tdee: 2200,
            caloMucTieu: 2200,
          };
          setUser(demoUser);
        }
      }
      setLoading(false);
    };

    initAuth();
  }, []);

  const login = (accessToken: string, refreshToken: string, userProfile: UserProfile) => {
    localStorage.setItem("nv_access_token", accessToken);
    localStorage.setItem("nv_refresh_token", refreshToken);
    localStorage.setItem("nv_user", JSON.stringify(userProfile));
    setUser(userProfile);
    if (userProfile.vaiTro === "ADMIN") {
      router.push("/admin");
    } else {
      router.push("/dashboard");
    }
  };

  const logout = () => {
    authApi.logout();
    setUser(null);
    router.push("/login");
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        loading,
        login,
        logout,
        refreshUser,
        isAdmin: user?.vaiTro === "ADMIN",
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);
