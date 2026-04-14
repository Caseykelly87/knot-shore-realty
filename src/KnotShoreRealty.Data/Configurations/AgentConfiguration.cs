using KnotShoreRealty.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnotShoreRealty.Data.Configurations;

public class AgentConfiguration : IEntityTypeConfiguration<Agent>
{
    public void Configure(EntityTypeBuilder<Agent> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(a => a.Slug)
            .IsUnique();

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Bio)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(a => a.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Phone)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.PhotoUrl)
            .HasMaxLength(500);
    }
}
