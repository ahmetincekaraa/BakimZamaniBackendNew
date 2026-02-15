namespace BakimZamani.Application.DTOs.Admin;

/// <summary>
/// Revenue analytics DTOs.
/// </summary>

/// <summary>
/// Revenue statistics for a single salon.
/// </summary>
public class SalonRevenueItem
{
    public string SalonId { get; set; } = string.Empty;
    public string SalonName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AveragePrice { get; set; }
    public double AverageRating { get; set; }
    public double CancellationRate { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Overall revenue summary.
/// </summary>
public class RevenueSummary
{
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int TotalTransactions { get; set; }
    public int ActiveSalons { get; set; }
    public decimal RevenueGrowth { get; set; }
    public List<MonthlyRevenueItem> MonthlyRevenue { get; set; } = new();
    public List<CityRevenueItem> CityRevenue { get; set; } = new();
    public List<SalonRevenueItem> TopSalons { get; set; } = new();
}

/// <summary>
/// Monthly revenue item.
/// </summary>
public class MonthlyRevenueItem
{
    public string Month { get; set; } = string.Empty;
    public string MonthLabel { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Appointments { get; set; }
}

/// <summary>
/// City-based revenue breakdown.
/// </summary>
public class CityRevenueItem
{
    public string City { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int SalonCount { get; set; }
    public int AppointmentCount { get; set; }
    public string Color { get; set; } = string.Empty;
}

