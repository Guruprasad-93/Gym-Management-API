using Gym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gym.Infrastructure.Persistence.Configurations;

public class PrivilegeConfiguration : IEntityTypeConfiguration<Privilege>
{
    public void Configure(EntityTypeBuilder<Privilege> builder)
    {
        builder.ToTable("Privileges");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("PrivilegeId");

        builder.Property(p => p.PrivilegeName)
            .IsRequired()
            .HasMaxLength(Privilege.MaxNameLength);

        builder.HasIndex(p => p.PrivilegeName).IsUnique();

        builder.Property(p => p.Description)
            .HasMaxLength(Privilege.MaxDescriptionLength);

        builder.Property(p => p.Category)
            .IsRequired()
            .HasMaxLength(Privilege.MaxCategoryLength);

        builder.Property(p => p.CreatedDate).IsRequired();
    }
}
