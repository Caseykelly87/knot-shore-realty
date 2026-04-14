using KnotShoreRealty.Core.Interfaces;
using KnotShoreRealty.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KnotShoreRealty.Data.Repositories;

public class InquiryRepository : IInquiryRepository
{
    private readonly KnotShoreRealtyDbContext _context;

    public InquiryRepository(KnotShoreRealtyDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Inquiry inquiry, CancellationToken cancellationToken = default)
    {
        await _context.Inquiries.AddAsync(inquiry, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Inquiries.CountAsync(cancellationToken);
    }
}
