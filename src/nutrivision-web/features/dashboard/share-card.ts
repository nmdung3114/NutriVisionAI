import { mealTypeEmoji, MEAL_TYPES, MACRO_COLORS } from "@/lib/meal";
import type { Dashboard } from "@/types/api";

export const SHARE_W = 1080;
export const SHARE_H = 1350;

function roundRect(ctx: CanvasRenderingContext2D, x: number, y: number, w: number, h: number, r: number) {
  ctx.beginPath();
  ctx.moveTo(x + r, y);
  ctx.arcTo(x + w, y, x + w, y + h, r);
  ctx.arcTo(x + w, y + h, x, y + h, r);
  ctx.arcTo(x, y + h, x, y, r);
  ctx.arcTo(x, y, x + w, y, r);
  ctx.closePath();
}

const vn = (n: number, d = 0) => n.toLocaleString("vi-VN", { maximumFractionDigits: d });

/**
 * Vẽ thẻ tổng kết ngày (1080×1350, tỷ lệ 4:5 hợp mạng xã hội) bằng Canvas 2D:
 * vòng calo, 3 thanh đạm/tinh bột/béo, các bữa đã ăn, chuỗi ngày.
 */
export function drawShareCard(canvas: HTMLCanvasElement, d: Dashboard, opts: { streak?: number; name?: string; font: string }) {
  canvas.width = SHARE_W;
  canvas.height = SHARE_H;
  const ctx = canvas.getContext("2d");
  if (!ctx) return;
  const F = (w: number, size: number) => `${w} ${size}px ${opts.font}`;

  // Nền gradient xanh tươi + đốm sáng
  const bg = ctx.createLinearGradient(0, 0, SHARE_W, SHARE_H);
  bg.addColorStop(0, "#2ed16f");
  bg.addColorStop(1, "#0f7a3b");
  ctx.fillStyle = bg;
  ctx.fillRect(0, 0, SHARE_W, SHARE_H);
  ctx.fillStyle = "rgba(255,255,255,0.08)";
  ctx.beginPath();
  ctx.arc(SHARE_W - 120, 140, 260, 0, Math.PI * 2);
  ctx.fill();
  ctx.beginPath();
  ctx.arc(80, SHARE_H - 60, 200, 0, Math.PI * 2);
  ctx.fill();

  // Tiêu đề
  ctx.fillStyle = "#ffffff";
  ctx.font = F(800, 44);
  ctx.fillText("NutriVision AI", 72, 110);
  ctx.font = F(500, 30);
  ctx.globalAlpha = 0.85;
  const [y, m, dd] = d.ngay.split("-");
  ctx.fillText(`${opts.name ? opts.name + " · " : ""}Ngày ${dd}/${m}/${y}`, 72, 158);
  ctx.globalAlpha = 1;
  if (opts.streak && opts.streak > 0) {
    const label = `🔥 ${opts.streak} ngày liên tiếp`;
    ctx.font = F(800, 30);
    const w = ctx.measureText(label).width + 48;
    ctx.fillStyle = "rgba(255,255,255,0.18)";
    roundRect(ctx, SHARE_W - 72 - w, 78, w, 64, 32);
    ctx.fill();
    ctx.fillStyle = "#ffffff";
    ctx.fillText(label, SHARE_W - 72 - w + 24, 121);
  }

  // Thẻ trắng chính
  ctx.fillStyle = "#ffffff";
  roundRect(ctx, 56, 220, SHARE_W - 112, SHARE_H - 300, 48);
  ctx.fill();

  // Vòng calo
  const cx = SHARE_W / 2, cy = 470, r = 170, lw = 34;
  const pct = d.mucTieuCalo > 0 ? d.homNay.calo / d.mucTieuCalo : 0;
  ctx.lineCap = "round";
  ctx.lineWidth = lw;
  ctx.strokeStyle = "#e8efeb";
  ctx.beginPath();
  ctx.arc(cx, cy, r, 0, Math.PI * 2);
  ctx.stroke();
  ctx.strokeStyle = pct > 1.1 ? "#ff795e" : "#21b45a";
  ctx.beginPath();
  ctx.arc(cx, cy, r, -Math.PI / 2, -Math.PI / 2 + Math.PI * 2 * Math.min(pct, 1));
  ctx.stroke();
  ctx.textAlign = "center";
  ctx.fillStyle = "#1a2420";
  ctx.font = F(800, 96);
  ctx.fillText(vn(d.homNay.calo), cx, cy + 20);
  ctx.font = F(600, 30);
  ctx.fillStyle = "#5b6b64";
  ctx.fillText(`/ ${vn(d.mucTieuCalo)} kcal · ${vn(d.homNay.phanTramMucTieu)}%`, cx, cy + 70);
  ctx.textAlign = "left";

  // Thanh macro
  const macros: [string, number, number, string][] = [
    ["Chất đạm", d.homNay.chatDam, d.mucTieuMacro.chatDam, MACRO_COLORS.chatDam],
    ["Tinh bột", d.homNay.tinhBot, d.mucTieuMacro.tinhBot, MACRO_COLORS.tinhBot],
    ["Chất béo", d.homNay.chatBeo, d.mucTieuMacro.chatBeo, MACRO_COLORS.chatBeo],
  ];
  let by = 730;
  for (const [label, val, target, color] of macros) {
    ctx.fillStyle = "#1a2420";
    ctx.font = F(700, 30);
    ctx.fillText(label, 120, by);
    ctx.textAlign = "right";
    ctx.fillStyle = "#5b6b64";
    ctx.font = F(600, 28);
    ctx.fillText(`${vn(val, 1)} / ${vn(target)} g`, SHARE_W - 120, by);
    ctx.textAlign = "left";
    ctx.fillStyle = "#eef2f0";
    roundRect(ctx, 120, by + 18, SHARE_W - 240, 20, 10);
    ctx.fill();
    ctx.fillStyle = color;
    roundRect(ctx, 120, by + 18, Math.max(20, (SHARE_W - 240) * Math.min(target > 0 ? val / target : 0, 1)), 20, 10);
    ctx.fill();
    by += 92;
  }

  // Các bữa đã ăn
  ctx.fillStyle = "#1a2420";
  ctx.font = F(800, 32);
  ctx.fillText("Bữa ăn hôm nay", 120, by + 24);
  let my = by + 80;
  for (const t of MEAL_TYPES) {
    const meals = d.buaAnHomNay.filter((x) => x.loaiBuaAn === t.value);
    if (!meals.length) continue;
    const names = meals.map((x) => x.tenMonAn).join(", ");
    const kcal = meals.reduce((s, x) => s + x.tongDinhDuong.calo, 0);
    ctx.font = F(600, 28);
    ctx.fillStyle = "#1a2420";
    let text = `${mealTypeEmoji(t.value)} ${names}`;
    while (ctx.measureText(text).width > SHARE_W - 420 && text.length > 4) text = text.slice(0, -2) + "…";
    ctx.fillText(text, 120, my);
    ctx.textAlign = "right";
    ctx.fillStyle = "#1a9a4c";
    ctx.font = F(800, 28);
    ctx.fillText(`${vn(kcal)} kcal`, SHARE_W - 120, my);
    ctx.textAlign = "left";
    my += 50;
    if (my > SHARE_H - 120) break;
  }
  if (!d.buaAnHomNay.length) {
    ctx.font = F(500, 28);
    ctx.fillStyle = "#5b6b64";
    ctx.fillText("Chưa ghi nhận bữa ăn nào.", 120, my);
  }

  ctx.textAlign = "center";
  ctx.fillStyle = "rgba(255,255,255,0.9)";
  ctx.font = F(600, 26);
  ctx.fillText("Theo dõi dinh dưỡng bằng AI cùng NutriVision AI", SHARE_W / 2, SHARE_H - 34);
  ctx.textAlign = "left";
}
