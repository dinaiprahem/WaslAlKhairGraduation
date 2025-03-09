using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Data.Configurations
{
    public class GiftDonationConfiguration : IEntityTypeConfiguration<GiftDonation>
    {
        public void Configure(EntityTypeBuilder<GiftDonation> builder)
        {
            builder.HasKey(g => g.Id);

            builder.Property(g => g.RecipientName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(g => g.RecipientPhone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(g => g.ShowAmount)
                .HasDefaultValue(false);

            builder.Property(g => g.ShowOpportunity)
                .HasDefaultValue(false);

            // Configure the one-to-one relationship with Donation
            builder.HasOne(g => g.Donation)
                .WithOne(d => d.GiftDonation)
                .HasForeignKey<GiftDonation>(g => g.DonationId);
        }
    }
}