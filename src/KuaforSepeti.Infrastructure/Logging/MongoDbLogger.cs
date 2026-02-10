namespace KuaforSepeti.Infrastructure.Logging;

using Microsoft.Extensions.Options;
using MongoDB.Driver;

/// <summary>
/// MongoDB logging service interface.
/// </summary>
public interface IMongoDbLogger
{
    Task LogAsync(LogEntry entry);
    Task LogInfoAsync(string message, string? source = null, Dictionary<string, object>? properties = null);
    Task LogWarningAsync(string message, string? source = null, Dictionary<string, object>? properties = null);
    Task LogErrorAsync(string message, Exception? exception = null, string? source = null, Dictionary<string, object>? properties = null);
    Task<IEnumerable<LogEntry>> GetLogsAsync(DateTime? from = null, DateTime? to = null, string? level = null, int limit = 100);
}

/// <summary>
/// MongoDB logging service implementation.
/// </summary>
public class MongoDbLogger : IMongoDbLogger
{
    private readonly IMongoCollection<LogEntry> _collection;

    public MongoDbLogger(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<LogEntry>(settings.Value.CollectionName);

        // Create indexes
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var indexKeysDefinition = Builders<LogEntry>.IndexKeys
            .Descending(x => x.Timestamp)
            .Ascending(x => x.Level);

        _collection.Indexes.CreateOne(new CreateIndexModel<LogEntry>(indexKeysDefinition));
    }

    public async Task LogAsync(LogEntry entry)
    {
        await _collection.InsertOneAsync(entry);
    }

    public async Task LogInfoAsync(string message, string? source = null, Dictionary<string, object>? properties = null)
    {
        var entry = new LogEntry
        {
            Level = "Information",
            Message = message,
            Source = source,
            Properties = properties
        };
        await LogAsync(entry);
    }

    public async Task LogWarningAsync(string message, string? source = null, Dictionary<string, object>? properties = null)
    {
        var entry = new LogEntry
        {
            Level = "Warning",
            Message = message,
            Source = source,
            Properties = properties
        };
        await LogAsync(entry);
    }

    public async Task LogErrorAsync(string message, Exception? exception = null, string? source = null, Dictionary<string, object>? properties = null)
    {
        var entry = new LogEntry
        {
            Level = "Error",
            Message = message,
            Exception = exception?.Message,
            StackTrace = exception?.StackTrace,
            Source = source,
            Properties = properties
        };
        await LogAsync(entry);
    }

    public async Task<IEnumerable<LogEntry>> GetLogsAsync(DateTime? from = null, DateTime? to = null, string? level = null, int limit = 100)
    {
        var builder = Builders<LogEntry>.Filter;
        var filter = builder.Empty;

        if (from.HasValue)
            filter &= builder.Gte(x => x.Timestamp, from.Value);

        if (to.HasValue)
            filter &= builder.Lte(x => x.Timestamp, to.Value);

        if (!string.IsNullOrEmpty(level))
            filter &= builder.Eq(x => x.Level, level);

        return await _collection
            .Find(filter)
            .SortByDescending(x => x.Timestamp)
            .Limit(limit)
            .ToListAsync();
    }
}
