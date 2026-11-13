import type { MetadataRoute } from "next";

export default function manifest(): MetadataRoute.Manifest {
  return {
    name: "NutriVision AI – Theo dõi dinh dưỡng bằng AI",
    short_name: "NutriVision",
    description: "Chụp ảnh món ăn, AI tính calo và dinh dưỡng, theo dõi mục tiêu sức khỏe mỗi ngày.",
    lang: "vi",
    start_url: "/dashboard",
    scope: "/",
    display: "standalone",
    orientation: "portrait",
    background_color: "#f5f8f6",
    theme_color: "#21b45a",
    categories: ["health", "food", "lifestyle"],
    icons: [
      { src: "/icon-192.png", sizes: "192x192", type: "image/png", purpose: "any" },
      { src: "/icon-512.png", sizes: "512x512", type: "image/png", purpose: "any" },
      { src: "/icon-maskable-512.png", sizes: "512x512", type: "image/png", purpose: "maskable" },
    ],
    shortcuts: [
      { name: "Thêm bữa ăn", short_name: "Thêm bữa", url: "/meals/new", icons: [{ src: "/icon-192.png", sizes: "192x192" }] },
      { name: "Lịch sử bữa ăn", short_name: "Lịch sử", url: "/meals", icons: [{ src: "/icon-192.png", sizes: "192x192" }] },
    ],
  };
}
