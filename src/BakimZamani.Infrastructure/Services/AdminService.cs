namespace BakimZamani.Infrastructure.Services;

using BakimZamani.Application.DTOs.Admin;
using BakimZamani.Application.DTOs.Common;
using BakimZamani.Application.Services.Interfaces;
using BakimZamani.Domain.Entities;
using BakimZamani.Domain.Enums;
using BakimZamani.Infrastructure.Data;
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
            return ApiResponse<AdminSalonDetail>.Fail("Salon bulunamadÄ±");

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
            
            // Platform KatkÄ±sÄ± - UygulamamÄ±zdan giden benzersiz mÃ¼ÅŸteri sayÄ±sÄ±
            TotalCustomers = await _context.Appointments
                .Where(a => a.SalonId == salonId && a.Status == AppointmentStatus.Completed && !a.IsDeleted)
                .Select(a => a.CustomerId)
                .Distinct()
                .CountAsync(),
            
            // Toplam kazanÃ§ (tÃ¼m zamanlar - tamamlanmÄ±ÅŸ randevulardan)
            TotalRevenue = await _context.Appointments
                .Where(a => a.SalonId == salonId && a.Status == AppointmentStatus.Completed && !a.IsDeleted)
                .SumAsync(a => a.TotalPrice),
            
            // Bu ayki kazanÃ§
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
            return ApiResponse.Fail("Salon bulunamadÄ±");

        salon.IsActive = true;
        salon.IsVerified = true;
        salon.UpdatedAt = DateTime.UtcNow;
        
        // Audit
        salon.ApprovedByAdminId = adminId;
        salon.ApprovedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var adminName = await GetAdminNameAsync(adminId);
        await LogAdminActionAsync(adminId, adminName, "Salon OnaylandÄ±", "Salon", salonId, salon.Name);

        return ApiResponse.Ok("Salon onaylandÄ±");
    }

    public async Task<ApiResponse> SuspendSalonAsync(string salonId, string adminId)
    {
        var salon = await _context.Salons.FindAsync(salonId);
        if (salon == null || salon.IsDeleted)
            return ApiResponse.Fail("Salon bulunamadÄ±");

        salon.IsActive = false;
        salon.UpdatedAt = DateTime.UtcNow;

        // Audit
        salon.SuspendedByAdminId = adminId;
        salon.SuspendedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var adminName = await GetAdminNameAsync(adminId);
        await LogAdminActionAsync(adminId, adminName, "Salon AskÄ±ya AlÄ±ndÄ±", "Salon", salonId, salon.Name);

        return ApiResponse.Ok("Salon askÄ±ya alÄ±ndÄ±");
    }

    public async Task<ApiResponse> ActivateSalonAsync(string salonId, string adminId)
    {
        var salon = await _context.Salons.FindAsync(salonId);
        if (salon == null || salon.IsDeleted)
            return ApiResponse.Fail("Salon bulunamadÄ±");

        salon.IsActive = true;
        salon.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var adminName = await GetAdminNameAsync(adminId);
        await LogAdminActionAsync(adminId, adminName, "Salon AktifleÅŸtirildi", "Salon", salonId, salon.Name);

        return ApiResponse.Ok("Salon aktifleÅŸtirildi");
    }

    public async Task<ApiResponse> DeleteSalonAsync(string salonId, string reason, string adminId)
    {
        var salon = await _context.Salons.FindAsync(salonId);
        if (salon == null || salon.IsDeleted)
            return ApiResponse.Fail("Salon bulunamadÄ±");

        salon.IsDeleted = true;
        salon.IsActive = false;
        salon.DeletionReason = reason;
        salon.DeletedAt = DateTime.UtcNow;
        salon.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var adminName = await GetAdminNameAsync(adminId);
        await LogAdminActionAsync(adminId, adminName, "Salon Silindi", "Salon", salonId, salon.Name, $"Sebep: {reason}");

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

        // Adminleri ve Salon Sahiplerini bu listede getirme (Onlar iÃ§in ayrÄ± sayfalar var)
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
                return ApiResponse<AdminUserDetail>.Fail("KullanÄ±cÄ± bulunamadÄ±");

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
            return ApiResponse<AdminUserDetail>.Fail($"KullanÄ±cÄ± detaylarÄ± yÃ¼klenirken hata: {ex.Message}");
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
                if (string.Equals(admin.Email, "admin@bakimzamani.com", StringComparison.OrdinalIgnoreCase))
                {
                    admin.Level = 1;
                    admin.IsProtected = true;
                }
                else if (string.Equals(admin.Email, "ahmet@bakimzamani.com", StringComparison.OrdinalIgnoreCase) || 
                         string.Equals(admin.Email, "alper@bakimzamani.com", StringComparison.OrdinalIgnoreCase))
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
            return ApiResponse<List<AdminUserListItem>>.Fail($"Admin listesi alÄ±nÄ±rken hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse> SuspendUserAsync(string userId, string adminId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.IsDeleted)
            return ApiResponse.Fail("KullanÄ±cÄ± bulunamadÄ±");

        if (IsProtectedAdmin(user.Email))
            return ApiResponse.Fail("Bu yÃ¶netici hesabÄ± askÄ±ya alÄ±namaz");

        if (user.Role == UserRole.Admin)
            return ApiResponse.Fail("Admin kullanÄ±cÄ±larÄ± askÄ±ya alÄ±namaz");

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        user.RefreshToken = null; // Force re-login
        
        await _context.SaveChangesAsync();

        var adminName = await GetAdminNameAsync(adminId);
        await LogAdminActionAsync(adminId, adminName, "KullanÄ±cÄ± AskÄ±ya AlÄ±ndÄ±", "User", userId, user.FullName);

        return ApiResponse.Ok("KullanÄ±cÄ± askÄ±ya alÄ±ndÄ±");
    }

    public async Task<ApiResponse> ActivateUserAsync(string userId, string adminId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.IsDeleted)
            return ApiResponse.Fail("KullanÄ±cÄ± bulunamadÄ±");

        if (IsProtectedAdmin(user.Email))
             return ApiResponse.Fail("Bu yÃ¶netici hesabÄ± Ã¼zerinde iÅŸlem yapÄ±lamaz");

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        var adminName = await GetAdminNameAsync(adminId);
        await LogAdminActionAsync(adminId, adminName, "KullanÄ±cÄ± AktifleÅŸtirildi", "User", userId, user.FullName);

        return ApiResponse.Ok("KullanÄ±cÄ± aktifleÅŸtirildi");
    }

    public async Task<ApiResponse> UpdateUserRoleAsync(string userId, UserRole newRole, string adminId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.IsDeleted)
            return ApiResponse.Fail("KullanÄ±cÄ± bulunamadÄ±");

        if (IsProtectedAdmin(user.Email))
            return ApiResponse.Fail("Bu kullanÄ±cÄ±nÄ±n yetkisi deÄŸiÅŸtirilemez");

        var oldRole = user.Role;
        user.Role = newRole;
        user.UpdatedAt = DateTime.UtcNow;
        user.RefreshToken = null; // Force re-login
        
        await _context.SaveChangesAsync();

        var adminName = await GetAdminNameAsync(adminId);
        await LogAdminActionAsync(adminId, adminName, "KullanÄ±cÄ± RolÃ¼ GÃ¼ncellendi", "User", userId, user.FullName, $"{oldRole} â†’ {newRole}");

        return ApiResponse.Ok("KullanÄ±cÄ± yetkisi gÃ¼ncellendi");
    }

    private bool IsProtectedAdmin(string? email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        var protectedEmails = new[] 
        { 
            "admin@bakimzamani.com", 
            "ahmet@bakimzamani.com", 
            "alper@bakimzamani.com" 
        };
        return protectedEmails.Contains(email.ToLower());
    }

    private async Task<string> GetAdminNameAsync(string adminId)
    {
        var admin = await _context.Users.FindAsync(adminId);
        return admin?.FullName ?? "Bilinmeyen Admin";
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

    public async Task<ApiResponse> HideReviewAsync(string reviewId, string adminId)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review == null)
            return ApiResponse.Fail("Yorum bulunamadÄ±");

        review.IsVisible = false;
        await _context.SaveChangesAsync();

        var adminName = await GetAdminNameAsync(adminId);
        await LogAdminActionAsync(adminId, adminName, "Yorum Gizlendi", "Review", reviewId);

        return ApiResponse.Ok("Yorum gizlendi");
    }

    public async Task<ApiResponse> ShowReviewAsync(string reviewId, string adminId)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review == null)
            return ApiResponse.Fail("Yorum bulunamadÄ±");

        review.IsVisible = true;
        await _context.SaveChangesAsync();

        var adminName = await GetAdminNameAsync(adminId);
        await LogAdminActionAsync(adminId, adminName, "Yorum GÃ¶sterildi", "Review", reviewId);

        return ApiResponse.Ok("Yorum gÃ¶rÃ¼nÃ¼r yapÄ±ldÄ±");
    }

    #endregion

    #region Reports

    private static readonly string[] TurkishMonths = { "", "Oca", "Åžub", "Mar", "Nis", "May", "Haz", "Tem", "AÄŸu", "Eyl", "Eki", "Kas", "Ara" };

    public async Task<ApiResponse<List<MonthlyTrendData>>> GetMonthlyTrendsAsync()
    {

        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-5);
        var startDate = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

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

    public async Task<ApiResponse<DetailedReportData>> GetDetailedReportAsync(DateTime startDate, DateTime endDate)
    {
        // Normalize dates to UTC
        startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc).Date;
        endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc).Date.AddDays(1).AddTicks(-1); // End of day

        // ==== Appointments ====
        var appointments = await _context.Appointments
            .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate)
            .Select(a => new { a.Status, a.TotalPrice, a.CreatedAt })
            .ToListAsync();

        var totalAppointments = appointments.Count;
        var completedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed);
        var cancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.CancelledByCustomer || a.Status == AppointmentStatus.CancelledBySalon);
        var totalRevenue = appointments.Where(a => a.Status == AppointmentStatus.Completed).Sum(a => a.TotalPrice);
        var avgPrice = completedAppointments > 0 ? totalRevenue / completedAppointments : 0;
        var cancellationRate = totalAppointments > 0 ? (double)cancelledAppointments / totalAppointments * 100 : 0;

        // ==== Status breakdown ====
        var statusLabels = new Dictionary<AppointmentStatus, (string Label, string Color)>
        {
            { AppointmentStatus.Pending, ("Bekliyor", "#F59E0B") },
            { AppointmentStatus.Confirmed, ("OnaylÄ±", "#3B82F6") },
            { AppointmentStatus.Completed, ("TamamlandÄ±", "#10B981") },
            { AppointmentStatus.CancelledByCustomer, ("MÃ¼ÅŸteri Ä°ptali", "#EF4444") },
            { AppointmentStatus.CancelledBySalon, ("Salon Ä°ptali", "#F97316") },
            { AppointmentStatus.NoShow, ("Gelmedi", "#6B7280") },
        };

        var statusBreakdown = appointments
            .GroupBy(a => a.Status)
            .Select(g => new StatusBreakdownItem
            {
                Status = g.Key.ToString(),
                StatusLabel = statusLabels.ContainsKey(g.Key) ? statusLabels[g.Key].Label : g.Key.ToString(),
                Count = g.Count(),
                Color = statusLabels.ContainsKey(g.Key) ? statusLabels[g.Key].Color : "#9CA3AF",
            })
            .OrderByDescending(s => s.Count)
            .ToList();

        // ==== Daily revenue ====
        var dailyRevenue = appointments
            .Where(a => a.Status == AppointmentStatus.Completed)
            .GroupBy(a => a.CreatedAt.Date)
            .Select(g => new DailyRevenueItem
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                DateLabel = g.Key.ToString("dd MMM", new System.Globalization.CultureInfo("tr-TR")),
                Revenue = g.Sum(a => a.TotalPrice),
                Appointments = g.Count(),
            })
            .OrderBy(d => d.Date)
            .ToList();

        // ==== Top 5 salons ====
        var topSalons = await _context.Appointments
            .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate && a.Status == AppointmentStatus.Completed)
            .GroupBy(a => a.SalonId)
            .Select(g => new
            {
                SalonId = g.Key,
                AppointmentCount = g.Count(),
                Revenue = g.Sum(a => a.TotalPrice),
            })
            .OrderByDescending(s => s.Revenue)
            .Take(5)
            .ToListAsync();

        var salonIds = topSalons.Select(s => s.SalonId).ToList();
        var salons = await _context.Salons
            .Where(s => salonIds.Contains(s.Id))
            .Select(s => new { s.Id, s.Name, s.City })
            .ToListAsync();

        var salonRatings = await _context.Reviews
            .Where(r => salonIds.Contains(r.SalonId))
            .GroupBy(r => r.SalonId)
            .Select(g => new { SalonId = g.Key, AvgRating = g.Average(r => r.Rating) })
            .ToListAsync();

        var topSalonItems = topSalons.Select(ts =>
        {
            var salon = salons.FirstOrDefault(s => s.Id == ts.SalonId);
            var rating = salonRatings.FirstOrDefault(r => r.SalonId == ts.SalonId);
            return new TopSalonItem
            {
                SalonId = ts.SalonId,
                SalonName = salon?.Name ?? "Bilinmiyor",
                City = salon?.City ?? "",
                AppointmentCount = ts.AppointmentCount,
                Revenue = ts.Revenue,
                AverageRating = rating != null ? Math.Round(rating.AvgRating, 1) : 0,
            };
        }).ToList();

        // ==== New salons & users ====
        var newSalons = await _context.Salons.CountAsync(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate);
        var newUsers = await _context.Users.CountAsync(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate && u.Role == UserRole.Customer);

        // ==== Reviews ====
        var reviewsInRange = await _context.Reviews
            .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
            .ToListAsync();
        var totalReviews = reviewsInRange.Count;
        var avgRating = totalReviews > 0 ? Math.Round(reviewsInRange.Average(r => r.Rating), 1) : 0;

        var report = new DetailedReportData
        {
            TotalAppointments = totalAppointments,
            TotalRevenue = totalRevenue,
            CompletedAppointments = completedAppointments,
            CancelledAppointments = cancelledAppointments,
            CancellationRate = Math.Round(cancellationRate, 1),
            AveragePrice = avgPrice,
            NewSalons = newSalons,
            NewUsers = newUsers,
            TotalReviews = totalReviews,
            AverageRating = avgRating,
            StatusBreakdown = statusBreakdown,
            DailyRevenue = dailyRevenue,
            TopSalons = topSalonItems,
        };

        return ApiResponse<DetailedReportData>.Ok(report);
    }

    public async Task<ApiResponse<RevenueSummary>> GetRevenueSummaryAsync(DateTime startDate, DateTime endDate)
    {
        startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        var completedAppointments = await _context.Appointments
            .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate && a.Status == AppointmentStatus.Completed)
            .Select(a => new { a.TotalPrice, a.CreatedAt, a.SalonId })
            .ToListAsync();

        var totalRevenue = completedAppointments.Sum(a => a.TotalPrice);
        var totalTransactions = completedAppointments.Count;
        var avgOrderValue = totalTransactions > 0 ? totalRevenue / totalTransactions : 0;

        // Active salons count
        var activeSalons = await _context.Salons.CountAsync(s => s.IsActive && !s.IsDeleted);

        // Revenue growth compared to previous period
        var periodLength = (endDate - startDate).TotalDays;
        var prevStart = startDate.AddDays(-periodLength);
        var prevEnd = startDate;
        var prevRevenue = await _context.Appointments
            .Where(a => a.CreatedAt >= prevStart && a.CreatedAt < prevEnd && a.Status == AppointmentStatus.Completed)
            .SumAsync(a => a.TotalPrice);
        var revenueGrowth = prevRevenue > 0 ? ((totalRevenue - prevRevenue) / prevRevenue) * 100 : 0;

        // Monthly revenue (last 6 months)
        var months = Enumerable.Range(0, 6).Select(i => DateTime.UtcNow.AddMonths(-i)).Reverse().ToList();
        var monthlyRevenue = new List<MonthlyRevenueItem>();
        foreach (var month in months)
        {
            var monthStart = new DateTime(month.Year, month.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);
            var monthAppts = await _context.Appointments
                .Where(a => a.CreatedAt >= monthStart && a.CreatedAt < monthEnd && a.Status == AppointmentStatus.Completed)
                .ToListAsync();
            var turkishMonths = new[] { "Oca", "Åžub", "Mar", "Nis", "May", "Haz", "Tem", "AÄŸu", "Eyl", "Eki", "Kas", "Ara" };
            monthlyRevenue.Add(new MonthlyRevenueItem
            {
                Month = monthStart.ToString("yyyy-MM"),
                MonthLabel = $"{turkishMonths[month.Month - 1]} {month.Year}",
                Revenue = monthAppts.Sum(a => a.TotalPrice),
                Appointments = monthAppts.Count,
            });
        }

        // City revenue breakdown
        var cityColors = new[] { "#6366f1", "#8b5cf6", "#a855f7", "#ec4899", "#f43f5e", "#f97316", "#eab308", "#22c55e" };
        var cityData = await _context.Appointments
            .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate && a.Status == AppointmentStatus.Completed)
            .Include(a => a.Salon)
            .GroupBy(a => a.Salon!.City)
            .Select(g => new
            {
                City = g.Key ?? "Bilinmiyor",
                Revenue = g.Sum(a => a.TotalPrice),
                AppointmentCount = g.Count(),
            })
            .OrderByDescending(x => x.Revenue)
            .Take(8)
            .ToListAsync();

        var cityRevenue = cityData.Select((c, i) => new CityRevenueItem
        {
            City = c.City,
            Revenue = c.Revenue,
            SalonCount = 0, // will be filled
            AppointmentCount = c.AppointmentCount,
            Color = cityColors[i % cityColors.Length],
        }).ToList();

        // Fill salon counts per city
        foreach (var city in cityRevenue)
        {
            city.SalonCount = await _context.Salons.CountAsync(s => s.City == city.City && s.IsActive && !s.IsDeleted);
        }

        // Top salons
        var topSalons = await _context.Appointments
            .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate && a.Status == AppointmentStatus.Completed)
            .GroupBy(a => a.SalonId)
            .Select(g => new
            {
                SalonId = g.Key,
                Revenue = g.Sum(a => a.TotalPrice),
                Count = g.Count(),
            })
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToListAsync();

        var topSalonItems = new List<SalonRevenueItem>();
        foreach (var ts in topSalons)
        {
            var salon = await _context.Salons.Include(s => s.Owner).FirstOrDefaultAsync(s => s.Id == ts.SalonId);
            if (salon == null) continue;
            var totalAppts = await _context.Appointments.CountAsync(a => a.SalonId == ts.SalonId && a.CreatedAt >= startDate && a.CreatedAt <= endDate);
            var cancelledAppts = await _context.Appointments.CountAsync(a => a.SalonId == ts.SalonId && a.CreatedAt >= startDate && a.CreatedAt <= endDate && (a.Status == AppointmentStatus.CancelledByCustomer || a.Status == AppointmentStatus.CancelledBySalon));
            var avgRating = await _context.Reviews.Where(r => r.SalonId == ts.SalonId && !r.IsDeleted).AverageAsync(r => (double?)r.Rating) ?? 0;

            topSalonItems.Add(new SalonRevenueItem
            {
                SalonId = ts.SalonId,
                SalonName = salon.Name,
                City = salon.City ?? "",
                OwnerName = salon.Owner?.FullName ?? "",
                TotalAppointments = totalAppts,
                CompletedAppointments = ts.Count,
                CancelledAppointments = cancelledAppts,
                TotalRevenue = ts.Revenue,
                AveragePrice = ts.Count > 0 ? ts.Revenue / ts.Count : 0,
                AverageRating = Math.Round(avgRating, 1),
                CancellationRate = totalAppts > 0 ? Math.Round((double)cancelledAppts / totalAppts * 100, 1) : 0,
                Status = salon.IsActive ? "Aktif" : "AskÄ±da",
            });
        }

        var summary = new RevenueSummary
        {
            TotalRevenue = totalRevenue,
            AverageOrderValue = avgOrderValue,
            TotalTransactions = totalTransactions,
            ActiveSalons = activeSalons,
            RevenueGrowth = revenueGrowth,
            MonthlyRevenue = monthlyRevenue,
            CityRevenue = cityRevenue,
            TopSalons = topSalonItems,
        };

        return ApiResponse<RevenueSummary>.Ok(summary);
    }

    public async Task<ApiResponse<PaginatedResult<SalonRevenueItem>>> GetSalonRevenuesAsync(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 20, string? sortBy = null)
    {
        startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        var salonRevData = await _context.Appointments
            .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate && a.Status == AppointmentStatus.Completed)
            .GroupBy(a => a.SalonId)
            .Select(g => new
            {
                SalonId = g.Key,
                Revenue = g.Sum(a => a.TotalPrice),
                Count = g.Count(),
            })
            .ToListAsync();

        var items = new List<SalonRevenueItem>();
        foreach (var sr in salonRevData)
        {
            var salon = await _context.Salons.Include(s => s.Owner).FirstOrDefaultAsync(s => s.Id == sr.SalonId);
            if (salon == null) continue;
            var totalAppts = await _context.Appointments.CountAsync(a => a.SalonId == sr.SalonId && a.CreatedAt >= startDate && a.CreatedAt <= endDate);
            var cancelledAppts = await _context.Appointments.CountAsync(a => a.SalonId == sr.SalonId && a.CreatedAt >= startDate && a.CreatedAt <= endDate && (a.Status == AppointmentStatus.CancelledByCustomer || a.Status == AppointmentStatus.CancelledBySalon));
            var avgRating = await _context.Reviews.Where(r => r.SalonId == sr.SalonId && !r.IsDeleted).AverageAsync(r => (double?)r.Rating) ?? 0;

            items.Add(new SalonRevenueItem
            {
                SalonId = sr.SalonId,
                SalonName = salon.Name,
                City = salon.City ?? "",
                OwnerName = salon.Owner?.FullName ?? "",
                TotalAppointments = totalAppts,
                CompletedAppointments = sr.Count,
                CancelledAppointments = cancelledAppts,
                TotalRevenue = sr.Revenue,
                AveragePrice = sr.Count > 0 ? sr.Revenue / sr.Count : 0,
                AverageRating = Math.Round(avgRating, 1),
                CancellationRate = totalAppts > 0 ? Math.Round((double)cancelledAppts / totalAppts * 100, 1) : 0,
                Status = salon.IsActive ? "Aktif" : "AskÄ±da",
            });
        }

        // Sort
        items = (sortBy?.ToLower()) switch
        {
            "appointments" => items.OrderByDescending(x => x.TotalAppointments).ToList(),
            "rating" => items.OrderByDescending(x => x.AverageRating).ToList(),
            "name" => items.OrderBy(x => x.SalonName).ToList(),
            _ => items.OrderByDescending(x => x.TotalRevenue).ToList(),
        };

        var totalCount = items.Count;
        var paged = items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        var result = new PaginatedResult<SalonRevenueItem>
        {
            Items = paged,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        return ApiResponse<PaginatedResult<SalonRevenueItem>>.Ok(result);
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
            return ApiResponse.Fail("Bildirim bulunamadÄ±");

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Bildirim okundu olarak iÅŸaretlendi");
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
        return ApiResponse.Ok($"{unread.Count} bildirim okundu olarak iÅŸaretlendi");
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

    #region Campaigns

    private string GetCampaignStatus(Campaign c)
    {
        if (!c.IsActive) return "Pasif";
        if (DateTime.UtcNow > c.EndDate) return "SÃ¼resi DolmuÅŸ";
        if (c.MaxUsageCount.HasValue && c.CurrentUsageCount >= c.MaxUsageCount.Value) return "Limit DolmuÅŸ";
        if (DateTime.UtcNow < c.StartDate) return "PlanlandÄ±";
        return "Aktif";
    }

    public async Task<ApiResponse<PaginatedResult<CampaignListItem>>> GetCampaignsAsync(int pageNumber = 1, int pageSize = 20)
    {
        var query = _context.Campaigns.Where(c => !c.IsDeleted).OrderByDescending(c => c.CreatedAt);
        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        var result = new PaginatedResult<CampaignListItem>
        {
            Items = items.Select(c => new CampaignListItem
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                DiscountType = c.DiscountType,
                DiscountValue = c.DiscountValue,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                MaxUsageCount = c.MaxUsageCount,
                CurrentUsageCount = c.CurrentUsageCount,
                IsActive = c.IsActive,
                TargetAudience = c.TargetAudience,
                Status = GetCampaignStatus(c),
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };
        return ApiResponse<PaginatedResult<CampaignListItem>>.Ok(result);
    }

    public async Task<ApiResponse<CampaignDetailItem>> GetCampaignDetailAsync(string campaignId)
    {
        var c = await _context.Campaigns.FirstOrDefaultAsync(x => x.Id == campaignId && !x.IsDeleted);
        if (c == null) return ApiResponse<CampaignDetailItem>.Fail("Kampanya bulunamadÄ±");

        return ApiResponse<CampaignDetailItem>.Ok(new CampaignDetailItem
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Code = c.Code,
            DiscountType = c.DiscountType,
            DiscountValue = c.DiscountValue,
            MinimumOrderAmount = c.MinimumOrderAmount,
            MaxDiscountAmount = c.MaxDiscountAmount,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            MaxUsageCount = c.MaxUsageCount,
            CurrentUsageCount = c.CurrentUsageCount,
            IsActive = c.IsActive,
            TargetAudience = c.TargetAudience,
            Status = GetCampaignStatus(c),
            CreatedByAdminId = c.CreatedByAdminId,
            CreatedAt = c.CreatedAt,
        });
    }

    public async Task<ApiResponse> CreateCampaignAsync(CreateCampaignRequest request, string adminId)
    {
        // Check unique code
        var exists = await _context.Campaigns.AnyAsync(c => c.Code == request.Code && !c.IsDeleted);
        if (exists) return ApiResponse.Fail("Bu kampanya kodu zaten kullanÄ±lÄ±yor");

        var campaign = new Campaign
        {
            Name = request.Name,
            Description = request.Description,
            Code = request.Code.ToUpper(),
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MinimumOrderAmount = request.MinimumOrderAmount,
            MaxDiscountAmount = request.MaxDiscountAmount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            MaxUsageCount = request.MaxUsageCount,
            TargetAudience = request.TargetAudience,
            CreatedByAdminId = adminId,
        };

        _context.Campaigns.Add(campaign);
        await _context.SaveChangesAsync();

        var adminName = await GetAdminNameAsync(adminId);
        await LogAdminActionAsync(adminId, adminName, "Kampanya OluÅŸturuldu", "Campaign", campaign.Id, campaign.Name, $"Kod: {campaign.Code}");

        return ApiResponse.Ok("Kampanya oluÅŸturuldu");
    }

    public async Task<ApiResponse> UpdateCampaignAsync(string campaignId, UpdateCampaignRequest request, string adminId)
    {
        var c = await _context.Campaigns.FirstOrDefaultAsync(x => x.Id == campaignId && !x.IsDeleted);
        if (c == null) return ApiResponse.Fail("Kampanya bulunamadÄ±");

        if (request.Name != null) c.Name = request.Name;
        if (request.Description != null) c.Description = request.Description;
        if (request.DiscountType != null) c.DiscountType = request.DiscountType;
        if (request.DiscountValue.HasValue) c.DiscountValue = request.DiscountValue.Value;
        if (request.MinimumOrderAmount.HasValue) c.MinimumOrderAmount = request.MinimumOrderAmount.Value;
        if (request.MaxDiscountAmount.HasValue) c.MaxDiscountAmount = request.MaxDiscountAmount.Value;
        if (request.StartDate.HasValue) c.StartDate = request.StartDate.Value;
        if (request.EndDate.HasValue) c.EndDate = request.EndDate.Value;
        if (request.MaxUsageCount.HasValue) c.MaxUsageCount = request.MaxUsageCount.Value;
        if (request.TargetAudience != null) c.TargetAudience = request.TargetAudience;
        if (request.IsActive.HasValue) c.IsActive = request.IsActive.Value;

        c.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var adminName = await GetAdminNameAsync(adminId);
        await LogAdminActionAsync(adminId, adminName, "Kampanya GÃ¼ncellendi", "Campaign", c.Id, c.Name);

        return ApiResponse.Ok("Kampanya gÃ¼ncellendi");
    }

    public async Task<ApiResponse> DeleteCampaignAsync(string campaignId, string adminId)
    {
        var c = await _context.Campaigns.FirstOrDefaultAsync(x => x.Id == campaignId && !x.IsDeleted);
        if (c == null) return ApiResponse.Fail("Kampanya bulunamadÄ±");

        c.IsDeleted = true;
        c.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var adminName = await GetAdminNameAsync(adminId);
        await LogAdminActionAsync(adminId, adminName, "Kampanya Silindi", "Campaign", c.Id, c.Name);

        return ApiResponse.Ok("Kampanya silindi");
    }

    #endregion

    #region Content Management

    // Announcements
    public async Task<ApiResponse<List<AnnouncementListItem>>> GetAnnouncementsAsync()
    {
        var items = await _context.Announcements
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AnnouncementListItem
            {
                Id = a.Id,
                Title = a.Title,
                Content = a.Content,
                IsActive = a.IsActive,
                ExpirationDate = a.ExpirationDate,
                TargetAudience = a.TargetAudience,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<AnnouncementListItem>>.Ok(items);
    }

    public async Task<ApiResponse> CreateAnnouncementAsync(CreateAnnouncementRequest request, string adminId)
    {
        var announcement = new Announcement
        {
            Title = request.Title,
            Content = request.Content,
            ExpirationDate = request.ExpirationDate,
            TargetAudience = request.TargetAudience,
            CreatedByAdminId = adminId
        };

        _context.Announcements.Add(announcement);
        await _context.SaveChangesAsync();

        var adminName = await GetAdminNameAsync(adminId);
        await LogAdminActionAsync(adminId, adminName, "Duyuru OluÅŸturuldu", "Announcement", announcement.Id, announcement.Title);

        return ApiResponse.Ok("Duyuru oluÅŸturuldu");
    }

    public async Task<ApiResponse> UpdateAnnouncementAsync(string id, UpdateAnnouncementRequest request, string adminId)
    {
        var announcement = await _context.Announcements.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        if (announcement == null) return ApiResponse.Fail("Duyuru bulunamadÄ±");

        if (request.Title != null) announcement.Title = request.Title;
        if (request.Content != null) announcement.Content = request.Content;
        if (request.IsActive.HasValue) announcement.IsActive = request.IsActive.Value;
        if (request.ExpirationDate.HasValue) announcement.ExpirationDate = request.ExpirationDate.Value;
        if (request.TargetAudience != null) announcement.TargetAudience = request.TargetAudience;

        announcement.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var adminName = await GetAdminNameAsync(adminId);
        await LogAdminActionAsync(adminId, adminName, "Duyuru GÃ¼ncellendi", "Announcement", announcement.Id, announcement.Title);

        return ApiResponse.Ok("Duyuru gÃ¼ncellendi");
    }

    public async Task<ApiResponse> DeleteAnnouncementAsync(string id, string adminId)
    {
        var announcement = await _context.Announcements.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        if (announcement == null) return ApiResponse.Fail("Duyuru bulunamadÄ±");

        announcement.IsDeleted = true;
        announcement.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var adminName = await GetAdminNameAsync(adminId);
        await LogAdminActionAsync(adminId, adminName, "Duyuru Silindi", "Announcement", announcement.Id, announcement.Title);

        return ApiResponse.Ok("Duyuru silindi");
    }

    // FAQs
    public async Task<ApiResponse<List<FAQListItem>>> GetFAQsAsync()
    {
        var items = await _context.FAQs
            .Where(f => !f.IsDeleted)
            .OrderBy(f => f.DisplayOrder)
            .Select(f => new FAQListItem
            {
                Id = f.Id,
                Question = f.Question,
                Answer = f.Answer,
                Category = f.Category,
                DisplayOrder = f.DisplayOrder,
                IsActive = f.IsActive
            })
            .ToListAsync();

        return ApiResponse<List<FAQListItem>>.Ok(items);
    }

    public async Task<ApiResponse> CreateFAQAsync(CreateFAQRequest request, string adminId)
    {
        var faq = new FAQ
        {
            Question = request.Question,
            Answer = request.Answer,
            Category = request.Category,
            DisplayOrder = request.DisplayOrder
        };

        _context.FAQs.Add(faq);
        await _context.SaveChangesAsync();

        var adminName = await GetAdminNameAsync(adminId);
        await LogAdminActionAsync(adminId, adminName, "SSS OluÅŸturuldu", "FAQ", faq.Id, faq.Question);

        return ApiResponse.Ok("SSS eklendi");
    }

    public async Task<ApiResponse> UpdateFAQAsync(string id, UpdateFAQRequest request, string adminId)
    {
        var faq = await _context.FAQs.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);
        if (faq == null) return ApiResponse.Fail("KayÄ±t bulunamadÄ±");

        if (request.Question != null) faq.Question = request.Question;
        if (request.Answer != null) faq.Answer = request.Answer;
        if (request.Category != null) faq.Category = request.Category;
        if (request.DisplayOrder.HasValue) faq.DisplayOrder = request.DisplayOrder.Value;
        if (request.IsActive.HasValue) faq.IsActive = request.IsActive.Value;

        faq.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var adminName = await GetAdminNameAsync(adminId);
        await LogAdminActionAsync(adminId, adminName, "SSS GÃ¼ncellendi", "FAQ", faq.Id, faq.Question);

        return ApiResponse.Ok("SSS gÃ¼ncellendi");
    }

    public async Task<ApiResponse> DeleteFAQAsync(string id, string adminId)
    {
        var faq = await _context.FAQs.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);
        if (faq == null) return ApiResponse.Fail("KayÄ±t bulunamadÄ±");

        faq.IsDeleted = true;
        faq.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var adminName = await GetAdminNameAsync(adminId);
        await LogAdminActionAsync(adminId, adminName, "SSS Silindi", "FAQ", faq.Id, faq.Question);

        return ApiResponse.Ok("SSS silindi");
    }

    #endregion
}

