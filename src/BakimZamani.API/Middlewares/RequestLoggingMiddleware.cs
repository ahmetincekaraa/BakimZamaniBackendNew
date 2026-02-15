namespace BakimZamani.API.Middlewares;

using BakimZamani.Infrastructure.Logging;
using System.Diagnostics;

/// <summary>
/// Request logging middleware with MongoDB integration.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IMongoDbLogger mongoLogger)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = Guid.NewGuid().ToString();
        
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var logEntry = new LogEntry
            {
                Level = context.Response.StatusCode >= 400 ? "Warning" : "Information",
                Message = $"{context.Request.Method} {context.Request.Path} - {context.Response.StatusCode}",
                Source = "RequestLoggingMiddleware",
                RequestPath = context.Request.Path,
                RequestMethod = context.Request.Method,
                UserId = context.User?.FindFirst("sub")?.Value,
                CorrelationId = correlationId,
                Properties = new Dictionary<string, object>
                {
                    ["StatusCode"] = context.Response.StatusCode,
                    ["ElapsedMs"] = stopwatch.ElapsedMilliseconds,
                    ["QueryString"] = context.Request.QueryString.Value ?? ""
                }
            };

            // Only log to MongoDB for important requests (not health checks, etc.)
            if (!context.Request.Path.StartsWithSegments("/health") && 
                !context.Request.Path.StartsWithSegments("/swagger"))
            {
                await mongoLogger.LogAsync(logEntry);
            }

            _logger.LogInformation(
                "{Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds
            );
        }
    }
}

/// <summary>
/// Extension method to register request logging middleware.
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}

