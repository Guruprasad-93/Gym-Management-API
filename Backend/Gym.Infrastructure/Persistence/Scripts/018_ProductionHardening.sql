/*
  Production Hardening Sprint 1
  - Require @GymId on tenant-scoped procedures
  - Helper lookups for platform admin entity resolution
*/

CREATE OR ALTER PROCEDURE dbo.sp_Member_GetGymId
    @MemberId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.GymId
    FROM dbo.Members m
    WHERE m.MemberId = @MemberId AND m.IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Trainer_GetGymId
    @TrainerId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.GymId
    FROM dbo.Trainers t
    WHERE t.TrainerId = @TrainerId AND t.IsActive = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetRevenueDashboard
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    IF @GymId IS NULL
        THROW 50001, 'GymId is required.', 1;

    BEGIN TRY
        SELECT
            ISNULL((SELECT SUM(Amount) FROM dbo.Payments WHERE Status = N'Completed' AND GymId = @GymId), 0) AS TotalRevenue,
            ISNULL((SELECT SUM(Amount) FROM dbo.Payments WHERE Status = N'Completed' AND GymId = @GymId
                AND YEAR(PaymentDate) = YEAR(GETUTCDATE()) AND MONTH(PaymentDate) = MONTH(GETUTCDATE())), 0) AS MonthlyRevenue,
            (SELECT COUNT(*) FROM dbo.Memberships ms WHERE ms.Status <> N'Cancelled' AND ms.EndDate < CAST(GETUTCDATE() AS DATE) AND ms.GymId = @GymId) AS ExpiredMemberships,
            (SELECT COUNT(*) FROM dbo.Memberships ms WHERE ms.Status <> N'Cancelled' AND ms.EndDate >= CAST(GETUTCDATE() AS DATE) AND ms.GymId = @GymId) AS ActiveMemberships,
            (SELECT COUNT(*) FROM dbo.Memberships ms WHERE ms.Status <> N'Cancelled'
                AND ms.EndDate BETWEEN CAST(GETUTCDATE() AS DATE) AND DATEADD(DAY, 30, CAST(GETUTCDATE() AS DATE))
                AND ms.GymId = @GymId) AS PendingRenewals;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMonthlyRevenueSummary
    @GymId UNIQUEIDENTIFIER,
    @Months INT = 12
AS
BEGIN
    SET NOCOUNT ON;
    IF @GymId IS NULL
        THROW 50001, 'GymId is required.', 1;

    BEGIN TRY
        IF @Months < 1 SET @Months = 12;
        IF @Months > 24 SET @Months = 24;

        ;WITH Months AS (
            SELECT TOP (@Months)
                DATEADD(MONTH, -ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + 1, DATEFROMPARTS(YEAR(GETUTCDATE()), MONTH(GETUTCDATE()), 1)) AS MonthStart
            FROM sys.all_objects
        )
        SELECT
            m.MonthStart,
            ISNULL(SUM(p.Amount), 0) AS Revenue
        FROM Months m
        LEFT JOIN dbo.Payments p ON p.GymId = @GymId
            AND p.Status = N'Completed'
            AND p.PaymentDate >= m.MonthStart
            AND p.PaymentDate < DATEADD(MONTH, 1, m.MonthStart)
        GROUP BY m.MonthStart
        ORDER BY m.MonthStart;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
