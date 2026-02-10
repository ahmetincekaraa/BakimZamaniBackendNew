namespace KuaforSepeti.Domain.Entities;

/// <summary>
/// Review entity for customer feedback on salons.
/// </summary>
public class Review : BaseEntity
{
    /// <summary>
    /// Salon ID.
    /// </summary>
    public string SalonId { get; set; } = string.Empty;

    /// <summary>
    /// Customer user ID.
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Related appointment ID (optional).
    /// </summary>
    public string? AppointmentId { get; set; }

    /// <summary>
    /// Rating (1-5).
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// Review comment.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Salon owner reply.
    /// </summary>
    public string? Reply { get; set; }

    /// <summary>
    /// When the reply was posted.
    /// </summary>
    public DateTime? RepliedAt { get; set; }

    /// <summary>
    /// Whether the review is visible (can be hidden by admin).
    /// </summary>
    public bool IsVisible { get; set; } = true;

    // Navigation properties
    /// <summary>
    /// Salon.
    /// </summary>
    public virtual Salon Salon { get; set; } = null!;

    /// <summary>
    /// Customer.
    /// </summary>
    public virtual User Customer { get; set; } = null!;
}
