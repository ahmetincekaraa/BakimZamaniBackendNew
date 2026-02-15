namespace BakimZamani.Infrastructure.Services;

using AutoMapper;
using BakimZamani.Application.DTOs.Common;
using BakimZamani.Application.DTOs.Salon;
using BakimZamani.Application.Services.Interfaces;
using BakimZamani.Domain.Entities;
using BakimZamani.Domain.Interfaces;
using BakimZamani.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using System.Net.Http.Json;
using System.Text.Json;

/// <summary>
/// Vector search service implementation using pgvector.
/// Supports both OpenAI embeddings and keyword-based search.
/// </summary>
public class VectorSearchService : IVectorSearchService
{
    private readonly AppDbContext _context;
    private readonly IRepository<Service> _serviceRepository;
    private readonly IRepository<Salon> _salonRepository;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    // Pre-defined Turkish beauty/salon keywords with simple hash-based vectors
    private static readonly Dictionary<string, float[]> _keywordVectors = new()
    {
        // Hair services
        ["saÃ§"] = GenerateSimpleVector(1),
        ["kesim"] = GenerateSimpleVector(2),
        ["boya"] = GenerateSimpleVector(3),
        ["rÃ¶fle"] = GenerateSimpleVector(4),
        ["fÃ¶n"] = GenerateSimpleVector(5),
        ["perma"] = GenerateSimpleVector(6),
        ["keratin"] = GenerateSimpleVector(7),
        ["bakÄ±m"] = GenerateSimpleVector(8),
        
        // Nail services
        ["tÄ±rnak"] = GenerateSimpleVector(10),
        ["manikÃ¼r"] = GenerateSimpleVector(11),
        ["pedikÃ¼r"] = GenerateSimpleVector(12),
        ["protez"] = GenerateSimpleVector(13),
        ["jel"] = GenerateSimpleVector(14),
        ["kalÄ±cÄ±"] = GenerateSimpleVector(15),
        ["oje"] = GenerateSimpleVector(16),
        
        // Face services
        ["cilt"] = GenerateSimpleVector(20),
        ["yÃ¼z"] = GenerateSimpleVector(21),
        ["maske"] = GenerateSimpleVector(22),
        ["peeling"] = GenerateSimpleVector(23),
        
        // Makeup
        ["makyaj"] = GenerateSimpleVector(30),
        ["gelin"] = GenerateSimpleVector(31),
        ["dÃ¼ÄŸÃ¼n"] = GenerateSimpleVector(32),
        
        // Body
        ["epilasyon"] = GenerateSimpleVector(40),
        ["lazer"] = GenerateSimpleVector(41),
        ["aÄŸda"] = GenerateSimpleVector(42),
        ["masaj"] = GenerateSimpleVector(43),
        
        // Beard
        ["sakal"] = GenerateSimpleVector(50),
        ["tÄ±raÅŸ"] = GenerateSimpleVector(51),
        ["bÄ±yÄ±k"] = GenerateSimpleVector(52),
    };

    public VectorSearchService(
        AppDbContext context,
        IRepository<Service> serviceRepository,
        IRepository<Salon> salonRepository,
        IMapper mapper,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _serviceRepository = serviceRepository;
        _salonRepository = salonRepository;
        _mapper = mapper;
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<ApiResponse<List<SalonListResponse>>> SearchSalonsByServiceAsync(string query, string? city = null, int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return ApiResponse<List<SalonListResponse>>.Fail("Arama sorgusu boÅŸ olamaz.");
        }

        // Keyword-based search (works without OpenAI)
        var searchLower = query.ToLower().Trim();
        var searchWords = searchLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        var salonQuery = _salonRepository.Query()
            .Where(s => s.IsActive && s.IsVerified)
            .Where(s => s.Services.Any(svc => 
                svc.IsActive && (
                    // Exact match
                    svc.Name.ToLower().Contains(searchLower) ||
                    // Word match
                    searchWords.Any(word => svc.Name.ToLower().Contains(word)) ||
                    // Description match
                    (svc.Description != null && (
                        svc.Description.ToLower().Contains(searchLower) ||
                        searchWords.Any(word => svc.Description.ToLower().Contains(word))
                    )) ||
                    // Keywords match
                    (svc.SearchKeywords != null && (
                        svc.SearchKeywords.ToLower().Contains(searchLower) ||
                        searchWords.Any(word => svc.SearchKeywords.ToLower().Contains(word))
                    ))
                )
            ));

        if (!string.IsNullOrEmpty(city))
        {
            salonQuery = salonQuery.Where(s => s.City.ToLower() == city.ToLower());
        }

        var salons = await salonQuery
            .OrderByDescending(s => s.Rating)
            .ThenByDescending(s => s.ReviewCount)
            .Take(limit)
            .ToListAsync();

        var response = _mapper.Map<List<SalonListResponse>>(salons);
        return ApiResponse<List<SalonListResponse>>.Ok(response, $"{response.Count} salon bulundu.");
    }

    public async Task<float[]?> GenerateEmbeddingAsync(string text)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        
        // If no OpenAI key, use local keyword-based embedding
        if (string.IsNullOrEmpty(apiKey))
        {
            return GenerateLocalEmbedding(text);
        }

        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var requestBody = new
            {
                input = text,
                model = "text-embedding-3-small"
            };

            var response = await _httpClient.PostAsJsonAsync(
                "https://api.openai.com/v1/embeddings",
                requestBody);

            if (!response.IsSuccessStatusCode)
            {
                // Fallback to local embedding
                return GenerateLocalEmbedding(text);
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            var embeddingArray = doc.RootElement
                .GetProperty("data")[0]
                .GetProperty("embedding")
                .EnumerateArray()
                .Select(e => e.GetSingle())
                .ToArray();

            return embeddingArray;
        }
        catch
        {
            return GenerateLocalEmbedding(text);
        }
    }

    public async Task<int> UpdateAllServiceEmbeddingsAsync()
    {
        // This method now works without OpenAI using local embeddings
        var services = await _serviceRepository.Query()
            .Where(s => s.IsActive)
            .ToListAsync();

        var count = 0;

        foreach (var service in services)
        {
            var textToEmbed = $"{service.Name} {service.Description ?? ""} {service.SearchKeywords ?? ""}".Trim();
            var embedding = GenerateLocalEmbedding(textToEmbed);
            
            if (embedding != null)
            {
                // Update via raw SQL since Embedding is ignored in EF
                var vectorString = $"[{string.Join(",", embedding)}]";
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE services SET \"Embedding\" = @p0::vector WHERE \"Id\" = @p1",
                    vectorString, service.Id);
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Generate a simple keyword-based local embedding without external API.
    /// </summary>
    private static float[] GenerateLocalEmbedding(string text)
    {
        var words = text.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var embedding = new float[128]; // Small embedding size for local use

        foreach (var word in words)
        {
            if (_keywordVectors.TryGetValue(word, out var vector))
            {
                // Add keyword vector to embedding
                for (int i = 0; i < Math.Min(vector.Length, embedding.Length); i++)
                {
                    embedding[i] += vector[i];
                }
            }
            else
            {
                // Hash-based fallback for unknown words
                var hash = word.GetHashCode();
                var index = Math.Abs(hash % embedding.Length);
                embedding[index] += 1.0f;
            }
        }

        // Normalize
        var magnitude = (float)Math.Sqrt(embedding.Sum(x => x * x));
        if (magnitude > 0)
        {
            for (int i = 0; i < embedding.Length; i++)
            {
                embedding[i] /= magnitude;
            }
        }

        return embedding;
    }

    /// <summary>
    /// Generate a simple deterministic vector for a keyword.
    /// </summary>
    private static float[] GenerateSimpleVector(int seed)
    {
        var random = new Random(seed * 12345);
        var vector = new float[128];
        for (int i = 0; i < vector.Length; i++)
        {
            vector[i] = (float)(random.NextDouble() * 2 - 1);
        }
        return vector;
    }
}

