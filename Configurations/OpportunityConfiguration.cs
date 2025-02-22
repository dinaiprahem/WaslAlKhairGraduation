using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Data.Configurations
{
	public class OpportunityConfiguration : IEntityTypeConfiguration<Opportunity>
	{
		public void Configure(EntityTypeBuilder<Opportunity> builder)
		{
			builder.HasKey(o => o.Id);

			builder.Property(o => o.Title)
				   .IsRequired()
				   .HasMaxLength(255);

			builder.Property(o => o.Description)
				   .IsRequired();

			builder.Property(o => o.Tasks)
				   .IsRequired();

			builder.Property(o => o.StartDate)
		   .IsRequired()
		   .HasColumnType("DATE");

			builder.Property(o => o.EndDate)
		   .IsRequired()
		   .HasColumnType("DATE");

			builder.Property(o => o.SeatsAvailable)
				   .IsRequired();

			builder.Property(o => o.Location)
				   .IsRequired()
				   .HasMaxLength(255);

			builder.Property(o => o.Benefits)
				   .IsRequired();

			builder.Property(o => o.RequiredAge)
				   .IsRequired();

			builder.Property(o => o.Type)
				   .IsRequired();

			// CreatedBy relationship
			builder.HasOne(o => o.CreatedBy)
				   .WithMany(u => u.CreatedOpportunities)
				   .HasForeignKey(o => o.CreatedById)
				   .OnDelete(DeleteBehavior.Cascade);

			// One-to-Many: Opportunity -> OpportunityParticipation
			builder.HasMany(o => o.Participants)
				   .WithOne(p => p.Opportunity)
				   .HasForeignKey(p => p.OpportunityId)
				   .OnDelete(DeleteBehavior.Cascade);

			builder.Property(o => o.PhotoUrl)
				   .HasMaxLength(500)
				   .IsRequired(false);

			builder.Property(o => o.IsClosed)
				   .HasDefaultValue(false);
		}
	}
}
