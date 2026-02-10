namespace KuaforSepeti.Domain.Entities;

/// <summary>
/// Service package entity - a bundle of multiple services with a special price.
/// </summary>
public class ServicePackage : BaseEntity
{
    /// <summary>
    /// Salon ID this package belongs to.
    /// </summary>
    public string SalonId { get; set; } = string.Empty;

    /// <summary>
    /// Package name (e.g., "Gelin Paketi", "VIP Bakım").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Package description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Discounted package price.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Original total price (sum of individual services).
    /// </summary>
    public decimal OriginalPrice { get; set; }

    /// <summary>
    /// Total duration in minutes.
    /// </summary>
    public int TotalDurationMinutes { get; set; }

    /// <summary>
    /// Package image URL.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Whether the package is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order in the package list.
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    // Navigation properties
    /// <summary>
    /// Salon this package belongs to.
    /// </summary>
    public virtual Salon Salon { get; set; } = null!;

    /// <summary>
    /// Services included in this package.
    /// </summary>
    public virtual ICollection<PackageService> PackageServices { get; set; } = new List<PackageService>();
}
