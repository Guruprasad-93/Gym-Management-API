-- 056_SubscriptionExpiryNotificationsFix.sql
-- Fix role column names in tenant user lookup for subscription notifications.

CREATE OR ALTER PROCEDURE dbo.sp_SubscriptionExpiry_GetGymTenantUsers
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT u.Id AS UserId, r.RoleName AS RoleName
    FROM dbo.Users u
    INNER JOIN dbo.UserRoles ur ON ur.UserId = u.Id
    INNER JOIN dbo.Roles r ON r.RoleId = ur.RoleId
    WHERE u.GymId = @GymId
      AND u.IsActive = 1
      AND r.RoleName IN (N'GymAdmin', N'Trainer', N'Member');
END
GO
