namespace BakimZamani.Application.DTOs.Admin;

/// <summary>
/// Detailed report data for a given date range.
/// </summary>
public class DetailedReportData
{
    // Summary KPIs
    public int TotalAppointments { get; set; }
    public decimal TotalRevenue { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public double CancellationRate { get; set; }
    public decimal AveragePrice { get; set; }
    public int NewSalons { get; set; }
    public int NewUsers { get; set; }
    public int TotalReviews { get; set; }
    public double AverageRating { get; set; }

    // Status breakdown
    public List<StatusBreakdownItem> StatusBreakdown { get; set; } = new();

    // Daily revenue data
    public List<DailyRevenueItem> DailyRevenue { get; set; } = new();

    // Top salons
    public List<TopSalonItem> TopSalons { get; set; } = new();
}

public class StatusBreakdownItem
{
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public int Count { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class DailyRevenueItem
{
    public string Date { get; set; } = string.Empty;
    public string DateLabel { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Appointments { get; set; }
}

public class TopSalonItem
{
    public string SalonId { get; set; } = string.Empty;
    public string SalonName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int AppointmentCount { get; set; }
    public decimal Revenue { get; set; }
    public double AverageRating { get; set; }
}

