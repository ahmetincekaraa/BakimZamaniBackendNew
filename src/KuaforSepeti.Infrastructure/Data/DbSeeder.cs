namespace KuaforSepeti.Infrastructure.Data;

using KuaforSepeti.Domain.Entities;
using KuaforSepeti.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

public class DbSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(AppDbContext context, ILogger<DbSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Seed super admin
            await SeedProtectedAdmin("admin@kuaforsepeti.com", "System Administrator");

            // Seed protected admins
            await SeedProtectedAdmin("ahmet@kuaforsepeti.com", "Ahmet Yönetici");
            await SeedProtectedAdmin("alper@kuaforsepeti.com", "Alper Yönetici");

            _logger.LogInformation("All admin users seeded/verified successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedProtectedAdmin(string email, string fullName)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existingUser != null)
        {
            if (existingUser.Role != UserRole.Admin)
            {
                existingUser.Role = UserRole.Admin;
                existingUser.IsActive = true;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Updated role for {email} to Admin.");
            }
        }
        else
        {
            var adminUser = new User
            {
                Email = email,
                FullName = fullName,
                PhoneNumber = "+905555555555",
                Gender = Gender.Unisex,
                PasswordHash = BCrypt.HashPassword("Admin123!"),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            await _context.Users.AddAsync(adminUser);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Seeded admin {email} successfully.");
        }
    }
}
