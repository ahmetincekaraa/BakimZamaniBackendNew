using BakimZamani.Application.DTOs.Common;
using BakimZamani.Application.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BakimZamani.Infrastructure.Services;

/// <summary>
/// Email service using Brevo SMTP with MailKit.
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    // Protected admin email addresses
    private static readonly string[] ProtectedAdminEmails = new[]
    {
        "admin@bakimzamani.com",
        "ahmet@bakimzamani.com",
        "alper@bakimzamani.com"
    };

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        await SendEmailAsync(new[] { to }, subject, htmlBody);
    }

    public async Task SendEmailAsync(IEnumerable<string> to, string subject, string htmlBody)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));

            foreach (var recipient in to)
            {
                message.To.Add(MailboxAddress.Parse(recipient));
            }

            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {Recipients}: {Subject}", string.Join(", ", to), subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipients}: {Subject}", string.Join(", ", to), subject);
            // Don't throw â€” email failure shouldn't break the main flow
        }
    }

    public async Task SendNewSalonNotificationAsync(string salonName, string ownerName, string city)
    {
        var subject = $"ðŸ†• Yeni Salon BaÅŸvurusu: {salonName}";

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #F3F4F6; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #6366F1, #4F46E5); padding: 32px; text-align: center; color: white; }}
        .header h1 {{ margin: 0; font-size: 24px; }}
        .header p {{ margin: 8px 0 0; opacity: 0.9; font-size: 14px; }}
        .content {{ padding: 32px; }}
        .badge {{ display: inline-block; background: #EEF2FF; color: #6366F1; padding: 4px 12px; border-radius: 20px; font-size: 12px; font-weight: 600; margin-bottom: 16px; }}
        .info-card {{ background: #F9FAFB; border-radius: 12px; padding: 20px; margin: 16px 0; }}
        .info-row {{ display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #E5E7EB; }}
        .info-row:last-child {{ border-bottom: none; }}
        .info-label {{ color: #6B7280; font-size: 14px; }}
        .info-value {{ font-weight: 600; color: #1F2937; font-size: 14px; }}
        .cta {{ display: inline-block; background: #6366F1; color: white; padding: 12px 24px; border-radius: 8px; text-decoration: none; font-weight: 600; margin-top: 16px; }}
        .footer {{ text-align: center; padding: 16px 32px; color: #9CA3AF; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>BakÄ±m ZamanÄ±</h1>
            <p>Super Admin Bildirimi</p>
        </div>
        <div class='content'>
            <span class='badge'>ðŸ†• Yeni BaÅŸvuru</span>
            <h2 style='margin: 0 0 8px; color: #1F2937;'>Yeni Salon BaÅŸvurusu</h2>
            <p style='color: #6B7280; margin: 0 0 16px;'>Yeni bir salon sisteme kayÄ±t oldu ve onayÄ±nÄ±zÄ± bekliyor.</p>
            
            <div class='info-card'>
                <div class='info-row'>
                    <span class='info-label'>Salon AdÄ±</span>
                    <span class='info-value'>{salonName}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Salon Sahibi</span>
                    <span class='info-value'>{ownerName}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Åžehir</span>
                    <span class='info-value'>{city}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Tarih</span>
                    <span class='info-value'>{DateTime.UtcNow:dd.MM.yyyy HH:mm}</span>
                </div>
            </div>

            <p style='color: #6B7280; font-size: 14px;'>Admin paneline girerek salonu inceleyebilir ve onaylayabilirsiniz.</p>
            <a href='http://localhost:3001/salons' class='cta'>Admin Paneline Git â†’</a>
        </div>
        <div class='footer'>
            Bu e-posta BakÄ±m ZamanÄ± sistemi tarafÄ±ndan otomatik gÃ¶nderilmiÅŸtir.
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(ProtectedAdminEmails, subject, htmlBody);
    }
}

