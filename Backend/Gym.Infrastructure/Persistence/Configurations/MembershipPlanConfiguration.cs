using Gym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gym.Infrastructure.Persistence.Configurations;

public class MembershipPlanConfiguration : IEntityTypeConfiguration<MembershipPlan>
{
    public void Configure(EntityTypeBuilder<MembershipPlan> builder)
    {
        builder.ToTable("MembershipPlans");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("MembershipPlanId");
        builder.Property(p => p.PlanName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.Price).HasPrecision(18, 2);

        builder.HasOne(p => p.Gym)
            .WithMany()
            .HasForeignKey(p => p.GymId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
