namespace KuaforSepeti.API.Controllers;

using KuaforSepeti.Application.DTOs.Admin;
using KuaforSepeti.Application.DTOs.Common;
using KuaforSepeti.Application.Services.Interfaces;
using KuaforSepeti.Infrastructure.Data;
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
        var result = await _adminService.ActivateSalonAsync(salonId);
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
        var result = await _adminService.DeleteSalonAsync(salonId, request.Reason);
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
        var result = await _adminService.SuspendUserAsync(userId);
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
        var result = await _adminService.ActivateUserAsync(userId);
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
        var result = await _adminService.UpdateUserRoleAsync(userId, request.Role);
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
        var result = await _adminService.HideReviewAsync(reviewId);
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
        var result = await _adminService.ShowReviewAsync(reviewId);
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
}
