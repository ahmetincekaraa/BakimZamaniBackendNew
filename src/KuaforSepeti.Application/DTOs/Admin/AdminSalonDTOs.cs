namespace KuaforSepeti.Application.DTOs.Admin;

/// <summary>
/// Salon list item for admin panel.
/// </summary>
public class AdminSalonListItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }  // Onay durumu
    public double Rating { get; set; }
    public int TotalReviews { get; set; }
    public int TotalAppointments { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Salon detail for admin panel.
/// </summary>
public class AdminSalonDetail
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }  // Onay durumu
    public double Rating { get; set; }
    public int TotalReviews { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Owner info
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerPhone { get; set; } = string.Empty;
    
    // Stats
    public int TotalStaff { get; set; }
    public int TotalServices { get; set; }
    
    // Randevu İstatistikleri (Tüm zamanlar)
    public int TotalAppointments { get; set; }      // Toplam başvuru
    public int CompletedAppointments { get; set; }  // Tamamlanan
    public int PendingAppointments { get; set; }    // Bekleyen (anlık)
    public int CancelledAppointments { get; set; }  // İptal edilen
    
    // Platform Katkısı
    public int TotalCustomers { get; set; }         // Uygulamamızdan giden benzersiz müşteri sayısı
    public decimal TotalRevenue { get; set; }       // Toplam kazanç (tüm zamanlar)
    public decimal MonthlyRevenue { get; set; }     // Bu ayki kazanç

    // Audit Info
    public string? ApprovedByAdminName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? SuspendedByAdminName { get; set; }
    public DateTime? SuspendedAt { get; set; }
}

/// <summary>
/// Deleted salon item for admin panel.
/// </summary>
public class DeletedSalonItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string? DeletionReason { get; set; }
    public DateTime? DeletedAt { get; set; }
}

/// <summary>
/// Recent salon item for dashboard.
/// </summary>
public class RecentSalonItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Delete salon request.
/// </summary>
public class DeleteSalonRequest
{
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Salon status filter enum.
/// </summary>
public enum SalonStatusFilter
{
    All = 0,
    Active = 1,       // IsActive=true && IsVerified=true
    Pending = 2,      // IsActive=true && IsVerified=false
    Suspended = 3     // IsActive=false && IsVerified=true
}

/// <summary>
/// Salon filter request for admin.
/// </summary>
public class AdminSalonFilterRequest
{
    public string? Search { get; set; }
    public SalonStatusFilter? Status { get; set; }  // Yeni status filtresi
    public string? City { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
