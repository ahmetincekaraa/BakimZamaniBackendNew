namespace KuaforSepeti.Domain.Entities;

/// <summary>
/// Admin action log entity for audit trail.
/// </summary>
public class AdminLog : BaseEntity
{
    /// <summary>
    /// Admin user who performed the action.
    /// </summary>
    public string AdminId { get; set; } = string.Empty;

    /// <summary>
    /// Admin user name for display.
    /// </summary>
    public string AdminName { get; set; } = string.Empty;

    /// <summary>
    /// Action type (e.g., "SalonApproved", "UserSuspended", "ReviewHidden").
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Target entity type (e.g., "Salon", "User", "Review").
    /// </summary>
    public string TargetEntity { get; set; } = string.Empty;

    /// <summary>
    /// Target entity ID.
    /// </summary>
    public string? TargetId { get; set; }

    /// <summary>
    /// Target entity name for display.
    /// </summary>
    public string? TargetName { get; set; }

    /// <summary>
    /// Additional details about the action.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// IP address of the admin.
    /// </summary>
    public string? IpAddress { get; set; }

    // Navigation properties
    public virtual User Admin { get; set; } = null!;
}
