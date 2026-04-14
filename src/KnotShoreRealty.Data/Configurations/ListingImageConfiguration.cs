using KnotShoreRealty.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnotShoreRealty.Data.Configurations;

public class ListingImageConfiguration : IEntityTypeConfiguration<ListingImage>
{
    public void Configure(EntityTypeBuilder<ListingImage> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.AltText)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.SortOrder)
            .HasDefaultValue(0);

        builder.Property(i => i.IsPrimary)
            .HasDefaultValue(false);

        builder.HasOne(i => i.Listing)
            .WithMany(l => l.Images)
            .HasForeignKey(i => i.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
