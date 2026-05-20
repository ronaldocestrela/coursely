using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.Name).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Bio).HasMaxLength(2000);
        builder.Property(u => u.AvatarUrl).HasMaxLength(2048);
    }
}
