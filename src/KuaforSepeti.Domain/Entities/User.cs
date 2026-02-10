namespace KuaforSepeti.Domain.Entities;

using KuaforSepeti.Domain.Enums;

/// <summary>
/// User entity for customers, salon owners, staff, and admins.
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// User's email address (unique).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// User's full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// User's phone number.
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// User's role in the system.
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Customer;

    /// <summary>
    /// Profile image URL.
    /// </summary>
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// User's gender.
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// Email verification status.
    /// </summary>
    public bool IsEmailVerified { get; set; } = false;

    /// <summary>
    /// Account active status (true: active, false: suspended).
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Phone verification status.
    /// </summary>
    public bool IsPhoneVerified { get; set; } = false;

    /// <summary>
    /// Last login timestamp.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// FCM token for push notifications.
    /// </summary>
    public string? FcmToken { get; set; }

    /// <summary>
    /// Refresh token for JWT authentication.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Refresh token expiry date.
    /// </summary>
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Navigation properties
    /// <summary>
    /// Salons owned by this user.
    /// </summary>
    public virtual ICollection<Salon> OwnedSalons { get; set; } = new List<Salon>();

    /// <summary>
    /// Appointments made by this user (as customer).
    /// </summary>
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    /// <summary>
    /// Reviews written by this user.
    /// </summary>
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    /// <summary>
    /// Notifications for this user.
    /// </summary>
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
