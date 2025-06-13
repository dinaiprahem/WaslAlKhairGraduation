using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


public class UserReviewConfiguration : IEntityTypeConfiguration<UserReview>
{
    public void Configure(EntityTypeBuilder<UserReview> builder)
    {
        // Primary key
        builder.HasKey(r => r.Id);



        // Required relationships
        builder.HasOne(r => r.Reviewer)
               .WithMany(u => u.ReviewsGiven)
               .HasForeignKey(r => r.ReviewerId)
               .OnDelete(DeleteBehavior.Restrict); // مهم علشان ما يحذفش الريفيوهات مع حذف اليوزر

        builder.HasOne(r => r.TargetUser)
               .WithMany(u => u.ReviewsReceived)
               .HasForeignKey(r => r.TargetUserId)
               .OnDelete(DeleteBehavior.Restrict);



        // Unique constraint to prevent duplicate reviews
        builder.HasIndex(r => new { r.ReviewerId, r.TargetUserId }).IsUnique();

        // Rating: required + range enforced in validation
        builder.Property(r => r.Rating)
               .IsRequired();

        // Comment: optional but limited in length
        builder.Property(r => r.Comment)
               .HasMaxLength(500)
               .IsRequired();

        // CreatedAt: default value
        builder.Property(r => r.CreatedAt)
               .HasDefaultValueSql("GETUTCDATE()");
    }
}
