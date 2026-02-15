namespace BakimZamani.Infrastructure.Services;

using AutoMapper;
using BakimZamani.Application.DTOs.Appointment;
using BakimZamani.Application.DTOs.Common;
using BakimZamani.Application.Services.Interfaces;
using BakimZamani.Domain.Entities;
using BakimZamani.Domain.Enums;
using BakimZamani.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Review service implementation.
/// </summary>
public class ReviewService : IReviewService
{
    private readonly IRepository<Review> _reviewRepository;
    private readonly IRepository<Appointment> _appointmentRepository;
    private readonly IRepository<Salon> _salonRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ReviewService(
        IRepository<Review> reviewRepository,
        IRepository<Appointment> appointmentRepository,
        IRepository<Salon> salonRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _appointmentRepository = appointmentRepository;
        _salonRepository = salonRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ReviewResponse>> CreateReviewAsync(string customerId, CreateReviewRequest request)
    {
        // Check if appointment exists and is completed
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId);
        if (appointment == null)
        {
            return ApiResponse<ReviewResponse>.Fail("Randevu bulunamadÄ±.");
        }

        if (appointment.CustomerId != customerId)
        {
            return ApiResponse<ReviewResponse>.Fail("Bu randevu size ait deÄŸil.");
        }

        if (appointment.Status != AppointmentStatus.Completed)
        {
            return ApiResponse<ReviewResponse>.Fail("Sadece tamamlanmÄ±ÅŸ randevular deÄŸerlendirilebilir.");
        }

        // Check if already reviewed
        var existingReview = await _reviewRepository.Query()
            .FirstOrDefaultAsync(r => r.AppointmentId == request.AppointmentId);
        if (existingReview != null)
        {
            return ApiResponse<ReviewResponse>.Fail("Bu randevu zaten deÄŸerlendirilmiÅŸ.");
        }

        // Create review
        var review = new Review
        {
            CustomerId = customerId,
            SalonId = appointment.SalonId,
            AppointmentId = request.AppointmentId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        await _reviewRepository.AddAsync(review);
        await _unitOfWork.SaveChangesAsync();

        // Update salon average rating
        await UpdateSalonRatingAsync(appointment.SalonId);

        var response = _mapper.Map<ReviewResponse>(review);
        return ApiResponse<ReviewResponse>.Ok(response, "DeÄŸerlendirme eklendi.");
    }

    public async Task<ApiResponse<PaginatedResult<ReviewResponse>>> GetSalonReviewsAsync(string salonId, int pageNumber = 1, int pageSize = 10)
    {
        var query = _reviewRepository.Query()
            .Include(r => r.Customer)
            .Where(r => r.SalonId == salonId && r.IsVisible)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var mappedItems = _mapper.Map<List<ReviewResponse>>(items);
        var result = new PaginatedResult<ReviewResponse>(mappedItems, totalCount, pageNumber, pageSize);

        return ApiResponse<PaginatedResult<ReviewResponse>>.Ok(result);
    }

    public async Task<ApiResponse<ReviewResponse>> ReplyToReviewAsync(string reviewId, string ownerId, ReplyToReviewRequest request)
    {
        var review = await _reviewRepository.Query()
            .Include(r => r.Salon)
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
        {
            return ApiResponse<ReviewResponse>.Fail("DeÄŸerlendirme bulunamadÄ±.");
        }

        if (review.Salon.OwnerId != ownerId)
        {
            return ApiResponse<ReviewResponse>.Fail("Bu iÅŸlem iÃ§in yetkiniz yok.");
        }

        review.Reply = request.Reply;
        review.RepliedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<ReviewResponse>(review);
        return ApiResponse<ReviewResponse>.Ok(response, "YanÄ±t eklendi.");
    }

    public async Task<ApiResponse> DeleteReviewAsync(string reviewId, string userId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null)
        {
            return ApiResponse.Fail("DeÄŸerlendirme bulunamadÄ±.");
        }

        if (review.CustomerId != userId)
        {
            return ApiResponse.Fail("Bu iÅŸlem iÃ§in yetkiniz yok.");
        }

        var salonId = review.SalonId;
        _reviewRepository.Delete(review);
        await _unitOfWork.SaveChangesAsync();

        // Update salon average rating
        await UpdateSalonRatingAsync(salonId);

        return ApiResponse.Ok("DeÄŸerlendirme silindi.");
    }

    private async Task UpdateSalonRatingAsync(string salonId)
    {
        var salon = await _salonRepository.GetByIdAsync(salonId);
        if (salon == null) return;

        var reviews = await _reviewRepository.Query()
            .Where(r => r.SalonId == salonId && r.IsVisible)
            .ToListAsync();

        if (reviews.Any())
        {
            salon.Rating = (decimal)reviews.Average(r => r.Rating);
            salon.ReviewCount = reviews.Count;
        }
        else
        {
            salon.Rating = 0;
            salon.ReviewCount = 0;
        }

        await _unitOfWork.SaveChangesAsync();
    }
}

