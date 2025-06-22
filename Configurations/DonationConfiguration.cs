using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Data.Configurations
{
    public class DonationConfiguration : IEntityTypeConfiguration<Donation>
    {
        public void Configure(EntityTypeBuilder<Donation> builder)
        {
            // Primary Key
            builder.HasKey(d => d.Id);

            // Amount is required and uses decimal(18, 2)
            builder.Property(d => d.Amount)
                .IsRequired()
                .HasColumnType("decimal(18, 2)");

            // DonatedAt has a default value of UTC Now
            builder.Property(d => d.DonatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

		//payment
			builder.Property(d => d.StripeSessionId)
				.HasMaxLength(255);

			builder.Property(d => d.IsPaid)
	           .HasDefaultValue(false); 
			builder.Property(d => d.PaymentConfirmedAt)
				.IsRequired(false); 


			// Relationships
			builder.HasOne(d => d.Donor)
                .WithMany()
                .HasForeignKey(d => d.DonorId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of the donor if donations exist

            builder.HasOne(d => d.Category)
                .WithMany(c => c.Donations)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of the category if donations exist

            builder.HasMany(d => d.Distributions)
                .WithOne(dd => dd.Donation)
                .HasForeignKey(dd => dd.DonationId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // Indexes
            builder.HasIndex(d => d.DonorId);
            builder.HasIndex(d => d.CategoryId);
        }
    }
}
