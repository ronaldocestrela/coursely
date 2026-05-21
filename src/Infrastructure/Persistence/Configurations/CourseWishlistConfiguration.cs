using Domain.Courses;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class CourseWishlistConfiguration : IEntityTypeConfiguration<CourseWishlist>
{
    public void Configure(EntityTypeBuilder<CourseWishlist> builder)
    {
        builder.ToTable("CourseWishlists");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).HasMaxLength(300).IsRequired();

        builder.Property(e => e.Description).HasMaxLength(4000);

        builder.Property(e => e.PurchaseLink).HasMaxLength(2048);

        builder.Property(e => e.ThumbnailUrl).HasMaxLength(2048);

        builder.Property(e => e.Category).HasMaxLength(200);

        builder.Property(e => e.Visibility).HasConversion<int>().IsRequired();

        builder.Property(e => e.CreatedAt).IsRequired();

        builder.Property(e => e.UpdatedAt).IsRequired();

        builder.HasIndex(e => e.UserId);

        builder.HasIndex(e => new { e.UserId, e.CreatedAt });

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
