namespace BakimZamani.API.Controllers;

using BakimZamani.Application.DTOs.Admin;
using BakimZamani.Application.DTOs.Common;
using BakimZamani.Application.Services.Interfaces;
using BakimZamani.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Admin-only controller for system management.
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminController : BaseApiController
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Get dashboard statistics.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<AdminDashboardStatsResponse>), 200)]
    public async Task<IActionResult> GetDashboardStats()
    {
        var result = await _adminService.GetDashboardStatsAsync();
        return Ok(result);
    }

    #region Salon Management

    /// <summary>
    /// Get all salons with filtering and pagination.
    /// </summary>
    [HttpGet("salons")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<AdminSalonListItem>>), 200)]
    public async Task<IActionResult> GetAllSalons([FromQuery] AdminSalonFilterRequest request)
    {
        var result = await _adminService.GetAllSalonsAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Get salon details.
    /// </summary>
    [HttpGet("salons/{salonId}")]
    [ProducesResponseType(typeof(ApiResponse<AdminSalonDetail>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetSalonDetail(string salonId)
    {
        var result = await _adminService.GetSalonDetailAsync(salonId);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Approve a salon.
    /// </summary>
    [HttpPut("salons/{salonId}/approve")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> ApproveSalon(string salonId)
    {
        var adminId = CurrentUserId;
        if (string.IsNullOrEmpty(adminId))
            return Unauthorized();

        var result = await _adminService.ApproveSalonAsync(salonId, adminId);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Suspend a salon.
    /// </summary>
    [HttpPut("salons/{salonId}/suspend")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> SuspendSalon(string salonId)
    {
        var adminId = CurrentUserId;
        if (string.IsNullOrEmpty(adminId))
            return Unauthorized();

        var result = await _adminService.SuspendSalonAsync(salonId, adminId);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Get all admin users.
    /// </summary>
    [HttpGet("admins")]
    [ProducesResponseType(typeof(ApiResponse<List<AdminUserListItem>>), 200)]
    public async Task<IActionResult> GetAdmins()
    {
        var result = await _adminService.GetAdminsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Activate a suspended salon.
    /// </summary>
    [HttpPut("salons/{salonId}/activate")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> ActivateSalon(string salonId)
    {
        var adminId = CurrentUserId;
        if (string.IsNullOrEmpty(adminId))
            return Unauthorized();

        var result = await _adminService.ActivateSalonAsync(salonId, adminId);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Delete a salon with reason.
    /// </summary>
    [HttpDelete("salons/{salonId}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> DeleteSalon(string salonId, [FromBody] DeleteSalonRequest request)
    {
        var adminId = CurrentUserId;
        if (string.IsNullOrEmpty(adminId))
            return Unauthorized();

        var result = await _adminService.DeleteSalonAsync(salonId, request.Reason, adminId);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Get recent salons for dashboard.
    /// </summary>
    [HttpGet("salons/recent")]
    [ProducesResponseType(typeof(ApiResponse<List<RecentSalonItem>>), 200)]
    public async Task<IActionResult> GetRecentSalons([FromQuery] int count = 5)
    {
        var result = await _adminService.GetRecentSalonsAsync(count);
        return Ok(result);
    }

    /// <summary>
    /// Get pending salons (awaiting approval).
    /// </summary>
    [HttpGet("salons/pending")]
    [ProducesResponseType(typeof(ApiResponse<List<RecentSalonItem>>), 200)]
    public async Task<IActionResult> GetPendingSalons()
    {
        var result = await _adminService.GetPendingSalonsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get recently deleted salons.
    /// </summary>
    [HttpGet("salons/deleted")]
    [ProducesResponseType(typeof(ApiResponse<List<DeletedSalonItem>>), 200)]
    public async Task<IActionResult> GetDeletedSalons([FromQuery] int count = 3)
    {
        var result = await _adminService.GetDeletedSalonsAsync(count);
        return Ok(result);
    }

    #endregion
    #region User Management

    /// <summary>
    /// Manually trigger seeding of protected admin users.
    /// </summary>
    [HttpPost("seed-users")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> SeedUsers()
    {
        // Manual trigger for seeding protected admins
        // This is a temporary helper due to restart issues
        var seeder = HttpContext.RequestServices.GetRequiredService<DbSeeder>();
        await seeder.SeedAsync();
        return Ok(new { message = "Seeding triggered manually" });
    }

    /// <summary>
    /// Get all users with filtering and pagination.
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<AdminUserListItem>>), 200)]
    public async Task<IActionResult> GetAllUsers([FromQuery] AdminUserFilterRequest request)
    {
        var result = await _adminService.GetAllUsersAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Get user details.
    /// </summary>
    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(ApiResponse<AdminUserDetail>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetUserDetail(string userId)
    {
        var result = await _adminService.GetUserDetailAsync(userId);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Suspend a user.
    /// </summary>
    [HttpPut("users/{userId}/suspend")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> SuspendUser(string userId)
    {
        var adminId = CurrentUserId;
        if (string.IsNullOrEmpty(adminId))
            return Unauthorized();

        var result = await _adminService.SuspendUserAsync(userId, adminId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Activate a suspended user.
    /// </summary>
    [HttpPut("users/{userId}/activate")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> ActivateUser(string userId)
    {
        var adminId = CurrentUserId;
        if (string.IsNullOrEmpty(adminId))
            return Unauthorized();

        var result = await _adminService.ActivateUserAsync(userId, adminId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Update user role.
    /// </summary>
    [HttpPut("users/{userId}/role")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> UpdateUserRole(string userId, [FromBody] UpdateUserRoleRequest request)
    {
        var adminId = CurrentUserId;
        if (string.IsNullOrEmpty(adminId))
            return Unauthorized();

        var result = await _adminService.UpdateUserRoleAsync(userId, request.Role, adminId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    #endregion

    #region Review Management

    /// <summary>
    /// Get all reviews with filtering and pagination.
    /// </summary>
    [HttpGet("reviews")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<AdminReviewListItem>>), 200)]
    public async Task<IActionResult> GetAllReviews([FromQuery] AdminReviewFilterRequest request)
    {
        var result = await _adminService.GetAllReviewsAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Hide a review.
    /// </summary>
    [HttpPut("reviews/{reviewId}/hide")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> HideReview(string reviewId)
    {
        var adminId = CurrentUserId;
        if (string.IsNullOrEmpty(adminId))
            return Unauthorized();

        var result = await _adminService.HideReviewAsync(reviewId, adminId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Show a hidden review.
    /// </summary>
    [HttpPut("reviews/{reviewId}/show")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> ShowReview(string reviewId)
    {
        var adminId = CurrentUserId;
        if (string.IsNullOrEmpty(adminId))
            return Unauthorized();

        var result = await _adminService.ShowReviewAsync(reviewId, adminId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    #endregion

    #region Reports

    /// <summary>
    /// Get monthly trend data (last 6 months).
    /// </summary>
    [HttpGet("reports/trends")]
    [ProducesResponseType(typeof(ApiResponse<List<MonthlyTrendData>>), 200)]
    public async Task<IActionResult> GetMonthlyTrends()
    {
        var result = await _adminService.GetMonthlyTrendsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get detailed report for a date range.
    /// </summary>
    [HttpGet("reports/detailed")]
    [ProducesResponseType(typeof(ApiResponse<DetailedReportData>), 200)]
    public async Task<IActionResult> GetDetailedReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var result = await _adminService.GetDetailedReportAsync(startDate, endDate);
        return Ok(result);
    }

    /// <summary>
    /// Get revenue summary with breakdowns.
    /// </summary>
    [HttpGet("reports/revenue")]
    [ProducesResponseType(typeof(ApiResponse<RevenueSummary>), 200)]
    public async Task<IActionResult> GetRevenueSummary(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var result = await _adminService.GetRevenueSummaryAsync(startDate, endDate);
        return Ok(result);
    }

    /// <summary>
    /// Get salon-level revenue list.
    /// </summary>
    [HttpGet("reports/salon-revenues")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<SalonRevenueItem>>), 200)]
    public async Task<IActionResult> GetSalonRevenues(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null)
    {
        var result = await _adminService.GetSalonRevenuesAsync(startDate, endDate, pageNumber, pageSize, sortBy);
        return Ok(result);
    }

    #endregion

    #region Notifications

    /// <summary>
    /// Get admin notifications.
    /// </summary>
    [HttpGet("notifications")]
    [ProducesResponseType(typeof(ApiResponse<List<AdminNotificationItem>>), 200)]
    public async Task<IActionResult> GetNotifications([FromQuery] int count = 20)
    {
        var result = await _adminService.GetAdminNotificationsAsync(count);
        return Ok(result);
    }

    /// <summary>
    /// Get unread notification count.
    /// </summary>
    [HttpGet("notifications/unread-count")]
    [ProducesResponseType(typeof(ApiResponse<int>), 200)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var result = await _adminService.GetUnreadNotificationCountAsync();
        return Ok(result);
    }

    /// <summary>
    /// Mark a notification as read.
    /// </summary>
    [HttpPut("notifications/{notificationId}/read")]
    public async Task<IActionResult> MarkNotificationRead(string notificationId)
    {
        var result = await _adminService.MarkNotificationReadAsync(notificationId);
        return Ok(result);
    }

    /// <summary>
    /// Mark all notifications as read.
    /// </summary>
    [HttpPut("notifications/read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var result = await _adminService.MarkAllNotificationsReadAsync();
        return Ok(result);
    }

    #endregion

    #region Admin Logs

    /// <summary>
    /// Get admin action logs.
    /// </summary>
    [HttpGet("logs")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<AdminLogItem>>), 200)]
    public async Task<IActionResult> GetAdminLogs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? adminId = null,
        [FromQuery] string? action = null)
    {
        var result = await _adminService.GetAdminLogsAsync(pageNumber, pageSize, adminId, action);
        return Ok(result);
    }

    #endregion

    #region Campaigns

    /// <summary>
    /// Get all campaigns with pagination.
    /// </summary>
    [HttpGet("campaigns")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<CampaignListItem>>), 200)]
    public async Task<IActionResult> GetCampaigns([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _adminService.GetCampaignsAsync(pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get campaign details.
    /// </summary>
    [HttpGet("campaigns/{campaignId}")]
    [ProducesResponseType(typeof(ApiResponse<CampaignDetailItem>), 200)]
    public async Task<IActionResult> GetCampaignDetail(string campaignId)
    {
        var result = await _adminService.GetCampaignDetailAsync(campaignId);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Create a new campaign.
    /// </summary>
    [HttpPost("campaigns")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> CreateCampaign([FromBody] CreateCampaignRequest request)
    {
        var adminId = CurrentUserId;
        if (string.IsNullOrEmpty(adminId)) return Unauthorized();

        var result = await _adminService.CreateCampaignAsync(request, adminId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Update a campaign.
    /// </summary>
    [HttpPut("campaigns/{campaignId}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> UpdateCampaign(string campaignId, [FromBody] UpdateCampaignRequest request)
    {
        var adminId = CurrentUserId;
        if (string.IsNullOrEmpty(adminId)) return Unauthorized();

        var result = await _adminService.UpdateCampaignAsync(campaignId, request, adminId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Delete a campaign.
    /// </summary>
    [HttpDelete("campaigns/{campaignId}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> DeleteCampaign(string campaignId)
    {
        var adminId = CurrentUserId;
        if (string.IsNullOrEmpty(adminId)) return Unauthorized();

        var result = await _adminService.DeleteCampaignAsync(campaignId, adminId);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    #endregion

    #region Content Management

    // Announcements

    [HttpGet("announcements")]
    [ProducesResponseType(typeof(ApiResponse<List<AnnouncementListItem>>), 200)]
    public async Task<IActionResult> GetAnnouncements()
    {
        var result = await _adminService.GetAnnouncementsAsync();
        return Ok(result);
    }

    [HttpPost("announcements")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> CreateAnnouncement([FromBody] CreateAnnouncementRequest request)
    {
        var adminId = CurrentUserId;
        if (string.IsNullOrEmpty(adminId)) return Unauthorized();

        var result = await _adminService.CreateAnnouncementAsync(request, adminId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("announcements/{id}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> UpdateAnnouncement(string id, [FromBody] UpdateAnnouncementRequest request)
    {
        var adminId = CurrentUserId;
        if (string.IsNullOrEmpty(adminId)) return Unauthorized();

        var result = await _adminService.UpdateAnnouncementAsync(id, request, adminId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("announcements/{id}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> DeleteAnnouncement(string id)
    {
        var adminId = CurrentUserId;
        if (string.IsNullOrEmpty(adminId)) return Unauthorized();

        var result = await _adminService.DeleteAnnouncementAsync(id, adminId);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    // FAQs

    [HttpGet("faqs")]
    [ProducesResponseType(typeof(ApiResponse<List<FAQListItem>>), 200)]
    public async Task<IActionResult> GetFAQs()
    {
        var result = await _adminService.GetFAQsAsync();
        return Ok(result);
    }

    [HttpPost("faqs")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> CreateFAQ([FromBody] CreateFAQRequest request)
    {
        var adminId = CurrentUserId;
        if (string.IsNullOrEmpty(adminId)) return Unauthorized();

        var result = await _adminService.CreateFAQAsync(request, adminId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("faqs/{id}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> UpdateFAQ(string id, [FromBody] UpdateFAQRequest request)
    {
        var adminId = CurrentUserId;
        if (string.IsNullOrEmpty(adminId)) return Unauthorized();

        var result = await _adminService.UpdateFAQAsync(id, request, adminId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("faqs/{id}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> DeleteFAQ(string id)
    {
        var adminId = CurrentUserId;
        if (string.IsNullOrEmpty(adminId)) return Unauthorized();

        var result = await _adminService.DeleteFAQAsync(id, adminId);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    #endregion
}

