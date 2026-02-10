namespace KuaforSepeti.Domain.Entities;

/// <summary>
/// Working hours for salon or specific staff.
/// </summary>
public class WorkingHours : BaseEntity
{
    /// <summary>
    /// Salon ID.
    /// </summary>
    public string SalonId { get; set; } = string.Empty;

    /// <summary>
    /// Staff ID if specific to a staff member (null = salon default).
    /// </summary>
    public string? StaffId { get; set; }

    /// <summary>
    /// Day of week (0 = Sunday, 6 = Saturday).
    /// </summary>
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>
    /// Opening time.
    /// </summary>
    public TimeSpan OpenTime { get; set; }

    /// <summary>
    /// Closing time.
    /// </summary>
    public TimeSpan CloseTime { get; set; }

    /// <summary>
    /// Whether closed on this day.
    /// </summary>
    public bool IsClosed { get; set; } = false;

    /// <summary>
    /// Break start time (optional).
    /// </summary>
    public TimeSpan? BreakStartTime { get; set; }

    /// <summary>
    /// Break end time (optional).
    /// </summary>
    public TimeSpan? BreakEndTime { get; set; }

    // Navigation properties
    /// <summary>
    /// Salon.
    /// </summary>
    public virtual Salon Salon { get; set; } = null!;

    /// <summary>
    /// Staff member (optional).
    /// </summary>
    public virtual Staff? Staff { get; set; }
}
