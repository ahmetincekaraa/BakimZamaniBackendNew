namespace KuaforSepeti.Domain.Entities;

/// <summary>
/// Many-to-many relationship between ServicePackage and Service.
/// </summary>
public class PackageService : BaseEntity
{
    /// <summary>
    /// Package ID.
    /// </summary>
    public string PackageId { get; set; } = string.Empty;

    /// <summary>
    /// Service ID.
    /// </summary>
    public string ServiceId { get; set; } = string.Empty;

    /// <summary>
    /// Quantity of this service in the package.
    /// </summary>
    public int Quantity { get; set; } = 1;

    // Navigation properties
    /// <summary>
    /// Package.
    /// </summary>
    public virtual ServicePackage Package { get; set; } = null!;

    /// <summary>
    /// Service.
    /// </summary>
    public virtual Service Service { get; set; } = null!;
}
