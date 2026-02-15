namespace BakimZamani.Application.Services.Interfaces;

using BakimZamani.Application.DTOs.Auth;
using BakimZamani.Application.DTOs.Common;

/// <summary>
/// Authentication service interface.
/// </summary>
public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<ApiResponse> ChangePasswordAsync(string userId, ChangePasswordRequest request);
    Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequest request);
    Task<ApiResponse<UserResponse>> GetProfileAsync(string userId);
    Task<ApiResponse<UserResponse>> UpdateProfileAsync(string userId, UpdateProfileRequest request);
    Task<ApiResponse> UpdateFcmTokenAsync(string userId, UpdateFcmTokenRequest request);
    Task<ApiResponse> LogoutAsync(string userId);
}

