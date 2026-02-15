namespace BakimZamani.Domain.Entities;

using BakimZamani.Domain.Enums;

/// <summary>
/// Notification entity for push and in-app notifications.
/// </summary>
public class Notification : BaseEntity
{
    /// <summary>
    /// Target user ID.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Notification title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Notification message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Notification type.
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// Whether the notification has been read.
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// When the notification was read.
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Related entity type (e.g., "Appointment", "Review").
    /// </summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>
    /// Related entity ID.
    /// </summary>
    public string? RelatedEntityId { get; set; }

    /// <summary>
    /// Additional data as JSON.
    /// </summary>
    public string? DataJson { get; set; }

    // Navigation properties
    /// <summary>
    /// Target user.
    /// </summary>
    public virtual User User { get; set; } = null!;
}

