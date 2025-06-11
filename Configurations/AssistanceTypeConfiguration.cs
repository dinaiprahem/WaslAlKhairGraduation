using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WaslAlkhair.Api.Models;
using System;

namespace WaslAlkhair.Api.Data.Configurations
{
	public class AssistanceTypeConfiguration : IEntityTypeConfiguration<AssistanceType>
	{
		public void Configure(EntityTypeBuilder<AssistanceType> builder)
		{ 
			builder.HasKey(a => a.Id);
			builder.Property(a => a.Name)
				.IsRequired()
				.HasMaxLength(100);

			// Seed default Assistance Types
			builder.HasData(
				new AssistanceType { Id = Guid.NewGuid(), Name = "طبية" },
				new AssistanceType { Id = Guid.NewGuid(), Name = "غذائية" },
				new AssistanceType { Id = Guid.NewGuid(), Name = "بيطرية" },
				new AssistanceType { Id = Guid.NewGuid(), Name = "تعليمية" },
				new AssistanceType { Id = Guid.NewGuid(), Name = "مالية" },
				new AssistanceType { Id = Guid.NewGuid(), Name = "سكنية" },
				new AssistanceType { Id = Guid.NewGuid(), Name = "بيئية" },
				new AssistanceType { Id = Guid.NewGuid(), Name = "ذوي الاحتياجات الخاصة" },
				new AssistanceType { Id = Guid.NewGuid(), Name = "طارئة وإغاثية" }
			);
		}
	}
}
