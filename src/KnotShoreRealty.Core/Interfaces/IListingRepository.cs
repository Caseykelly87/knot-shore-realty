using KnotShoreRealty.Core.Models;

namespace KnotShoreRealty.Core.Interfaces;

public interface IListingRepository
{
    Task<IEnumerable<Listing>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Listing?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Listing?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> GetPublicListingsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> GetByAgentIdAsync(int agentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> GetByAgentIdIncludingSoldAsync(int agentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> GetByNeighborhoodIdAsync(int neighborhoodId, CancellationToken cancellationToken = default);
    Task AddAsync(Listing listing, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
