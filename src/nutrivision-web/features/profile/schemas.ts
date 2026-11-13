import { z } from "zod";
import { calcAge } from "@/lib/date";

const num = (label: string, min: number, max: number) =>
  z
    .number({ error: `Vui lòng nhập ${label}` })
    .refine((v) => Number.isFinite(v), `Vui lòng nhập ${label}`)
    .min(min, `${label} tối thiểu ${min}`)
    .max(max, `${label} tối đa ${max}`);

export const profileSchema = z
  .object({
    hoTen: z.string().trim().min(2, "Họ tên tối thiểu 2 ký tự").max(100, "Họ tên tối đa 100 ký tự"),
    gioiTinh: z.enum(["Nam", "Nu"], { error: "Vui lòng chọn giới tính" }),
    ngaySinh: z.string().min(1, "Vui lòng chọn ngày sinh"),
    chieuCao: num("Chiều cao", 50, 200),
    canNang: num("Cân nặng", 10, 150),
    mucDoVanDong: z.enum(["ItVanDong", "VanDongNhe", "VuaPhai", "NangDong", "RatNangDong"], { error: "Vui lòng chọn mức độ vận động" }),
    mucTieuMode: z.enum(["tdee", "custom"]),
    mucTieuCalo: z.number().nullable(),
  })
  .superRefine((v, ctx) => {
    if (v.ngaySinh) {
      const age = calcAge(v.ngaySinh);
      if (Number.isNaN(age) || age < 10 || age > 100) ctx.addIssue({ code: "custom", path: ["ngaySinh"], message: "Tuổi phải từ 10 đến 100" });
    }
    if (v.mucTieuMode === "custom") {
      if (v.mucTieuCalo === null || !Number.isFinite(v.mucTieuCalo)) ctx.addIssue({ code: "custom", path: ["mucTieuCalo"], message: "Vui lòng nhập mục tiêu calo" });
      else if (v.mucTieuCalo < 800 || v.mucTieuCalo > 6000) ctx.addIssue({ code: "custom", path: ["mucTieuCalo"], message: "Mục tiêu calo từ 800 đến 6000 kcal" });
    }
  });
export type ProfileValues = z.infer<typeof profileSchema>;
