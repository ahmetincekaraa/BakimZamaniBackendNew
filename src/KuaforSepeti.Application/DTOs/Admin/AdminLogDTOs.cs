namespace BakimZamani.Application.DTOs.Admin;

/// <summary>
/// Admin log list item.
/// </summary>
public class AdminLogItem
{
    public string Id { get; set; } = string.Empty;
    public string AdminId { get; set; } = string.Empty;
    public string AdminName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string TargetEntity { get; set; } = string.Empty;
    public string? TargetId { get; set; }
    public string? TargetName { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}

