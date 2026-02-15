namespace BakimZamani.Application.Services.Interfaces;

using BakimZamani.Application.DTOs.Common;
using BakimZamani.Application.DTOs.Salon;

/// <summary>
/// Salon service interface.
/// </summary>
public interface ISalonService
{
    // Public endpoints
    Task<ApiResponse<PaginatedResult<SalonListResponse>>> SearchSalonsAsync(SalonSearchRequest request);
    Task<ApiResponse<SalonDetailResponse>> GetSalonByIdAsync(string salonId);
    
    // Salon owner endpoints
    Task<ApiResponse<SalonDetailResponse>> CreateSalonAsync(string ownerId, CreateSalonRequest request);
    Task<ApiResponse<SalonDetailResponse>> UpdateSalonAsync(string salonId, string ownerId, UpdateSalonRequest request);
    Task<ApiResponse> DeleteSalonAsync(string salonId, string ownerId);
    Task<ApiResponse<List<SalonListResponse>>> GetMySalonsAsync(string ownerId);
    
    // Staff management
    Task<ApiResponse<StaffResponse>> AddStaffAsync(string salonId, string ownerId, CreateStaffRequest request);
    Task<ApiResponse<StaffResponse>> UpdateStaffAsync(string salonId, string staffId, string ownerId, CreateStaffRequest request);
    Task<ApiResponse> RemoveStaffAsync(string salonId, string staffId, string ownerId);
    
    // Service management
    Task<ApiResponse<ServiceResponse>> AddServiceAsync(string salonId, string ownerId, CreateServiceRequest request);
    Task<ApiResponse<ServiceResponse>> UpdateServiceAsync(string salonId, string serviceId, string ownerId, CreateServiceRequest request);
    Task<ApiResponse> RemoveServiceAsync(string salonId, string serviceId, string ownerId);
    
    // Working hours management
    Task<ApiResponse<List<WorkingHoursResponse>>> GetWorkingHoursAsync(string salonId);
    Task<ApiResponse<List<WorkingHoursResponse>>> UpdateWorkingHoursAsync(string salonId, string ownerId, UpdateWorkingHoursRequest request);
}


