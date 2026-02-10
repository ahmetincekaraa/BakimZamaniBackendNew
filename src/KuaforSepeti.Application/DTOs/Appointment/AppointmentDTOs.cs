namespace KuaforSepeti.Application.DTOs.Appointment;

using KuaforSepeti.Domain.Enums;

/// <summary>
/// Available time slot response.
/// </summary>
public class TimeSlotResponse
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
}

/// <summary>
/// Available slots for a specific date.
/// </summary>
public class AvailableSlotsResponse
{
    public DateOnly Date { get; set; }
    public string StaffId { get; set; } = string.Empty;
    public string StaffName { get; set; } = string.Empty;
    public List<TimeSlotResponse> Slots { get; set; } = new();
}

/// <summary>
/// Create appointment request.
/// </summary>
public class CreateAppointmentRequest
{
    public string SalonId { get; set; } = string.Empty;
    public string StaffId { get; set; } = string.Empty;
    
    /// <summary>
    /// Selected individual service IDs.
    /// </summary>
    public List<string> ServiceIds { get; set; } = new();
    
    /// <summary>
    /// Selected package ID (optional - if package selected instead of individual services).
    /// </summary>
    public string? PackageId { get; set; }
    
    public DateOnly AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public string? CustomerNote { get; set; }
}

/// <summary>
/// Appointment response DTO.
/// </summary>
public class AppointmentResponse
{
    public string Id { get; set; } = string.Empty;
    public string SalonId { get; set; } = string.Empty;
    public string SalonName { get; set; } = string.Empty;
    public string SalonAddress { get; set; } = string.Empty;
    public string? SalonLogoUrl { get; set; }
    public string StaffId { get; set; } = string.Empty;
    public string StaffName { get; set; } = string.Empty;
    public string? StaffProfileImageUrl { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public DateOnly AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal TotalPrice { get; set; }
    public int TotalDurationMinutes { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? CustomerNote { get; set; }
    public string? SalonNote { get; set; }
    public string? CancellationReason { get; set; }
    public List<AppointmentServiceResponse> Services { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// Appointment service item response.
/// </summary>
public class AppointmentServiceResponse
{
    public string ServiceId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
}

/// <summary>
/// Confirm appointment request.
/// </summary>
public class ConfirmAppointmentRequest
{
    public string? SalonNote { get; set; }
}

/// <summary>
/// Cancel appointment request.
/// </summary>
public class CancelAppointmentRequest
{
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Reschedule appointment request.
/// </summary>
public class RescheduleAppointmentRequest
{
    public DateOnly NewDate { get; set; }
    public TimeSpan NewStartTime { get; set; }
    public string? StaffId { get; set; } // Optional: change staff
}

/// <summary>
/// Get available slots request.
/// </summary>
public class GetAvailableSlotsRequest
{
    public string SalonId { get; set; } = string.Empty;
    public string? StaffId { get; set; } // Optional: specific staff
    public DateOnly Date { get; set; }
    public List<string> ServiceIds { get; set; } = new(); // To calculate required duration
}

/// <summary>
/// Appointment list filter request.
/// </summary>
public class AppointmentFilterRequest
{
    public AppointmentStatus? Status { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Review request.
/// </summary>
public class CreateReviewRequest
{
    public string AppointmentId { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
}

/// <summary>
/// Review response.
/// </summary>
public class ReviewResponse
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerProfileImageUrl { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string? Reply { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RepliedAt { get; set; }
}

/// <summary>
/// Reply to review request.
/// </summary>
public class ReplyToReviewRequest
{
    public string Reply { get; set; } = string.Empty;
}
