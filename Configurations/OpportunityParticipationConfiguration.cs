using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Data.Configurations
{
	public class OpportunityParticipationConfiguration : IEntityTypeConfiguration<OpportunityParticipation>
	{
		public void Configure(EntityTypeBuilder<OpportunityParticipation> builder)
		{
			builder.HasKey(p => p.Id);

			// Relationship with AppUser
			builder.HasOne(p => p.AppUser)
				.WithMany(u => u.OpportunityParticipations)
				.HasForeignKey(p => p.AppUserId)
				.OnDelete(DeleteBehavior.Cascade);

			// Relationship with Opportunity
			builder.HasOne(p => p.Opportunity)
				.WithMany(o => o.Participants)
				.HasForeignKey(p => p.OpportunityId)
				.OnDelete(DeleteBehavior.NoAction);

			builder.Property(p => p.FullName)
				.IsRequired();

			builder.Property(p => p.NationalId)
				.IsRequired()
				.HasMaxLength(14);

			builder.Property(p => p.Age)
				.IsRequired();

			builder.Property(p => p.Gender)
				.IsRequired();

			builder.Property(p => p.Email)
				.IsRequired();

			builder.Property(p => p.Specialization)
				.IsRequired();

			builder.Property(p => p.PhoneNumber)
				.IsRequired();

			builder.Property(p => p.Address)
				.IsRequired();
		}
	}
}
