using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using NutriVision.Application.Abstractions;
using NutriVision.Infrastructure.Settings;

namespace NutriVision.Infrastructure.Email;

/// <summary>Gửi email qua SMTP (cấu hình mục Email trong appsettings / biến môi trường).</summary>
public class SmtpEmailSender(IOptions<EmailSettings> options) : IEmailSender
{
    private readonly EmailSettings _s = options.Value;

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken ct = default)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_s.FromName, _s.FromAddress));
        msg.To.Add(new MailboxAddress(toName, toEmail));
        msg.Subject = subject;
        msg.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_s.Host, _s.Port, _s.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto, ct);
        if (!string.IsNullOrEmpty(_s.User)) await client.AuthenticateAsync(_s.User, _s.Password, ct);
        await client.SendAsync(msg, ct);
        await client.DisconnectAsync(true, ct);
    }
}

/// <summary>
/// Môi trường phát triển: không gửi thư thật; ghi email (kèm link) ra log và thư mục outbox để tự kiểm thử luồng xác thực.
/// </summary>
public class OutboxEmailSender(IOptions<EmailSettings> options, ILogger<OutboxEmailSender> logger) : IEmailSender
{
    private readonly EmailSettings _s = options.Value;

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken ct = default)
    {
        Directory.CreateDirectory(_s.OutboxPath);
        var safe = string.Concat(toEmail.Select(c => char.IsLetterOrDigit(c) ? c : '_'));
        var file = Path.Combine(_s.OutboxPath, $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{safe}.html");
        await File.WriteAllTextAsync(file, $"<!-- To: {toEmail} | Subject: {subject} -->\n{htmlBody}", ct);
        logger.LogInformation("[EMAIL-OUTBOX] To={To} Subject={Subject} File={File}", toEmail, subject, file);
    }
}
