using KnotShoreRealty.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnotShoreRealty.Data.Configurations;

public class InquiryConfiguration : IEntityTypeConfiguration<Inquiry>
{
    public void Configure(EntityTypeBuilder<Inquiry> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Phone)
            .HasMaxLength(50);

        builder.Property(i => i.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(i => i.SubmittedAt)
            .IsRequired();

        builder.HasOne(i => i.Listing)
            .WithMany()
            .HasForeignKey(i => i.ListingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
