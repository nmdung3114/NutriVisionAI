export const API_URL = (process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000").replace(/\/+$/, "");

/** Ghép URL tương đối đã ký của backend thành URL tuyệt đối dùng cho <img src>. */
export function assetUrl(url: string | null | undefined): string | null {
  if (!url) return null;
  if (/^https?:\/\//i.test(url)) return url;
  return `${API_URL}${url.startsWith("/") ? "" : "/"}${url}`;
}
