namespace KuaforSepeti.Application.DTOs.Salon;

/// <summary>
/// Service package response DTO.
/// </summary>
public class PackageResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DiscountPercent => OriginalPrice > 0 ? Math.Round((1 - Price / OriginalPrice) * 100, 0) : 0;
    public int TotalDurationMinutes { get; set; }
    public string? ImageUrl { get; set; }
    public List<PackageServiceItem> Services { get; set; } = new();
}

/// <summary>
/// Service item within a package.
/// </summary>
public class PackageServiceItem
{
    public string ServiceId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public int Quantity { get; set; } = 1;
}

/// <summary>
/// Create package request DTO.
/// </summary>
public class CreatePackageRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public List<PackageServiceInput> Services { get; set; } = new();
}

/// <summary>
/// Service input for package creation.
/// </summary>
public class PackageServiceInput
{
    public string ServiceId { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
}

/// <summary>
/// Update package request DTO.
/// </summary>
public class UpdatePackageRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool? IsActive { get; set; }
    public List<PackageServiceInput>? Services { get; set; }
}
