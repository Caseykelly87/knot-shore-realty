using KnotShoreRealty.Core.Models;

namespace KnotShoreRealty.Core.Interfaces;

public interface IAgentRepository
{
    Task<IEnumerable<Agent>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Agent?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Agent?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task AddAsync(Agent agent, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
