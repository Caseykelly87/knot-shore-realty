using Microsoft.EntityFrameworkCore;

namespace KnotShoreRealty.Data;

public class KnotShoreRealtyDbContext : DbContext
{
    public KnotShoreRealtyDbContext(DbContextOptions<KnotShoreRealtyDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KnotShoreRealtyDbContext).Assembly);
    }
}
