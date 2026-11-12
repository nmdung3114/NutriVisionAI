"use client";

import { useEffect, useRef, useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Camera, Info, Upload } from "lucide-react";
import { Button } from "@/components/ui/button";
import { UserAvatar } from "@/components/layout/user-avatar";
import { profileApi } from "@/lib/api/profile";
import { ApiError, errorMessage } from "@/lib/api/errors";
import { AVATAR_MAX, formatBytes, validateImageBasics } from "@/lib/image";
import { useAuth } from "@/providers/auth-provider";
import type { Profile } from "@/types/api";

export function AvatarCard({ profile }: { profile: Profile }) {
  const qc = useQueryClient();
  const { patchUser } = useAuth();
  const ref = useRef<HTMLInputElement>(null);
  const [file, setFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!file) return setPreview(null);
    const url = URL.createObjectURL(file);
    setPreview(url);
    return () => URL.revokeObjectURL(url);
  }, [file]);

  const upload = useMutation({
    mutationFn: (f: File) => profileApi.uploadAvatar(f),
    onSuccess: (r) => {
      patchUser({ anhDaiDienUrl: r.anhDaiDienUrl });
      void qc.invalidateQueries({ queryKey: ["profile"] });
      setFile(null);
      toast.success("Đã cập nhật ảnh đại diện");
    },
    onError: (e) => {
      if (e instanceof ApiError && e.code === "PROFILE_REQUIRED") setError("Hãy lưu hồ sơ trước khi đổi ảnh đại diện.");
      else setError(errorMessage(e));
    },
  });

  const pick = (f: File | undefined) => {
    if (!f) return;
    const err = validateImageBasics(f, AVATAR_MAX);
    if (err) {
      setError(err);
      setFile(null);
      return;
    }
    setError(null);
    setFile(f);
  };

  return (
    <section className="flex flex-col items-center gap-4 rounded-2xl border bg-card p-5 shadow-soft sm:flex-row" aria-label="Ảnh đại diện">
      {preview ? (
        // eslint-disable-next-line @next/next/no-img-element
        <img src={preview} alt="Ảnh đại diện mới" className="size-24 rounded-full object-cover ring-4 ring-brand-100" />
      ) : (
        <UserAvatar name={profile.hoTen} url={profile.anhDaiDienUrl} className="size-24 text-2xl ring-4 ring-brand-100" />
      )}
      <div className="flex-1 text-center sm:text-left">
        <p className="font-extrabold">{profile.hoTen}</p>
        <p className="text-sm text-muted-foreground">{profile.email}</p>
        <p className="mt-1 text-xs text-muted-foreground">JPG, PNG hoặc WEBP, tối đa 5MB.</p>
        {file && <p className="mt-1 text-xs font-medium">{file.name} · {formatBytes(file.size)}</p>}
        {!profile.daDayDu && !error && (
          <p className="mt-2 flex items-center justify-center gap-1.5 text-xs text-warn sm:justify-start">
            <Info className="size-3.5" /> Hãy lưu hồ sơ trước khi đổi ảnh đại diện.
          </p>
        )}
        {error && (
          <p role="alert" className="mt-2 text-sm font-medium text-destructive">
            {error}
          </p>
        )}
        <div className="mt-3 flex flex-wrap justify-center gap-2 sm:justify-start">
          <Button type="button" variant="outline" size="sm" onClick={() => ref.current?.click()}>
            <Camera /> Chọn ảnh
          </Button>
          {file && (
            <>
              <Button type="button" size="sm" disabled={upload.isPending} onClick={() => upload.mutate(file)}>
                <Upload /> {upload.isPending ? "Đang tải lên…" : "Lưu ảnh"}
              </Button>
              <Button type="button" variant="ghost" size="sm" onClick={() => { setFile(null); setError(null); }}>
                Bỏ chọn
              </Button>
            </>
          )}
        </div>
        <input ref={ref} type="file" accept="image/jpeg,image/png,image/webp" className="sr-only" tabIndex={-1} aria-label="Chọn ảnh đại diện" onChange={(e) => { pick(e.target.files?.[0]); e.target.value = ""; }} />
      </div>
    </section>
  );
}
