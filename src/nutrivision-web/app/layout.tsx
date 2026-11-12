import type { Metadata, Viewport } from "next";
import { Plus_Jakarta_Sans } from "next/font/google";
import "./globals.css";
import { QueryProvider } from "@/providers/query-provider";
import { ThemeProvider } from "@/providers/theme-provider";
import { AuthProvider } from "@/providers/auth-provider";
import { TooltipProvider } from "@/components/ui/tooltip";
import { Toaster } from "@/components/ui/sonner";

const jakarta = Plus_Jakarta_Sans({
  variable: "--font-jakarta",
  subsets: ["latin", "vietnamese"],
  display: "swap",
});

export const metadata: Metadata = {
  title: { default: "NutriVision AI – Theo dõi dinh dưỡng bằng AI", template: "%s | NutriVision AI" },
  description:
    "NutriVision AI giúp bạn nhận diện món ăn từ ảnh, tính calo và dinh dưỡng, theo dõi mục tiêu hằng ngày và báo cáo sức khỏe.",
  icons: { icon: "/favicon.svg" },
};

export const viewport: Viewport = {
  themeColor: [
    { media: "(prefers-color-scheme: light)", color: "#21b45a" },
    { media: "(prefers-color-scheme: dark)", color: "#0d1411" },
  ],
  width: "device-width",
  initialScale: 1,
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="vi" className={jakarta.variable} suppressHydrationWarning>
      <body className="min-h-dvh bg-background font-sans">
        <ThemeProvider>
          <QueryProvider>
            <AuthProvider>
              <TooltipProvider delayDuration={150}>{children}</TooltipProvider>
              <Toaster position="top-center" richColors closeButton />
            </AuthProvider>
          </QueryProvider>
        </ThemeProvider>
      </body>
    </html>
  );
}
