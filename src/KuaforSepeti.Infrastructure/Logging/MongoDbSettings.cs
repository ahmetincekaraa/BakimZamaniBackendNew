namespace KuaforSepeti.Infrastructure.Logging;

/// <summary>
/// MongoDB settings for logging.
/// </summary>
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "kuaforsepeti_logs";
    public string CollectionName { get; set; } = "logs";
}
