namespace BakimZamani.Application.Services.Interfaces;

using BakimZamani.Domain.Enums;

/// <summary>
/// Notification service interface.
/// </summary>
public interface INotificationService
{
    Task SendNotificationAsync(string userId, string title, string message, NotificationType type, string? relatedEntityType = null, string? relatedEntityId = null);
    Task SendPushNotificationAsync(string userId, string title, string message, Dictionary<string, string>? data = null);
    Task SendBulkNotificationAsync(List<string> userIds, string title, string message, NotificationType type);
    Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, int limit = 50);
    Task MarkAsReadAsync(string notificationId, string userId);
    Task MarkAllAsReadAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
}


/// <summary>
/// Notification DTO.
/// </summary>
public class NotificationDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public string? RelatedEntityType { get; set; }
    public string? RelatedEntityId { get; set; }
    public DateTime CreatedAt { get; set; }
}

