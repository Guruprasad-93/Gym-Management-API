using Gym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gym.Infrastructure.Persistence.Configurations;

public class TrainerConfiguration : IEntityTypeConfiguration<Trainer>
{
    public void Configure(EntityTypeBuilder<Trainer> builder)
    {
        builder.ToTable("Trainers");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("TrainerId");
        builder.Property(t => t.Specialization).HasMaxLength(200);
        builder.Property(t => t.Bio).HasMaxLength(1000);

        builder.HasOne(t => t.Gym).WithMany().HasForeignKey(t => t.GymId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.NoAction);
    }
}
