using KnotShoreRealty.Core.Models;

namespace KnotShoreRealty.Core.Interfaces;

public interface IInquiryRepository
{
    Task AddAsync(Inquiry inquiry, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
