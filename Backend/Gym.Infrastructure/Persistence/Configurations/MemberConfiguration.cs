using Gym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gym.Infrastructure.Persistence.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("MemberId");
        builder.Property(m => m.Gender).HasMaxLength(20);
        builder.Property(m => m.Phone).HasMaxLength(20);
        builder.Property(m => m.EmergencyContact).HasMaxLength(200);
        builder.HasIndex(m => new { m.GymId, m.UserId }).IsUnique();

        builder.HasOne(m => m.Gym).WithMany().HasForeignKey(m => m.GymId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.User).WithMany().HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(m => m.Trainer).WithMany().HasForeignKey(m => m.TrainerId).OnDelete(DeleteBehavior.NoAction);
    }
}
