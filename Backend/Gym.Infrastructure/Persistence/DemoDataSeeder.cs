using Gym.Application.DTOs.GymAdmins;
using Gym.Application.DTOs.Gyms;
using Gym.Application.DTOs.Members;
using Gym.Application.DTOs.Memberships;
using Gym.Application.DTOs.Payments;
using Gym.Application.DTOs.Trainers;
using Gym.Application.Interfaces;
using Gym.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gym.Infrastructure.Persistence;

/// <summary>
/// Seeds a complete demo tenant for presentations. Idempotent — skips when demo gym already exists.
/// </summary>
public static class DemoDataSeeder
{
    public const string DemoGymName = "FitZone Demo Gym";
    public const string DemoGymAdminEmail = "admin@fitzone-demo.com";
    public const string DefaultDemoPassword = "Demo@123";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        if (!configuration.GetValue("Demo:Enabled", false))
            return;

        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
        var gymRepository = services.GetRequiredService<IGymRepository>();
        var demoPassword = configuration["Demo:Password"] ?? DefaultDemoPassword;

        var existingGyms = await gymRepository.GetAllAsync();
        if (existingGyms.Any(g => string.Equals(g.Name, DemoGymName, StringComparison.OrdinalIgnoreCase)))
        {
            logger.LogInformation("Demo gym '{GymName}' already exists — skipping demo seed.", DemoGymName);
            return;
        }

        await EnsureSuperAdminAsync(services, configuration, logger);

        var gymId = Guid.NewGuid();
        await gymRepository.CreateAsync(gymId, new CreateGymDto
        {
            Name = DemoGymName,
            Address = "123 Fitness Avenue, Demo City",
            Phone = "+1-555-0100",
            Email = "contact@fitzone-demo.com"
        });

        var gymAdminRepository = services.GetRequiredService<IGymAdminRepository>();
        if (!await services.GetRequiredService<IUserRepository>().ExistsByEmailAsync(DemoGymAdminEmail))
        {
            await gymAdminRepository.CreateAsync(
                Guid.NewGuid(),
                gymId,
                "Demo Gym Admin",
                DemoGymAdminEmail,
                services.GetRequiredService<IPasswordHasher>().Hash(demoPassword),
                mustChangePassword: false);
        }

        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        var userRepository = services.GetRequiredService<IUserRepository>();
        var userRoleRepository = services.GetRequiredService<IUserRoleRepository>();
        var roleRepository = services.GetRequiredService<IRoleRepository>();
        var trainerRepository = services.GetRequiredService<ITrainerRepository>();
        var memberRepository = services.GetRequiredService<IMemberRepository>();
        var planRepository = services.GetRequiredService<IMembershipPlanRepository>();
        var membershipRepository = services.GetRequiredService<IMembershipRepository>();
        var paymentRepository = services.GetRequiredService<IPaymentRepository>();

        var trainerRole = await roleRepository.GetByNameAsync("Trainer");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var trainer1 = await CreateTrainerAsync(
            trainerRepository, userRepository, userRoleRepository, trainerRole,
            passwordHasher, gymId, "Alex Rivera", "trainer1@fitzone-demo.com", demoPassword,
            "Strength & Conditioning", "Certified strength coach with 8 years of experience.");

        var trainer2 = await CreateTrainerAsync(
            trainerRepository, userRepository, userRoleRepository, trainerRole,
            passwordHasher, gymId, "Sam Patel", "trainer2@fitzone-demo.com", demoPassword,
            "Cardio & HIIT", "Specializes in fat loss and endurance programs.");

        var monthlyPlan = await planRepository.CreateAsync(gymId, new CreateMembershipPlanDto
        {
            PlanName = "Monthly Basic",
            DurationInMonths = 1,
            Price = 999m,
            Description = "Full gym access for 1 month"
        });

        var quarterlyPlan = await planRepository.CreateAsync(gymId, new CreateMembershipPlanDto
        {
            PlanName = "Quarterly Pro",
            DurationInMonths = 3,
            Price = 2499m,
            Description = "Full gym access for 3 months — best value"
        });

        var annualPlan = await planRepository.CreateAsync(gymId, new CreateMembershipPlanDto
        {
            PlanName = "Annual Elite",
            DurationInMonths = 12,
            Price = 8999m,
            Description = "Premium 12-month membership with priority booking"
        });

        var member1 = await CreateMemberAsync(
            memberRepository, userRepository, userRoleRepository, roleRepository, passwordHasher, gymId, trainer1.Id, today,
            "John Smith", "member1@fitzone-demo.com", demoPassword, "Male", "+1-555-1001");

        var member2 = await CreateMemberAsync(
            memberRepository, userRepository, userRoleRepository, roleRepository, passwordHasher, gymId, trainer1.Id, today,
            "Jane Doe", "member2@fitzone-demo.com", demoPassword, "Female", "+1-555-1002");

        var member3 = await CreateMemberAsync(
            memberRepository, userRepository, userRoleRepository, roleRepository, passwordHasher, gymId, trainer2.Id, today,
            "Mike Wilson", "member3@fitzone-demo.com", demoPassword, "Male", "+1-555-1003");

        var member4 = await CreateMemberAsync(
            memberRepository, userRepository, userRoleRepository, roleRepository, passwordHasher, gymId, trainer2.Id, today,
            "Sarah Brown", "member4@fitzone-demo.com", demoPassword, "Female", "+1-555-1004");

        var member5 = await CreateMemberAsync(
            memberRepository, userRepository, userRoleRepository, roleRepository, passwordHasher, gymId, trainer2.Id, today.AddMonths(-6),
            "Tom Lee", "member5@fitzone-demo.com", demoPassword, "Male", "+1-555-1005");

        var membership1 = await membershipRepository.CreateAsync(gymId, new CreateMembershipDto
        {
            MemberId = member1.Id,
            MembershipPlanId = monthlyPlan.Id,
            StartDate = today.AddDays(-10)
        });

        var membership2 = await membershipRepository.CreateAsync(gymId, new CreateMembershipDto
        {
            MemberId = member2.Id,
            MembershipPlanId = quarterlyPlan.Id,
            StartDate = today.AddDays(-20)
        });

        var membership3 = await membershipRepository.CreateAsync(gymId, new CreateMembershipDto
        {
            MemberId = member3.Id,
            MembershipPlanId = monthlyPlan.Id,
            StartDate = today.AddDays(-5)
        });

        var membership4 = await membershipRepository.CreateAsync(gymId, new CreateMembershipDto
        {
            MemberId = member4.Id,
            MembershipPlanId = annualPlan.Id,
            StartDate = today.AddMonths(-1)
        });

        await membershipRepository.CreateAsync(gymId, new CreateMembershipDto
        {
            MemberId = member5.Id,
            MembershipPlanId = monthlyPlan.Id,
            StartDate = today.AddMonths(-3),
            Notes = "Expired demo membership"
        });

        var paymentDate = DateTime.UtcNow.AddDays(-1);
        await paymentRepository.CreateAsync(gymId, new CreatePaymentDto
        {
            MemberId = member1.Id,
            MembershipId = membership1.Id,
            Amount = monthlyPlan.Price,
            PaymentMethod = "Cash",
            PaymentDate = paymentDate,
            TransactionReference = "DEMO-CASH-001"
        });

        await paymentRepository.CreateAsync(gymId, new CreatePaymentDto
        {
            MemberId = member2.Id,
            MembershipId = membership2.Id,
            Amount = quarterlyPlan.Price,
            PaymentMethod = "UPI",
            PaymentDate = paymentDate,
            TransactionReference = "DEMO-UPI-002"
        });

        await paymentRepository.CreateAsync(gymId, new CreatePaymentDto
        {
            MemberId = member3.Id,
            MembershipId = membership3.Id,
            Amount = monthlyPlan.Price,
            PaymentMethod = "Card",
            PaymentDate = paymentDate,
            TransactionReference = "DEMO-CARD-003"
        });

        await paymentRepository.CreateAsync(gymId, new CreatePaymentDto
        {
            MemberId = member4.Id,
            MembershipId = membership4.Id,
            Amount = annualPlan.Price,
            PaymentMethod = "Bank Transfer",
            PaymentDate = paymentDate,
            TransactionReference = "DEMO-BANK-004"
        });

        var superAdminEmail = configuration["Bootstrap:SuperAdminEmail"] ?? "superadmin@gym.com";
        logger.LogInformation(
            "Demo data seeded for {GymName}. SuperAdmin: {SuperAdminEmail}. GymAdmin: {GymAdminEmail}. Demo password: {DemoPassword}. Trainers: trainer1@fitzone-demo.com, trainer2@fitzone-demo.com. Members: member1@fitzone-demo.com through member5@fitzone-demo.com.",
            DemoGymName,
            superAdminEmail,
            DemoGymAdminEmail,
            demoPassword);
    }

    private static async Task EnsureSuperAdminAsync(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger logger)
    {
        var bootstrapEmail = configuration["Bootstrap:SuperAdminEmail"];
        var bootstrapPassword = configuration["Bootstrap:SuperAdminPassword"];
        if (string.IsNullOrWhiteSpace(bootstrapEmail) || string.IsNullOrWhiteSpace(bootstrapPassword))
            return;

        var userRepository = services.GetRequiredService<IUserRepository>();
        var email = bootstrapEmail.Trim().ToLowerInvariant();
        if (await userRepository.GetByEmailAsync(email) is not null)
            return;

        var roleRepository = services.GetRequiredService<IRoleRepository>();
        var userRoleRepository = services.GetRequiredService<IUserRoleRepository>();
        var superAdminRole = await roleRepository.GetByNameAsync("SuperAdmin")
            ?? throw new InvalidOperationException("SuperAdmin role was not seeded.");

        var user = User.Create(
            "Super Admin",
            email,
            services.GetRequiredService<IPasswordHasher>().Hash(bootstrapPassword));

        await userRepository.AddAsync(user);
        await userRoleRepository.AddAsync(UserRole.Create(user.Id, superAdminRole.Id));
        logger.LogInformation("Bootstrap Super Admin user created for {Email}.", email);
    }

    private static async Task<TrainerDto> CreateTrainerAsync(
        ITrainerRepository trainerRepository,
        IUserRepository userRepository,
        IUserRoleRepository userRoleRepository,
        Role? trainerRole,
        IPasswordHasher passwordHasher,
        Guid gymId,
        string name,
        string email,
        string password,
        string specialization,
        string bio)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (await userRepository.ExistsByEmailAsync(normalizedEmail))
        {
            var existingUser = await userRepository.GetByEmailAsync(normalizedEmail)
                ?? throw new InvalidOperationException($"User {email} exists but could not be loaded.");
            return (await trainerRepository.GetByUserIdAsync(existingUser.Id))
                ?? throw new InvalidOperationException($"Trainer user {email} exists but trainer profile is missing.");
        }

        var user = User.Create(name, normalizedEmail, passwordHasher.Hash(password), gymId);
        await userRepository.AddAsync(user);

        if (trainerRole is not null &&
            await userRoleRepository.GetAsync(user.Id, trainerRole.Id) is null)
        {
            await userRoleRepository.AddAsync(UserRole.Create(user.Id, trainerRole.Id));
        }

        return await trainerRepository.CreateAsync(gymId, new CreateTrainerDto
        {
            UserId = user.Id,
            Specialization = specialization,
            Bio = bio
        });
    }

    private static async Task<MemberResponseDto> CreateMemberAsync(
        IMemberRepository memberRepository,
        IUserRepository userRepository,
        IUserRoleRepository userRoleRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        Guid gymId,
        int trainerId,
        DateOnly joinDate,
        string name,
        string email,
        string password,
        string gender,
        string phone)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (await userRepository.ExistsByEmailAsync(normalizedEmail))
        {
            throw new InvalidOperationException($"Demo member email {email} already exists outside demo seed scope.");
        }

        var user = User.Create(name, normalizedEmail, passwordHasher.Hash(password), gymId);
        await userRepository.AddAsync(user);

        var memberRole = await roleRepository.GetByNameAsync("Member");
        if (memberRole is not null &&
            await userRoleRepository.GetAsync(user.Id, memberRole.Id) is null)
        {
            await userRoleRepository.AddAsync(UserRole.Create(user.Id, memberRole.Id));
        }

        return await memberRepository.CreateAsync(gymId, user.Id, new CreateMemberDto
        {
            TrainerId = trainerId,
            Gender = gender,
            Phone = phone,
            JoinDate = joinDate,
            Address = "Demo City"
        });
    }
}
