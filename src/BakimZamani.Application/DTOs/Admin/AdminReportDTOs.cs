namespace BakimZamani.Application.DTOs.Admin;

/// <summary>
/// Monthly trend data for dashboard charts.
/// </summary>
public class MonthlyTrendData
{
    public string Month { get; set; } = string.Empty; // "2026-01"
    public string MonthLabel { get; set; } = string.Empty; // "Ocak 2026"
    public int NewSalons { get; set; }
    public int NewUsers { get; set; }
    public int Appointments { get; set; }
    public decimal Revenue { get; set; }
    public int Reviews { get; set; }
}

