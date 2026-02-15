namespace BakimZamani.Application.DTOs.Admin;

/// <summary>
/// Admin notification item.
/// </summary>
public class AdminNotificationItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "NewSalon", "NewReview", "System"
    public string? RelatedEntityType { get; set; }
    public string? RelatedEntityId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

