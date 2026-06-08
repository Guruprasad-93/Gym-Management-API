using Gym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gym.Infrastructure.Persistence.Configurations;

public class RolePrivilegeConfiguration : IEntityTypeConfiguration<RolePrivilege>
{
    public void Configure(EntityTypeBuilder<RolePrivilege> builder)
    {
        builder.ToTable("RolePrivileges");
        builder.HasKey(rp => rp.Id);
        builder.Property(rp => rp.Id).HasColumnName("RolePrivilegeId");

        builder.HasIndex(rp => new { rp.RoleId, rp.PrivilegeId }).IsUnique();

        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePrivileges)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Privilege)
            .WithMany(p => p.RolePrivileges)
            .HasForeignKey(rp => rp.PrivilegeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
