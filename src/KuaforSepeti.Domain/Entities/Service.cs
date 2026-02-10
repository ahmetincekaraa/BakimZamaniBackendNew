namespace KuaforSepeti.Domain.Entities;

using KuaforSepeti.Domain.Enums;

/// <summary>
/// Service entity representing services offered by salons.
/// </summary>
public class Service : BaseEntity
{
    /// <summary>
    /// Salon ID this service belongs to.
    /// </summary>
    public string SalonId { get; set; } = string.Empty;

    /// <summary>
    /// Service name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Service description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Price in TRY.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Discounted price if applicable.
    /// </summary>
    public decimal? DiscountedPrice { get; set; }

    /// <summary>
    /// Duration in minutes.
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Service category.
    /// </summary>
    public ServiceCategory Category { get; set; }

    /// <summary>
    /// Target gender for this service.
    /// </summary>
    public Gender TargetGender { get; set; } = Gender.Unisex;

    /// <summary>
    /// Service image URL.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Whether the service is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order in the service list.
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Embedding vector for semantic search (1536 dimensions for OpenAI embeddings).
    /// </summary>
    public float[]? Embedding { get; set; }

    /// <summary>
    /// Search keywords/tags for better matching.
    /// </summary>
    public string? SearchKeywords { get; set; }

    // Navigation properties
    /// <summary>
    /// Salon this service belongs to.
    /// </summary>
    public virtual Salon Salon { get; set; } = null!;

    /// <summary>
    /// Staff members who can provide this service.
    /// </summary>
    public virtual ICollection<StaffService> StaffServices { get; set; } = new List<StaffService>();

    /// <summary>
    /// Appointments including this service.
    /// </summary>
    public virtual ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();
}
