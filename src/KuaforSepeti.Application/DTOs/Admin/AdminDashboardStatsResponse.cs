namespace KuaforSepeti.Application.DTOs.Admin;

/// <summary>
/// Admin dashboard statistics response.
/// </summary>
public class AdminDashboardStatsResponse
{
    /// <summary>Today's total appointments count.</summary>
    public int TodayAppointments { get; set; }

    /// <summary>Pending approval appointments count.</summary>
    public int PendingApprovals { get; set; }

    /// <summary>Total registered customers count.</summary>
    public int TotalCustomers { get; set; }

    /// <summary>This month's total revenue.</summary>
    public decimal MonthlyRevenue { get; set; }

    /// <summary>Total active salons count.</summary>
    public int TotalSalons { get; set; }

    /// <summary>Total staff members count.</summary>
    public int TotalStaff { get; set; }

    /// <summary>Today's completed appointments.</summary>
    public int CompletedToday { get; set; }

    /// <summary>Today's cancelled appointments.</summary>
    public int CancelledToday { get; set; }
}
