namespace BakimZamani.Infrastructure.Services;

using BakimZamani.Application.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Local file upload service implementation.
/// </summary>
public class LocalFileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _uploadPath = "uploads";

    public LocalFileUploadService(
        IWebHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor)
    {
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string folder)
    {
        if (fileStream == null || fileStream.Length == 0)
            throw new ArgumentException("File is empty");

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            throw new ArgumentException("Invalid file type. Allowed: jpg, jpeg, png, gif, webp");

        // Validate file size (max 5MB)
        if (fileStream.Length > 5 * 1024 * 1024)
            throw new ArgumentException("File size cannot exceed 5MB");

        // Create upload directory if it doesn't exist
        var uploadDir = Path.Combine(_environment.WebRootPath, _uploadPath, folder);
        if (!Directory.Exists(uploadDir))
            Directory.CreateDirectory(uploadDir);

        // Generate unique filename
        var newFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadDir, newFileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream);
        }

        // Return relative path
        return $"/{_uploadPath}/{folder}/{newFileName}";
    }

    public Task DeleteImageAsync(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
            return Task.CompletedTask;

        var relativePath = imageUrl.TrimStart('/');
        var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public string GetImageUrl(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return string.Empty;

        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null)
            return relativePath;

        return $"{request.Scheme}://{request.Host}{relativePath}";
    }
}

