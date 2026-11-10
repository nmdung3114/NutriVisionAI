import axios from "axios";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000/api";

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

// Request interceptor to attach JWT token
apiClient.interceptors.request.use((config) => {
  if (typeof window !== "undefined") {
    const token = localStorage.getItem("nv_access_token");
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
  }
  return config;
});

// Response interceptor to handle 401 and refresh token
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    if (error.response?.status === 401 && !originalRequest._retry && typeof window !== "undefined") {
      originalRequest._retry = true;
      const refreshToken = localStorage.getItem("nv_refresh_token");
      if (refreshToken) {
        try {
          const res = await axios.post(`${API_BASE_URL}/auth/refresh-token`, { refreshToken });
          const newAccessToken = res.data.data.accessToken;
          const newRefreshToken = res.data.data.refreshToken;
          
          localStorage.setItem("nv_access_token", newAccessToken);
          localStorage.setItem("nv_refresh_token", newRefreshToken);
          
          originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
          return apiClient(originalRequest);
        } catch (refreshErr) {
          localStorage.removeItem("nv_access_token");
          localStorage.removeItem("nv_refresh_token");
          localStorage.removeItem("nv_user");
          window.location.href = "/login";
        }
      }
    }
    return Promise.reject(error);
  }
);

// Type definitions
export interface UserProfile {
  id: string;
  email: string;
  hoTen: string;
  soDienThoai?: string;
  anhDaiDien?: string;
  vaiTro: "ADMIN" | "USER";
  chieuCaoCm?: number;
  canNangKg?: number;
  tuoi?: number;
  gioiTinh?: string;
  mucDoHoatDong?: string;
  mucTieu?: string;
  bmr?: number;
  tdee?: number;
  caloMucTieu?: number;
}

export interface FoodItemAi {
  thucPhamId?: string;
  tenMon: string;
  khoiLuongGam: number;
  donVi: string;
  calo: number;
  protein: number;
  carbs: number;
  fat: number;
  chatSo?: number;
  tiLeTinCay: number;
}

export interface AiScanResult {
  lichSuId: string;
  urlAnh: string;
  doTinCayTongThe: number;
  thoiGianXuLyMs: number;
  danhSachMon: FoodItemAi[];
  canhBao?: string;
}

export interface MealItemDetail {
  id: string;
  tenMon: string;
  khoiLuongGam: number;
  donVi: string;
  calo: number;
  protein: number;
  carbs: number;
  fat: number;
  tiLeTinCay?: number;
}

export interface MealDetail {
  id: string;
  loaiBuaAn: string;
  thoiGianBuaAn: string;
  urlAnh?: string;
  tongCalo: number;
  tongProtein: number;
  tongCarbs: number;
  tongFat: number;
  ghiChu?: string;
  nguonNhap: string;
  thanhPhans: MealItemDetail[];
}

export interface DashboardSummary {
  ngay: string;
  tongCalo: number;
  tongProtein: number;
  tongCarbs: number;
  tongFat: number;
  mucTieuCalo: number;
  tyLeDatMucTieu: number;
  caloConLai: number;
  soBuaAn: number;
  buaAns: MealDetail[];
}

export interface DailyTrendItem {
  ngay: string;
  thuTrongTuan: string;
  tongCalo: number;
  protein: number;
  carbs: number;
  fat: number;
  mucTieuCalo: number;
  datMucTieu: boolean;
}

export interface DashboardWeeklyTrend {
  tuNgay: string;
  denNgay: string;
  caloTrungBinh: number;
  tyLeDatMucTieu: number;
  ngayDatas: DailyTrendItem[];
}

// API Services
export const authApi = {
  register: (data: any) => apiClient.post("/auth/register", data),
  login: (data: any) => apiClient.post("/auth/login", data),
  getMe: () => apiClient.get("/auth/me"),
  refreshToken: (refreshToken: string) => apiClient.post("/auth/refresh-token", { refreshToken }),
  logout: () => {
    if (typeof window !== "undefined") {
      localStorage.removeItem("nv_access_token");
      localStorage.removeItem("nv_refresh_token");
      localStorage.removeItem("nv_user");
    }
  },
};

export const profileApi = {
  getProfile: () => apiClient.get("/profile"),
  updateProfile: (data: any) => apiClient.put("/profile", data),
  uploadAvatar: (formData: FormData) =>
    apiClient.post("/profile/avatar", formData, {
      headers: { "Content-Type": "multipart/form-data" },
    }),
};

export const mealApi = {
  scanMeal: (formData: FormData) =>
    apiClient.post("/meal/scan", formData, {
      headers: { "Content-Type": "multipart/form-data" },
    }),
  createMeal: (data: any) => apiClient.post("/meal", data),
  getMeals: (params?: { tuNgay?: string; denNgay?: string; loaiBuaAn?: string }) =>
    apiClient.get("/meal", { params }),
  getMealById: (id: string) => apiClient.get(`/meal/${id}`),
  updateMeal: (id: string, data: any) => apiClient.put(`/meal/${id}`, data),
  deleteMeal: (id: string) => apiClient.delete(`/meal/${id}`),
  undoMeal: (id: string) => apiClient.post(`/meal/${id}/undo`),
};

export const dashboardApi = {
  getSummaryToday: (date?: string) =>
    apiClient.get("/dashboard/today", { params: { date } }),
  getWeeklyTrends: () => apiClient.get("/dashboard/weekly"),
  exportCsv: async () => {
    const res = await apiClient.get("/dashboard/export-csv", { responseType: "blob" });
    const url = window.URL.createObjectURL(new Blob([res.data], { type: "text/csv;charset=utf-8;" }));
    const link = document.createElement("a");
    link.href = url;
    link.setAttribute("download", `NutriVision_Export_${new Date().toISOString().slice(0, 10)}.csv`);
    document.body.appendChild(link);
    link.click();
    link.remove();
  },
};

export const adminApi = {
  getStats: () => apiClient.get("/admin/stats"),
  getUsers: (params?: { page?: number; pageSize?: number; search?: string }) =>
    apiClient.get("/admin/users", { params }),
  toggleLock: (userId: string, data: { isLocked: boolean; reason?: string }) =>
    apiClient.put(`/admin/users/${userId}/lock`, data),
  changeRole: (userId: string, role: string) =>
    apiClient.put(`/admin/users/${userId}/role`, { role }),
  getLogs: (limit?: number) => apiClient.get("/admin/logs", { params: { limit } }),
};
