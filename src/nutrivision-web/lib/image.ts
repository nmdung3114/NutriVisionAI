export const MEAL_IMAGE_MAX = 10 * 1024 * 1024;
export const AVATAR_MAX = 5 * 1024 * 1024;
export const ALLOWED_IMAGE_TYPES = ["image/jpeg", "image/png", "image/webp"];
export const MIN_DIMENSION = 224;

export function formatBytes(n: number): string {
  if (n < 1024) return `${n} B`;
  if (n < 1024 * 1024) return `${(n / 1024).toFixed(1)} KB`;
  return `${(n / 1024 / 1024).toFixed(2)} MB`;
}

export function validateImageBasics(file: File, maxBytes: number): string | null {
  const okType = ALLOWED_IMAGE_TYPES.includes(file.type) || /\.(jpe?g|png|webp)$/i.test(file.name);
  if (!okType) return "Định dạng không hợp lệ. Chỉ chấp nhận ảnh JPG, JPEG, PNG hoặc WEBP.";
  if (file.size > maxBytes) return `Ảnh quá lớn (${formatBytes(file.size)}). Dung lượng tối đa ${formatBytes(maxBytes)}.`;
  if (file.size === 0) return "Tệp ảnh rỗng.";
  return null;
}

export function readImageSize(file: File): Promise<{ width: number; height: number }> {
  return new Promise((resolve, reject) => {
    const url = URL.createObjectURL(file);
    const img = new Image();
    img.onload = () => {
      resolve({ width: img.naturalWidth, height: img.naturalHeight });
      URL.revokeObjectURL(url);
    };
    img.onerror = () => {
      URL.revokeObjectURL(url);
      reject(new Error("Không đọc được ảnh"));
    };
    img.src = url;
  });
}

/** Kiểm tra đầy đủ ảnh bữa ăn phía client; trả về lỗi hoặc kích thước. */
export async function validateMealImage(
  file: File,
): Promise<{ error: string } | { width: number; height: number }> {
  const basic = validateImageBasics(file, MEAL_IMAGE_MAX);
  if (basic) return { error: basic };
  try {
    const size = await readImageSize(file);
    if (size.width < MIN_DIMENSION || size.height < MIN_DIMENSION) {
      return { error: `Ảnh quá nhỏ (${size.width}×${size.height}). Kích thước tối thiểu ${MIN_DIMENSION}×${MIN_DIMENSION} px.` };
    }
    return size;
  } catch {
    return { error: "Không đọc được ảnh. Tệp có thể bị hỏng." };
  }
}
