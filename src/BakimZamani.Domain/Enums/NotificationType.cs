namespace BakimZamani.Domain.Enums;

/// <summary>
/// Notification types for push and in-app notifications.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// New appointment request.
    /// </summary>
    NewAppointment = 0,

    /// <summary>
    /// Appointment confirmed.
    /// </summary>
    AppointmentConfirmed = 1,

    /// <summary>
    /// Appointment cancelled.
    /// </summary>
    AppointmentCancelled = 2,

    /// <summary>
    /// Appointment reminder.
    /// </summary>
    AppointmentReminder = 3,

    /// <summary>
    /// Appointment completed - review request.
    /// </summary>
    ReviewRequest = 4,

    /// <summary>
    /// System notification.
    /// </summary>
    System = 5,

    /// <summary>
    /// Promotional notification.
    /// </summary>
    Promotion = 6
}

