using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WaslAlkhair.Api.Models;

public class AssistanceConfiguration : IEntityTypeConfiguration<Assistance>
{
	public void Configure(EntityTypeBuilder<Assistance> builder)
	{
		builder.HasKey(a => a.Id);

		builder.Property(a => a.Title)
			.IsRequired()
			.HasMaxLength(255);

		builder.Property(a => a.Description)
			.IsRequired();

		builder.Property(a => a.AvailableSpots)
			.IsRequired();

		builder.Property(a => a.CreatedAt)
			.HasDefaultValueSql("GETUTCDATE()");

		builder.Property(a => a.ContactInfo)
			.IsRequired()
			.HasMaxLength(255);

		builder.Property(a => a.IsOpen)
			.HasDefaultValue(true);

		builder.Property(a => a.DescriptionUpdatedAt)
			.IsRequired(false); 

		// Foreign Key for AssistanceType
		builder.HasOne(a => a.AssistanceType)
			.WithMany(at => at.Assistances)
			.HasForeignKey(a => a.AssistanceTypeId)
			.OnDelete(DeleteBehavior.Cascade);

		// Foreign Key for CreatedBy
		builder.HasOne(a => a.CreatedBy)
			.WithMany(u => u.Assistances)
			.HasForeignKey(a => a.CreatedById)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
