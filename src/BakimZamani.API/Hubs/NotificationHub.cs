namespace BakimZamani.API.Hubs;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

/// <summary>
/// SignalR Hub for real-time notifications.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("User {UserId} connected to NotificationHub", userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("sub")?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("User {UserId} disconnected from NotificationHub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join salon group (for salon owners to receive salon-related notifications).
    /// </summary>
    public async Task JoinSalonGroup(string salonId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"salon_{salonId}");
        _logger.LogInformation("Connection {ConnectionId} joined salon group {SalonId}", Context.ConnectionId, salonId);
    }

    /// <summary>
    /// Leave salon group.
    /// </summary>
    public async Task LeaveSalonGroup(string salonId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"salon_{salonId}");
        _logger.LogInformation("Connection {ConnectionId} left salon group {SalonId}", Context.ConnectionId, salonId);
    }
}

/// <summary>
/// Notification Hub service for sending notifications from backend.
/// </summary>
public interface INotificationHubService
{
    Task SendToUserAsync(string userId, string method, object data);
    Task SendToSalonAsync(string salonId, string method, object data);
    Task SendNewAppointmentNotificationAsync(string salonId, object appointmentData);
    Task SendAppointmentConfirmedNotificationAsync(string customerId, object appointmentData);
    Task SendAppointmentCancelledNotificationAsync(string userId, object appointmentData);
}

public class NotificationHubService : INotificationHubService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationHubService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToUserAsync(string userId, string method, object data)
    {
        await _hubContext.Clients.Group($"user_{userId}").SendAsync(method, data);
    }

    public async Task SendToSalonAsync(string salonId, string method, object data)
    {
        await _hubContext.Clients.Group($"salon_{salonId}").SendAsync(method, data);
    }

    public async Task SendNewAppointmentNotificationAsync(string salonId, object appointmentData)
    {
        await SendToSalonAsync(salonId, "NewAppointment", appointmentData);
    }

    public async Task SendAppointmentConfirmedNotificationAsync(string customerId, object appointmentData)
    {
        await SendToUserAsync(customerId, "AppointmentConfirmed", appointmentData);
    }

    public async Task SendAppointmentCancelledNotificationAsync(string userId, object appointmentData)
    {
        await SendToUserAsync(userId, "AppointmentCancelled", appointmentData);
    }
}

