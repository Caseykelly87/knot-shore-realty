using FluentAssertions;
using KnotShoreRealty.Data.Seed;
using Microsoft.Extensions.Logging.Abstractions;

namespace KnotShoreRealty.Web.Tests.Seed;

public class SeedDataLoaderTests : IDisposable
{
    private readonly string _tempDir;

    public SeedDataLoaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    private SeedDataLoader CreateLoader() =>
        new(_tempDir, NullLogger<SeedDataLoader>.Instance);

    [Fact]
    public async Task LoadNeighborhoods_ParsesValidJson()
    {
        var json = """
            [
              { "slug": "soulard", "name": "Soulard", "description": "Historic district", "parentSlug": null },
              { "slug": "benton-park", "name": "Benton Park", "description": "Brewery row", "parentSlug": "soulard" }
            ]
            """;
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "neighborhoods.json"), json);

        var results = (await CreateLoader().LoadNeighborhoodsAsync()).ToList();

        results.Should().HaveCount(2);
        results[0].Slug.Should().Be("soulard");
        results[1].ParentSlug.Should().Be("soulard");
    }

    [Fact]
    public async Task LoadNeighborhoods_ReturnsEmpty_WhenFileMissing()
    {
        var results = await CreateLoader().LoadNeighborhoodsAsync();
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadNeighborhoods_ReturnsEmpty_WhenJsonMalformed()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "neighborhoods.json"), "{ not valid json [");

        var results = await CreateLoader().LoadNeighborhoodsAsync();
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAgents_ParsesValidJson()
    {
        var json = """
            [
              { "id": "agent_001", "name": "Sarah Jenkins", "title": "Broker", "email": "s@k.com", "phone": "314-555-0101", "bio": "Bio here.", "image": "/img/sarah.jpg" }
            ]
            """;
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "agents.json"), json);

        var results = (await CreateLoader().LoadAgentsAsync()).ToList();

        results.Should().HaveCount(1);
        results[0].Id.Should().Be("agent_001");
        results[0].Image.Should().Be("/img/sarah.jpg");
    }

    [Fact]
    public async Task LoadListings_ParsesValidJson()
    {
        var json = """
            [
              {
                "id": "list_101",
                "address": "7522 Maryland Ave",
                "neighborhood": "clayton",
                "price": 895000,
                "bedrooms": 3,
                "bathrooms": 2.5,
                "sqft": 2400,
                "type": "Condo",
                "main_image": "/img/101-main.jpg",
                "images": ["/img/101-1.jpg"],
                "description": "Great condo.",
                "agent_id": "agent_001"
              }
            ]
            """;
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "listings.json"), json);

        var results = (await CreateLoader().LoadListingsAsync()).ToList();

        results.Should().HaveCount(1);
        results[0].Neighborhood.Should().Be("clayton");
        results[0].AgentId.Should().Be("agent_001");
        results[0].Sqft.Should().Be(2400);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }
}
