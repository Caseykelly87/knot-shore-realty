using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace KnotShoreRealty.Data.Seed;

public class SeedDataLoader
{
    private readonly string _seedDataPath;
    private readonly ILogger<SeedDataLoader> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public SeedDataLoader(string seedDataPath, ILogger<SeedDataLoader> logger)
    {
        _seedDataPath = seedDataPath;
        _logger = logger;
    }

    public async Task<IEnumerable<NeighborhoodSeedDto>> LoadNeighborhoodsAsync()
    {
        return await LoadFileAsync<NeighborhoodSeedDto>("neighborhoods.json");
    }

    public async Task<IEnumerable<AgentSeedDto>> LoadAgentsAsync()
    {
        return await LoadFileAsync<AgentSeedDto>("agents.json");
    }

    public async Task<IEnumerable<ListingSeedDto>> LoadListingsAsync()
    {
        return await LoadFileAsync<ListingSeedDto>("listings.json");
    }

    private async Task<IEnumerable<T>> LoadFileAsync<T>(string fileName)
    {
        var path = Path.Combine(_seedDataPath, fileName);

        if (!File.Exists(path))
        {
            _logger.LogWarning("Seed file not found, skipping: {Path}", path);
            return Enumerable.Empty<T>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(path);
            var result = JsonSerializer.Deserialize<List<T>>(json, JsonOptions);
            _logger.LogInformation("Loaded {Count} records from {File}", result?.Count ?? 0, fileName);
            return result ?? Enumerable.Empty<T>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse seed file: {Path}", path);
            return Enumerable.Empty<T>();
        }
    }

    // DTOs mirror the JSON shape exactly. They are internal to the seed infrastructure
    // and are not exposed through any public interface.

    public class NeighborhoodSeedDto
    {
        public string Slug { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ParentSlug { get; set; }
    }

    public class AgentSeedDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string? Image { get; set; }
    }

    public class ListingSeedDto
    {
        public string Id { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Neighborhood { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Bedrooms { get; set; }
        public decimal Bathrooms { get; set; }
        public int Sqft { get; set; }
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("main_image")]
        public string MainImage { get; set; } = string.Empty;

        public List<string> Images { get; set; } = new();
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("agent_id")]
        public string AgentId { get; set; } = string.Empty;
    }
}
