using KnotShoreRealty.Core.Enums;
using KnotShoreRealty.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnotShoreRealty.Data.Configurations;

public class ListingConfiguration : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(l => l.Slug)
            .IsUnique();

        builder.Property(l => l.Address)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.State)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(l => l.Zip)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(l => l.Price)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(l => l.Bedrooms)
            .IsRequired();

        builder.Property(l => l.Bathrooms)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(l => l.SquareFeet)
            .IsRequired();

        builder.Property(l => l.Description)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(l => l.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(l => l.ListingType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(l => l.PropertyType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(l => l.ListedDate)
            .IsRequired();

        builder.HasOne(l => l.Neighborhood)
            .WithMany(n => n.Listings)
            .HasForeignKey(l => l.NeighborhoodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Agent)
            .WithMany(a => a.Listings)
            .HasForeignKey(l => l.AgentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
