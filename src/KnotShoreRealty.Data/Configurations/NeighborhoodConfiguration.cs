using KnotShoreRealty.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnotShoreRealty.Data.Configurations;

public class NeighborhoodConfiguration : IEntityTypeConfiguration<Neighborhood>
{
    public void Configure(EntityTypeBuilder<Neighborhood> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(n => n.Slug)
            .IsUnique();

        builder.Property(n => n.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(n => n.HeroImageUrl)
            .HasMaxLength(500);

        builder.HasOne(n => n.Parent)
            .WithMany(n => n.Children)
            .HasForeignKey(n => n.ParentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
