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
    private readonly IEmailService _emailService;

    public AuthService(
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IConfiguration configuration,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _configuration = configuration;
        _emailService = emailService;
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
            return ApiResponse.Ok("Doğrulama kodu e-posta adresinize gönderildi.");
        }

        // Generate 6-digit verification code
        var code = new Random().Next(100000, 999999).ToString();
        user.PasswordResetCode = code;
        user.PasswordResetCodeExpiry = DateTime.UtcNow.AddMinutes(10);
        await _unitOfWork.SaveChangesAsync();

        // Send email with verification code
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #F3F4F6; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #FE725D, #E57999); padding: 32px; text-align: center; color: white; }}
        .header h1 {{ margin: 0; font-size: 24px; }}
        .content {{ padding: 32px; text-align: center; }}
        .code {{ font-size: 36px; font-weight: bold; letter-spacing: 8px; color: #333; background: #F9FAFB; padding: 16px 32px; border-radius: 12px; display: inline-block; margin: 24px 0; }}
        .footer {{ text-align: center; padding: 16px 32px; color: #9CA3AF; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Bakım Zamanı</h1>
        </div>
        <div class='content'>
            <h2 style='color: #1F2937; margin-bottom: 8px;'>Şifre Sıfırlama</h2>
            <p style='color: #6B7280;'>Şifrenizi sıfırlamak için aşağıdaki doğrulama kodunu kullanın:</p>
            <div class='code'>{code}</div>
            <p style='color: #9CA3AF; font-size: 14px;'>Bu kod 10 dakika içinde geçerliliğini yitirecektir.</p>
        </div>
        <div class='footer'>
            Bu e-posta Bakım Zamanı sistemi tarafından otomatik gönderilmiştir.
        </div>
    </div>
</body>
</html>";

        try
        {
            await _emailService.SendEmailRequiredAsync(user.Email, "Şifre Sıfırlama - Doğrulama Kodu", htmlBody);
        }
        catch (Exception)
        {
            // Clear the code since email failed
            user.PasswordResetCode = null;
            user.PasswordResetCodeExpiry = null;
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse.Fail("E-posta gönderilemedi. Lütfen daha sonra tekrar deneyin.");
        }

        return ApiResponse.Ok("Doğrulama kodu e-posta adresinize gönderildi.");
    }

    public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userRepository.Query()
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return ApiResponse.Fail("Geçersiz istek.");
        }

        // Validate reset code
        if (string.IsNullOrEmpty(user.PasswordResetCode) ||
            user.PasswordResetCode != request.Token ||
            user.PasswordResetCodeExpiry == null ||
            user.PasswordResetCodeExpiry < DateTime.UtcNow)
        {
            return ApiResponse.Fail("Doğrulama kodu geçersiz veya süresi dolmuş.");
        }

        // Update password and clear reset code
        user.PasswordHash = HashPassword(request.NewPassword);
        user.PasswordResetCode = null;
        user.PasswordResetCodeExpiry = null;
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Şifreniz başarıyla değiştirildi.");
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

