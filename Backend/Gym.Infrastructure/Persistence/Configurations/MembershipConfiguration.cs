using Gym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gym.Infrastructure.Persistence.Configurations;

public class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.ToTable("Memberships");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("MembershipId");
        builder.Property(m => m.Status).IsRequired().HasMaxLength(20);

        builder.HasOne(m => m.Gym).WithMany().HasForeignKey(m => m.GymId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(m => m.Member).WithMany().HasForeignKey(m => m.MemberId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.MembershipPlan).WithMany().HasForeignKey(m => m.MembershipPlanId).OnDelete(DeleteBehavior.NoAction);
    }
}
