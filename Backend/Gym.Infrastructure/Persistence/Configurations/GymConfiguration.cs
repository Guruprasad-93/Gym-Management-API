using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GymEntity = Gym.Domain.Entities.Gym;

namespace Gym.Infrastructure.Persistence.Configurations;

public class GymConfiguration : IEntityTypeConfiguration<GymEntity>
{
    public void Configure(EntityTypeBuilder<GymEntity> builder)
    {
        builder.ToTable("Gyms");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).HasColumnName("GymId");
        builder.Property(g => g.Name).IsRequired().HasMaxLength(200);
        builder.Property(g => g.Address).HasMaxLength(500);
        builder.Property(g => g.Phone).HasMaxLength(20);
        builder.Property(g => g.Email).HasMaxLength(256);
        builder.Property(g => g.LogoUrl).HasMaxLength(500);
    }
}
