namespace BakimZamani.Application.DTOs.Common;

/// <summary>
/// Email settings for Brevo SMTP.
/// </summary>
public class EmailSettings
{
    public string SmtpHost { get; set; } = "smtp-relay.brevo.com";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "noreply@BakimZamani.com";
    public string FromName { get; set; } = "BakÄ±m ZamanÄ±";
}

