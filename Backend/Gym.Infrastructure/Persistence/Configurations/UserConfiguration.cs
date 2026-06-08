using Gym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gym.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedNever();

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(User.MaxNameLength);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(User.MaxEmailLength);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.Password)
            .IsRequired()
            .HasMaxLength(User.MaxPasswordLength);

        builder.Property(u => u.GymId)
            .IsRequired(false);

        builder.Property(u => u.CreatedDate)
            .IsRequired();

        builder.HasOne<Domain.Entities.Gym>()
            .WithMany()
            .HasForeignKey(u => u.GymId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
