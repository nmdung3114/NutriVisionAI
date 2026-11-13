"use client";

export default function GlobalError({ reset }: { error: Error; reset: () => void }) {
  return (
    <html lang="vi">
      <body style={{ fontFamily: "sans-serif", textAlign: "center", padding: "4rem 1rem" }}>
        <h1>Đã có lỗi nghiêm trọng</h1>
        <p>Vui lòng tải lại trang.</p>
        <button onClick={reset}>Thử lại</button>
      </body>
    </html>
  );
}
