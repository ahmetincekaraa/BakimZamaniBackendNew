namespace KuaforSepeti.Infrastructure.Services;

using KuaforSepeti.Application.DTOs.Admin;
using KuaforSepeti.Application.DTOs.Common;
using KuaforSepeti.Application.Services.Interfaces;
using KuaforSepeti.Domain.Entities;
using KuaforSepeti.Domain.Enums;
using KuaforSepeti.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Admin service implementation.
/// </summary>
public class AdminService : IAdminService
{
    private readonly AppDbContext _context;

    public AdminService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<AdminDashboardStatsResponse>> GetDashboardStatsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var todayDateOnly = DateOnly.FromDateTime(today);
        var monthStartDateOnly = DateOnly.FromDateTime(monthStart);

        var todayAppointments = await _context.Appointments
            .CountAsync(a => a.AppointmentDate == todayDateOnly && !a.IsDeleted);

        var pendingApprovals = await _context.Appointments
            .CountAsync(a => a.Status == AppointmentStatus.Pending && !a.IsDeleted);

        var totalCustomers = await _context.Users
            .CountAsync(u => u.Role == UserRole.Customer && !u.IsDeleted);

        var monthlyRevenue = await _context.Appointments
            .Where(a => a.Status == AppointmentStatus.Completed 
                && a.AppointmentDate >= monthStartDateOnly
                && !a.IsDeleted)
            .SumAsync(a => a.TotalPrice);

        var totalSalons = await _context.Salons
            .CountAsync(s => s.IsActive && !s.IsDeleted);

        var totalStaff = await _context.Staff
            .CountAsync(s => s.IsActive && !s.IsDeleted);

        var completedToday = await _context.Appointments
            .CountAsync(a => a.Status == AppointmentStatus.Completed 
                && a.AppointmentDate == todayDateOnly
                && !a.IsDeleted);

        var cancelledToday = await _context.Appointments
            .CountAsync(a => (a.Status == AppointmentStatus.CancelledByCustomer 
                || a.Status == AppointmentStatus.CancelledBySalon)
                && a.AppointmentDate == todayDateOnly
                && !a.IsDeleted);

        var stats = new AdminDashboardStatsResponse
        {
            TodayAppointments = todayAppointments,
            PendingApprovals = pendingApprovals,
            TotalCustomers = totalCustomers,
            MonthlyRevenue = monthlyRevenue,
            TotalSalons = totalSalons,
            TotalStaff = totalStaff,
            CompletedToday = completedToday,
            CancelledToday = cancelledToday
        };

        return ApiResponse<AdminDashboardStatsResponse>.Ok(stats);
    }

    public async Task<ApiResponse<PaginatedResult<AdminSalonListItem>>> GetAllSalonsAsync(AdminSalonFilterRequest request)
    {
        var query = _context.Salons
            .Include(s => s.Owner)
            .Where(s => !s.IsDeleted);

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(s => 
                s.Name.ToLower().Contains(search) || 
                (s.Owner != null && (s.Owner.FullName.ToLower().Contains(search) || s.Owner.Email.ToLower().Contains(search))));
        }

        // Apply status filter
        if (request.Status.HasValue && request.Status.Value != SalonStatusFilter.All)
        {
            switch (request.Status.Value)
            {
                case SalonStatusFilter.Active:
                    query = query.Where(s => s.IsActive && s.IsVerified);
                    break;
                case SalonStatusFilter.Pending:
                    query = query.Where(s => s.IsActive && !s.IsVerified);
                    break;
                case SalonStatusFilter.Suspended:
                    query = query.Where(s => !s.IsActive && s.IsVerified);
                    break;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            query = query.Where(s => s.City == request.City);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new AdminSalonListItem
            {
                Id = s.Id,
                Name = s.Name,
                OwnerName = s.Owner != null ? s.Owner.FullName : "",
                OwnerEmail = s.Owner != null ? s.Owner.Email : "",
                City = s.City,
                District = s.District,
                IsActive = s.IsActive,
                IsVerified = s.IsVerified,
                Rating = (double)s.Rating,
                TotalReviews = s.ReviewCount,
                TotalAppointments = _context.Appointments.Count(a => a.SalonId == s.Id && !a.IsDeleted),
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        var result = new PaginatedResult<AdminSalonListItem>(items, totalCount, request.PageNumber, request.PageSize);
        return ApiResponse<PaginatedResult<AdminSalonListItem>>.Ok(result);
    }

    public async Task<ApiResponse<AdminSalonDetail>> GetSalonDetailAsync(string salonId)
    {
        var salon = await _context.Salons
            .Include(s => s.Owner)
            .Include(s => s.ApprovedByAdmin)
            .Include(s => s.SuspendedByAdmin)
            .FirstOrDefaultAsync(s => s.Id == salonId && !s.IsDeleted);

        if (salon == null)
            return ApiResponse<AdminSalonDetail>.Fail("Salon bulunamadı");

        var today = DateTime.UtcNow.Date;
        var monthStartDateOnly = DateOnly.FromDateTime(new DateTime(today.Year, today.Month, 1));

        var detail = new AdminSalonDetail
        {
            Id = salon.Id,
            Name = salon.Name,
            Description = salon.Description,
            Address = salon.Address,
            City = salon.City,
            District = salon.District,
            Phone = salon.PhoneNumber,
            LogoUrl = salon.LogoUrl,
            IsActive = salon.IsActive,
            IsVerified = salon.IsVerified,
            Rating = (double)salon.Rating,
            TotalReviews = salon.ReviewCount,
            CreatedAt = salon.CreatedAt,
            OwnerId = salon.OwnerId,
            OwnerName = salon.Owner != null ? salon.Owner.FullName : "",
            OwnerEmail = salon.Owner != null ? salon.Owner.Email : "",
            OwnerPhone = salon.Owner != null ? salon.Owner.PhoneNumber ?? "" : "",
            TotalStaff = await _context.Staff.CountAsync(s => s.SalonId == salonId && !s.IsDeleted),
            TotalServices = await _context.Services.CountAsync(s => s.SalonId == salonId && !s.IsDeleted),
            TotalAppointments = await _context.Appointments.CountAsync(a => a.SalonId == salonId && !a.IsDeleted),
            CompletedAppointments = await _context.Appointments.CountAsync(a => a.SalonId == salonId && a.Status == AppointmentStatus.Completed && !a.IsDeleted),
            PendingAppointments = await _context.Appointments.CountAsync(a => a.SalonId == salonId && a.Status == AppointmentStatus.Pending && !a.IsDeleted),
            CancelledAppointments = await _context.Appointments.CountAsync(a => a.SalonId == salonId && (a.Status == AppointmentStatus.CancelledByCustomer || a.Status == AppointmentStatus.CancelledBySalon) && !a.IsDeleted),
            
            // Platform Katkısı - Uygulamamızdan giden benzersiz müşteri sayısı
            TotalCustomers = await _context.Appointments
                .Where(a => a.SalonId == salonId && a.Status == AppointmentStatus.Completed && !a.IsDeleted)
                .Select(a => a.CustomerId)
                .Distinct()
                .CountAsync(),
            
            // Toplam kazanç (tüm zamanlar - tamamlanmış randevulardan)
            TotalRevenue = await _context.Appointments
                .Where(a => a.SalonId == salonId && a.Status == AppointmentStatus.Completed && !a.IsDeleted)
                .SumAsync(a => a.TotalPrice),
            
            // Bu ayki kazanç
            MonthlyRevenue = await _context.Appointments
                .Where(a => a.SalonId == salonId 
                    && a.Status == AppointmentStatus.Completed 
                    && a.AppointmentDate >= monthStartDateOnly
                    && !a.IsDeleted)
                .SumAsync(a => a.TotalPrice),

            // Audit Info
            ApprovedByAdminName = salon.ApprovedByAdmin?.FullName,
            ApprovedAt = salon.ApprovedAt,
            SuspendedByAdminName = salon.SuspendedByAdmin?.FullName,
            SuspendedAt = salon.SuspendedAt
        };

        return ApiResponse<AdminSalonDetail>.Ok(detail);
    }

    public async Task<ApiResponse> ApproveSalonAsync(string salonId, string adminId)
    {
        var salon = await _context.Salons.FindAsync(salonId);
        if (salon == null || salon.IsDeleted)
            return ApiResponse.Fail("Salon bulunamadı");

        salon.IsActive = true;
        salon.IsVerified = true;
        salon.UpdatedAt = DateTime.UtcNow;
        
        // Audit
        salon.ApprovedByAdminId = adminId;
        salon.ApprovedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Salon onaylandı");
    }

    public async Task<ApiResponse> SuspendSalonAsync(string salonId, string adminId)
    {
        var salon = await _context.Salons.FindAsync(salonId);
        if (salon == null || salon.IsDeleted)
            return ApiResponse.Fail("Salon bulunamadı");

        salon.IsActive = false;
        salon.UpdatedAt = DateTime.UtcNow;

        // Audit
        salon.SuspendedByAdminId = adminId;
        salon.SuspendedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Salon askıya alındı");
    }

    public async Task<ApiResponse> ActivateSalonAsync(string salonId)
    {
        var salon = await _context.Salons.FindAsync(salonId);
        if (salon == null || salon.IsDeleted)
            return ApiResponse.Fail("Salon bulunamadı");

        salon.IsActive = true;
        salon.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Salon aktifleştirildi");
    }

    public async Task<ApiResponse> DeleteSalonAsync(string salonId, string reason)
    {
        var salon = await _context.Salons.FindAsync(salonId);
        if (salon == null || salon.IsDeleted)
            return ApiResponse.Fail("Salon bulunamadı");

        salon.IsDeleted = true;
        salon.IsActive = false;
        salon.DeletionReason = reason;
        salon.DeletedAt = DateTime.UtcNow;
        salon.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Salon silindi");
    }

    public async Task<ApiResponse<List<RecentSalonItem>>> GetRecentSalonsAsync(int count = 5)
    {
        var salons = await _context.Salons
            .Include(s => s.Owner)
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .Take(count)
            .Select(s => new RecentSalonItem
            {
                Id = s.Id,
                Name = s.Name,
                OwnerName = s.Owner != null ? s.Owner.FullName : "",
                City = s.City,
                IsVerified = s.IsVerified,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<RecentSalonItem>>.Ok(salons);
    }

    public async Task<ApiResponse<List<RecentSalonItem>>> GetPendingSalonsAsync()
    {
        var salons = await _context.Salons
            .Include(s => s.Owner)
            .Where(s => !s.IsDeleted && s.IsActive && !s.IsVerified)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new RecentSalonItem
            {
                Id = s.Id,
                Name = s.Name,
                OwnerName = s.Owner != null ? s.Owner.FullName : "",
                City = s.City,
                IsVerified = s.IsVerified,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<RecentSalonItem>>.Ok(salons);
    }

    public async Task<ApiResponse<List<DeletedSalonItem>>> GetDeletedSalonsAsync(int count = 3)
    {
        var salons = await _context.Salons
            .Include(s => s.Owner)
            .Where(s => s.IsDeleted)
            .OrderByDescending(s => s.DeletedAt)
            .Take(count)
            .Select(s => new DeletedSalonItem
            {
                Id = s.Id,
                Name = s.Name,
                OwnerName = s.Owner != null ? s.Owner.FullName : "",
                DeletionReason = s.DeletionReason,
                DeletedAt = s.DeletedAt
            })
            .ToListAsync();

        return ApiResponse<List<DeletedSalonItem>>.Ok(salons);
    }

    #region User Management

    public async Task<ApiResponse<PaginatedResult<AdminUserListItem>>> GetAllUsersAsync(AdminUserFilterRequest request)
    {
        var query = _context.Users.AsQueryable();

        // Adminleri ve Salon Sahiplerini bu listede getirme (Onlar için ayrı sayfalar var)
        query = query.Where(u => u.Role != UserRole.Admin && u.Role != UserRole.SalonOwner);

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(search) || u.Email.ToLower().Contains(search));
        }

        if (request.Role.HasValue)
        {
            query = query.Where(u => u.Role == request.Role.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new AdminUserListItem
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber ?? "",
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLoginDate = u.LastLoginAt
            })
            .ToListAsync();

        var result = new PaginatedResult<AdminUserListItem>(items, totalCount, request.PageNumber, request.PageSize);
        return ApiResponse<PaginatedResult<AdminUserListItem>>.Ok(result);
    }

    public async Task<ApiResponse<AdminUserDetail>> GetUserDetailAsync(string userId)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
                                                         
            if (user == null)
                return ApiResponse<AdminUserDetail>.Fail("Kullanıcı bulunamadı");

            var appointmentCount = 0;
            var reviewCount = 0;
            
            try
            {
                appointmentCount = await _context.Appointments.CountAsync(a => a.CustomerId == userId && !a.IsDeleted);
                reviewCount = await _context.Reviews.CountAsync(r => r.CustomerId == userId && !r.IsDeleted);
            }
            catch
            {
                // Ignore count errors - tables might not exist yet
            }
            
            // Get salon separately to avoid DeletionReason column issue
            Salon? salon = null;
            try
            {
                salon = await _context.Salons.FirstOrDefaultAsync(s => s.OwnerId == userId && !s.IsDeleted);
            }
            catch
            {
                // Ignore if salons table has schema issues
            }

            var detail = new AdminUserDetail
            {
                Id = user.Id,
                FullName = user.FullName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginDate = user.LastLoginAt,
                AppointmentCount = appointmentCount,
                ReviewCount = reviewCount,
                SalonId = salon?.Id,
                SalonName = salon?.Name
            };

            return ApiResponse<AdminUserDetail>.Ok(detail);
        }
        catch (Exception ex)
        {
            return ApiResponse<AdminUserDetail>.Fail($"Kullanıcı detayları yüklenirken hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<AdminUserListItem>>> GetAdminsAsync()
    {
        try
        {
            var admins = await _context.Users
                .Where(u => u.Role == UserRole.Admin && !u.IsDeleted)
                .Select(u => new AdminUserListItem
                {
                    Id = u.Id,
                    FullName = u.FullName ?? "",
                    Email = u.Email ?? "",
                    PhoneNumber = u.PhoneNumber ?? "",
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    LastLoginDate = u.LastLoginAt
                })
                .ToListAsync();

            // Hierarchy Logic
            foreach (var admin in admins)
            {
                if (string.Equals(admin.Email, "admin@kuaforsepeti.com", StringComparison.OrdinalIgnoreCase))
                {
                    admin.Level = 1;
                    admin.IsProtected = true;
                }
                else if (string.Equals(admin.Email, "ahmet@kuaforsepeti.com", StringComparison.OrdinalIgnoreCase) || 
                         string.Equals(admin.Email, "alper@kuaforsepeti.com", StringComparison.OrdinalIgnoreCase))
                {
                    admin.Level = 2;
                    admin.IsProtected = true;
                }
                else
                {
                    admin.Level = 3;
                    admin.IsProtected = false;
                }
            }

            // Sort: Level 1 -> Level 2 -> Level 3 -> Newest First
            admins = admins.OrderBy(a => a.Level).ThenByDescending(a => a.CreatedAt).ToList();

            return ApiResponse<List<AdminUserListItem>>.Ok(admins);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<AdminUserListItem>>.Fail($"Admin listesi alınırken hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse> SuspendUserAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.IsDeleted)
            return ApiResponse.Fail("Kullanıcı bulunamadı");

        if (IsProtectedAdmin(user.Email))
            return ApiResponse.Fail("Bu yönetici hesabı askıya alınamaz");

        if (user.Role == UserRole.Admin)
            return ApiResponse.Fail("Admin kullanıcıları askıya alınamaz");

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        user.RefreshToken = null; // Force re-login
        
        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Kullanıcı askıya alındı");
    }

    public async Task<ApiResponse> ActivateUserAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.IsDeleted)
            return ApiResponse.Fail("Kullanıcı bulunamadı");

        if (IsProtectedAdmin(user.Email))
             return ApiResponse.Fail("Bu yönetici hesabı üzerinde işlem yapılamaz");

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return ApiResponse.Ok("Kullanıcı aktifleştirildi");
    }

    public async Task<ApiResponse> UpdateUserRoleAsync(string userId, UserRole newRole)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.IsDeleted)
            return ApiResponse.Fail("Kullanıcı bulunamadı");

        if (IsProtectedAdmin(user.Email))
            return ApiResponse.Fail("Bu kullanıcının yetkisi değiştirilemez");

        user.Role = newRole;
        user.UpdatedAt = DateTime.UtcNow;
        user.RefreshToken = null; // Force re-login
        
        await _context.SaveChangesAsync();
        return ApiResponse.Ok("Kullanıcı yetkisi güncellendi");
    }

    private bool IsProtectedAdmin(string? email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        var protectedEmails = new[] 
        { 
            "admin@kuaforsepeti.com", 
            "ahmet@kuaforsepeti.com", 
            "alper@kuaforsepeti.com" 
        };
        return protectedEmails.Contains(email.ToLower());
    }

    #endregion

    #region Review Management

    public async Task<ApiResponse<PaginatedResult<AdminReviewListItem>>> GetAllReviewsAsync(AdminReviewFilterRequest request)
    {
        var query = _context.Reviews
            .Include(r => r.Salon)
            .Include(r => r.Customer)
            .AsQueryable();

        // Filters
        if (request.MinRating.HasValue)
            query = query.Where(r => r.Rating >= request.MinRating.Value);

        if (request.MaxRating.HasValue)
            query = query.Where(r => r.Rating <= request.MaxRating.Value);

        if (request.IsVisible.HasValue)
            query = query.Where(r => r.IsVisible == request.IsVisible.Value);

        if (!string.IsNullOrEmpty(request.SalonId))
            query = query.Where(r => r.SalonId == request.SalonId);

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            query = query.Where(r =>
                (r.Comment != null && r.Comment.ToLower().Contains(search)) ||
                r.Salon.Name.ToLower().Contains(search) ||
                r.Customer.FullName.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new AdminReviewListItem
            {
                Id = r.Id,
                SalonId = r.SalonId,
                SalonName = r.Salon.Name,
                CustomerId = r.CustomerId,
                CustomerName = r.Customer.FullName,
                CustomerEmail = r.Customer.Email ?? "",
                Rating = r.Rating,
                Comment = r.Comment,
                Reply = r.Reply,
                RepliedAt = r.RepliedAt,
                IsVisible = r.IsVisible,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        var result = new PaginatedResult<AdminReviewListItem>
        {
            Items = reviews,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return ApiResponse<PaginatedResult<AdminReviewListItem>>.Ok(result);
    }

    public async Task<ApiResponse> HideReviewAsync(string reviewId)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review == null)
            return ApiResponse.Fail("Yorum bulunamadı");

        review.IsVisible = false;
        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Yorum gizlendi");
    }

    public async Task<ApiResponse> ShowReviewAsync(string reviewId)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review == null)
            return ApiResponse.Fail("Yorum bulunamadı");

        review.IsVisible = true;
        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Yorum görünür yapıldı");
    }

    #endregion

    #region Reports

    private static readonly string[] TurkishMonths = { "", "Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara" };

    public async Task<ApiResponse<List<MonthlyTrendData>>> GetMonthlyTrendsAsync()
    {
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-5);
        var startDate = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

        var months = Enumerable.Range(0, 6).Select(i => startDate.AddMonths(i)).ToList();

        var newSalons = await _context.Salons
            .Where(s => s.CreatedAt >= startDate)
            .GroupBy(s => new { s.CreatedAt.Year, s.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync();

        var newUsers = await _context.Users
            .Where(u => u.CreatedAt >= startDate)
            .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync();

        var appointments = await _context.Appointments
            .Where(a => a.CreatedAt >= startDate)
            .GroupBy(a => new { a.CreatedAt.Year, a.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count(), Revenue = g.Sum(a => a.TotalPrice) })
            .ToListAsync();

        var reviews = await _context.Reviews
            .Where(r => r.CreatedAt >= startDate)
            .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync();

        var result = months.Select(m => new MonthlyTrendData
        {
            Month = $"{m.Year}-{m.Month:D2}",
            MonthLabel = $"{TurkishMonths[m.Month]} {m.Year}",
            NewSalons = newSalons.FirstOrDefault(s => s.Year == m.Year && s.Month == m.Month)?.Count ?? 0,
            NewUsers = newUsers.FirstOrDefault(u => u.Year == m.Year && u.Month == m.Month)?.Count ?? 0,
            Appointments = appointments.FirstOrDefault(a => a.Year == m.Year && a.Month == m.Month)?.Count ?? 0,
            Revenue = appointments.FirstOrDefault(a => a.Year == m.Year && a.Month == m.Month)?.Revenue ?? 0,
            Reviews = reviews.FirstOrDefault(r => r.Year == m.Year && r.Month == m.Month)?.Count ?? 0,
        }).ToList();

        return ApiResponse<List<MonthlyTrendData>>.Ok(result);
    }

    #endregion

    #region Notifications

    public async Task<ApiResponse<List<AdminNotificationItem>>> GetAdminNotificationsAsync(int count = 20)
    {
        // Get notifications targeted at admin users
        var adminUserIds = await _context.Users
            .Where(u => u.Role == UserRole.Admin)
            .Select(u => u.Id)
            .ToListAsync();

        var notifications = await _context.Notifications
            .Where(n => adminUserIds.Contains(n.UserId) || n.Type == NotificationType.System)
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .Select(n => new AdminNotificationItem
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type.ToString(),
                RelatedEntityType = n.RelatedEntityType,
                RelatedEntityId = n.RelatedEntityId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<AdminNotificationItem>>.Ok(notifications);
    }

    public async Task<ApiResponse<int>> GetUnreadNotificationCountAsync()
    {
        var adminUserIds = await _context.Users
            .Where(u => u.Role == UserRole.Admin)
            .Select(u => u.Id)
            .ToListAsync();

        var count = await _context.Notifications
            .Where(n => (adminUserIds.Contains(n.UserId) || n.Type == NotificationType.System) && !n.IsRead)
            .CountAsync();

        return ApiResponse<int>.Ok(count);
    }

    public async Task<ApiResponse> MarkNotificationReadAsync(string notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification == null)
            return ApiResponse.Fail("Bildirim bulunamadı");

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Bildirim okundu olarak işaretlendi");
    }

    public async Task<ApiResponse> MarkAllNotificationsReadAsync()
    {
        var adminUserIds = await _context.Users
            .Where(u => u.Role == UserRole.Admin)
            .Select(u => u.Id)
            .ToListAsync();

        var unread = await _context.Notifications
            .Where(n => (adminUserIds.Contains(n.UserId) || n.Type == NotificationType.System) && !n.IsRead)
            .ToListAsync();

        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return ApiResponse.Ok($"{unread.Count} bildirim okundu olarak işaretlendi");
    }

    #endregion

    #region Admin Logs

    public async Task<ApiResponse<PaginatedResult<AdminLogItem>>> GetAdminLogsAsync(int pageNumber = 1, int pageSize = 20, string? adminId = null, string? action = null)
    {
        var query = _context.AdminLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(adminId))
            query = query.Where(l => l.AdminId == adminId);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(l => l.Action.Contains(action));

        var totalCount = await query.CountAsync();
        var logs = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new AdminLogItem
            {
                Id = l.Id,
                AdminId = l.AdminId,
                AdminName = l.AdminName,
                Action = l.Action,
                TargetEntity = l.TargetEntity,
                TargetId = l.TargetId,
                TargetName = l.TargetName,
                Details = l.Details,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<PaginatedResult<AdminLogItem>>.Ok(new PaginatedResult<AdminLogItem>
        {
            Items = logs,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    public async Task LogAdminActionAsync(string adminId, string adminName, string action, string targetEntity, string? targetId = null, string? targetName = null, string? details = null)
    {
        var log = new AdminLog
        {
            AdminId = adminId,
            AdminName = adminName,
            Action = action,
            TargetEntity = targetEntity,
            TargetId = targetId,
            TargetName = targetName,
            Details = details,
        };

        _context.AdminLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    #endregion
}
