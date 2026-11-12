/** Kiểu dữ liệu phản ánh docs/api-contract.md */

export interface ApiEnvelope<T> {
  success: boolean;
  message: string | null;
  code: string | null;
  data: T | null;
  errors: string[] | null;
}

export interface Paged<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export type Role = "USER" | "ADMIN";
export type UserStatus = "PENDING" | "ACTIVE" | "LOCKED";

export interface UserInfo {
  id: string;
  email: string;
  hoTen: string;
  vaiTro: Role;
  trangThai: UserStatus;
  emailDaXacThuc: boolean;
  anhDaiDienUrl: string | null;
  ngayTao?: string;
}

export interface AuthResult {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  user: UserInfo;
}

export type Gender = "Nam" | "Nu";
export type ActivityLevel =
  | "ItVanDong"
  | "VanDongNhe"
  | "VuaPhai"
  | "NangDong"
  | "RatNangDong";

export interface Profile {
  hoTen: string;
  email: string;
  anhDaiDienUrl: string | null;
  daDayDu: boolean;
  ngaySinh: string | null;
  tuoi: number | null;
  gioiTinh: Gender | null;
  chieuCao: number | null;
  canNang: number | null;
  mucDoVanDong: ActivityLevel | null;
  chiSoBMI: number | null;
  phanLoaiBMI: string | null;
  chiSoBMR: number | null;
  chiSoTDEE: number | null;
  mucTieuCalo: number;
  ngayCapNhat: string | null;
}

export interface ProfileUpdate {
  hoTen: string;
  ngaySinh: string;
  gioiTinh: Gender;
  chieuCao: number;
  canNang: number;
  mucDoVanDong: ActivityLevel;
  mucTieuCalo: number | null;
}

export interface Nutrition {
  calo: number;
  chatDam: number;
  tinhBot: number;
  chatBeo: number;
  chatXo: number;
}

export interface Food {
  id: string;
  maThucPham: string;
  tenThucPham: string;
  nhomThucPham: string;
  donViChuan: string;
  khoiLuongChuan: number;
  caloTren100g: number;
  chatDamTren100g: number;
  tinhBotTren100g: number;
  chatBeoTren100g: number;
  chatXoTren100g: number;
}

export interface FoodMatch {
  food: Food;
  diem: number;
}

export interface UnitConversion {
  donVi: string;
  gram: number;
}

export type LoaiBuaAn = "Sang" | "Trua" | "Toi" | "Phu";

export interface MealComponent {
  id: string | null;
  thucPhamThamChieuId: string | null;
  tenThanhPhan: string;
  khauPhan: number;
  donViTinh: string;
  khoiLuongQuyDoiGram: number;
  luongCalo: number;
  luongChatDam: number;
  luongTinhBot: number;
  luongChatBeo: number;
  luongChatXo: number;
  thamChieu: Food | null;
  dinhDuongTren100g: Nutrition | null;
  thieuDuLieuDinhDuong: boolean;
}

export interface Per100g {
  caloTren100g: number;
  chatDamTren100g: number;
  tinhBotTren100g: number;
  chatBeoTren100g: number;
  chatXoTren100g: number;
}

export interface ComponentInput {
  tenThanhPhan: string;
  thucPhamThamChieuId?: string | null;
  khauPhan: number;
  donViTinh: string;
  nhapTay?: Per100g | null;
}

export interface MealUpsert {
  loaiBuaAn: LoaiBuaAn;
  ngayAn?: string;
  tenMonAn: string;
  thanhPhan: ComponentInput[];
}

export interface AiOriginal {
  tenMonAn: string;
  doTinCay: number | null;
  thanhPhan: {
    tenThanhPhan: string;
    khauPhan: number;
    donViTinh: string;
    khoiLuongGram: number;
    thucPhamThamChieuId: string | null;
  }[];
}

export interface Meal {
  id: string;
  loaiBuaAn: LoaiBuaAn;
  ngayAn: string;
  thoiGianGhiNhan: string;
  tenMonAn: string;
  hinhAnhUrl: string | null;
  taoTuAI: boolean;
  daXacNhan: boolean;
  thanhPhan: MealComponent[];
  tongDinhDuong: Nutrition;
  mucTieuCalo: number;
  phanTramNhuCauNgay: number;
  doTinCay: number | null;
  aiGoc: AiOriginal | null;
  nguoiDungCoSua: boolean | null;
  aiThatBai: boolean;
  thongBaoAI: string | null;
}

export interface MealSummary {
  id: string;
  loaiBuaAn: LoaiBuaAn;
  ngayAn: string;
  thoiGianGhiNhan: string;
  tenMonAn: string;
  hinhAnhUrl: string | null;
  taoTuAI: boolean;
  tongDinhDuong: Nutrition;
  soThanhPhan: number;
}

export interface DayTotal {
  ngay: string;
  tongCalo: number;
  mucTieuCalo: number;
  phanTram: number;
  soBuaAn: number;
}

export interface MealHistory extends Paged<MealSummary> {
  tongHopNgay: DayTotal[];
}

export interface Suggestion {
  food: Food;
  lyDo: string;
}

export type DayState = "Thieu" | "DatChuan" | "Vuot";

export interface Dashboard {
  ngay: string;
  mucTieuCalo: number;
  hoSoDayDu: boolean;
  coDuLieu: boolean;
  homNay: Nutrition & {
    soBuaAn: number;
    phanTramMucTieu: number;
    trangThai: DayState;
  };
  mucTieuMacro: { chatDam: number; tinhBot: number; chatBeo: number };
  tyLeMacro: { chatDam: number; tinhBot: number; chatBeo: number };
  buaAnHomNay: MealSummary[];
  theoLoaiBuaAn: { loaiBuaAn: LoaiBuaAn; calo: number; soBua: number }[];
  bayNgay: { ngay: string; calo: number; mucTieuCalo: number; soBuaAn: number }[];
}

export type Rating = "Thap" | "DatChuan" | "Cao";
export type Trend = "Tang" | "Giam" | "OnDinh" | "KhongDuDuLieu";

export interface Report {
  from: string;
  to: string;
  soNgay: number;
  soNgayCoDuLieu: number;
  tongSoBuaAn: number;
  mucTieuCaloTrungBinh: number;
  trungBinhNgay: Nutrition;
  tong: Nutrition;
  phanTramMucTieu: number;
  danhGia: Record<keyof Nutrition, Rating>;
  tyLeNangLuong: { chatDam: number; tinhBot: number; chatBeo: number };
  xuHuong: Trend;
  soNgayVuotMucTieu: number;
  soNgayThieuNangLuong: number;
  theoNgay: (Nutrition & { ngay: string; soBuaAn: number; mucTieuCalo: number })[];
  khuyenNghi: { muc: "info" | "warning" | "success"; noiDung: string }[];
}

export interface AdminUser {
  id: string;
  email: string;
  hoTen: string;
  vaiTro: Role;
  trangThai: UserStatus;
  emailDaXacThuc: boolean;
  ngayTao: string;
}

export interface AdminLog {
  id: string;
  nguoiDungThucHienId: string | null;
  emailNguoiThucHien: string | null;
  loaiHanhDong: string;
  bangTacDong: string | null;
  idDoiTuong: string | null;
  chiTiet: string | null;
  diaChiIP: string | null;
  ngayTao: string;
}

export interface DayCount {
  ngay: string;
  soLuong: number;
}

export interface AdminStatistics {
  tongNguoiDung: number;
  nguoiDungHoatDong: number;
  nguoiDungBiKhoa: number;
  tongBuaAn: number;
  soAnhAIXuLy: number;
  soLanAIThatBai: number;
  tyLeAIChinhXacPhanTram: number | null;
  thoiGianXuLyAITrungBinhMs: number | null;
  nguoiDungMoiTheoNgay: DayCount[];
  buaAnTheoNgay: DayCount[];
  aiTheoNgay: DayCount[];
}

export interface CalendarDay {
  ngay: string;
  calo: number;
  mucTieuCalo: number;
  soBuaAn: number;
  phanTramMucTieu: number;
  trangThai: DayState | null;
}

export interface CalendarMonth {
  nam: number;
  thang: number;
  homNay: string;
  soNgayCoDuLieu: number;
  soNgayDatChuan: number;
  ngay: CalendarDay[];
}
