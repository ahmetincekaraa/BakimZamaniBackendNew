namespace KuaforSepeti.Domain.Entities;

/// <summary>
/// Many-to-many relationship between Staff and Service.
/// </summary>
public class StaffService : BaseEntity
{
    /// <summary>
    /// Staff ID.
    /// </summary>
    public string StaffId { get; set; } = string.Empty;

    /// <summary>
    /// Service ID.
    /// </summary>
    public string ServiceId { get; set; } = string.Empty;

    // Navigation properties
    /// <summary>
    /// Staff member.
    /// </summary>
    public virtual Staff Staff { get; set; } = null!;

    /// <summary>
    /// Service.
    /// </summary>
    public virtual Service Service { get; set; } = null!;
}
