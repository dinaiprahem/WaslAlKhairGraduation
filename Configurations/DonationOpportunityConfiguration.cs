using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Data.Configurations
{
    public class DonationOpportunityConfiguration : IEntityTypeConfiguration<DonationOpportunity>
    {
        public void Configure(EntityTypeBuilder<DonationOpportunity> builder)
        {
            // Primary Key
            builder.HasKey(o => o.Id);

            // Title is required and has a max length of 255
            builder.Property(o => o.Title)
                .IsRequired()
                .HasMaxLength(255);

            // Description is required
            builder.Property(o => o.Description)
                .IsRequired();

            // ImageUrl is required
            builder.Property(o => o.ImageUrl)
                .IsRequired();

            // TargetAmount is optional and uses decimal(18, 2)
            builder.Property(o => o.TargetAmount)
                .HasColumnType("decimal(18, 2)");

            // CollectedAmount has a default value of 0 and uses decimal(18, 2)
            builder.Property(o => o.CollectedAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");

            // NumberOfDonors has a default value of 0
            builder.Property(o => o.NumberOfDonors)
                .HasDefaultValue(0);

            // Deadline is optional
            builder.Property(o => o.Deadline)
                .IsRequired(false);

            // Status is stored as a string
            builder.Property(o => o.Status)
                .HasConversion<string>() // Store enum as string
                .HasMaxLength(20); // Set max length for the string

            // PageVisits has a default value of 0
            builder.Property(o => o.PageVisits)
                .HasDefaultValue(0);

            // CreatedAt has a default value of UTC Now
            builder.Property(o => o.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // UpdatedAt is automatically updated on save
            builder.Property(o => o.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAddOrUpdate();



            // Relationships 
            builder.HasOne(o => o.Category)
                .WithMany(c => c.DonationOpportunities)
                .HasForeignKey(o => o.CategoryId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete opportunities when a category is deleted

            builder.HasOne(o => o.Charity)
                .WithMany()
                .HasForeignKey(o => o.CharityId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of the charity if opportunities exist

            builder.HasMany(o => o.DonationDistribution)
                .WithOne(dd => dd.DonationOpportunity)
                .HasForeignKey(dd => dd.DonationOpportunityId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // Indexes
            builder.HasIndex(o => o.CategoryId);
            builder.HasIndex(o => o.CharityId);
        }
    }
}