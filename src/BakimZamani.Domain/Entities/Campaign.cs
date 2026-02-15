namespace BakimZamani.Domain.Entities;

/// <summary>
/// Campaign/Promotion entity for managing discount codes and promotions.
/// </summary>
public class Campaign : BaseEntity
{
    /// <summary>
    /// Campaign name/title.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Campaign description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Discount code (unique).
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Discount type: Percentage or FixedAmount.
    /// </summary>
    public string DiscountType { get; set; } = "Percentage"; // Percentage, FixedAmount

    /// <summary>
    /// Discount value (percentage or amount).
    /// </summary>
    public decimal DiscountValue { get; set; }

    /// <summary>
    /// Minimum order amount to apply discount.
    /// </summary>
    public decimal MinimumOrderAmount { get; set; }

    /// <summary>
    /// Maximum discount amount (for percentage discounts).
    /// </summary>
    public decimal? MaxDiscountAmount { get; set; }

    /// <summary>
    /// Campaign start date.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Campaign end date.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Maximum number of uses (null = unlimited).
    /// </summary>
    public int? MaxUsageCount { get; set; }

    /// <summary>
    /// Current usage count.
    /// </summary>
    public int CurrentUsageCount { get; set; }

    /// <summary>
    /// Whether the campaign is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Target audience: All, NewUsers, ReturningUsers.
    /// </summary>
    public string TargetAudience { get; set; } = "All";

    /// <summary>
    /// Admin who created the campaign.
    /// </summary>
    public string CreatedByAdminId { get; set; } = string.Empty;
}

