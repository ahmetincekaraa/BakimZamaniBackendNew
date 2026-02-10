namespace KuaforSepeti.Application.DTOs.Auth;

using KuaforSepeti.Domain.Enums;

/// <summary>
/// User registration request.
/// </summary>
public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Gender? Gender { get; set; }
}

/// <summary>
/// User login request.
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Token refresh request.
/// </summary>
public class RefreshTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Authentication response with tokens.
/// </summary>
public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserResponse User { get; set; } = null!;
}

/// <summary>
/// User response DTO.
/// </summary>
public class UserResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? ProfileImageUrl { get; set; }
    public Gender? Gender { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Password change request.
/// </summary>
public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Password reset request.
/// </summary>
public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Reset password with token.
/// </summary>
public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Update user profile request.
/// </summary>
public class UpdateProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Gender? Gender { get; set; }
    public string? ProfileImageUrl { get; set; }
}

/// <summary>
/// FCM token update request.
/// </summary>
public class UpdateFcmTokenRequest
{
    public string FcmToken { get; set; } = string.Empty;
}
