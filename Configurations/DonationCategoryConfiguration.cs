using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Data.Configurations
{
    public class DonationCategoryConfiguration : IEntityTypeConfiguration<DonationCategory>
    {
        public void Configure(EntityTypeBuilder<DonationCategory> builder)
        {
            builder.ToTable("DonationCategories");

            // Primary Key
            builder.HasKey(c => c.Id);

            // Name is required and has a max length of 100
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Description is optional
            builder.Property(c => c.Description)
                .IsRequired();

            // CreatedAt has a default value of UTC Now
            builder.Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // ImageUrl is optional
            builder.Property(c => c.ImageUrl)
                .IsRequired();

            // One-to-Many relationship with DonationOpportunity
            builder.HasMany(c => c.DonationOpportunities)
                .WithOne(o => o.Category)
                .HasForeignKey(o => o.CategoryId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete opportunities when a category is deleted

            // One-to-Many relationship with Donation
            builder.HasMany(c => c.Donations)
                .WithOne(d => d.Category)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of category if donations exist


            // Index with case-insensitive comparison
            builder.HasIndex(c => c.Name)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("IX_DonationCategory_Name_CaseInsensitive")
                .IsClustered(false); // Non-clustered index for better performance

            // Configure case-insensitive comparison for Name property
            builder.Property(c => c.Name)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS"); // Case-insensitive collation

            // Configure IsDeleted property
            builder.Property(c => c.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);
        }
    }
}