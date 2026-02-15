namespace BakimZamani.Infrastructure.Services;

using AutoMapper;
using BakimZamani.Application.DTOs.Auth;
using BakimZamani.Application.DTOs.Common;
using BakimZamani.Application.Services.Interfaces;
using BakimZamani.Domain.Entities;
using BakimZamani.Domain.Enums;
using BakimZamani.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

/// <summary>
/// Authentication service implementation.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AuthService(
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        // Check if email exists
        var existingUser = await _userRepository.Query()
            .FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            return ApiResponse<AuthResponse>.Fail("Bu e-posta adresi zaten kayÄ±tlÄ±.");
        }

        // Check if phone exists
        var existingPhone = await _userRepository.Query()
            .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);
        if (existingPhone != null)
        {
            return ApiResponse<AuthResponse>.Fail("Bu telefon numarasÄ± zaten kayÄ±tlÄ±.");
        }

        // Create user
        var user = new User
        {
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Gender = request.Gender,
            PasswordHash = HashPassword(request.Password),
            Role = UserRole.Customer
        };

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Generate tokens
        var tokens = GenerateTokens(user);

        // Update refresh token
        user.RefreshToken = tokens.RefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(GetRefreshTokenExpirationDays());
        await _unitOfWork.SaveChangesAsync();

        var response = new AuthResponse
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            ExpiresAt = tokens.ExpiresAt,
            User = _mapper.Map<UserResponse>(user)
        };

        return ApiResponse<AuthResponse>.Ok(response, "KayÄ±t baÅŸarÄ±lÄ±.");
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.Query()
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return ApiResponse<AuthResponse>.Fail("E-posta veya ÅŸifre hatalÄ±.");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;

        // Generate tokens
        var tokens = GenerateTokens(user);

        // Update refresh token
        user.RefreshToken = tokens.RefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(GetRefreshTokenExpirationDays());
        await _unitOfWork.SaveChangesAsync();

        var response = new AuthResponse
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            ExpiresAt = tokens.ExpiresAt,
            User = _mapper.Map<UserResponse>(user)
        };

        return ApiResponse<AuthResponse>.Ok(response, "GiriÅŸ baÅŸarÄ±lÄ±.");
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            return ApiResponse<AuthResponse>.Fail("GeÃ§ersiz token.");
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return ApiResponse<AuthResponse>.Fail("GeÃ§ersiz token.");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || 
            user.RefreshToken != request.RefreshToken || 
            user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return ApiResponse<AuthResponse>.Fail("GeÃ§ersiz veya sÃ¼resi dolmuÅŸ refresh token.");
        }

        // Generate new tokens
        var tokens = GenerateTokens(user);

        // Update refresh token
        user.RefreshToken = tokens.RefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(GetRefreshTokenExpirationDays());
        await _unitOfWork.SaveChangesAsync();

        var response = new AuthResponse
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            ExpiresAt = tokens.ExpiresAt,
            User = _mapper.Map<UserResponse>(user)
        };

        return ApiResponse<AuthResponse>.Ok(response);
    }

    public async Task<ApiResponse> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse.Fail("KullanÄ±cÄ± bulunamadÄ±.");
        }

        if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            return ApiResponse.Fail("Mevcut ÅŸifre hatalÄ±.");
        }

        user.PasswordHash = HashPassword(request.NewPassword);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Åžifre baÅŸarÄ±yla deÄŸiÅŸtirildi.");
    }

    public async Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userRepository.Query()
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        // Don't reveal if user exists
        if (user == null)
        {
            return ApiResponse.Ok("Åžifre sÄ±fÄ±rlama baÄŸlantÄ±sÄ± gÃ¶nderildi.");
        }

        // TODO: Generate reset token and send email
        return ApiResponse.Ok("Åžifre sÄ±fÄ±rlama baÄŸlantÄ±sÄ± gÃ¶nderildi.");
    }

    public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        // TODO: Implement password reset with token
        return ApiResponse.Ok("Åžifre baÅŸarÄ±yla sÄ±fÄ±rlandÄ±.");
    }

    public async Task<ApiResponse<UserResponse>> GetProfileAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<UserResponse>.Fail("KullanÄ±cÄ± bulunamadÄ±.");
        }

        var response = _mapper.Map<UserResponse>(user);
        return ApiResponse<UserResponse>.Ok(response);
    }

    public async Task<ApiResponse<UserResponse>> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<UserResponse>.Fail("KullanÄ±cÄ± bulunamadÄ±.");
        }

        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        user.Gender = request.Gender;
        user.ProfileImageUrl = request.ProfileImageUrl;

        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<UserResponse>(user);
        return ApiResponse<UserResponse>.Ok(response, "Profil gÃ¼ncellendi.");
    }

    public async Task<ApiResponse> UpdateFcmTokenAsync(string userId, UpdateFcmTokenRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse.Fail("KullanÄ±cÄ± bulunamadÄ±.");
        }

        user.FcmToken = request.FcmToken;
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok();
    }

    public async Task<ApiResponse> LogoutAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            user.FcmToken = null;
            await _unitOfWork.SaveChangesAsync();
        }

        return ApiResponse.Ok("Ã‡Ä±kÄ±ÅŸ yapÄ±ldÄ±.");
    }

    #region Private Methods

    private string HashPassword(string password)
    {
        return BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Verify(password, hash);
    }

    private (string AccessToken, string RefreshToken, DateTime ExpiresAt) GenerateTokens(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT Key not configured")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var expiresAt = DateTime.UtcNow.AddMinutes(GetAccessTokenExpirationMinutes());

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = GenerateRefreshToken();

        return (accessToken, refreshToken, expiresAt);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT Key not configured"))),
            ValidateIssuer = true,
            ValidIssuer = _configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = _configuration["JwtSettings:Audience"],
            ValidateLifetime = false // Important: don't validate lifetime for expired tokens
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            return principal;
        }
        catch
        {
            return null;
        }
    }

    private int GetAccessTokenExpirationMinutes() =>
        int.TryParse(_configuration["JwtSettings:ExpirationMinutes"], out var minutes) ? minutes : 60;

    private int GetRefreshTokenExpirationDays() =>
        int.TryParse(_configuration["JwtSettings:RefreshTokenExpirationDays"], out var days) ? days : 7;

    #endregion
}

