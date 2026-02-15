namespace BakimZamani.Domain.Entities;

using BakimZamani.Domain.Enums;

/// <summary>
/// Appointment entity for booking management.
/// </summary>
public class Appointment : BaseEntity
{
    /// <summary>
    /// Salon ID.
    /// </summary>
    public string SalonId { get; set; } = string.Empty;

    /// <summary>
    /// Assigned staff ID.
    /// </summary>
    public string StaffId { get; set; } = string.Empty;

    /// <summary>
    /// Customer user ID.
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Appointment date.
    /// </summary>
    public DateOnly AppointmentDate { get; set; }

    /// <summary>
    /// Start time.
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// End time.
    /// </summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// Total price of all services.
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Total duration in minutes.
    /// </summary>
    public int TotalDurationMinutes { get; set; }

    /// <summary>
    /// Appointment status.
    /// </summary>
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    /// <summary>
    /// Customer note.
    /// </summary>
    public string? CustomerNote { get; set; }

    /// <summary>
    /// Salon note (internal).
    /// </summary>
    public string? SalonNote { get; set; }

    /// <summary>
    /// Cancellation reason if cancelled.
    /// </summary>
    public string? CancellationReason { get; set; }

    /// <summary>
    /// When the appointment was confirmed.
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// When the appointment was cancelled.
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// When the appointment was completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// Salon.
    /// </summary>
    public virtual Salon Salon { get; set; } = null!;

    /// <summary>
    /// Assigned staff.
    /// </summary>
    public virtual Staff Staff { get; set; } = null!;

    /// <summary>
    /// Customer.
    /// </summary>
    public virtual User Customer { get; set; } = null!;

    /// <summary>
    /// Services included in this appointment.
    /// </summary>
    public virtual ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();
}

