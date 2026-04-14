using KnotShoreRealty.Core.Interfaces;
using KnotShoreRealty.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KnotShoreRealty.Data.Repositories;

public class NeighborhoodRepository : INeighborhoodRepository
{
    private readonly KnotShoreRealtyDbContext _context;

    public NeighborhoodRepository(KnotShoreRealtyDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Neighborhood>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Neighborhoods
            .AsNoTracking()
            .OrderBy(n => n.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Neighborhood?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Neighborhoods
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<Neighborhood?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Neighborhoods
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Slug == slug, cancellationToken);
    }

    public async Task<IEnumerable<Neighborhood>> GetTaxonomyTreeAsync(CancellationToken cancellationToken = default)
    {
        // Load all neighborhoods in one query and build the tree in memory.
        // For the current dataset (~39 records) this is straightforward and avoids N+1 queries.
        // A recursive CTE would be more efficient for large trees, but that would be premature
        // optimization here — the record count is bounded and stable.
        var all = await _context.Neighborhoods
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var lookup = all.ToDictionary(n => n.Id);

        foreach (var node in all)
        {
            node.Children = new List<Neighborhood>();
        }

        foreach (var node in all)
        {
            if (node.ParentId.HasValue && lookup.TryGetValue(node.ParentId.Value, out var parent))
            {
                parent.Children.Add(node);
            }
        }

        return all.Where(n => n.ParentId == null).OrderBy(n => n.Name);
    }

    public async Task<IEnumerable<Neighborhood>> GetAncestryAsync(int neighborhoodId, CancellationToken cancellationToken = default)
    {
        // Load all neighborhoods once, then walk up the parent chain in memory.
        // The tree is small enough that this is cheaper than multiple round trips.
        var all = await _context.Neighborhoods
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var lookup = all.ToDictionary(n => n.Id);

        var chain = new List<Neighborhood>();

        if (!lookup.TryGetValue(neighborhoodId, out var current))
            return chain;

        while (current != null)
        {
            chain.Insert(0, current);
            current = current.ParentId.HasValue && lookup.TryGetValue(current.ParentId.Value, out var parent)
                ? parent
                : null;
        }

        return chain;
    }

    public async Task AddAsync(Neighborhood neighborhood, CancellationToken cancellationToken = default)
    {
        await _context.Neighborhoods.AddAsync(neighborhood, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Neighborhoods.CountAsync(cancellationToken);
    }
}
