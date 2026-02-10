namespace KuaforSepeti.Application.DTOs.Admin;

/// <summary>
/// Review list item for admin panel.
/// </summary>
public class AdminReviewListItem
{
    public string Id { get; set; } = string.Empty;
    public string SalonId { get; set; } = string.Empty;
    public string SalonName { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string? Reply { get; set; }
    public DateTime? RepliedAt { get; set; }
    public bool IsVisible { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Filter request for admin review listing.
/// </summary>
public class AdminReviewFilterRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? MinRating { get; set; }
    public int? MaxRating { get; set; }
    public bool? IsVisible { get; set; }
    public string? SalonId { get; set; }
    public string? SearchTerm { get; set; }
}
