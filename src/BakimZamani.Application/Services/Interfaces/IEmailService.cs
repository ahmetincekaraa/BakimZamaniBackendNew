namespace BakimZamani.Application.Services.Interfaces;

/// <summary>
/// Email service interface.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send a generic email.
    /// </summary>
    Task SendEmailAsync(string to, string subject, string htmlBody);

    /// <summary>
    /// Send email to multiple recipients.
    /// </summary>
    Task SendEmailAsync(IEnumerable<string> to, string subject, string htmlBody);

    /// <summary>
    /// Notify protected admins about a new salon registration.
    /// </summary>
    Task SendNewSalonNotificationAsync(string salonName, string ownerName, string city);
}

