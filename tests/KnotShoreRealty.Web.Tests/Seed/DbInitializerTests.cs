using FluentAssertions;
using KnotShoreRealty.Core.Enums;
using KnotShoreRealty.Data;
using KnotShoreRealty.Data.Seed;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace KnotShoreRealty.Web.Tests.Seed;

public class DbInitializerTests : IDisposable
{
    private readonly string _tempDir;
    private readonly SqliteConnection _connection;
    private readonly KnotShoreRealtyDbContext _context;

    public DbInitializerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);

        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<KnotShoreRealtyDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new KnotShoreRealtyDbContext(options);
        _context.Database.EnsureCreated();
    }

    private DbInitializer CreateInitializer() =>
        new(_context, new SeedDataLoader(_tempDir, NullLogger<SeedDataLoader>.Instance),
            NullLogger<DbInitializer>.Instance);

    private async Task WriteFixtureFiles()
    {
        var neighborhoods = """
            [
              { "slug": "st-louis-metro", "name": "St. Louis Metro", "description": "Root region", "parentSlug": null },
              { "slug": "city-of-st-louis", "name": "City of St. Louis", "description": "The city", "parentSlug": "st-louis-metro" },
              { "slug": "soulard", "name": "Soulard", "description": "Historic district", "parentSlug": "city-of-st-louis" }
            ]
            """;

        var agents = """
            [
              { "id": "agent_001", "name": "Sarah Jenkins", "title": "Broker", "email": "sarah@k.com", "phone": "314-555-0101", "bio": "15 years experience.", "image": "/img/sarah.jpg" }
            ]
            """;

        var listings = """
            [
              {
                "id": "list_101",
                "address": "1812 Ninth St",
                "neighborhood": "soulard",
                "price": 349900,
                "bedrooms": 2,
                "bathrooms": 2.0,
                "sqft": 1650,
                "type": "Townhouse",
                "main_image": "/img/101-main.jpg",
                "images": ["/img/101-1.jpg", "/img/101-2.jpg"],
                "description": "Classic Soulard brick.",
                "agent_id": "agent_001"
              },
              {
                "id": "list_102",
                "address": "22 Log Cabin Ln",
                "neighborhood": "soulard",
                "price": 2650000,
                "bedrooms": 6,
                "bathrooms": 5.5,
                "sqft": 7200,
                "type": "Single Family",
                "main_image": "/img/102-main.jpg",
                "images": [],
                "description": "Gated estate.",
                "agent_id": "agent_001"
              }
            ]
            """;

        await File.WriteAllTextAsync(Path.Combine(_tempDir, "neighborhoods.json"), neighborhoods);
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "agents.json"), agents);
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "listings.json"), listings);
    }

    [Fact]
    public async Task InitializeAsync_LoadsSeedData_WhenDatabaseEmpty()
    {
        await WriteFixtureFiles();
        await CreateInitializer().InitializeAsync();

        _context.Neighborhoods.Count().Should().Be(3);
        _context.Agents.Count().Should().Be(1);
        _context.Listings.Count().Should().Be(2);
    }

    [Fact]
    public async Task InitializeAsync_DoesNothing_WhenDatabaseAlreadyPopulated()
    {
        await WriteFixtureFiles();
        var initializer = CreateInitializer();

        await initializer.InitializeAsync();
        await initializer.InitializeAsync(); // second call should be a no-op

        _context.Listings.Count().Should().Be(2);
    }

    [Fact]
    public async Task InitializeAsync_ResolvesNeighborhoodSlugReferences()
    {
        await WriteFixtureFiles();
        await CreateInitializer().InitializeAsync();

        var soulard = _context.Neighborhoods.First(n => n.Slug == "soulard");
        var city = _context.Neighborhoods.First(n => n.Slug == "city-of-st-louis");

        soulard.ParentId.Should().Be(city.Id);
    }

    [Fact]
    public async Task InitializeAsync_ResolvesAgentIdReferences()
    {
        await WriteFixtureFiles();
        await CreateInitializer().InitializeAsync();

        var agent = _context.Agents.First();
        var listing = _context.Listings.First();

        listing.AgentId.Should().Be(agent.Id);
    }

    [Fact]
    public async Task InitializeAsync_InfersListingTypeFromPropertyType()
    {
        await WriteFixtureFiles();
        await CreateInitializer().InitializeAsync();

        // Townhouse and SingleFamily are both Residential
        _context.Listings.Should().OnlyContain(l => l.ListingType == ListingType.Residential);
    }

    [Fact]
    public async Task InitializeAsync_CreatesPrimaryAndSecondaryImages()
    {
        await WriteFixtureFiles();
        await CreateInitializer().InitializeAsync();

        // list_101 has main_image + 2 images = 3 total; list_102 has main_image + 0 images = 1 total
        var listing101 = _context.Listings
            .Include(l => l.Images)
            .First(l => l.Address == "1812 Ninth St");

        listing101.Images.Should().HaveCount(3);
        listing101.Images.Should().ContainSingle(i => i.IsPrimary);
        listing101.Images.Where(i => !i.IsPrimary).Should().HaveCount(2);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }
}
