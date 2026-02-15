namespace BakimZamani.Application.DTOs.Admin;

/// <summary>
/// Announcement DTOs.
/// </summary>
public class AnnouncementListItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string TargetAudience { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateAnnouncementRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime? ExpirationDate { get; set; }
    public string TargetAudience { get; set; } = "All";
}

public class UpdateAnnouncementRequest
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? TargetAudience { get; set; }
}

/// <summary>
/// FAQ DTOs.
/// </summary>
public class FAQListItem
{
    public string Id { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class CreateFAQRequest
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public int DisplayOrder { get; set; }
}

public class UpdateFAQRequest
{
    public string? Question { get; set; }
    public string? Answer { get; set; }
    public string? Category { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
}

