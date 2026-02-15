namespace BakimZamani.Application.Services.Interfaces;

using BakimZamani.Application.DTOs.Admin;
using BakimZamani.Application.DTOs.Common;
using BakimZamani.Domain.Enums;

/// <summary>
/// Admin-specific service interface.
/// </summary>
public interface IAdminService
{
    /// <summary>
    /// Get dashboard statistics for admin panel.
    /// </summary>
    Task<ApiResponse<AdminDashboardStatsResponse>> GetDashboardStatsAsync();

    /// <summary>
    /// Get all salons with filtering and pagination.
    /// </summary>
    Task<ApiResponse<PaginatedResult<AdminSalonListItem>>> GetAllSalonsAsync(AdminSalonFilterRequest request);

    /// <summary>
    /// Get salon details for admin.
    /// </summary>
    Task<ApiResponse<AdminSalonDetail>> GetSalonDetailAsync(string salonId);

    /// <summary>
    /// Approve a salon (activate).
    /// </summary>
    Task<ApiResponse> ApproveSalonAsync(string salonId, string adminId);

    /// <summary>
    /// Suspend a salon (deactivate).
    /// </summary>
    Task<ApiResponse> SuspendSalonAsync(string salonId, string adminId);

    /// <summary>
    /// Activate a suspended salon.
    /// </summary>
    Task<ApiResponse> ActivateSalonAsync(string salonId, string adminId);

    /// <summary>
    /// Get all admin users.
    /// </summary>
    Task<ApiResponse<List<AdminUserListItem>>> GetAdminsAsync();

    /// <summary>
    /// Delete a salon with reason.
    /// </summary>
    Task<ApiResponse> DeleteSalonAsync(string salonId, string reason, string adminId);

    /// <summary>
    /// Get recent salons for dashboard (last 5).
    /// </summary>
    Task<ApiResponse<List<RecentSalonItem>>> GetRecentSalonsAsync(int count = 5);

    /// <summary>
    /// Get pending salons (not verified).
    /// </summary>
    Task<ApiResponse<List<RecentSalonItem>>> GetPendingSalonsAsync();

    /// <summary>
    /// Get recently deleted salons (last 3).
    /// </summary>
    Task<ApiResponse<List<DeletedSalonItem>>> GetDeletedSalonsAsync(int count = 3);

    #region User Management

    /// <summary>
    /// Get all users with filtering and pagination.
    /// </summary>
    Task<ApiResponse<PaginatedResult<AdminUserListItem>>> GetAllUsersAsync(AdminUserFilterRequest request);

    /// <summary>
    /// Get user details for admin.
    /// </summary>
    Task<ApiResponse<AdminUserDetail>> GetUserDetailAsync(string userId);

    /// <summary>
    /// Suspend a user (deactivate).
    /// </summary>
    Task<ApiResponse> SuspendUserAsync(string userId, string adminId);

    /// <summary>
    /// Activate a suspended user.
    /// </summary>
    Task<ApiResponse> ActivateUserAsync(string userId, string adminId);

    /// <summary>
    /// Update user role.
    /// </summary>
    Task<ApiResponse> UpdateUserRoleAsync(string userId, UserRole newRole, string adminId);

    #endregion

    #region Review Management

    /// <summary>
    /// Get all reviews with filtering and pagination.
    /// </summary>
    Task<ApiResponse<PaginatedResult<AdminReviewListItem>>> GetAllReviewsAsync(AdminReviewFilterRequest request);

    /// <summary>
    /// Hide a review (set IsVisible = false).
    /// </summary>
    Task<ApiResponse> HideReviewAsync(string reviewId, string adminId);

    /// <summary>
    /// Show a hidden review (set IsVisible = true).
    /// </summary>
    Task<ApiResponse> ShowReviewAsync(string reviewId, string adminId);

    #endregion

    #region Reports

    /// <summary>
    /// Get monthly trend data for the last 6 months.
    /// </summary>
    Task<ApiResponse<List<MonthlyTrendData>>> GetMonthlyTrendsAsync();

    /// <summary>
    /// Get detailed report for a date range.
    /// </summary>
    Task<ApiResponse<DetailedReportData>> GetDetailedReportAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get revenue summary with breakdowns.
    /// </summary>
    Task<ApiResponse<RevenueSummary>> GetRevenueSummaryAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get salon-level revenue list with pagination.
    /// </summary>
    Task<ApiResponse<PaginatedResult<SalonRevenueItem>>> GetSalonRevenuesAsync(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 20, string? sortBy = null);

    #endregion

    #region Notifications

    /// <summary>
    /// Get recent admin notifications.
    /// </summary>
    Task<ApiResponse<List<AdminNotificationItem>>> GetAdminNotificationsAsync(int count = 20);

    /// <summary>
    /// Get unread notification count.
    /// </summary>
    Task<ApiResponse<int>> GetUnreadNotificationCountAsync();

    /// <summary>
    /// Mark a notification as read.
    /// </summary>
    Task<ApiResponse> MarkNotificationReadAsync(string notificationId);

    /// <summary>
    /// Mark all notifications as read.
    /// </summary>
    Task<ApiResponse> MarkAllNotificationsReadAsync();

    #endregion

    #region Admin Logs

    /// <summary>
    /// Get admin action logs with pagination.
    /// </summary>
    Task<ApiResponse<PaginatedResult<AdminLogItem>>> GetAdminLogsAsync(int pageNumber = 1, int pageSize = 20, string? adminId = null, string? action = null);

    /// <summary>
    /// Log an admin action.
    /// </summary>
    Task LogAdminActionAsync(string adminId, string adminName, string action, string targetEntity, string? targetId = null, string? targetName = null, string? details = null);

    #endregion

    #region Campaigns

    /// <summary>
    /// Get all campaigns with pagination.
    /// </summary>
    Task<ApiResponse<PaginatedResult<CampaignListItem>>> GetCampaignsAsync(int pageNumber = 1, int pageSize = 20);

    /// <summary>
    /// Get campaign details.
    /// </summary>
    Task<ApiResponse<CampaignDetailItem>> GetCampaignDetailAsync(string campaignId);

    /// <summary>
    /// Create a new campaign.
    /// </summary>
    Task<ApiResponse> CreateCampaignAsync(CreateCampaignRequest request, string adminId);

    /// <summary>
    /// Update an existing campaign.
    /// </summary>
    Task<ApiResponse> UpdateCampaignAsync(string campaignId, UpdateCampaignRequest request, string adminId);

    /// <summary>
    /// Delete a campaign.
    /// </summary>
    Task<ApiResponse> DeleteCampaignAsync(string campaignId, string adminId);

    #endregion

    #region Content Management

    // Announcements
    Task<ApiResponse<List<AnnouncementListItem>>> GetAnnouncementsAsync();
    Task<ApiResponse> CreateAnnouncementAsync(CreateAnnouncementRequest request, string adminId);
    Task<ApiResponse> UpdateAnnouncementAsync(string id, UpdateAnnouncementRequest request, string adminId);
    Task<ApiResponse> DeleteAnnouncementAsync(string id, string adminId);

    // FAQs
    Task<ApiResponse<List<FAQListItem>>> GetFAQsAsync();
    Task<ApiResponse> CreateFAQAsync(CreateFAQRequest request, string adminId);
    Task<ApiResponse> UpdateFAQAsync(string id, UpdateFAQRequest request, string adminId);
    Task<ApiResponse> DeleteFAQAsync(string id, string adminId);

    #endregion
}

