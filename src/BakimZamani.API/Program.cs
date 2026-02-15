using BakimZamani.API.Extensions;
using BakimZamani.API.Hubs;
using BakimZamani.API.Middlewares;
using BakimZamani.Application;
using BakimZamani.Infrastructure;
using BakimZamani.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// Application Layer
builder.Services.AddApplication();

// Infrastructure Layer (PostgreSQL, MongoDB, Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// SignalR
builder.Services.AddSignalR();
builder.Services.AddSingleton<INotificationHubService, NotificationHubService>();

// Controllers
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient(); // For vector search embeddings

// CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Allow any origin
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BakÄ±m ZamanÄ± API",
        Version = "v1",
        Description = "BakÄ±m ZamanÄ± Randevu Sistemi API"
    });

    // JWT Bearer Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline

// Exception handling middleware (must be first)
app.UseExceptionMiddleware();

// Request logging middleware
app.UseRequestLogging();

// Swagger (development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BakÄ±m ZamanÄ± API v1");
        c.RoutePrefix = "swagger";
    });
}



// CORS
app.UseCors("AllowFrontend");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map SignalR hubs
app.MapHub<NotificationHub>("/hubs/notifications");

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Fix admin role if needed
    var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@BakimZamani.com");
    if (adminUser != null && adminUser.Role != BakimZamani.Domain.Enums.UserRole.Admin)
    {
        adminUser.Role = BakimZamani.Domain.Enums.UserRole.Admin;
        adminUser.IsActive = true;
        await context.SaveChangesAsync();
        Console.WriteLine("âœ… Admin role fixed!");
    }
    
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAsync();
}

app.Run();

