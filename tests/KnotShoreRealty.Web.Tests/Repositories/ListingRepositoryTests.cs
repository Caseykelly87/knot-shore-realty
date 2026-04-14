using FluentAssertions;
using KnotShoreRealty.Core.Enums;
using KnotShoreRealty.Core.Models;
using KnotShoreRealty.Data;
using KnotShoreRealty.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace KnotShoreRealty.Web.Tests.Repositories;

public class ListingRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly KnotShoreRealtyDbContext _context;
    private readonly ListingRepository _repo;

    public ListingRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<KnotShoreRealtyDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new KnotShoreRealtyDbContext(options);
        _context.Database.EnsureCreated();

        SeedTestData();

        _repo = new ListingRepository(_context);
    }

    private void SeedTestData()
    {
        var agent = new Agent { Slug = "agent-a", Name = "Agent A", Title = "T", Bio = "B", Email = "a@a.com", Phone = "555-0000" };
        var neighborhood = new Neighborhood { Slug = "test-area", Name = "Test Area", Description = "Desc" };
        _context.Agents.Add(agent);
        _context.Neighborhoods.Add(neighborhood);
        _context.SaveChanges();

        var listings = new[]
        {
            MakeListing("active-1",    ListingStatus.Active,    agent.Id, neighborhood.Id),
            MakeListing("active-2",    ListingStatus.Active,    agent.Id, neighborhood.Id),
            MakeListing("pending-1",   ListingStatus.Pending,   agent.Id, neighborhood.Id),
            MakeListing("draft-1",     ListingStatus.Draft,     agent.Id, neighborhood.Id),
            MakeListing("sold-1",      ListingStatus.Sold,      agent.Id, neighborhood.Id),
            MakeListing("withdrawn-1", ListingStatus.Withdrawn, agent.Id, neighborhood.Id),
        };

        _context.Listings.AddRange(listings);
        _context.SaveChanges();
    }

    private static Listing MakeListing(string slug, ListingStatus status, int agentId, int neighborhoodId) =>
        new()
        {
            Slug = slug,
            Address = slug,
            City = "St. Louis",
            State = "MO",
            Zip = "63101",
            Price = 100000m,
            Bedrooms = 2,
            Bathrooms = 1,
            SquareFeet = 1000,
            Description = "Test listing",
            Status = status,
            ListingType = ListingType.Residential,
            PropertyType = PropertyType.SingleFamily,
            ListedDate = DateTime.UtcNow,
            AgentId = agentId,
            NeighborhoodId = neighborhoodId
        };

    [Fact]
    public async Task GetPublicListingsAsync_ReturnsActiveAndPending()
    {
        var results = await _repo.GetPublicListingsAsync();
        results.Should().HaveCount(3);
        results.Should().OnlyContain(l => l.Status == ListingStatus.Active || l.Status == ListingStatus.Pending);
    }

    [Fact]
    public async Task GetPublicListingsAsync_ExcludesDraft()
    {
        var results = await _repo.GetPublicListingsAsync();
        results.Should().NotContain(l => l.Status == ListingStatus.Draft);
    }

    [Fact]
    public async Task GetPublicListingsAsync_ExcludesSold()
    {
        var results = await _repo.GetPublicListingsAsync();
        results.Should().NotContain(l => l.Status == ListingStatus.Sold);
    }

    [Fact]
    public async Task GetPublicListingsAsync_ExcludesWithdrawn()
    {
        var results = await _repo.GetPublicListingsAsync();
        results.Should().NotContain(l => l.Status == ListingStatus.Withdrawn);
    }

    [Fact]
    public async Task GetByAgentIdAsync_ReturnsOnlyMatchingAgentActiveListings()
    {
        var agentId = _context.Agents.First().Id;
        var results = await _repo.GetByAgentIdAsync(agentId);
        results.Should().OnlyContain(l => l.AgentId == agentId);
        results.Should().OnlyContain(l => l.Status == ListingStatus.Active || l.Status == ListingStatus.Pending);
    }

    [Fact]
    public async Task GetByAgentIdIncludingSoldAsync_IncludesSoldButNotDraft()
    {
        var agentId = _context.Agents.First().Id;
        var results = await _repo.GetByAgentIdIncludingSoldAsync(agentId);
        results.Should().Contain(l => l.Status == ListingStatus.Sold);
        results.Should().NotContain(l => l.Status == ListingStatus.Draft);
        results.Should().NotContain(l => l.Status == ListingStatus.Withdrawn);
    }

    [Fact]
    public async Task GetByIdAsync_IncludesNavigationProperties()
    {
        var listingId = _context.Listings.First(l => l.Status == ListingStatus.Active).Id;
        var result = await _repo.GetByIdAsync(listingId);
        result.Should().NotBeNull();
        result!.Agent.Should().NotBeNull();
        result.Neighborhood.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
