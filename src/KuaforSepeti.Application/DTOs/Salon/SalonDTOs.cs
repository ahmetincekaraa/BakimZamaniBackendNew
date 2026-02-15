namespace BakimZamani.Application.DTOs.Salon;

using BakimZamani.Domain.Enums;

/// <summary>
/// Salon list item response.
/// </summary>
public class SalonListResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public string? LogoUrl { get; set; }
    public Gender TargetGender { get; set; }
    public double? Distance { get; set; } // km if location provided
}

/// <summary>
/// Salon detail response.
/// </summary>
public class SalonDetailResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public List<string> GalleryImages { get; set; } = new();
    public Gender TargetGender { get; set; }
    public bool IsVerified { get; set; }
    public string? TaxNumber { get; set; }
    // BusinessLicenseUrl is not returned to public usually, but maybe to owner.
    public string? BusinessLicenseUrl { get; set; } 
    public int MinAdvanceBookingHours { get; set; }
    public int MaxAdvanceBookingDays { get; set; }
    public int CancellationPolicyHours { get; set; }
    public List<StaffResponse> Staff { get; set; } = new();
    public List<ServiceResponse> Services { get; set; } = new();
    public List<WorkingHoursResponse> WorkingHours { get; set; } = new();
}

/// <summary>
/// Staff response DTO.
/// </summary>
public class StaffResponse
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }
    public List<string> ServiceIds { get; set; } = new();
}

/// <summary>
/// Service response DTO.
/// </summary>
public class ServiceResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public int DurationMinutes { get; set; }
    public ServiceCategory Category { get; set; }
    public Gender TargetGender { get; set; }
    public string? ImageUrl { get; set; }
}

/// <summary>
/// Working hours response DTO.
/// </summary>
public class WorkingHoursResponse
{
    public DayOfWeek DayOfWeek { get; set; }
    public string? StaffId { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
    public bool IsClosed { get; set; }
    public TimeSpan? BreakStartTime { get; set; }
    public TimeSpan? BreakEndTime { get; set; }
}

/// <summary>
/// Create salon request.
/// </summary>
public class CreateSalonRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public Gender TargetGender { get; set; } = Gender.Female;
    public string? TaxNumber { get; set; }
    public string? BusinessLicenseUrl { get; set; }
    public List<string>? GalleryImages { get; set; }
}

public class CreateSalonRegisterRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public Gender TargetGender { get; set; } = Gender.Female;
    
    public string? TaxNumber { get; set; }
    public Microsoft.AspNetCore.Http.IFormFile? BusinessLicense { get; set; }
    public List<Microsoft.AspNetCore.Http.IFormFile>? SalonPhotos { get; set; }
}

/// <summary>
/// Update salon request.
/// </summary>
public class UpdateSalonRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public List<string>? GalleryImages { get; set; }
    public Gender TargetGender { get; set; }
    public int MinAdvanceBookingHours { get; set; }
    public int MaxAdvanceBookingDays { get; set; }
    public int CancellationPolicyHours { get; set; }
}

/// <summary>
/// Create staff request.
/// </summary>
public class CreateStaffRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public List<string> ServiceIds { get; set; } = new();
}

/// <summary>
/// Create service request.
/// </summary>
public class CreateServiceRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public int DurationMinutes { get; set; }
    public ServiceCategory Category { get; set; }
    public Gender TargetGender { get; set; } = Gender.Unisex;
    public string? ImageUrl { get; set; }
}

/// <summary>
/// Update working hours request.
/// </summary>
public class UpdateWorkingHoursRequest
{
    public List<WorkingHoursItem> WorkingHours { get; set; } = new();
}

public class WorkingHoursItem
{
    public DayOfWeek DayOfWeek { get; set; }
    public string? StaffId { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
    public bool IsClosed { get; set; }
    public TimeSpan? BreakStartTime { get; set; }
    public TimeSpan? BreakEndTime { get; set; }
}

/// <summary>
/// Salon search/filter request.
/// </summary>
public class SalonSearchRequest
{
    public string? SearchTerm { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public Gender? TargetGender { get; set; }
    public ServiceCategory? ServiceCategory { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? RadiusKm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } // rating, distance, name
}

