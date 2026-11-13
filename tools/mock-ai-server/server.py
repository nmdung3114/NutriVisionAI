#!/usr/bin/env python3
"""
MOCK dịch vụ AI nhận diện món ăn cho NutriVision AI – chỉ dùng để phát triển/kiểm thử/demo.
KHÔNG nhận diện thật: chọn 1 kết quả mẫu theo hash của ảnh (cùng ảnh → cùng kết quả).

Đây cũng là ví dụ mẫu về HỢP ĐỒNG mà dịch vụ AI thật của nhóm AI cần tuân theo (docs/ai-integration.md):
  POST /analyze   Content-Type: application/json   [Authorization: Bearer <key>]
  body:     {"image": "<base64>", "contentType": "image/jpeg", "ngonNgu": "vi"}
  200 OK:   {"isFood": true, "tenMonAn": "...", "doTinCay": 0.0-1.0, "thanhPhan": [{"ten": "...", "gram": 123}]}
  4xx:      lỗi do request (web KHÔNG thử lại)   ·   429/5xx: lỗi tạm thời (web thử lại tối đa 2 lần)

Chạy:  python3 tools/mock-ai-server/server.py [--port 8000] [--key BI_MAT] [--delay 1.5]
Rồi chạy backend với:  Ai__Provider=Http Ai__BaseUrl=http://localhost:8000 [Ai__ApiKey=BI_MAT]
Chỉ dùng thư viện chuẩn Python 3.8+.
"""
import argparse
import base64
import binascii
import hashlib
import json
import time
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer

# Tên thành phần trùng với bảng thực phẩm tham chiếu để web tự khớp dinh dưỡng.
SAMPLES = [
    {"isFood": True, "tenMonAn": "Cơm tấm sườn bì chả", "doTinCay": 0.86,
     "thanhPhan": [{"ten": "Cơm tấm sườn bì chả", "gram": 420}, {"ten": "Dưa leo", "gram": 40}]},
    {"isFood": True, "tenMonAn": "Phở bò tái nạm", "doTinCay": 0.91,
     "thanhPhan": [{"ten": "Phở bò tái nạm", "gram": 480}, {"ten": "Rau sống các loại", "gram": 30}]},
    {"isFood": True, "tenMonAn": "Bún chả Hà Nội", "doTinCay": 0.78,
     "thanhPhan": [{"ten": "Bún chả Hà Nội", "gram": 380}, {"ten": "Rau sống các loại", "gram": 50}]},
    {"isFood": True, "tenMonAn": "Cơm gà luộc rau cải", "doTinCay": 0.72,
     "thanhPhan": [{"ten": "Cơm trắng gạo tẻ", "gram": 200}, {"ten": "Gà luộc", "gram": 140}, {"ten": "Rau cải luộc", "gram": 120}]},
    {"isFood": True, "tenMonAn": "Bánh mì ốp la", "doTinCay": 0.83,
     "thanhPhan": [{"ten": "Bánh mì ốp la", "gram": 150}, {"ten": "Cà chua", "gram": 30}]},
]

ALLOWED_TYPES = {"image/jpeg", "image/png", "image/webp"}


class Handler(BaseHTTPRequestHandler):
    server_version = "NutriVisionMockAI/1.0"

    def _send(self, code, payload):
        body = json.dumps(payload, ensure_ascii=False).encode("utf-8")
        self.send_response(code)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Content-Length", str(len(body)))
        self.end_headers()
        self.wfile.write(body)

    def do_GET(self):
        if self.path == "/health":
            return self._send(200, {"status": "ok", "mock": True})
        self._send(404, {"error": "not found"})

    def do_POST(self):
        if self.path != "/analyze":
            return self._send(404, {"error": "not found"})
        if self.server.api_key and self.headers.get("Authorization") != f"Bearer {self.server.api_key}":
            return self._send(401, {"error": "invalid api key"})
        try:
            length = int(self.headers.get("Content-Length", "0"))
            req = json.loads(self.rfile.read(length))
            image = base64.b64decode(req["image"], validate=True)
            content_type = req.get("contentType", "")
        except (ValueError, KeyError, TypeError, binascii.Error):
            return self._send(400, {"error": "body phải là JSON {image: base64, contentType}"})
        if content_type not in ALLOWED_TYPES or not image:
            return self._send(400, {"error": "contentType phải là image/jpeg, image/png hoặc image/webp"})

        time.sleep(self.server.delay)  # giả lập thời gian suy luận của mô hình
        idx = int(hashlib.sha256(image).hexdigest(), 16) % len(SAMPLES)
        self._send(200, SAMPLES[idx])

    def log_message(self, fmt, *args):
        print(f"[mock-ai] {self.address_string()} {fmt % args}", flush=True)


def main():
    ap = argparse.ArgumentParser(description="Mock AI server cho NutriVision AI")
    ap.add_argument("--host", default="127.0.0.1")
    ap.add_argument("--port", type=int, default=8000)
    ap.add_argument("--key", default="", help="nếu đặt, yêu cầu header Authorization: Bearer <key>")
    ap.add_argument("--delay", type=float, default=1.5, help="giây chờ giả lập suy luận")
    args = ap.parse_args()
    httpd = ThreadingHTTPServer((args.host, args.port), Handler)
    httpd.api_key, httpd.delay = args.key, args.delay
    print(f"Mock AI đang chạy tại http://{args.host}:{args.port}/analyze (Ctrl+C để dừng)", flush=True)
    httpd.serve_forever()


if __name__ == "__main__":
    main()
