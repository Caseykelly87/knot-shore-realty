using KnotShoreRealty.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KnotShoreRealty.Data;

public class KnotShoreRealtyDbContext : DbContext
{
    public KnotShoreRealtyDbContext(DbContextOptions<KnotShoreRealtyDbContext> options)
        : base(options)
    {
    }

    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Neighborhood> Neighborhoods => Set<Neighborhood>();
    public DbSet<ListingImage> ListingImages => Set<ListingImage>();
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KnotShoreRealtyDbContext).Assembly);
    }
}
