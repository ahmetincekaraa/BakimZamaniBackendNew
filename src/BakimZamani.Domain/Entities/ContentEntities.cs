namespace BakimZamani.Domain.Entities;

/// <summary>
/// System announcement or notification for users.
/// </summary>
public class Announcement : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? ExpirationDate { get; set; }
    public string TargetAudience { get; set; } = "All"; // All, Salons, Users
    public string CreatedByAdminId { get; set; } = string.Empty;
}

/// <summary>
/// Frequently Asked Question.
/// </summary>
public class FAQ : BaseEntity
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string Category { get; set; } = "General"; // General, Account, Payment, etc.
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

