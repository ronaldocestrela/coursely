using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TokenHash).HasMaxLength(128).IsRequired();

        builder.HasIndex(e => e.TokenHash).IsUnique();

        builder.HasIndex(e => e.UserId);

        builder.HasIndex(e => e.FamilyId);

        builder.HasIndex(e => e.ReplacedByTokenId);

        builder
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(e => e.ReplacedByToken)
            .WithMany()
            .HasForeignKey(e => e.ReplacedByTokenId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
