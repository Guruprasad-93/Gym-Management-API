using Gym.Application.Interfaces;
using Gym.Application.Validation;
using Gym.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gym.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await EnsureMissingPrivilegesAsync(scope.ServiceProvider);
        await EnsureTrainerRoleAndGymAdminPrivilegesAsync(scope.ServiceProvider);
        await EnsureMemberRoleAndPrivilegesAsync(scope.ServiceProvider);
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
        var privilegeRepository = scope.ServiceProvider.GetRequiredService<IPrivilegeRepository>();
        var rolePrivilegeRepository = scope.ServiceProvider.GetRequiredService<IRolePrivilegeRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var userRoleRepository = scope.ServiceProvider.GetRequiredService<IUserRoleRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        await EnsureCoreAuthorizationSeedAsync(
            roleRepository, privilegeRepository, rolePrivilegeRepository, logger);

        var bootstrapEmail = configuration["Bootstrap:SuperAdminEmail"];
        var bootstrapPassword = configuration["Bootstrap:SuperAdminPassword"];

        if (!string.IsNullOrWhiteSpace(bootstrapEmail) && !string.IsNullOrWhiteSpace(bootstrapPassword))
        {
            var email = bootstrapEmail.Trim().ToLowerInvariant();
            if (await userRepository.GetByEmailAsync(email) is null)
            {
                var superAdminRoleEntity = await roleRepository.GetByNameAsync("SuperAdmin")
                    ?? throw new InvalidOperationException("SuperAdmin role was not seeded.");

                var loginIdentifier = LoginIdentifierRules.FromEmailLocalPart(email);
                if (string.IsNullOrWhiteSpace(loginIdentifier))
                    loginIdentifier = "superadmin";

                var user = User.Create(
                    "Super Admin",
                    loginIdentifier,
                    passwordHasher.Hash(bootstrapPassword),
                    gymId: null,
                    email: email);

                await userRepository.AddAsync(user);
                await userRoleRepository.AddAsync(UserRole.Create(user.Id, superAdminRoleEntity.Id));

                logger.LogInformation("Bootstrap Super Admin user created for {Email}.", email);
            }
        }

        await DemoDataSeeder.SeedAsync(serviceProvider);
        await GymSubscriptionSeeder.EnsureAllGymsHaveAccessAsync(serviceProvider);
    }

    private static async Task EnsureCoreAuthorizationSeedAsync(
        IRoleRepository roleRepository,
        IPrivilegeRepository privilegeRepository,
        IRolePrivilegeRepository rolePrivilegeRepository,
        ILogger logger)
    {
        var superAdminRole = await roleRepository.GetByNameAsync("SuperAdmin");
        if (superAdminRole is null)
        {
            superAdminRole = await roleRepository.AddAsync(
                Role.Create(
                    "SuperAdmin",
                    "Platform super administrator with full access",
                    isSystemRole: true));

            foreach (var definition in GetBootstrapPrivilegeDefinitions())
            {
                var privilege = await privilegeRepository.AddAsync(
                    Privilege.Create(definition.Name, definition.Description, definition.Category));

                await rolePrivilegeRepository.AddAsync(
                    RolePrivilege.Create(superAdminRole.Id, privilege.Id));
            }

            logger.LogInformation("Seeded SuperAdmin role with privileges.");
        }

        var gymAdminRole = await roleRepository.GetByNameAsync("GymAdmin");
        if (gymAdminRole is null)
        {
            gymAdminRole = await roleRepository.AddAsync(
                Role.Create("GymAdmin", "Gym administrator", isSystemRole: true));

            var gymAdminPrivileges = new[]
            {
                "VIEW_TRAINERS", "CREATE_TRAINER", "UPDATE_TRAINER", "DELETE_TRAINER", "ASSIGN_MEMBER_TO_TRAINER",
                "VIEW_MEMBERS", "CREATE_MEMBER", "UPDATE_MEMBER", "DELETE_MEMBER", "ASSIGN_TRAINER", "VIEW_MEMBER_DETAILS",
                "VIEW_MEMBERSHIPS", "CREATE_MEMBERSHIP", "UPDATE_MEMBERSHIP", "RENEW_MEMBERSHIP",
                "VIEW_PAYMENTS", "CREATE_PAYMENT", "VIEW_REVENUE", "DOWNLOAD_INVOICE",
                "INITIATE_ONLINE_PAYMENT", "REFUND_PAYMENT",
                "VIEW_NOTIFICATIONS", "MANAGE_NOTIFICATIONS", "SEND_NOTIFICATIONS",
                "VIEW_DASHBOARD",
                "VIEW_ANALYTICS", "VIEW_REVENUE_ANALYTICS", "VIEW_MEMBER_ANALYTICS",
                "VIEW_SAAS_SUBSCRIPTION", "MANAGE_SAAS_SUBSCRIPTION", "MANAGE_GYM_BRANDING",
                "VIEW_LEADS", "MANAGE_LEADS", "CONVERT_LEADS", "VIEW_LEAD_ANALYTICS",
                "VIEW_EXPENSES", "MANAGE_EXPENSES", "VIEW_PAYROLL", "MANAGE_PAYROLL", "VIEW_FINANCIAL_ANALYTICS",
                "VIEW_ATTENDANCE", "MANAGE_ATTENDANCE", "VIEW_TRAINER_ATTENDANCE", "MANAGE_TRAINER_ATTENDANCE", "EXPORT_ATTENDANCE_REPORTS",
                "VIEW_AUDIT_LOGS", "EXPORT_AUDIT_LOGS",
                "VIEW_DIET_PLANS", "MANAGE_DIET_PLANS", "ASSIGN_DIET_PLAN", "VIEW_MEMBER_DIET", "EXPORT_DIET_PLANS",
                "VIEW_WORKOUT_PLANS", "MANAGE_WORKOUT_PLANS", "ASSIGN_WORKOUT_PLAN", "VIEW_MEMBER_WORKOUT", "EXPORT_WORKOUT_PLANS",
                "VIEW_FILES", "UPLOAD_FILES", "DELETE_FILES", "MANAGE_FILES",
                "VIEW_BRANCHES", "MANAGE_BRANCHES", "VIEW_BRANCH_ANALYTICS", "TRANSFER_MEMBERS", "TRANSFER_TRAINERS",
                "VIEW_AI_INSIGHTS", "VIEW_AI_RECOMMENDATIONS",
                "VIEW_BOOKINGS", "MANAGE_BOOKINGS", "MANAGE_SCHEDULES", "VIEW_BOOKING_ANALYTICS",
                "VIEW_WEBSITE_BUILDER", "MANAGE_WEBSITE_BUILDER", "VIEW_WEBSITE_ANALYTICS",
                "VIEW_WHITE_LABEL", "MANAGE_WHITE_LABEL"
            };

            var allPrivileges = await privilegeRepository.GetAllAsync();
            foreach (var privilege in allPrivileges.Where(p => gymAdminPrivileges.Contains(p.PrivilegeName)))
            {
                await rolePrivilegeRepository.AddAsync(
                    RolePrivilege.Create(gymAdminRole.Id, privilege.Id));
            }

            logger.LogInformation("Seeded GymAdmin role with privileges.");
        }
    }

    private static async Task EnsureMissingPrivilegesAsync(IServiceProvider serviceProvider)
    {
        var roleRepository = serviceProvider.GetRequiredService<IRoleRepository>();
        var privilegeRepository = serviceProvider.GetRequiredService<IPrivilegeRepository>();
        var rolePrivilegeRepository = serviceProvider.GetRequiredService<IRolePrivilegeRepository>();
        var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        var superAdminRole = await roleRepository.GetByNameAsync("SuperAdmin");
        if (superAdminRole is null)
            return;

        foreach (var definition in GetBootstrapPrivilegeDefinitions())
        {
            var existing = await privilegeRepository.GetByNameAsync(definition.Name);
            if (existing is not null)
                continue;

            var privilege = await privilegeRepository.AddAsync(
                Privilege.Create(definition.Name, definition.Description, definition.Category));

            await rolePrivilegeRepository.AddAsync(
                RolePrivilege.Create(superAdminRole.Id, privilege.Id));

            logger.LogInformation("Added missing privilege {Privilege} for SuperAdmin.", definition.Name);

            await EnsureGymAdminHasPrivilegeAsync(
                roleRepository, privilegeRepository, rolePrivilegeRepository, definition.Name, logger);
        }
    }

    private static async Task EnsureTrainerRoleAndGymAdminPrivilegesAsync(IServiceProvider serviceProvider)
    {
        var roleRepository = serviceProvider.GetRequiredService<IRoleRepository>();
        var privilegeRepository = serviceProvider.GetRequiredService<IPrivilegeRepository>();
        var rolePrivilegeRepository = serviceProvider.GetRequiredService<IRolePrivilegeRepository>();
        var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        var trainerRole = await roleRepository.GetByNameAsync("Trainer");
        if (trainerRole is null)
        {
            trainerRole = await roleRepository.AddAsync(
                Role.Create("Trainer", "Gym trainer with access to assigned members", isSystemRole: true));
            logger.LogInformation("Seeded Trainer role.");
        }

        foreach (var name in new[] { "VIEW_ANALYTICS", "VIEW_MEMBER_ANALYTICS",
            "VIEW_MEMBERS", "VIEW_MEMBER_DETAILS", "VIEW_DASHBOARD", "VIEW_TRAINERS", "VIEW_LEADS",
            "VIEW_ATTENDANCE", "MANAGE_ATTENDANCE", "VIEW_TRAINER_ATTENDANCE", "MANAGE_TRAINER_ATTENDANCE",
            "VIEW_AUDIT_LOGS",
            "VIEW_DIET_PLANS", "VIEW_MEMBER_DIET", "ASSIGN_DIET_PLAN",
            "VIEW_WORKOUT_PLANS", "VIEW_MEMBER_WORKOUT", "ASSIGN_WORKOUT_PLAN",
            "VIEW_FILES", "UPLOAD_FILES", "VIEW_AI_RECOMMENDATIONS",
            "VIEW_BOOKINGS", "MANAGE_BOOKINGS" })
            await EnsureTrainerHasPrivilegeAsync(roleRepository, privilegeRepository, rolePrivilegeRepository, name, logger);

        foreach (var name in new[] { "ASSIGN_MEMBER_TO_TRAINER", "ASSIGN_TRAINER", "VIEW_MEMBER_DETAILS", "UPDATE_MEMBERSHIP", "RENEW_MEMBERSHIP", "VIEW_REVENUE", "DOWNLOAD_INVOICE",
            "VIEW_LEADS", "MANAGE_LEADS", "CONVERT_LEADS", "VIEW_LEAD_ANALYTICS",
            "VIEW_EXPENSES", "MANAGE_EXPENSES", "VIEW_PAYROLL", "MANAGE_PAYROLL", "VIEW_FINANCIAL_ANALYTICS",
            "VIEW_ATTENDANCE", "MANAGE_ATTENDANCE", "VIEW_TRAINER_ATTENDANCE", "MANAGE_TRAINER_ATTENDANCE", "EXPORT_ATTENDANCE_REPORTS",
            "VIEW_BRANCHES", "MANAGE_BRANCHES", "VIEW_BRANCH_ANALYTICS", "TRANSFER_MEMBERS", "TRANSFER_TRAINERS",
            "VIEW_AI_INSIGHTS", "VIEW_AI_RECOMMENDATIONS",
            "VIEW_BOOKINGS", "MANAGE_BOOKINGS", "MANAGE_SCHEDULES", "VIEW_BOOKING_ANALYTICS",
            "VIEW_WEBSITE_BUILDER", "MANAGE_WEBSITE_BUILDER", "VIEW_WEBSITE_ANALYTICS",
            "VIEW_WHITE_LABEL", "MANAGE_WHITE_LABEL" })
            await EnsureGymAdminHasPrivilegeAsync(roleRepository, privilegeRepository, rolePrivilegeRepository, name, logger);
    }

    private static async Task EnsureMemberRoleAndPrivilegesAsync(IServiceProvider serviceProvider)
    {
        var roleRepository = serviceProvider.GetRequiredService<IRoleRepository>();
        var privilegeRepository = serviceProvider.GetRequiredService<IPrivilegeRepository>();
        var rolePrivilegeRepository = serviceProvider.GetRequiredService<IRolePrivilegeRepository>();
        var userRoleRepository = serviceProvider.GetRequiredService<IUserRoleRepository>();
        var memberRepository = serviceProvider.GetRequiredService<IMemberRepository>();
        var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        var memberRole = await roleRepository.GetByNameAsync("Member");
        if (memberRole is null)
        {
            memberRole = await roleRepository.AddAsync(
                Role.Create("Member", "Gym member with access to own profile", isSystemRole: true));
            logger.LogInformation("Seeded Member role.");
        }

        var memberPrivileges = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "VIEW_MEMBER_DETAILS", "VIEW_DASHBOARD", "VIEW_MEMBER_DIET", "VIEW_MEMBER_WORKOUT",
            "INITIATE_ONLINE_PAYMENT", "DOWNLOAD_INVOICE",
            "VIEW_MEMBER_DASHBOARD", "MANAGE_MEMBER_GOALS", "TRACK_MEMBER_PROGRESS", "SUBMIT_MEMBER_FEEDBACK",
            "VIEW_MOBILE_NOTIFICATIONS", "MANAGE_NOTIFICATION_PREFERENCES", "VIEW_BOOKINGS"
        };

        foreach (var name in memberPrivileges)
            await EnsureMemberHasPrivilegeAsync(roleRepository, privilegeRepository, rolePrivilegeRepository, name, logger);

        await RevokeMemberPrivilegesNotInListAsync(
            roleRepository, privilegeRepository, rolePrivilegeRepository, memberPrivileges, logger);

        var members = await memberRepository.GetPagedAsync(
            null,
            null,
            null,
            true,
            new Application.DTOs.Common.PagedRequestDto
            {
                PageNumber = 1,
                PageSize = 500,
                SortColumn = "FullName"
            });
        foreach (var member in members.Items)
        {
            if (await userRoleRepository.GetAsync(member.UserId, memberRole.Id) is not null)
                continue;

            await userRoleRepository.AddAsync(UserRole.Create(member.UserId, memberRole.Id));
            logger.LogInformation("Assigned Member role to user for member {MemberId}.", member.Id);
        }
    }

    private static async Task EnsureMemberHasPrivilegeAsync(
        IRoleRepository roleRepository,
        IPrivilegeRepository privilegeRepository,
        IRolePrivilegeRepository rolePrivilegeRepository,
        string privilegeName,
        ILogger logger)
    {
        var memberRole = await roleRepository.GetByNameAsync("Member");
        var privilege = await privilegeRepository.GetByNameAsync(privilegeName);
        if (memberRole is null || privilege is null)
            return;

        if (await rolePrivilegeRepository.GetAsync(memberRole.Id, privilege.Id) is not null)
            return;

        await rolePrivilegeRepository.AddAsync(RolePrivilege.Create(memberRole.Id, privilege.Id));
        logger.LogInformation("Assigned privilege {Privilege} to Member role.", privilegeName);
    }

    private static async Task RevokeMemberPrivilegesNotInListAsync(
        IRoleRepository roleRepository,
        IPrivilegeRepository privilegeRepository,
        IRolePrivilegeRepository rolePrivilegeRepository,
        IReadOnlySet<string> allowedPrivileges,
        ILogger logger)
    {
        var memberRole = await roleRepository.GetByNameAsync("Member");
        if (memberRole is null)
            return;

        var assigned = await rolePrivilegeRepository.GetByRoleIdAsync(memberRole.Id);
        foreach (var rolePrivilege in assigned)
        {
            if (allowedPrivileges.Contains(rolePrivilege.Privilege.PrivilegeName))
                continue;

            await rolePrivilegeRepository.RemoveAsync(memberRole.Id, rolePrivilege.PrivilegeId);
            logger.LogInformation(
                "Revoked privilege {Privilege} from Member role.",
                rolePrivilege.Privilege.PrivilegeName);
        }
    }

    private static async Task EnsureTrainerHasPrivilegeAsync(
        IRoleRepository roleRepository,
        IPrivilegeRepository privilegeRepository,
        IRolePrivilegeRepository rolePrivilegeRepository,
        string privilegeName,
        ILogger logger)
    {
        var trainerRole = await roleRepository.GetByNameAsync("Trainer");
        var privilege = await privilegeRepository.GetByNameAsync(privilegeName);
        if (trainerRole is null || privilege is null)
            return;

        if (await rolePrivilegeRepository.GetAsync(trainerRole.Id, privilege.Id) is not null)
            return;

        await rolePrivilegeRepository.AddAsync(RolePrivilege.Create(trainerRole.Id, privilege.Id));
        logger.LogInformation("Assigned privilege {Privilege} to Trainer role.", privilegeName);
    }

    private static async Task EnsureGymAdminHasPrivilegeAsync(
        IRoleRepository roleRepository,
        IPrivilegeRepository privilegeRepository,
        IRolePrivilegeRepository rolePrivilegeRepository,
        string privilegeName,
        ILogger logger)
    {
        var gymAdminRole = await roleRepository.GetByNameAsync("GymAdmin");
        var privilege = await privilegeRepository.GetByNameAsync(privilegeName);
        if (gymAdminRole is null || privilege is null)
            return;

        if (await rolePrivilegeRepository.GetAsync(gymAdminRole.Id, privilege.Id) is not null)
            return;

        await rolePrivilegeRepository.AddAsync(RolePrivilege.Create(gymAdminRole.Id, privilege.Id));
        logger.LogInformation("Assigned privilege {Privilege} to GymAdmin role.", privilegeName);
    }

    private static IEnumerable<(string Name, string Category, string Description)> GetBootstrapPrivilegeDefinitions()
    {
        yield return ("VIEW_ROLES", "Authorization", "View all roles");
        yield return ("CREATE_ROLE", "Authorization", "Create roles");
        yield return ("UPDATE_ROLE", "Authorization", "Update roles");
        yield return ("DELETE_ROLE", "Authorization", "Delete roles");
        yield return ("VIEW_PRIVILEGES", "Authorization", "View all privileges");
        yield return ("CREATE_PRIVILEGE", "Authorization", "Create privileges");
        yield return ("UPDATE_PRIVILEGE", "Authorization", "Update privileges");
        yield return ("DELETE_PRIVILEGE", "Authorization", "Delete privileges");
        yield return ("ASSIGN_ROLE_PRIVILEGE", "Authorization", "Assign privileges to roles");
        yield return ("REMOVE_ROLE_PRIVILEGE", "Authorization", "Remove privileges from roles");
        yield return ("VIEW_ROLE_PRIVILEGES", "Authorization", "View role privileges");
        yield return ("VIEW_PERMISSION_MATRIX", "Authorization", "View role-permission matrix");
        yield return ("ASSIGN_USER_ROLE", "Authorization", "Assign roles to users");
        yield return ("REMOVE_USER_ROLE", "Authorization", "Remove roles from users");
        yield return ("VIEW_USER_ROLES", "Authorization", "View user roles");

        yield return ("VIEW_GYMS", "Gym", "View gyms");
        yield return ("CREATE_GYM", "Gym", "Create gyms");
        yield return ("UPDATE_GYM", "Gym", "Update gyms");
        yield return ("DELETE_GYM", "Gym", "Delete gyms");
        yield return ("ACTIVATE_GYM", "Gym", "Activate gyms");
        yield return ("DEACTIVATE_GYM", "Gym", "Deactivate gyms");
        yield return ("VIEW_GYM_ADMINS", "Gym", "View gym administrators");
        yield return ("CREATE_GYM_ADMIN", "Gym", "Create gym administrators");
        yield return ("UPDATE_GYM_ADMIN", "Gym", "Update gym administrators");
        yield return ("DELETE_GYM_ADMIN", "Gym", "Deactivate gym administrators");
        yield return ("RESET_GYM_ADMIN_PASSWORD", "Gym", "Reset gym administrator passwords");

        yield return ("VIEW_TRAINERS", "Trainer", "View trainers");
        yield return ("CREATE_TRAINER", "Trainer", "Create trainers");
        yield return ("UPDATE_TRAINER", "Trainer", "Update trainers");
        yield return ("DELETE_TRAINER", "Trainer", "Delete trainers");
        yield return ("ASSIGN_MEMBER_TO_TRAINER", "Trainer", "Assign members to trainers");

        yield return ("VIEW_MEMBERS", "Member", "View members");
        yield return ("CREATE_MEMBER", "Member", "Create members");
        yield return ("UPDATE_MEMBER", "Member", "Update members");
        yield return ("DELETE_MEMBER", "Member", "Delete members");
        yield return ("ASSIGN_TRAINER", "Member", "Assign trainer to members");
        yield return ("VIEW_MEMBER_DETAILS", "Member", "View member details");

        yield return ("VIEW_MEMBERSHIPS", "Membership", "View memberships");
        yield return ("CREATE_MEMBERSHIP", "Membership", "Create memberships");
        yield return ("UPDATE_MEMBERSHIP", "Membership", "Update memberships and plans");
        yield return ("RENEW_MEMBERSHIP", "Membership", "Renew memberships");

        yield return ("VIEW_PAYMENTS", "Payment", "View payments");
        yield return ("CREATE_PAYMENT", "Payment", "Create payments");
        yield return ("VIEW_REVENUE", "Payment", "View revenue dashboard");
        yield return ("DOWNLOAD_INVOICE", "Payment", "Download invoices");
        yield return ("INITIATE_ONLINE_PAYMENT", "Payment", "Initiate Razorpay online payments");
        yield return ("REFUND_PAYMENT", "Payment", "Refund Razorpay payments");
        yield return ("VIEW_NOTIFICATIONS", "Notification", "View WhatsApp notifications");
        yield return ("MANAGE_NOTIFICATIONS", "Notification", "Manage notification templates and settings");
        yield return ("SEND_NOTIFICATIONS", "Notification", "Send and test WhatsApp notifications");

        yield return ("VIEW_DASHBOARD", "Dashboard", "View dashboard statistics");

        yield return ("VIEW_ANALYTICS", "Analytics", "View business analytics dashboard");
        yield return ("VIEW_REVENUE_ANALYTICS", "Analytics", "View revenue analytics");
        yield return ("VIEW_MEMBER_ANALYTICS", "Analytics", "View member analytics");

        yield return ("VIEW_SAAS_SUBSCRIPTION", "SaaS", "View gym subscription and usage");
        yield return ("MANAGE_SAAS_SUBSCRIPTION", "SaaS", "Upgrade, renew, or cancel subscription");
        yield return ("VIEW_PLATFORM_SAAS", "SaaS", "View platform SaaS metrics (MRR/ARR)");
        yield return ("MANAGE_GYM_BRANDING", "SaaS", "Manage gym branding and colors");

        yield return ("VIEW_LEADS", "CRM", "View leads and pipeline");
        yield return ("MANAGE_LEADS", "CRM", "Create and manage leads, follow-ups, and trials");
        yield return ("CONVERT_LEADS", "CRM", "Convert leads to members");
        yield return ("VIEW_LEAD_ANALYTICS", "CRM", "View lead analytics and export reports");

        yield return ("VIEW_EXPENSES", "Financial", "View gym expenses");
        yield return ("MANAGE_EXPENSES", "Financial", "Create and manage expenses");
        yield return ("VIEW_PAYROLL", "Financial", "View payroll and commissions");
        yield return ("MANAGE_PAYROLL", "Financial", "Generate, approve, and pay payroll");
        yield return ("VIEW_FINANCIAL_ANALYTICS", "Financial", "View profit & loss and financial analytics");

        yield return ("VIEW_ATTENDANCE", "Attendance", "View member attendance");
        yield return ("MANAGE_ATTENDANCE", "Attendance", "Check-in, check-out, mark attendance");
        yield return ("VIEW_TRAINER_ATTENDANCE", "Attendance", "View trainer attendance");
        yield return ("MANAGE_TRAINER_ATTENDANCE", "Attendance", "Manage trainer check-in/out");
        yield return ("EXPORT_ATTENDANCE_REPORTS", "Attendance", "Export attendance reports");

        yield return ("VIEW_AUDIT_LOGS", "Audit", "View audit logs");
        yield return ("EXPORT_AUDIT_LOGS", "Audit", "Export audit logs");

        yield return ("VIEW_DIET_PLANS", "Diet", "View diet plans");
        yield return ("MANAGE_DIET_PLANS", "Diet", "Create, edit, delete, clone diet plans");
        yield return ("ASSIGN_DIET_PLAN", "Diet", "Assign diet plans to members");
        yield return ("VIEW_MEMBER_DIET", "Diet", "View member diet assignments");
        yield return ("EXPORT_DIET_PLANS", "Diet", "Export diet plan reports");

        yield return ("VIEW_WORKOUT_PLANS", "Workout", "View workout plans and exercise library");
        yield return ("MANAGE_WORKOUT_PLANS", "Workout", "Manage exercises and workout plans");
        yield return ("ASSIGN_WORKOUT_PLAN", "Workout", "Assign workout plans to members");
        yield return ("VIEW_MEMBER_WORKOUT", "Workout", "View member workout progress");
        yield return ("EXPORT_WORKOUT_PLANS", "Workout", "Export workout plan reports");

        yield return ("VIEW_FILES", "Files", "View uploaded files and attachments");
        yield return ("UPLOAD_FILES", "Files", "Upload gym logos, photos, and attachments");
        yield return ("DELETE_FILES", "Files", "Delete uploaded files");
        yield return ("MANAGE_FILES", "Files", "Full file management including uploads and deletes");

        yield return ("VIEW_MEMBER_DASHBOARD", "Member Self-Service", "View member self-service dashboard");
        yield return ("MANAGE_MEMBER_GOALS", "Member Self-Service", "Create and manage personal fitness goals");
        yield return ("TRACK_MEMBER_PROGRESS", "Member Self-Service", "Track progress, workouts, diet, and water intake");
        yield return ("SUBMIT_MEMBER_FEEDBACK", "Member Self-Service", "Submit feedback for trainer or gym");

        yield return ("VIEW_BRANCHES", "Branches", "View branches and branch dashboard");
        yield return ("MANAGE_BRANCHES", "Branches", "Create and manage branches, targets, and announcements");
        yield return ("VIEW_BRANCH_ANALYTICS", "Branches", "View branch comparison analytics");
        yield return ("TRANSFER_MEMBERS", "Branches", "Transfer members between branches");
        yield return ("TRANSFER_TRAINERS", "Branches", "Transfer trainers between branches");
        yield return ("VIEW_MOBILE_NOTIFICATIONS", "Mobile", "View mobile push notifications");
        yield return ("MANAGE_NOTIFICATION_PREFERENCES", "Mobile", "Manage mobile notification preferences");
        yield return ("VIEW_AI_INSIGHTS", "AI", "View AI insights, risk scores, and lead scoring");
        yield return ("VIEW_AI_RECOMMENDATIONS", "AI", "View AI trainer recommendations");
        yield return ("VIEW_BOOKINGS", "Booking", "View class schedules and bookings");
        yield return ("MANAGE_BOOKINGS", "Booking", "Manage bookings and check-in");
        yield return ("MANAGE_SCHEDULES", "Booking", "Manage class schedules and settings");
        yield return ("VIEW_BOOKING_ANALYTICS", "Booking", "View booking analytics and exports");
        yield return ("VIEW_WEBSITE_BUILDER", "Website", "View gym website builder");
        yield return ("MANAGE_WEBSITE_BUILDER", "Website", "Manage gym website content and publishing");
        yield return ("VIEW_WEBSITE_ANALYTICS", "Website", "View website lead analytics and exports");
        yield return ("VIEW_WHITE_LABEL", "WhiteLabel", "View white label branding and domain settings");
        yield return ("MANAGE_WHITE_LABEL", "WhiteLabel", "Manage white label branding, domains, and templates");
        yield return ("VIEW_TENANT_MENUS", "Platform", "View tenant menu assignments per gym");
        yield return ("MANAGE_TENANT_MENUS", "Platform", "Enable or disable menus per gym tenant");
    }
}
