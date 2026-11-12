import { api } from "./client";
import type { Profile, ProfileUpdate } from "@/types/api";

export const profileApi = {
  get: () => api<Profile>("/api/profile"),
  update: (b: ProfileUpdate) => api<Profile>("/api/profile", { method: "PUT", body: b }),
  uploadAvatar: (file: File) => {
    const fd = new FormData();
    fd.append("file", file);
    return api<{ anhDaiDienUrl: string }>("/api/profile/avatar", { method: "POST", formData: fd, timeoutMs: 60_000 });
  },
};
