using KnotShoreRealty.Core.Interfaces;
using KnotShoreRealty.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KnotShoreRealty.Data.Repositories;

public class AgentRepository : IAgentRepository
{
    private readonly KnotShoreRealtyDbContext _context;

    public AgentRepository(KnotShoreRealtyDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Agent>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Agents
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Agent?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Agents
            .AsNoTracking()
            .Include(a => a.Listings)
                .ThenInclude(l => l.Images)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Agent?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Agents
            .AsNoTracking()
            .Include(a => a.Listings)
                .ThenInclude(l => l.Images)
            .FirstOrDefaultAsync(a => a.Slug == slug, cancellationToken);
    }

    public async Task AddAsync(Agent agent, CancellationToken cancellationToken = default)
    {
        await _context.Agents.AddAsync(agent, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Agents.CountAsync(cancellationToken);
    }
}
