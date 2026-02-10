namespace KuaforSepeti.Application.Services.Interfaces;

using KuaforSepeti.Application.DTOs.Common;
using KuaforSepeti.Application.DTOs.Salon;

/// <summary>
/// Vector search service interface for semantic search.
/// </summary>
public interface IVectorSearchService
{
    /// <summary>
    /// Search salons by service name using semantic similarity.
    /// </summary>
    /// <param name="query">Search query (e.g., "protez tırnak", "saç boyama")</param>
    /// <param name="city">Optional city filter</param>
    /// <param name="limit">Maximum number of results</param>
    Task<ApiResponse<List<SalonListResponse>>> SearchSalonsByServiceAsync(string query, string? city = null, int limit = 20);

    /// <summary>
    /// Generate embedding for a text using OpenAI or local model.
    /// </summary>
    Task<float[]?> GenerateEmbeddingAsync(string text);

    /// <summary>
    /// Update embeddings for all services (batch operation).
    /// </summary>
    Task<int> UpdateAllServiceEmbeddingsAsync();
}
