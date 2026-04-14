using KnotShoreRealty.Core.Enums;
using KnotShoreRealty.Core.Interfaces;
using KnotShoreRealty.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KnotShoreRealty.Data.Repositories;

public class ListingRepository : IListingRepository
{
    private readonly KnotShoreRealtyDbContext _context;

    public ListingRepository(KnotShoreRealtyDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Listing>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Listings
            .AsNoTracking()
            .Include(l => l.Agent)
            .Include(l => l.Neighborhood)
            .Include(l => l.Images)
            .OrderByDescending(l => l.ListedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Listing?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Listings
            .AsNoTracking()
            .Include(l => l.Agent)
            .Include(l => l.Neighborhood)
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<Listing?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Listings
            .AsNoTracking()
            .Include(l => l.Agent)
            .Include(l => l.Neighborhood)
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Slug == slug, cancellationToken);
    }

    public async Task<IEnumerable<Listing>> GetPublicListingsAsync(CancellationToken cancellationToken = default)
    {
        // Only Active and Pending listings appear on public pages — this rule is intentionally
        // enforced in one place so any change to it is caught by the repository tests immediately.
        return await _context.Listings
            .AsNoTracking()
            .Where(l => l.Status == ListingStatus.Active || l.Status == ListingStatus.Pending)
            .Include(l => l.Agent)
            .Include(l => l.Neighborhood)
            .Include(l => l.Images)
            .OrderByDescending(l => l.ListedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Listing>> GetByAgentIdAsync(int agentId, CancellationToken cancellationToken = default)
    {
        return await _context.Listings
            .AsNoTracking()
            .Where(l => l.AgentId == agentId &&
                        (l.Status == ListingStatus.Active || l.Status == ListingStatus.Pending))
            .Include(l => l.Images)
            .Include(l => l.Neighborhood)
            .OrderByDescending(l => l.ListedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Listing>> GetByAgentIdIncludingSoldAsync(int agentId, CancellationToken cancellationToken = default)
    {
        return await _context.Listings
            .AsNoTracking()
            .Where(l => l.AgentId == agentId &&
                        l.Status != ListingStatus.Draft &&
                        l.Status != ListingStatus.Withdrawn)
            .Include(l => l.Images)
            .Include(l => l.Neighborhood)
            .OrderByDescending(l => l.ListedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Listing>> GetByNeighborhoodIdAsync(int neighborhoodId, CancellationToken cancellationToken = default)
    {
        return await _context.Listings
            .AsNoTracking()
            .Where(l => l.NeighborhoodId == neighborhoodId &&
                        (l.Status == ListingStatus.Active || l.Status == ListingStatus.Pending))
            .Include(l => l.Agent)
            .Include(l => l.Images)
            .OrderByDescending(l => l.ListedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Listing listing, CancellationToken cancellationToken = default)
    {
        await _context.Listings.AddAsync(listing, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Listings.CountAsync(cancellationToken);
    }
}
