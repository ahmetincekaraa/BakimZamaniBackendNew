namespace KuaforSepeti.Application.Services.Interfaces;

using KuaforSepeti.Application.DTOs.Common;
using KuaforSepeti.Application.DTOs.Appointment;

/// <summary>
/// Appointment service interface.
/// </summary>
public interface IAppointmentService
{
    // Slot availability
    Task<ApiResponse<List<AvailableSlotsResponse>>> GetAvailableSlotsAsync(GetAvailableSlotsRequest request);
    
    // Customer endpoints
    Task<ApiResponse<AppointmentResponse>> CreateAppointmentAsync(string customerId, CreateAppointmentRequest request);
    Task<ApiResponse<PaginatedResult<AppointmentResponse>>> GetMyAppointmentsAsync(string customerId, AppointmentFilterRequest request);
    Task<ApiResponse<AppointmentResponse>> GetAppointmentByIdAsync(string appointmentId, string userId);
    Task<ApiResponse> CancelAppointmentByCustomerAsync(string appointmentId, string customerId, CancelAppointmentRequest request);
    
    // Salon owner/staff endpoints
    Task<ApiResponse<PaginatedResult<AppointmentResponse>>> GetSalonAppointmentsAsync(string salonId, string userId, AppointmentFilterRequest request);
    Task<ApiResponse> ConfirmAppointmentAsync(string appointmentId, string userId, ConfirmAppointmentRequest request);
    Task<ApiResponse> CancelAppointmentBySalonAsync(string appointmentId, string userId, CancelAppointmentRequest request);
    Task<ApiResponse> CompleteAppointmentAsync(string appointmentId, string userId);
    Task<ApiResponse> MarkAsNoShowAsync(string appointmentId, string userId);
    
    // Reschedule
    Task<ApiResponse<AppointmentResponse>> RescheduleAppointmentAsync(string appointmentId, string userId, RescheduleAppointmentRequest request);
}
