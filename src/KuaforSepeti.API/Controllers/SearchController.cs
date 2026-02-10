namespace KuaforSepeti.API.Controllers;

using KuaforSepeti.Application.DTOs.Salon;
using KuaforSepeti.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Search controller for semantic salon/service search.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IVectorSearchService _vectorSearchService;

    public SearchController(IVectorSearchService vectorSearchService)
    {
        _vectorSearchService = vectorSearchService;
    }

    /// <summary>
    /// Search salons by service name using semantic similarity.
    /// Example: GET /api/search?q=protez tırnak&city=Istanbul
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery(Name = "q")] string query,
        [FromQuery] string? city = null,
        [FromQuery] int limit = 20)
    {
        var result = await _vectorSearchService.SearchSalonsByServiceAsync(query, city, limit);
        return Ok(result);
    }

    /// <summary>
    /// Update embeddings for all services (admin only).
    /// </summary>
    [HttpPost("update-embeddings")]
    public async Task<IActionResult> UpdateEmbeddings()
    {
        var count = await _vectorSearchService.UpdateAllServiceEmbeddingsAsync();
        return Ok(new { message = $"{count} hizmet için embedding güncellendi." });
    }
}
