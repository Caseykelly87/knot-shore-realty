using KnotShoreRealty.Core.Enums;
using KnotShoreRealty.Core.Helpers;
using KnotShoreRealty.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KnotShoreRealty.Data.Seed;

public class DbInitializer
{
    private readonly KnotShoreRealtyDbContext _context;
    private readonly SeedDataLoader _loader;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(
        KnotShoreRealtyDbContext context,
        SeedDataLoader loader,
        ILogger<DbInitializer> logger)
    {
        _context = context;
        _loader = loader;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        await _context.Database.MigrateAsync();

        if (await _context.Listings.AnyAsync())
        {
            _logger.LogInformation("Database already populated, skipping seed");
            return;
        }

        _logger.LogInformation("Database is empty, beginning seed");

        var neighborhoodDtos = (await _loader.LoadNeighborhoodsAsync()).ToList();
        var agentDtos        = (await _loader.LoadAgentsAsync()).ToList();
        var listingDtos      = (await _loader.LoadListingsAsync()).ToList();

        if (!neighborhoodDtos.Any() && !agentDtos.Any() && !listingDtos.Any())
        {
            _logger.LogWarning("No seed data found, starting with empty database");
            return;
        }

        var slugToNeighborhoodId = await SeedNeighborhoodsAsync(neighborhoodDtos);
        var dtoIdToAgentId       = await SeedAgentsAsync(agentDtos);
        await SeedListingsAsync(listingDtos, slugToNeighborhoodId, dtoIdToAgentId);

        _logger.LogInformation(
            "Seed complete: {Neighborhoods} neighborhoods, {Agents} agents, {Listings} listings",
            neighborhoodDtos.Count, agentDtos.Count, listingDtos.Count);
    }

    private async Task<Dictionary<string, int>> SeedNeighborhoodsAsync(
        List<SeedDataLoader.NeighborhoodSeedDto> dtos)
    {
        // First pass: insert all neighborhoods with ParentId = null to avoid forward-reference issues.
        foreach (var dto in dtos)
        {
            var neighborhood = new Neighborhood
            {
                Slug        = dto.Slug,
                Name        = dto.Name,
                Description = dto.Description,
                ParentId    = null,
                HeroImageUrl = null   // deferred to a future phase
            };
            _context.Neighborhoods.Add(neighborhood);
        }

        await _context.SaveChangesAsync();

        // Build slug → ID map now that the rows have been inserted and IDs assigned.
        var slugToId = await _context.Neighborhoods
            .ToDictionaryAsync(n => n.Slug, n => n.Id);

        // Second pass: wire up parent references using the slug map.
        foreach (var dto in dtos.Where(d => d.ParentSlug != null))
        {
            if (!slugToId.TryGetValue(dto.ParentSlug!, out var parentId))
            {
                _logger.LogWarning("Parent slug not found: {ParentSlug} (for {Slug})", dto.ParentSlug, dto.Slug);
                continue;
            }

            var neighborhood = await _context.Neighborhoods.FirstAsync(n => n.Slug == dto.Slug);
            neighborhood.ParentId = parentId;
        }

        await _context.SaveChangesAsync();

        return slugToId;
    }

    private async Task<Dictionary<string, int>> SeedAgentsAsync(
        List<SeedDataLoader.AgentSeedDto> dtos)
    {
        // Maps the DTO's string id (e.g. "agent_001") to the database-generated int id
        // so listings can resolve their agent references.
        var dtoIdToAgentId = new Dictionary<string, int>();

        foreach (var dto in dtos)
        {
            var agent = new Agent
            {
                Slug     = SlugGenerator.Generate(dto.Name),
                Name     = dto.Name,
                Title    = dto.Title,
                Email    = dto.Email,
                Phone    = dto.Phone,
                Bio      = dto.Bio,
                PhotoUrl = dto.Image
            };
            _context.Agents.Add(agent);
            await _context.SaveChangesAsync();

            dtoIdToAgentId[dto.Id] = agent.Id;
        }

        return dtoIdToAgentId;
    }

    private async Task SeedListingsAsync(
        List<SeedDataLoader.ListingSeedDto> dtos,
        Dictionary<string, int> slugToNeighborhoodId,
        Dictionary<string, int> dtoIdToAgentId)
    {
        foreach (var dto in dtos)
        {
            if (!slugToNeighborhoodId.TryGetValue(dto.Neighborhood, out var neighborhoodId))
            {
                _logger.LogWarning("Neighborhood slug not found: {Slug} (listing {Id})", dto.Neighborhood, dto.Id);
                continue;
            }

            if (!dtoIdToAgentId.TryGetValue(dto.AgentId, out var agentId))
            {
                _logger.LogWarning("Agent id not found: {AgentId} (listing {Id})", dto.AgentId, dto.Id);
                continue;
            }

            var propertyType = ParsePropertyType(dto.Type, dto.Id);
            var listingType  = InferListingType(propertyType);

            var listing = new Listing
            {
                Slug           = SlugGenerator.Generate(dto.Address),
                Address        = dto.Address,
                // City, State, and Zip are not present in the seed JSON; these are placeholder
                // defaults. A future iteration could resolve City from the neighborhood hierarchy.
                City           = "St. Louis",
                State          = "MO",
                Zip            = "63101",
                Price          = dto.Price,
                Bedrooms       = dto.Bedrooms,
                Bathrooms      = dto.Bathrooms,
                SquareFeet     = dto.Sqft,
                Description    = dto.Description,
                Status         = ListingStatus.Active,
                ListingType    = listingType,
                PropertyType   = propertyType,
                // ListedDate is not in the seed JSON; default to 30 days ago so listings
                // appear as recently active without future-dating them.
                ListedDate     = DateTime.UtcNow.AddDays(-30),
                NeighborhoodId = neighborhoodId,
                AgentId        = agentId
            };

            // Primary image
            listing.Images.Add(new ListingImage
            {
                ImageUrl  = dto.MainImage,
                AltText   = $"{dto.Address} primary photo",
                SortOrder = 0,
                IsPrimary = true
            });

            // Secondary images
            for (int i = 0; i < dto.Images.Count; i++)
            {
                listing.Images.Add(new ListingImage
                {
                    ImageUrl  = dto.Images[i],
                    AltText   = $"{dto.Address} photo {i + 1}",
                    SortOrder = i + 1,
                    IsPrimary = false
                });
            }

            _context.Listings.Add(listing);
        }

        await _context.SaveChangesAsync();
    }

    private PropertyType ParsePropertyType(string raw, string listingId)
    {
        // Normalize the string from the JSON ("Single Family" → "SingleFamily",
        // "Multi-family" → "MultiFamily") before parsing.
        var normalized = raw.Replace(" ", "").Replace("-", "");

        if (Enum.TryParse<PropertyType>(normalized, ignoreCase: true, out var result))
            return result;

        _logger.LogWarning("Unknown property type '{Type}' for listing {Id}, defaulting to SingleFamily", raw, listingId);
        return PropertyType.SingleFamily;
    }

    private static ListingType InferListingType(PropertyType propertyType) =>
        propertyType switch
        {
            PropertyType.SingleFamily => ListingType.Residential,
            PropertyType.Condo        => ListingType.Residential,
            PropertyType.Townhouse    => ListingType.Residential,
            PropertyType.MultiFamily  => ListingType.Residential,
            _                         => ListingType.Commercial
        };
}
