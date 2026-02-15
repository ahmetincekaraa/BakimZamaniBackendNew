namespace BakimZamani.Application.Services.Interfaces;

/// <summary>
/// File upload service interface for handling image uploads.
/// </summary>
public interface IFileUploadService
{
    /// <summary>
    /// Upload a single image file.
    /// </summary>
    /// <param name="fileStream">File stream</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="folder">Target folder</param>
    Task<string> UploadImageAsync(Stream fileStream, string fileName, string folder);

    /// <summary>
    /// Delete an image by its URL.
    /// </summary>
    Task DeleteImageAsync(string imageUrl);

    /// <summary>
    /// Get the full URL for an image path.
    /// </summary>
    string GetImageUrl(string relativePath);
}

