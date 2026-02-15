namespace BakimZamani.Domain.Entities;

using BakimZamani.Domain.Enums;

/// <summary>
/// Salon entity representing a beauty salon or barbershop.
/// </summary>
public class Salon : BaseEntity
{
    /// <summary>
    /// Salon name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Salon description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Full address.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// City name.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// District name.
    /// </summary>
    public string District { get; set; } = string.Empty;

    /// <summary>
    /// Contact phone number.
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Owner user ID.
    /// </summary>
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>
    /// Latitude for location.
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Longitude for location.
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Average rating (1-5).
    /// </summary>
    public decimal Rating { get; set; } = 0;

    /// <summary>
    /// Total number of reviews.
    /// </summary>
    public int ReviewCount { get; set; } = 0;

    /// <summary>
    /// Salon logo/main image URL.
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Cover/banner image URL.
    /// </summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// Gallery image URLs (JSON array).
    /// </summary>
    public string? GalleryImagesJson { get; set; }

    /// <summary>
    /// Target gender for the salon.
    /// </summary>
    public Gender TargetGender { get; set; } = Gender.Female;

    /// <summary>
    /// Whether the salon is active and accepting appointments.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Reason for deletion/cancellation (if deleted).
    /// </summary>
    public string? DeletionReason { get; set; }

    /// <summary>
    /// Whether the salon is verified by admin.
    /// </summary>
    public bool IsVerified { get; set; } = false;

    /// <summary>
    /// Minimum advance booking time in hours.
    /// </summary>
    public int MinAdvanceBookingHours { get; set; } = 1;

    /// <summary>
    /// Maximum advance booking time in days.
    /// </summary>
    public int MaxAdvanceBookingDays { get; set; } = 30;

    /// <summary>
    /// Cancellation policy in hours before appointment.
    /// </summary>
    public int CancellationPolicyHours { get; set; } = 24;

    /// <summary>
    /// Slot duration in minutes for appointments.
    /// </summary>
    public int SlotDurationMinutes { get; set; } = 30;

    /// <summary>
    /// Maximum customers per hour capacity.
    /// </summary>
    public int MaxCustomersPerHour { get; set; } = 2;

    /// <summary>
    /// Instagram profile URL.
    /// </summary>
    public string? InstagramUrl { get; set; }

    /// <summary>
    /// Facebook page URL.
    /// </summary>
    public string? FacebookUrl { get; set; }

    /// <summary>
    /// Website URL.
    /// </summary>
    public string? WebsiteUrl { get; set; }

    /// <summary>
    /// Salon email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Tax Identification Number (Vergi NumarasÄ±).
    /// </summary>
    public string? TaxNumber { get; set; }

    /// <summary>
    /// URL to the business license/permit document (Ä°ÅŸ Yeri RuhsatÄ±).
    /// </summary>
    public string? BusinessLicenseUrl { get; set; }

    /// <summary>
    /// Salon amenities as JSON array (WiFi, Parking, AC, Tea/Coffee, etc.)
    /// </summary>
    public string? AmenitiesJson { get; set; }

    // Navigation properties
    /// <summary>
    /// Owner of the salon.
    /// </summary>
    public virtual User Owner { get; set; } = null!;

    /// <summary>
    /// Staff members of the salon.
    /// </summary>
    public virtual ICollection<Staff> StaffMembers { get; set; } = new List<Staff>();

    /// <summary>
    /// Services offered by the salon.
    /// </summary>
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();

    /// <summary>
    /// Working hours for the salon.
    /// </summary>
    public virtual ICollection<WorkingHours> WorkingHours { get; set; } = new List<WorkingHours>();

    /// <summary>
    /// Appointments at the salon.
    /// </summary>
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    /// <summary>
    /// Reviews for the salon.
    /// </summary>
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    /// <summary>
    /// Service packages offered by the salon.
    /// </summary>
    public virtual ICollection<ServicePackage> ServicePackages { get; set; } = new List<ServicePackage>();

    // Audit Properties
    public string? ApprovedByAdminId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public virtual User? ApprovedByAdmin { get; set; }

    public string? SuspendedByAdminId { get; set; }
    public DateTime? SuspendedAt { get; set; }
    public virtual User? SuspendedByAdmin { get; set; }
}

