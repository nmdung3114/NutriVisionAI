import type { Config } from "tailwindcss";

const config: Config = {
  darkMode: "class",
  content: [
    "./pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./components/**/*.{js,ts,jsx,tsx,mdx}",
    "./app/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      colors: {
        background: "var(--background)",
        foreground: "var(--foreground)",
        brand: {
          50: "#ecfdf5",
          100: "#d1fae5",
          200: "#a7f3d0",
          300: "#6ee7b7",
          400: "#34d399",
          500: "#10b981",
          600: "#059669",
          700: "#047857",
          800: "#065f46",
          900: "#064e3b",
          950: "#022c22",
        },
        tech: {
          cyan: "#06b6d4",
          teal: "#0d9488",
          dark: "#0b0f19",
          surface: "#111827",
          card: "#1e293b",
        },
        macro: {
          protein: "#6366f1", // Indigo
          carb: "#f59e0b",    // Amber
          fat: "#f43f5e",     // Rose
          calorie: "#10b981", // Emerald
        }
      },
      backgroundImage: {
        "gradient-radial": "radial-gradient(var(--tw-gradient-stops))",
        "health-glow": "radial-gradient(circle at 50% 50%, rgba(16, 185, 129, 0.15) 0%, rgba(15, 23, 42, 0) 70%)",
        "laser-gradient": "linear-gradient(180deg, rgba(16, 185, 129, 0) 0%, rgba(16, 185, 129, 0.8) 50%, rgba(16, 185, 129, 0) 100%)",
      },
      boxShadow: {
        "glow-sm": "0 0 15px -3px rgba(16, 185, 129, 0.25)",
        "glow-md": "0 0 25px -5px rgba(16, 185, 129, 0.35)",
        "glow-lg": "0 0 35px -5px rgba(16, 185, 129, 0.45)",
        "card-glass": "0 8px 32px 0 rgba(0, 0, 0, 0.15)",
      },
      animation: {
        "scan-laser": "scan 2.5s ease-in-out infinite",
        "pulse-slow": "pulse 4s cubic-bezier(0.4, 0, 0.6, 1) infinite",
        "spin-slow": "spin 12s linear infinite",
      },
      keyframes: {
        scan: {
          "0%, 100%": { transform: "translateY(0%)" },
          "50%": { transform: "translateY(360px)" },
        },
      },
    },
  },
  plugins: [],
};
export default config;
