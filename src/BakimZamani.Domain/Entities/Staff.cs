namespace BakimZamani.Domain.Entities;

/// <summary>
/// Staff entity representing salon employees.
/// </summary>
public class Staff : BaseEntity
{
    /// <summary>
    /// Salon ID this staff belongs to.
    /// </summary>
    public string SalonId { get; set; } = string.Empty;

    /// <summary>
    /// User ID if staff has a user account.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Staff full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Staff title/position (e.g., "Senior Stylist").
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Staff bio/description.
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Profile image URL.
    /// </summary>
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// Phone number.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Whether the staff is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order in the staff list.
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    // Navigation properties
    /// <summary>
    /// Salon this staff belongs to.
    /// </summary>
    public virtual Salon Salon { get; set; } = null!;

    /// <summary>
    /// User account if linked.
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// Services this staff can provide.
    /// </summary>
    public virtual ICollection<StaffService> StaffServices { get; set; } = new List<StaffService>();

    /// <summary>
    /// Working hours for this staff.
    /// </summary>
    public virtual ICollection<WorkingHours> WorkingHours { get; set; } = new List<WorkingHours>();

    /// <summary>
    /// Appointments assigned to this staff.
    /// </summary>
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

