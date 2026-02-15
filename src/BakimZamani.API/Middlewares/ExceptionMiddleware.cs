namespace BakimZamani.API.Middlewares;

using BakimZamani.Infrastructure.Logging;
using System.Net;
using System.Text.Json;

/// <summary>
/// Global exception handling middleware.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IMongoDbLogger _mongoLogger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IMongoDbLogger mongoLogger)
    {
        _next = next;
        _logger = logger;
        _mongoLogger = mongoLogger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            
            await _mongoLogger.LogErrorAsync(
                ex.Message,
                ex,
                "ExceptionMiddleware",
                new Dictionary<string, object>
                {
                    ["RequestPath"] = context.Request.Path.Value ?? "",
                    ["RequestMethod"] = context.Request.Method,
                    ["UserId"] = context.User?.FindFirst("sub")?.Value ?? "anonymous"
                }
            );

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ErrorResponse();

        switch (exception)
        {
            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = "Yetkisiz eriÅŸim.";
                break;
            case KeyNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = "KayÄ±t bulunamadÄ±.";
                break;
            case ArgumentException argEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = argEx.Message;
                break;
            case InvalidOperationException invEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = invEx.Message;
                break;
            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
#if DEBUG
                response.Message = exception.Message;
                response.Errors = new List<string> { exception.StackTrace ?? "" };
#else
                response.Message = "Bir hata oluÅŸtu. LÃ¼tfen daha sonra tekrar deneyiniz.";
#endif
                break;
        }

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

public class ErrorResponse
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }
}

/// <summary>
/// Extension method to register exception middleware.
/// </summary>
public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }
}

