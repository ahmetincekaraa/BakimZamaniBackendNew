namespace KuaforSepeti.Infrastructure.Services;

using AutoMapper;
using KuaforSepeti.Application.Services.Interfaces;
using KuaforSepeti.Domain.Entities;
using KuaforSepeti.Domain.Enums;
using KuaforSepeti.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Notification service implementation.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public NotificationService(
        IRepository<Notification> notificationRepository,
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task SendNotificationAsync(
        string userId, 
        string title, 
        string message, 
        NotificationType type, 
        string? relatedEntityType = null, 
        string? relatedEntityId = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId
        };

        await _notificationRepository.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();

        // TODO: Integrate with SignalR for real-time push via NotificationHub
    }

    public async Task SendPushNotificationAsync(
        string userId, 
        string title, 
        string message, 
        Dictionary<string, string>? data = null)
    {
        // Get user's FCM token
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.FcmToken))
        {
            return;
        }

        // TODO: Integrate with Firebase Cloud Messaging for push notifications
        // For now, just create a notification record
        await SendNotificationAsync(userId, title, message, NotificationType.System);
    }

    public async Task SendBulkNotificationAsync(
        List<string> userIds, 
        string title, 
        string message, 
        NotificationType type)
    {
        foreach (var userId in userIds)
        {
            await SendNotificationAsync(userId, title, message, type);
        }
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, int limit = 50)
    {
        var notifications = await _notificationRepository.Query()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync();

        return notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type,
            IsRead = n.IsRead,
            RelatedEntityType = n.RelatedEntityType,
            RelatedEntityId = n.RelatedEntityId,
            CreatedAt = n.CreatedAt
        }).ToList();
    }

    public async Task MarkAsReadAsync(string notificationId, string userId)
    {
        var notification = await _notificationRepository.Query()
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification != null)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        var notifications = await _notificationRepository.Query()
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _notificationRepository.Query()
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }
}
