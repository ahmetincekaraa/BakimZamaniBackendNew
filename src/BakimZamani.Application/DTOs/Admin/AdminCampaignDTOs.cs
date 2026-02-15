namespace BakimZamani.Application.DTOs.Admin;

/// <summary>
/// Campaign DTOs for admin panel.
/// </summary>
public class CampaignListItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? MaxUsageCount { get; set; }
    public int CurrentUsageCount { get; set; }
    public bool IsActive { get; set; }
    public string TargetAudience { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Aktif, SÃ¼resi DolmuÅŸ, Pasif, Limit DolmuÅŸ
}

public class CreateCampaignRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string DiscountType { get; set; } = "Percentage";
    public decimal DiscountValue { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? MaxUsageCount { get; set; }
    public string TargetAudience { get; set; } = "All";
}

public class UpdateCampaignRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? MaxUsageCount { get; set; }
    public string? TargetAudience { get; set; }
    public bool? IsActive { get; set; }
}

public class CampaignDetailItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? MaxUsageCount { get; set; }
    public int CurrentUsageCount { get; set; }
    public bool IsActive { get; set; }
    public string TargetAudience { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CreatedByAdminId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

