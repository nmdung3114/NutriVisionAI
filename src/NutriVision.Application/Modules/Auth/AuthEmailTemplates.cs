using System.Net;

namespace NutriVision.Application.Modules.Auth;

/// <summary>Mẫu email tiếng Việt. Mọi dữ liệu người dùng đều được HTML-encode.</summary>
public static class AuthEmailTemplates
{
    public static (string Subject, string Html) Verify(string hoTen, string link, int hours) => (
        "Xác thực tài khoản NutriVision AI",
        Layout($"Chào {WebUtility.HtmlEncode(hoTen)}!",
            $"Cảm ơn bạn đã đăng ký NutriVision AI. Vui lòng bấm nút bên dưới để xác thực email. Liên kết có hiệu lực {hours} giờ và chỉ dùng được một lần.",
            link, "Xác thực email"));

    public static (string Subject, string Html) Reset(string hoTen, string link, int minutes) => (
        "Đặt lại mật khẩu NutriVision AI",
        Layout($"Chào {WebUtility.HtmlEncode(hoTen)}!",
            $"Chúng tôi nhận được yêu cầu đặt lại mật khẩu. Liên kết có hiệu lực {minutes} phút và chỉ dùng được một lần. Nếu không phải bạn yêu cầu, hãy bỏ qua email này.",
            link, "Đặt lại mật khẩu"));

    private static string Layout(string title, string body, string link, string button)
    {
        var safe = WebUtility.HtmlEncode(link);
        return $"""
        <div style="font-family:Segoe UI,Arial,sans-serif;max-width:520px;margin:auto;padding:24px;border:1px solid #e9eef1;border-radius:16px">
          <h2 style="color:#168d47;margin:0 0 12px">NutriVision AI</h2>
          <h3 style="margin:0 0 8px">{title}</h3>
          <p style="color:#475569;line-height:1.6">{body}</p>
          <p style="text-align:center;margin:28px 0"><a href="{safe}" style="background:#21b45a;color:#fff;text-decoration:none;padding:12px 28px;border-radius:12px;font-weight:600">{button}</a></p>
          <p style="color:#94a3b8;font-size:12px">Nếu nút không hoạt động, hãy sao chép liên kết sau vào trình duyệt:<br>{safe}</p>
        </div>
        """;
    }
}
