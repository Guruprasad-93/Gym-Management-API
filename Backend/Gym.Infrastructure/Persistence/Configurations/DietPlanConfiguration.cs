using Gym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gym.Infrastructure.Persistence.Configurations;

public class DietPlanConfiguration : IEntityTypeConfiguration<DietPlan>
{
    public void Configure(EntityTypeBuilder<DietPlan> builder)
    {
        builder.ToTable("DietPlans");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("DietPlanId");
        builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(2000);

        builder.HasOne(p => p.Gym).WithMany().HasForeignKey(p => p.GymId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(p => p.Member).WithMany().HasForeignKey(p => p.MemberId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.Trainer).WithMany().HasForeignKey(p => p.TrainerId).OnDelete(DeleteBehavior.NoAction);
    }
}
