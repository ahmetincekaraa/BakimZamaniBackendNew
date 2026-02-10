namespace KuaforSepeti.API.Controllers;

using KuaforSepeti.Application.DTOs.Appointment;
using KuaforSepeti.Application.DTOs.Common;
using KuaforSepeti.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Review management controller.
/// </summary>
public class ReviewsController : BaseApiController
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// Get salon reviews.
    /// </summary>
    [HttpGet("salon/{salonId}")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<ReviewResponse>>), 200)]
    public async Task<IActionResult> GetSalonReviews(string salonId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _reviewService.GetSalonReviewsAsync(salonId, pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Create a review (customer).
    /// </summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ReviewResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _reviewService.CreateReviewAsync(CurrentUserId, request);
        
        if (!result.Success)
            return BadRequest(result);

        return Created("", result);
    }

    /// <summary>
    /// Reply to a review (salon owner).
    /// </summary>
    [Authorize]
    [HttpPost("{reviewId}/reply")]
    [ProducesResponseType(typeof(ApiResponse<ReviewResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ReplyToReview(string reviewId, [FromBody] ReplyToReviewRequest request)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _reviewService.ReplyToReviewAsync(reviewId, CurrentUserId, request);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete a review.
    /// </summary>
    [Authorize]
    [HttpDelete("{reviewId}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> DeleteReview(string reviewId)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _reviewService.DeleteReviewAsync(reviewId, CurrentUserId);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
