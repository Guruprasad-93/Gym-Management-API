using Gym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gym.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("RoleId");

        builder.Property(r => r.RoleName)
            .IsRequired()
            .HasMaxLength(Role.MaxNameLength);

        builder.HasIndex(r => r.RoleName).IsUnique();

        builder.Property(r => r.Description)
            .HasMaxLength(Role.MaxDescriptionLength);

        builder.Property(r => r.IsSystemRole).IsRequired();
        builder.Property(r => r.CreatedDate).IsRequired();
    }
}
