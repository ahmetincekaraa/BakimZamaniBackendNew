namespace KuaforSepeti.Infrastructure.Logging;

/// <summary>
/// Log entry model for MongoDB.
/// </summary>
public class LogEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string? StackTrace { get; set; }
    public string? Source { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public string? UserId { get; set; }
    public string? CorrelationId { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
}
