namespace BakimZamani.Domain.Enums;

/// <summary>
/// Appointment status states.
/// </summary>
public enum AppointmentStatus
{
    /// <summary>
    /// Waiting for salon confirmation.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Confirmed by salon.
    /// </summary>
    Confirmed = 1,

    /// <summary>
    /// Service completed.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Cancelled by customer.
    /// </summary>
    CancelledByCustomer = 3,

    /// <summary>
    /// Cancelled by salon.
    /// </summary>
    CancelledBySalon = 4,

    /// <summary>
    /// Customer did not show up.
    /// </summary>
    NoShow = 5
}

