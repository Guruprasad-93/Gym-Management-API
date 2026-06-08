using Gym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gym.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("PaymentId");
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.PaymentMethod).IsRequired().HasMaxLength(50);
        builder.Property(p => p.TransactionReference).HasMaxLength(100);
        builder.Property(p => p.Status).IsRequired().HasMaxLength(20);
        builder.Property(p => p.Notes).HasMaxLength(500);

        builder.HasOne(p => p.Gym).WithMany().HasForeignKey(p => p.GymId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(p => p.Member).WithMany().HasForeignKey(p => p.MemberId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(p => p.Membership).WithMany().HasForeignKey(p => p.MembershipId).OnDelete(DeleteBehavior.NoAction);
    }
}
