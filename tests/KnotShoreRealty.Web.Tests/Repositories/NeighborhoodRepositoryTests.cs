using FluentAssertions;
using KnotShoreRealty.Core.Models;
using KnotShoreRealty.Data;
using KnotShoreRealty.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace KnotShoreRealty.Web.Tests.Repositories;

public class NeighborhoodRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly KnotShoreRealtyDbContext _context;
    private readonly NeighborhoodRepository _repo;

    // IDs set during seed so tests can reference them
    private int _metroId;
    private int _countyId;
    private int _claytonId;
    private int _deMunId;

    public NeighborhoodRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<KnotShoreRealtyDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new KnotShoreRealtyDbContext(options);
        _context.Database.EnsureCreated();

        SeedHierarchy();

        _repo = new NeighborhoodRepository(_context);
    }

    private void SeedHierarchy()
    {
        // Four-level hierarchy: Metro -> County -> Clayton -> DeMun
        var metro = new Neighborhood { Slug = "st-louis-metro", Name = "St. Louis Metro", Description = "Root" };
        _context.Neighborhoods.Add(metro);
        _context.SaveChanges();
        _metroId = metro.Id;

        var county = new Neighborhood { Slug = "st-louis-county", Name = "St. Louis County", Description = "County", ParentId = metro.Id };
        _context.Neighborhoods.Add(county);
        _context.SaveChanges();
        _countyId = county.Id;

        var clayton = new Neighborhood { Slug = "clayton", Name = "Clayton", Description = "City", ParentId = county.Id };
        _context.Neighborhoods.Add(clayton);
        _context.SaveChanges();
        _claytonId = clayton.Id;

        var deMun = new Neighborhood { Slug = "de-mun", Name = "DeMun", Description = "Neighborhood", ParentId = clayton.Id };
        _context.Neighborhoods.Add(deMun);
        _context.SaveChanges();
        _deMunId = deMun.Id;
    }

    [Fact]
    public async Task GetTaxonomyTreeAsync_ReturnsRootsWithChildren()
    {
        var roots = (await _repo.GetTaxonomyTreeAsync()).ToList();
        roots.Should().HaveCount(1);
        roots[0].Slug.Should().Be("st-louis-metro");
        roots[0].Children.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetTaxonomyTreeAsync_PopulatesMultipleLevels()
    {
        var roots = (await _repo.GetTaxonomyTreeAsync()).ToList();
        var county = roots[0].Children[0];
        county.Slug.Should().Be("st-louis-county");
        county.Children.Should().HaveCount(1);
        county.Children[0].Slug.Should().Be("clayton");
        county.Children[0].Children.Should().HaveCount(1);
        county.Children[0].Children[0].Slug.Should().Be("de-mun");
    }

    [Fact]
    public async Task GetAncestryAsync_ReturnsRootToLeafChain()
    {
        var chain = (await _repo.GetAncestryAsync(_deMunId)).ToList();
        chain.Should().HaveCount(4);
        chain[0].Slug.Should().Be("st-louis-metro");
        chain[1].Slug.Should().Be("st-louis-county");
        chain[2].Slug.Should().Be("clayton");
        chain[3].Slug.Should().Be("de-mun");
    }

    [Fact]
    public async Task GetAncestryAsync_HandlesDirectChildOfRoot()
    {
        var chain = (await _repo.GetAncestryAsync(_countyId)).ToList();
        chain.Should().HaveCount(2);
        chain[0].Slug.Should().Be("st-louis-metro");
        chain[1].Slug.Should().Be("st-louis-county");
    }

    [Fact]
    public async Task GetBySlugAsync_FindsCorrectRecord()
    {
        var result = await _repo.GetBySlugAsync("clayton");
        result.Should().NotBeNull();
        result!.Id.Should().Be(_claytonId);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
