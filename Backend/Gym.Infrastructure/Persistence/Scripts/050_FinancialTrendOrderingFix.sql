/* Fix monthly trend ordering — use chronological date order instead of MonthLabel alphabetical sort. */
CREATE OR ALTER PROCEDURE dbo.sp_GetPayrollCostTrend
    @GymId UNIQUEIDENTIFIER = NULL,
    @Months INT = 12
AS
BEGIN
    SET NOCOUNT ON;
    IF @Months < 1 SET @Months = 1;

    ;WITH MonthSeries AS (
        SELECT 0 AS N UNION ALL SELECT N + 1 FROM MonthSeries WHERE N + 1 < @Months
    )
    SELECT
        YEAR(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE))) AS [Year],
        MONTH(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE))) AS [Month],
        FORMAT(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)), 'MMM yyyy') AS MonthLabel,
        ISNULL((SELECT SUM(NetSalary) FROM dbo.Payrolls pay
                WHERE pay.[Status] = N'Paid' AND (@GymId IS NULL OR pay.GymId = @GymId)
                  AND YEAR(pay.SalaryMonth) = YEAR(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)))
                  AND MONTH(pay.SalaryMonth) = MONTH(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)))), 0) AS PayrollCost
    FROM MonthSeries ms
    ORDER BY [Year] DESC, [Month] DESC
    OPTION (MAXRECURSION 24);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetCommissionTrend
    @GymId UNIQUEIDENTIFIER = NULL,
    @Months INT = 12
AS
BEGIN
    SET NOCOUNT ON;
    IF @Months < 1 SET @Months = 1;

    ;WITH MonthSeries AS (
        SELECT 0 AS N UNION ALL SELECT N + 1 FROM MonthSeries WHERE N + 1 < @Months
    )
    SELECT
        YEAR(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE))) AS [Year],
        MONTH(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE))) AS [Month],
        FORMAT(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)), 'MMM yyyy') AS MonthLabel,
        ISNULL((SELECT SUM(Amount) FROM dbo.TrainerCommissions tc
                WHERE (@GymId IS NULL OR tc.GymId = @GymId)
                  AND YEAR(tc.CreatedDate) = YEAR(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)))
                  AND MONTH(tc.CreatedDate) = MONTH(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)))), 0) AS CommissionTotal
    FROM MonthSeries ms
    ORDER BY [Year] DESC, [Month] DESC
    OPTION (MAXRECURSION 24);
END
GO
