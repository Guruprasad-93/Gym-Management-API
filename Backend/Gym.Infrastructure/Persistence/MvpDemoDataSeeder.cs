using Gym.Application.DTOs.Attendance;
using Gym.Application.DTOs.Booking;
using Gym.Application.DTOs.Branches;
using Gym.Application.DTOs.DietPlans;
using Gym.Application.DTOs.Financial;
using Gym.Application.DTOs.Gyms;
using Gym.Application.DTOs.Leads;
using Gym.Application.DTOs.Members;
using Gym.Application.DTOs.Memberships;
using Gym.Application.DTOs.Notifications;
using Gym.Application.DTOs.Payments;
using Gym.Application.DTOs.Saas;
using Gym.Application.DTOs.Trainers;
using Gym.Application.DTOs.WorkoutPlans;
using Gym.Application.Interfaces;
using Gym.Application.Options;
using Gym.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gym.Infrastructure.Persistence;

/// <summary>Seeds a production-quality Indian demo tenant: 5 trainers, 100 members, and module data.</summary>
public static class MvpDemoDataSeeder
{
    public const int TrainerCount = 5;
    public const int MemberCount = 100;
    public const int LeadCount = 30;

    public static async Task SeedAsync(
        IServiceProvider services,
        Guid gymId,
        string demoPassword,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var gymRepository = services.GetRequiredService<IGymRepository>();
        var gymMenuService = services.GetRequiredService<IGymMenuService>();
        var gymAdminRepository = services.GetRequiredService<IGymAdminRepository>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        var userRepository = services.GetRequiredService<IUserRepository>();
        var userRoleRepository = services.GetRequiredService<IUserRoleRepository>();
        var roleRepository = services.GetRequiredService<IRoleRepository>();
        var trainerRepository = services.GetRequiredService<ITrainerRepository>();
        var memberRepository = services.GetRequiredService<IMemberRepository>();
        var planRepository = services.GetRequiredService<IMembershipPlanRepository>();
        var membershipRepository = services.GetRequiredService<IMembershipRepository>();
        var paymentRepository = services.GetRequiredService<IPaymentRepository>();
        var attendanceRepository = services.GetRequiredService<IAttendanceRepository>();
        var leadRepository = services.GetRequiredService<ILeadRepository>();
        var branchRepository = services.GetRequiredService<IBranchRepository>();
        var expenseRepository = services.GetRequiredService<IExpenseRepository>();
        var workoutPlanRepository = services.GetRequiredService<IWorkoutPlanRepository>();
        var dietPlanRepository = services.GetRequiredService<IDietPlanRepository>();
        var notificationRepository = services.GetRequiredService<INotificationRepository>();
        var bookingRepository = services.GetRequiredService<IBookingRepository>();
        var saasRepository = services.GetRequiredService<ISaasSubscriptionRepository>();
        var saasSettings = services.GetRequiredService<IOptions<SaasSubscriptionSettings>>().Value;

        var random = new Random(42);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await gymRepository.CreateAsync(gymId, new CreateGymDto
        {
            Name = DemoDataSeeder.DemoGymName,
            Address = "Shop 12, Linking Road, Bandra West, Mumbai, Maharashtra 400050",
            Phone = "+91-22-26451234",
            Email = "contact@fitzonegym.in"
        }, cancellationToken);

        await expenseRepository.SeedCategoriesAsync(gymId, cancellationToken);
        await dietPlanRepository.SeedCategoriesAsync(gymId, cancellationToken);
        await workoutPlanRepository.SeedExerciseCategoriesAsync(gymId, cancellationToken);
        await workoutPlanRepository.SeedExerciseLibraryAsync(gymId, cancellationToken);
        await saasRepository.CreateTrialSubscriptionAsync(gymId, saasSettings.GracePeriodDays, cancellationToken);
        await EnsureDemoEnterpriseSubscriptionAsync(saasRepository, gymId, saasSettings.GracePeriodDays, cancellationToken);
        await saasRepository.SeedNotificationSettingsAsync(gymId, cancellationToken);

        var gymAdminUserId = Guid.NewGuid();
        await gymAdminRepository.CreateAsync(
            gymAdminUserId,
            gymId,
            "Priya Sharma",
            DemoDataSeeder.DemoGymAdminLoginIdentifier,
            "priya.sharma@fitzonegym.in",
            passwordHasher.Hash(demoPassword),
            mustChangePassword: false,
            cancellationToken);

        await gymMenuService.SeedMenusForGymAsync(gymId, gymAdminUserId, cancellationToken);

        var trainerRole = await roleRepository.GetByNameAsync("Trainer", cancellationToken);
        var memberRole = await roleRepository.GetByNameAsync("Member", cancellationToken);
        var trainers = new List<TrainerDto>(TrainerCount);

        for (var i = 1; i <= TrainerCount; i++)
        {
            var profile = DemoIndianDataGenerator.GenerateTrainer(i, random);
            trainers.Add(await CreateTrainerAsync(
                trainerRepository, userRepository, userRoleRepository, trainerRole,
                passwordHasher, gymId, profile, demoPassword, cancellationToken));
        }

        var branchBandra = await branchRepository.CreateAsync(gymId, new CreateBranchDto
        {
            BranchName = "FitZone Bandra",
            BranchCode = "MUM-BR",
            Address = "Shop 12, Linking Road, Bandra West",
            City = "Mumbai",
            Phone = "+91-22-26451234",
            Email = "bandra@fitzonegym.in"
        }, cancellationToken);

        var branchAndheri = await branchRepository.CreateAsync(gymId, new CreateBranchDto
        {
            BranchName = "FitZone Andheri",
            BranchCode = "MUM-AN",
            Address = "Veera Desai Road, Andheri West",
            City = "Mumbai",
            Phone = "+91-22-26781234",
            Email = "andheri@fitzonegym.in"
        }, cancellationToken);

        await SeedDemoClassSchedulesAsync(
            bookingRepository, gymId, branchBandra.BranchId, branchAndheri.BranchId, trainers, cancellationToken);

        var monthlyPlan = await planRepository.CreateAsync(gymId, new CreateMembershipPlanDto
        {
            PlanName = "Monthly Basic",
            DurationInMonths = 1,
            Price = 1499m,
            Description = "Full gym access for 1 month"
        }, cancellationToken);

        var quarterlyPlan = await planRepository.CreateAsync(gymId, new CreateMembershipPlanDto
        {
            PlanName = "Quarterly Pro",
            DurationInMonths = 3,
            Price = 3999m,
            Description = "Full gym access for 3 months"
        }, cancellationToken);

        var annualPlan = await planRepository.CreateAsync(gymId, new CreateMembershipPlanDto
        {
            PlanName = "Annual Elite",
            DurationInMonths = 12,
            Price = 12999m,
            Description = "Premium 12-month membership with priority booking"
        }, cancellationToken);

        var plans = new[] { monthlyPlan, quarterlyPlan, annualPlan };
        var members = new List<MemberResponseDto>(MemberCount);
        var memberships = new List<(MemberResponseDto Member, MembershipResponseDto Membership, MembershipPlanResponseDto Plan)>();

        for (var i = 1; i <= MemberCount; i++)
        {
            var profile = DemoIndianDataGenerator.GenerateMember(i, random);
            var trainer = trainers[(i - 1) % TrainerCount];
            var joinDate = today.AddDays(-(i % 180 + 1));

            var member = await CreateMemberAsync(
                memberRepository, userRepository, userRoleRepository, memberRole,
                passwordHasher, gymId, trainer.Id, joinDate, profile, demoPassword, cancellationToken);
            members.Add(member);

            var plan = plans[i % plans.Length];
            var startDate = joinDate.AddDays(-(i % 20));
            var membership = await membershipRepository.CreateAsync(gymId, new CreateMembershipDto
            {
                MemberId = member.Id,
                MembershipPlanId = plan.Id,
                StartDate = startDate
            }, cancellationToken);
            memberships.Add((member, membership, plan));

            if (i % 17 == 0)
            {
                await branchRepository.TransferMemberAsync(gymId, new TransferMemberBranchDto
                {
                    MemberId = member.Id,
                    ToBranchId = branchAndheri.BranchId,
                    Notes = "Relocated closer to Andheri branch"
                }, gymAdminUserId, cancellationToken);
            }
        }

        var paymentMethods = new[] { "UPI", "Cash", "Card", "Bank Transfer" };
        for (var i = 0; i < memberships.Count; i++)
        {
            if (i % 5 == 4)
                continue;

            var (member, membership, plan) = memberships[i];
            await paymentRepository.CreateAsync(gymId, new CreatePaymentDto
            {
                MemberId = member.Id,
                MembershipId = membership.Id,
                Amount = plan.Price,
                PaymentMethod = paymentMethods[i % paymentMethods.Length],
                PaymentDate = DateTime.UtcNow.AddDays(-(i % 25)),
                TransactionReference = $"FZ-{DateTime.UtcNow:yyyyMM}-{i + 1:D4}"
            }, cancellationToken);
        }

        var statuses = await attendanceRepository.GetStatusesAsync(cancellationToken);
        var presentStatus = statuses.FirstOrDefault(s =>
            s.Code.Contains("Present", StringComparison.OrdinalIgnoreCase)
            || s.Name.Contains("Present", StringComparison.OrdinalIgnoreCase))
            ?? statuses.First();

        for (var day = 0; day < 14; day++)
        {
            var date = today.AddDays(-day);
            if (date.DayOfWeek == DayOfWeek.Sunday)
                continue;

            for (var m = 0; m < 25; m++)
            {
                var member = members[(day * 7 + m) % members.Count];
                var trainer = trainers.FirstOrDefault(t => t.Id == member.TrainerId) ?? trainers[m % trainers.Count];

                await attendanceRepository.MarkMemberAsync(gymId, new MarkAttendanceDto
                {
                    MemberId = member.Id,
                    AttendanceDate = date,
                    AttendanceStatusId = presentStatus.AttendanceStatusId,
                    TrainerId = trainer.Id,
                    Notes = day % 7 == 0 ? "Morning session" : null
                }, gymAdminUserId, cancellationToken);
            }
        }

        var leadStatuses = new[] { "New", "Contacted", "Interested", "TrialScheduled", "Converted", "Lost" };
        for (var i = 1; i <= LeadCount; i++)
        {
            var profile = DemoIndianDataGenerator.GenerateLead(i, random);
            var trainer = trainers[i % trainers.Count];
            await leadRepository.CreateAsync(gymId, gymAdminUserId, new CreateLeadDto
            {
                FullName = profile.FullName,
                MobileNumber = profile.Phone,
                Email = profile.Email,
                Gender = profile.Gender,
                Age = today.Year - profile.DateOfBirth.Year,
                Address = profile.Address,
                LeadSource = DemoIndianDataGenerator.LeadSources[i % DemoIndianDataGenerator.LeadSources.Length],
                InterestedPlanId = plans[i % plans.Length].Id,
                Status = leadStatuses[i % leadStatuses.Length],
                AssignedTrainerId = trainer.Id,
                Notes = "Interested in strength training and flexible timings."
            }, cancellationToken);
        }

        var expenseCategories = await expenseRepository.GetCategoriesAsync(gymId, cancellationToken);
        if (expenseCategories.Count > 0)
        {
            var vendors = new[] { "PowerFit Equipment", "AquaPure Supplies", "CleanPro Services", "MuscleFuel Nutrition" };
            for (var i = 0; i < 12; i++)
            {
                await expenseRepository.CreateAsync(gymId, gymAdminUserId, new CreateExpenseDto
                {
                    CategoryId = expenseCategories[i % expenseCategories.Count].Id,
                    Amount = 2500m + (i * 750),
                    ExpenseDate = today.AddDays(-(i * 3)),
                    Description = "Monthly operational expense",
                    VendorName = vendors[i % vendors.Length],
                    PaymentMethod = paymentMethods[i % paymentMethods.Length]
                }, cancellationToken);
            }
        }

        var exercises = await workoutPlanRepository.GetExercisesAsync(gymId, includeInactive: false, categoryId: null, muscleGroup: null, search: null, cancellationToken);
        var exerciseIds = exercises.Take(6).Select(e => e.ExerciseId).ToList();

        var workoutPlans = new List<int>();
        var workoutTemplates = new[]
        {
            ("Beginner Strength", "Build foundational strength", "Strength"),
            ("Fat Loss HIIT", "High-intensity fat burning", "Weight Loss"),
            ("Muscle Gain Split", "Hypertrophy-focused split", "Muscle Gain")
        };

        for (var p = 0; p < workoutTemplates.Length; p++)
        {
            var (name, desc, goal) = workoutTemplates[p];
            var planExercises = exerciseIds.Select((exId, idx) => new WorkoutPlanExerciseInputDto
            {
                DayNumber = (idx % 3) + 1,
                ExerciseId = exId,
                Sets = 3,
                Reps = "10-12",
                RestSeconds = 60,
                SortOrder = idx + 1
            }).ToList();

            var planId = await workoutPlanRepository.CreatePlanAsync(gymId, new CreateWorkoutPlanDto
            {
                PlanName = name,
                Description = desc,
                Goal = goal,
                DurationWeeks = 8,
                Exercises = planExercises
            }, gymAdminUserId, cancellationToken);
            workoutPlans.Add(planId);
        }

        var dietCategories = await dietPlanRepository.GetCategoriesAsync(gymId, includeInactive: false, cancellationToken);
        var dietCategoryId = dietCategories.FirstOrDefault()?.DietCategoryId;

        var dietPlans = new List<int>();
        var dietTemplates = new[]
        {
            ("Vegetarian Lean", 1800, new[] { ("Breakfast", "Poha with peanuts", "1 bowl", 320m), ("Lunch", "Dal, roti, sabzi", "1 plate", 450m), ("Dinner", "Grilled paneer salad", "1 bowl", 380m) }),
            ("High Protein", 2200, new[] { ("Breakfast", "Egg whites and oats", "1 serving", 350m), ("Lunch", "Chicken brown rice", "1 plate", 520m), ("Dinner", "Fish with vegetables", "1 plate", 420m) }),
            ("South Indian Balanced", 2000, new[] { ("Breakfast", "Idli sambar", "3 pieces", 300m), ("Lunch", "Curd rice with pickle", "1 plate", 480m), ("Dinner", "Ragi dosa", "2 pieces", 360m) })
        };

        for (var p = 0; p < dietTemplates.Length; p++)
        {
            var (name, calories, meals) = dietTemplates[p];
            var items = meals.Select((meal, idx) => new DietPlanItemInputDto
            {
                MealTime = meal.Item1,
                FoodName = meal.Item2,
                Quantity = meal.Item3,
                Calories = meal.Item4,
                SortOrder = idx + 1
            }).ToList();

            var planId = await dietPlanRepository.CreatePlanAsync(gymId, new CreateDietPlanDto
            {
                PlanName = name,
                Description = $"Balanced Indian {name.ToLowerInvariant()} plan",
                DietCategoryId = dietCategoryId,
                TargetCalories = calories,
                Items = items
            }, gymAdminUserId, cancellationToken);
            dietPlans.Add(planId);
        }

        for (var i = 0; i < 20; i++)
        {
            var member = members[i];
            await workoutPlanRepository.AssignToMemberAsync(gymId, new AssignWorkoutPlanDto
            {
                MemberId = member.Id,
                WorkoutPlanId = workoutPlans[i % workoutPlans.Count],
                StartDate = today.AddDays(-14),
                Notes = "Assigned during onboarding"
            }, gymAdminUserId, cancellationToken);

            await dietPlanRepository.AssignToMemberAsync(gymId, new AssignDietPlanDto
            {
                MemberId = member.Id,
                DietPlanId = dietPlans[i % dietPlans.Count],
                StartDate = today.AddDays(-14),
                Notes = "Nutrition plan aligned with fitness goal"
            }, gymAdminUserId, cancellationToken);
        }

        await branchRepository.CreateAnnouncementAsync(gymId, new CreateBranchAnnouncementDto
        {
            BranchId = branchBandra.BranchId,
            Title = "Monsoon Fitness Challenge",
            Message = "Join our 30-day monsoon fitness challenge starting next Monday. Prizes for top performers!",
            TargetAudience = "All",
            ExpiryDate = DateTime.UtcNow.AddMonths(1)
        }, gymAdminUserId, cancellationToken);

        await branchRepository.CreateAnnouncementAsync(gymId, new CreateBranchAnnouncementDto
        {
            BranchId = branchAndheri.BranchId,
            Title = "New Evening Yoga Batch",
            Message = "Evening yoga sessions now available at Andheri branch from 7 PM to 8 PM.",
            TargetAudience = "Members",
            ExpiryDate = DateTime.UtcNow.AddMonths(2)
        }, gymAdminUserId, cancellationToken);

        await notificationRepository.CreateTemplateAsync(gymId, new CreateNotificationTemplateDto
        {
            NotificationType = "MembershipExpiry",
            TemplateName = "membership_expiry_reminder",
            BodyTemplate = "Hi {{member_name}}, your {{plan_name}} membership expires on {{expiry_date}}. Renew at FitZone!",
            IsActive = true
        }, cancellationToken);

        await notificationRepository.LogNotificationAsync(new LogNotificationCommand
        {
            GymId = gymId,
            NotificationType = "MembershipExpiry",
            RecipientPhone = members[0].Phone ?? "+91-9800000001",
            RecipientUserId = members[0].UserId,
            MemberId = members[0].Id,
            WhatsAppTemplateName = "membership_expiry_reminder",
            Status = "Sent",
            SentAt = DateTime.UtcNow.AddDays(-2)
        }, cancellationToken);

        logger.LogInformation(
            "MVP demo seeded: {GymName} with {TrainerCount} trainers, {MemberCount} members, {LeadCount} leads, branches, plans, attendance, payments, and notifications.",
            DemoDataSeeder.DemoGymName,
            TrainerCount,
            MemberCount,
            LeadCount);
    }

    private static async Task SeedDemoClassSchedulesAsync(
        IBookingRepository bookingRepository,
        Guid gymId,
        int branchBandraId,
        int branchAndheriId,
        IReadOnlyList<TrainerDto> trainers,
        CancellationToken cancellationToken)
    {
        if (trainers.Count == 0)
            return;

        var templates = new (string Name, int BranchId, int TrainerIndex, int Day, TimeSpan Start, TimeSpan End, int Capacity)[]
        {
            ("Morning HIIT", branchBandraId, 0, 1, new TimeSpan(7, 0, 0), new TimeSpan(8, 0, 0), 20),
            ("Strength Training", branchBandraId, 1, 3, new TimeSpan(18, 0, 0), new TimeSpan(19, 0, 0), 15),
            ("Yoga Flow", branchAndheriId, 2, 2, new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), 25),
            ("CrossFit", branchAndheriId, 3, 5, new TimeSpan(17, 0, 0), new TimeSpan(18, 30, 0), 18),
            ("Spin Class", branchBandraId, 4, 6, new TimeSpan(8, 0, 0), new TimeSpan(9, 0, 0), 12),
        };

        foreach (var template in templates)
        {
            var trainer = trainers[template.TrainerIndex % trainers.Count];
            await bookingRepository.CreateScheduleAsync(gymId, new CreateClassScheduleDto
            {
                BranchId = template.BranchId,
                ClassName = template.Name,
                Description = $"{template.Name} — demo class schedule",
                TrainerId = trainer.Id,
                DayOfWeek = template.Day,
                StartTime = template.Start,
                EndTime = template.End,
                Capacity = template.Capacity
            }, cancellationToken);
        }
    }

    private static async Task EnsureDemoEnterpriseSubscriptionAsync(
        ISaasSubscriptionRepository saasRepository,
        Guid gymId,
        int gracePeriodDays,
        CancellationToken cancellationToken)
    {
        await EnsureDemoEnterpriseSubscriptionForGymAsync(saasRepository, gymId, gracePeriodDays, cancellationToken);
    }

    public static SaasPlanDto ResolveTopDemoPlan(IReadOnlyList<SaasPlanDto> plans) =>
        plans.FirstOrDefault(p => p.PlanCode.Equals("Enterprise", StringComparison.OrdinalIgnoreCase))
        ?? plans.FirstOrDefault(p => p.PlanCode.Equals("PremiumPro", StringComparison.OrdinalIgnoreCase))
        ?? plans
            .OrderByDescending(p => p.MaxMembers < 0 ? int.MaxValue : p.MaxMembers)
            .ThenByDescending(p => p.Id)
            .First();

    public static async Task EnsureDemoEnterpriseSubscriptionForGymAsync(
        ISaasSubscriptionRepository saasRepository,
        Guid gymId,
        int gracePeriodDays,
        CancellationToken cancellationToken)
    {
        var plans = await saasRepository.GetAllPlansAsync(cancellationToken);
        var topPlan = ResolveTopDemoPlan(plans);

        await saasRepository.UpdateSubscriptionPlanAsync(
            gymId, topPlan.Id, "Yearly", null, 0,
            null, null, null, gracePeriodDays, cancellationToken);
    }

    private static async Task<TrainerDto> CreateTrainerAsync(
        ITrainerRepository trainerRepository,
        IUserRepository userRepository,
        IUserRoleRepository userRoleRepository,
        Role? trainerRole,
        IPasswordHasher passwordHasher,
        Guid gymId,
        DemoIndianDataGenerator.PersonProfile profile,
        string password,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = profile.Email?.Trim().ToLowerInvariant();
        if (await userRepository.ExistsByLoginIdentifierAsync(profile.LoginIdentifier, cancellationToken))
        {
            var existingUser = await userRepository.GetByLoginIdentifierAsync(profile.LoginIdentifier, cancellationToken)
                ?? throw new InvalidOperationException($"Trainer {profile.LoginIdentifier} exists but could not be loaded.");
            return (await trainerRepository.GetByUserIdAsync(existingUser.Id, cancellationToken))
                ?? throw new InvalidOperationException($"Trainer profile missing for {profile.LoginIdentifier}.");
        }

        var user = User.Create(profile.FullName, profile.LoginIdentifier, passwordHasher.Hash(password), gymId, normalizedEmail);
        await userRepository.AddAsync(user, cancellationToken);

        if (trainerRole is not null &&
            await userRoleRepository.GetAsync(user.Id, trainerRole.Id, cancellationToken) is null)
        {
            await userRoleRepository.AddAsync(UserRole.Create(user.Id, trainerRole.Id), cancellationToken);
        }

        return await trainerRepository.CreateAsync(gymId, new CreateTrainerDto
        {
            UserId = user.Id,
            Specialization = profile.Specialization ?? "General Fitness",
            Bio = profile.Bio ?? string.Empty
        }, cancellationToken);
    }

    private static async Task<MemberResponseDto> CreateMemberAsync(
        IMemberRepository memberRepository,
        IUserRepository userRepository,
        IUserRoleRepository userRoleRepository,
        Role? memberRole,
        IPasswordHasher passwordHasher,
        Guid gymId,
        int trainerId,
        DateOnly joinDate,
        DemoIndianDataGenerator.PersonProfile profile,
        string password,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = profile.Email?.Trim().ToLowerInvariant();
        if (await userRepository.ExistsByLoginIdentifierAsync(profile.LoginIdentifier, cancellationToken))
            throw new InvalidOperationException($"Member login {profile.LoginIdentifier} already exists.");

        var user = User.Create(profile.FullName, profile.LoginIdentifier, passwordHasher.Hash(password), gymId, normalizedEmail);
        await userRepository.AddAsync(user, cancellationToken);

        if (memberRole is not null &&
            await userRoleRepository.GetAsync(user.Id, memberRole.Id, cancellationToken) is null)
        {
            await userRoleRepository.AddAsync(UserRole.Create(user.Id, memberRole.Id), cancellationToken);
        }

        return await memberRepository.CreateAsync(gymId, user.Id, new CreateMemberDto
        {
            TrainerId = trainerId,
            Gender = profile.Gender,
            Phone = profile.Phone,
            DateOfBirth = profile.DateOfBirth,
            Address = profile.Address,
            EmergencyContact = profile.EmergencyContact,
            Height = 155 + (profile.FullName.GetHashCode() % 35),
            Weight = 52 + (profile.FullName.GetHashCode() % 40),
            JoinDate = joinDate
        }, cancellationToken);
    }
}
