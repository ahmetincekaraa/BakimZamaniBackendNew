namespace BakimZamani.Application.Services.Interfaces;

using BakimZamani.Application.DTOs.Common;
using BakimZamani.Application.DTOs.Appointment;

/// <summary>
/// Review service interface.
/// </summary>
public interface IReviewService
{
    Task<ApiResponse<ReviewResponse>> CreateReviewAsync(string customerId, CreateReviewRequest request);
    Task<ApiResponse<PaginatedResult<ReviewResponse>>> GetSalonReviewsAsync(string salonId, int pageNumber = 1, int pageSize = 10);
    Task<ApiResponse<ReviewResponse>> ReplyToReviewAsync(string reviewId, string ownerId, ReplyToReviewRequest request);
    Task<ApiResponse> DeleteReviewAsync(string reviewId, string userId);
}

