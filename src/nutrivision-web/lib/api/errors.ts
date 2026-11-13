export class ApiError extends Error {
  readonly code: string;
  readonly status: number;
  readonly fieldErrors: Record<string, string[]>;
  readonly errors: string[];
  readonly data: unknown;

  constructor(opts: {
    code: string;
    message: string;
    status: number;
    fieldErrors?: Record<string, string[]>;
    errors?: string[];
    data?: unknown;
  }) {
    super(opts.message);
    this.name = "ApiError";
    this.code = opts.code;
    this.status = opts.status;
    this.fieldErrors = opts.fieldErrors ?? {};
    this.errors = opts.errors ?? [];
    this.data = opts.data;
  }
}

export function isApiError(e: unknown): e is ApiError {
  return e instanceof ApiError;
}

/** Gợi ý tiếng Việt thân thiện cho từng mã lỗi của API. */
const HINTS: Record<string, string> = {
  VALIDATION_ERROR: "Vui lòng kiểm tra lại các trường được đánh dấu đỏ.",
  INVALID_CREDENTIALS: "Email hoặc mật khẩu chưa đúng. Hãy thử lại hoặc dùng \"Quên mật khẩu\".",
  EMAIL_NOT_VERIFIED: "Bạn cần xác thực email trước khi đăng nhập. Hãy kiểm tra hộp thư hoặc gửi lại email xác thực.",
  ACCOUNT_TEMP_LOCKED: "Tài khoản tạm khóa do nhập sai mật khẩu nhiều lần. Vui lòng thử lại sau.",
  ACCOUNT_DISABLED: "Tài khoản đã bị vô hiệu hóa. Hãy liên hệ quản trị viên để được hỗ trợ.",
  EMAIL_EXISTS: "Email này đã được đăng ký. Bạn có thể đăng nhập hoặc đặt lại mật khẩu.",
  TOKEN_INVALID: "Liên kết không hợp lệ. Hãy yêu cầu gửi lại liên kết mới.",
  TOKEN_EXPIRED: "Liên kết đã hết hạn. Hãy yêu cầu gửi lại liên kết mới.",
  TOKEN_USED: "Liên kết này đã được sử dụng trước đó.",
  WRONG_PASSWORD: "Mật khẩu hiện tại chưa đúng.",
  SAME_PASSWORD: "Mật khẩu mới phải khác mật khẩu hiện tại.",
  UNAUTHORIZED: "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
  FORBIDDEN: "Bạn không có quyền truy cập nội dung này.",
  NOT_FOUND: "Không tìm thấy dữ liệu yêu cầu. Có thể nó đã bị xóa.",
  CONFLICT: "Dữ liệu đã thay đổi hoặc đã được xử lý. Hãy tải lại trang.",
  PROFILE_REQUIRED: "Hãy lưu hồ sơ trước khi đổi ảnh đại diện.",
  SELF_ACTION_FORBIDDEN: "Bạn không thể thực hiện thao tác này trên chính tài khoản của mình.",
  INVALID_IMAGE: "Ảnh không hợp lệ. Hãy chọn ảnh JPG, PNG hoặc WEBP, tối thiểu 224×224 px.",
  FILE_TOO_LARGE: "Tệp quá lớn. Hãy chọn ảnh nhỏ hơn giới hạn cho phép.",
  NUTRITION_DATA_MISSING: "Một số thành phần chưa có dữ liệu dinh dưỡng. Hãy chọn thực phẩm gần đúng, nhập tay hoặc xóa chúng.",
  RATE_LIMITED: "Bạn thao tác quá nhanh. Vui lòng chờ một lát rồi thử lại.",
  SERVER_ERROR: "Máy chủ đang gặp sự cố. Vui lòng thử lại sau ít phút.",
  NETWORK_ERROR: "Không kết nối được tới máy chủ. Hãy kiểm tra mạng hoặc đảm bảo backend đang chạy.",
  TIMEOUT: "Máy chủ phản hồi quá lâu. Vui lòng thử lại.",
  ABORTED: "Yêu cầu đã được hủy.",
};

export function errorHint(code: string): string | undefined {
  return HINTS[code];
}

/** Thông báo hiển thị cho người dùng (ưu tiên message của server – đã là tiếng Việt). */
export function errorMessage(e: unknown, fallback = "Đã có lỗi xảy ra. Vui lòng thử lại."): string {
  if (isApiError(e)) {
    if (e.code === "NETWORK_ERROR" || e.code === "TIMEOUT" || e.code === "ABORTED") return HINTS[e.code];
    return e.message || HINTS[e.code] || fallback;
  }
  return fallback;
}

export function errorStatusTitle(e: unknown): string {
  if (isApiError(e)) {
    if (e.status === 403) return "Bạn không có quyền xem nội dung này";
    if (e.status === 404) return "Không tìm thấy nội dung";
    if (e.status === 0) return "Không kết nối được máy chủ";
  }
  return "Đã có lỗi xảy ra";
}
