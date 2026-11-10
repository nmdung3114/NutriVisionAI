import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "NutriVisionAI - Nền tảng Dinh dưỡng Cá nhân hóa & AI Nhận diện Món ăn",
  description: "Phân tích khẩu phần calo và vi chất dinh dưỡng tự động qua ảnh chụp bằng trí tuệ nhân tạo Gemini Vision",
  icons: {
    icon: "/favicon.ico",
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="vi" className="dark">
      <body className="antialiased selection:bg-brand-500 selection:text-white bg-[#090d16] text-slate-100 min-h-screen">
        {children}
      </body>
    </html>
  );
}
