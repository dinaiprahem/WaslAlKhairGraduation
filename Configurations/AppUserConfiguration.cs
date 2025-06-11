using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Data.Configurations
{
	public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
	{
		public void Configure(EntityTypeBuilder<AppUser> builder)
		{
			
			builder.Property(u => u.FullName)
				.IsRequired()
				.HasMaxLength(50);

			builder.Property(u => u.DateOfBirth)
				.HasColumnType("DATE");

			// Charity-specific fields
			builder.Property(u => u.CharityRegistrationNumber)
				.HasMaxLength(100)
				.IsRequired(false);

			builder.Property(u => u.CharityMission)
				.HasMaxLength(500)
				.IsRequired(false);

			builder.Property(u => u.Address)
				.HasMaxLength(255)
				.IsRequired(false);

			//  One-to-Many: User/Charity -> Created Opportunities
			builder.HasMany(u => u.CreatedOpportunities)
				.WithOne(o => o.CreatedBy) 
				.HasForeignKey(o => o.CreatedById)
				.OnDelete(DeleteBehavior.Cascade); 

			//  One-to-Many: User -> OpportunityParticipations
			builder.HasMany(u => u.OpportunityParticipations)
				.WithOne(p => p.AppUser)
				.HasForeignKey(p => p.AppUserId)
				.OnDelete(DeleteBehavior.Cascade);
			
			// One-to-Many: User -> Created Assistances

			builder.HasMany(u => u.Assistances)
				.WithOne(a => a.CreatedBy)
				.HasForeignKey(a => a.CreatedById)
				.OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete
		}
	}
}
