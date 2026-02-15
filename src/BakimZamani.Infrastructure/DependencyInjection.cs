namespace BakimZamani.Infrastructure;

using BakimZamani.Application.Services.Interfaces;
using BakimZamani.Domain.Interfaces;
using BakimZamani.Infrastructure.Data;
using BakimZamani.Infrastructure.Logging;
using BakimZamani.Infrastructure.Repositories;
using BakimZamani.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pgvector.EntityFrameworkCore;

/// <summary>
/// Dependency injection extensions for Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // PostgreSQL with pgvector support
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQL"),
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(3);
                    npgsqlOptions.CommandTimeout(30);
                    npgsqlOptions.UseVector(); // Enable pgvector
                }));

        // MongoDB Logging
        services.Configure<MongoDbSettings>(options =>
        {
            options.ConnectionString = configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
            options.DatabaseName = configuration.GetSection("MongoSettings:DatabaseName").Value ?? "BakimZamani_logs";
            options.CollectionName = configuration.GetSection("MongoSettings:CollectionName").Value ?? "logs";
        });
        services.AddSingleton<IMongoDbLogger, MongoDbLogger>();

        // Seeder
        services.AddScoped<DbSeeder>();

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISalonService, SalonService>();
        services.AddScoped<IAppointmentService, AppointmentBookingService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFileUploadService, LocalFileUploadService>();
        services.AddScoped<IVectorSearchService, VectorSearchService>();
        services.AddScoped<IAdminService, AdminService>();

        // Email
        services.Configure<BakimZamani.Application.DTOs.Common.EmailSettings>(
            configuration.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}


