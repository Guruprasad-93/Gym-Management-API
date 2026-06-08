using Gym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using GymEntity = Gym.Domain.Entities.Gym;

namespace Gym.Infrastructure.Persistence;

/// <summary>
/// EF Core context used ONLY for schema migrations and design-time tooling.
/// Business read/write operations MUST use <see cref="StoredProcedures.StoredProcedureExecutor"/> via repositories.
/// Do not call SaveChanges, Add, Update, or Remove on these DbSets in application code.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Privilege> Privileges => Set<Privilege>();
    public DbSet<RolePrivilege> RolePrivileges => Set<RolePrivilege>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<GymEntity> Gyms => Set<GymEntity>();
    public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<DietPlan> DietPlans => Set<DietPlan>();
    public DbSet<WorkoutPlan> WorkoutPlans => Set<WorkoutPlan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
