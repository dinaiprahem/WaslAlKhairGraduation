using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Data.Configurations
{
    public class DonationDistributionConfiguration : IEntityTypeConfiguration<DonationDistribution>
    {
        public void Configure(EntityTypeBuilder<DonationDistribution> builder)
        {
            // Composite Key
            builder.HasKey(dd => new { dd.DonationId, dd.DonationOpportunityId });

            // DistributedAmount is required and uses decimal(18, 2)
            builder.Property(dd => dd.DistributedAmount)
                .IsRequired()
                .HasColumnType("decimal(18, 2)");



            // Relationships
            builder.HasOne(dd => dd.Donation)
                .WithMany(d => d.Distributions)
                .HasForeignKey(dd => dd.DonationId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            builder.HasOne(dd => dd.DonationOpportunity)
                .WithMany(o => o.DonationDistribution)
                .HasForeignKey(dd => dd.DonationOpportunityId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete



            // Indexes
            builder.HasIndex(dd => dd.DonationId);
            builder.HasIndex(dd => dd.DonationOpportunityId);
        }
    }
}