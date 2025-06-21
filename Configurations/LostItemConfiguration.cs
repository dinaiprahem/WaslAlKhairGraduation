using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Data.Configurations
{
    public class LostItemConfiguration : IEntityTypeConfiguration<LostItem>
    {
        public void Configure(EntityTypeBuilder<LostItem> builder)
        {
            builder.ToTable("LostItems");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ImagePath)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Embedding)
                .HasColumnType("nvarchar(max)"); // تخزين كـ JSON string

            builder.Property(x => x.Metadata)
                .HasMaxLength(1000);

            builder.Property(x => x.Location)
                .HasMaxLength(300);

            builder.Property(x => x.ContactInfo)
                .HasMaxLength(200);

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.Date);

            builder.Property(x => x.IsResolved)
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // الربط مع المستخدم
            builder.HasOne(x => x.User)
                .WithMany(u => u.LostItems)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
