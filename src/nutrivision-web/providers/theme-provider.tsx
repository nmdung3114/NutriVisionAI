"use client";

import { ThemeProvider as NextThemesProvider } from "next-themes";

/** Sáng / tối / theo hệ thống; lưu lựa chọn ở localStorage, thêm class `dark` vào <html>. */
export function ThemeProvider({ children }: { children: React.ReactNode }) {
  return (
    <NextThemesProvider attribute="class" defaultTheme="system" enableSystem disableTransitionOnChange>
      {children}
    </NextThemesProvider>
  );
}
