namespace KuaforSepeti.Domain.Entities;

/// <summary>
/// Many-to-many relationship between Appointment and Service.
/// </summary>
public class AppointmentService : BaseEntity
{
    /// <summary>
    /// Appointment ID.
    /// </summary>
    public string AppointmentId { get; set; } = string.Empty;

    /// <summary>
    /// Service ID.
    /// </summary>
    public string ServiceId { get; set; } = string.Empty;

    /// <summary>
    /// Price at time of booking.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Duration at time of booking.
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Service name at time of booking (denormalized).
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    // Navigation properties
    /// <summary>
    /// Appointment.
    /// </summary>
    public virtual Appointment Appointment { get; set; } = null!;

    /// <summary>
    /// Service.
    /// </summary>
    public virtual Service Service { get; set; } = null!;
}
