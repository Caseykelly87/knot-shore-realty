using KnotShoreRealty.Core.Models;

namespace KnotShoreRealty.Core.Interfaces;

public interface INeighborhoodRepository
{
    Task<IEnumerable<Neighborhood>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Neighborhood?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Neighborhood?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Neighborhood>> GetTaxonomyTreeAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Neighborhood>> GetAncestryAsync(int neighborhoodId, CancellationToken cancellationToken = default);
    Task AddAsync(Neighborhood neighborhood, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
