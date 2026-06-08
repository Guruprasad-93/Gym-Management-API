/*
  Payroll, Expense Management & Profit/Loss Module
*/

IF OBJECT_ID(N'dbo.ExpenseCategories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ExpenseCategories
    (
        CategoryId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_ExpenseCategories_IsActive DEFAULT (1),
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_ExpenseCategories_CreatedDate DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_ExpenseCategories_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT UX_ExpenseCategories_Gym_Name UNIQUE (GymId, [Name])
    );
    CREATE INDEX IX_ExpenseCategories_GymId ON dbo.ExpenseCategories (GymId) WHERE IsActive = 1;
END
GO

IF OBJECT_ID(N'dbo.Expenses', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Expenses
    (
        ExpenseId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        CategoryId INT NOT NULL,
        Amount DECIMAL(18, 2) NOT NULL,
        ExpenseDate DATE NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        VendorName NVARCHAR(200) NULL,
        PaymentMethod NVARCHAR(50) NOT NULL,
        AttachmentFileId BIGINT NULL,
        CreatedBy UNIQUEIDENTIFIER NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_Expenses_CreatedDate DEFAULT (SYSUTCDATETIME()),
        UpdatedDate DATETIME2 NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_Expenses_IsDeleted DEFAULT (0),
        CONSTRAINT FK_Expenses_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_Expenses_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.ExpenseCategories (CategoryId),
        CONSTRAINT FK_Expenses_Files FOREIGN KEY (AttachmentFileId) REFERENCES dbo.Files (FileId)
    );
    CREATE INDEX IX_Expenses_GymId_Date ON dbo.Expenses (GymId, ExpenseDate DESC) WHERE IsDeleted = 0;
    CREATE INDEX IX_Expenses_GymId_Category ON dbo.Expenses (GymId, CategoryId) WHERE IsDeleted = 0;
END
GO

/* Upgrade legacy Expenses table (004) when payroll module runs after FutureTablesSchema */
IF OBJECT_ID(N'dbo.Expenses', N'U') IS NOT NULL AND COL_LENGTH('dbo.Expenses', 'IsDeleted') IS NULL
    ALTER TABLE dbo.Expenses ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Expenses_IsDeleted_Legacy DEFAULT (0);
GO

IF OBJECT_ID(N'dbo.Expenses', N'U') IS NOT NULL AND COL_LENGTH('dbo.Expenses', 'CategoryId') IS NULL
    ALTER TABLE dbo.Expenses ADD CategoryId INT NULL;
GO

IF OBJECT_ID(N'dbo.Expenses', N'U') IS NOT NULL AND COL_LENGTH('dbo.Expenses', 'CategoryId') IS NOT NULL
BEGIN
    INSERT INTO dbo.ExpenseCategories (GymId, [Name], [Description])
    SELECT DISTINCT e.GymId, N'General', N'Migrated from legacy expenses'
    FROM dbo.Expenses e
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.ExpenseCategories ec WHERE ec.GymId = e.GymId AND ec.[Name] = N'General');

    UPDATE e SET CategoryId = ec.CategoryId
    FROM dbo.Expenses e
    INNER JOIN dbo.ExpenseCategories ec ON ec.GymId = e.GymId AND ec.[Name] = N'General'
    WHERE e.CategoryId IS NULL;

    ALTER TABLE dbo.Expenses ALTER COLUMN CategoryId INT NOT NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Expenses_Categories')
        ALTER TABLE dbo.Expenses ADD CONSTRAINT FK_Expenses_Categories
            FOREIGN KEY (CategoryId) REFERENCES dbo.ExpenseCategories (CategoryId);

    IF COL_LENGTH('dbo.Expenses', 'Category') IS NOT NULL
        ALTER TABLE dbo.Expenses DROP COLUMN Category;
END
GO

IF OBJECT_ID(N'dbo.Expenses', N'U') IS NOT NULL AND COL_LENGTH('dbo.Expenses', 'VendorName') IS NULL
    ALTER TABLE dbo.Expenses ADD VendorName NVARCHAR(200) NULL;
GO

IF OBJECT_ID(N'dbo.Expenses', N'U') IS NOT NULL AND COL_LENGTH('dbo.Expenses', 'PaymentMethod') IS NULL
    ALTER TABLE dbo.Expenses ADD PaymentMethod NVARCHAR(50) NOT NULL CONSTRAINT DF_Expenses_PaymentMethod_Legacy DEFAULT (N'Cash');
GO

IF OBJECT_ID(N'dbo.Expenses', N'U') IS NOT NULL AND COL_LENGTH('dbo.Expenses', 'AttachmentFileId') IS NULL
    ALTER TABLE dbo.Expenses ADD AttachmentFileId BIGINT NULL;
GO

IF OBJECT_ID(N'dbo.Expenses', N'U') IS NOT NULL AND COL_LENGTH('dbo.Expenses', 'CreatedBy') IS NULL
    ALTER TABLE dbo.Expenses ADD CreatedBy UNIQUEIDENTIFIER NULL;
GO

IF OBJECT_ID(N'dbo.Expenses', N'U') IS NOT NULL
   AND COL_LENGTH('dbo.Expenses', 'CreatedDate') IS NULL
   AND COL_LENGTH('dbo.Expenses', 'CreatedAt') IS NOT NULL
    EXEC sp_rename N'dbo.Expenses.CreatedAt', N'CreatedDate', N'COLUMN';
GO

IF OBJECT_ID(N'dbo.Expenses', N'U') IS NOT NULL AND COL_LENGTH('dbo.Expenses', 'CreatedDate') IS NULL
    ALTER TABLE dbo.Expenses ADD CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_Expenses_CreatedDate_Legacy DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.Expenses', N'U') IS NOT NULL AND COL_LENGTH('dbo.Expenses', 'UpdatedDate') IS NULL
    ALTER TABLE dbo.Expenses ADD UpdatedDate DATETIME2 NULL;
GO

IF OBJECT_ID(N'dbo.Payrolls', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payrolls
    (
        PayrollId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        EmployeeType NVARCHAR(30) NOT NULL,
        EmployeeId INT NOT NULL,
        EmployeeUserId UNIQUEIDENTIFIER NULL,
        SalaryMonth DATE NOT NULL,
        BaseSalary DECIMAL(18, 2) NOT NULL,
        IncentiveAmount DECIMAL(18, 2) NOT NULL CONSTRAINT DF_Payrolls_Incentive DEFAULT (0),
        CommissionAmount DECIMAL(18, 2) NOT NULL CONSTRAINT DF_Payrolls_Commission DEFAULT (0),
        DeductionAmount DECIMAL(18, 2) NOT NULL CONSTRAINT DF_Payrolls_Deduction DEFAULT (0),
        NetSalary DECIMAL(18, 2) NOT NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_Payrolls_Status DEFAULT (N'Draft'),
        PaidDate DATETIME2 NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_Payrolls_CreatedDate DEFAULT (SYSUTCDATETIME()),
        CreatedBy UNIQUEIDENTIFIER NULL,
        UpdatedDate DATETIME2 NULL,
        CONSTRAINT FK_Payrolls_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT UX_Payrolls_Employee_Month UNIQUE (GymId, EmployeeType, EmployeeId, EmployeeUserId, SalaryMonth)
    );
    CREATE INDEX IX_Payrolls_GymId_Month ON dbo.Payrolls (GymId, SalaryMonth DESC);
    CREATE INDEX IX_Payrolls_GymId_Status ON dbo.Payrolls (GymId, [Status]);
END
GO

IF OBJECT_ID(N'dbo.TrainerCommissions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TrainerCommissions
    (
        CommissionId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        TrainerId INT NOT NULL,
        MemberId INT NULL,
        PaymentId INT NULL,
        Amount DECIMAL(18, 2) NOT NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_TrainerCommissions_CreatedDate DEFAULT (SYSUTCDATETIME()),
        CreatedBy UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_TrainerCommissions_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_TrainerCommissions_Trainers FOREIGN KEY (TrainerId) REFERENCES dbo.Trainers (TrainerId),
        CONSTRAINT FK_TrainerCommissions_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId),
        CONSTRAINT FK_TrainerCommissions_Payments FOREIGN KEY (PaymentId) REFERENCES dbo.Payments (PaymentId)
    );
    CREATE INDEX IX_TrainerCommissions_GymId_Trainer ON dbo.TrainerCommissions (GymId, TrainerId, CreatedDate DESC);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_SeedExpenseCategories
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.ExpenseCategories (GymId, [Name], [Description])
    SELECT @GymId, c.[Name], c.[Description]
    FROM (VALUES
        (N'Rent', N'Facility rent and lease'),
        (N'Electricity', N'Power and utilities'),
        (N'Water', N'Water supply'),
        (N'Equipment Purchase', N'New gym equipment'),
        (N'Equipment Maintenance', N'Repairs and servicing'),
        (N'Marketing', N'Ads and promotions'),
        (N'Staff Salary', N'Staff compensation'),
        (N'Miscellaneous', N'Other expenses')
    ) AS c([Name], [Description])
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.ExpenseCategories ec
        WHERE ec.GymId = @GymId AND ec.[Name] = c.[Name]);
END
GO

INSERT INTO dbo.ExpenseCategories (GymId, [Name], [Description])
SELECT g.GymId, c.[Name], c.[Description]
FROM dbo.Gyms g
CROSS JOIN (VALUES
    (N'Rent', N'Facility rent and lease'),
    (N'Electricity', N'Power and utilities'),
    (N'Water', N'Water supply'),
    (N'Equipment Purchase', N'New gym equipment'),
    (N'Equipment Maintenance', N'Repairs and servicing'),
    (N'Marketing', N'Ads and promotions'),
    (N'Staff Salary', N'Staff compensation'),
    (N'Miscellaneous', N'Other expenses')
) AS c([Name], [Description])
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.ExpenseCategories ec
    WHERE ec.GymId = g.GymId AND ec.[Name] = c.[Name]);
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetExpenseCategories
    @GymId UNIQUEIDENTIFIER,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CategoryId, GymId, [Name], [Description], IsActive, CreatedDate
    FROM dbo.ExpenseCategories
    WHERE GymId = @GymId AND (@IncludeInactive = 1 OR IsActive = 1)
    ORDER BY [Name];
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateExpense
    @GymId UNIQUEIDENTIFIER,
    @CategoryId INT,
    @Amount DECIMAL(18, 2),
    @ExpenseDate DATE,
    @Description NVARCHAR(1000) = NULL,
    @VendorName NVARCHAR(200) = NULL,
    @PaymentMethod NVARCHAR(50),
    @AttachmentFileId BIGINT = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @ExpenseId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.ExpenseCategories WHERE CategoryId = @CategoryId AND GymId = @GymId AND IsActive = 1)
        THROW 50001, 'Invalid expense category.', 1;

    INSERT INTO dbo.Expenses (GymId, CategoryId, Amount, ExpenseDate, [Description], VendorName, PaymentMethod, AttachmentFileId, CreatedBy)
    VALUES (@GymId, @CategoryId, @Amount, @ExpenseDate, @Description, @VendorName, @PaymentMethod, @AttachmentFileId, @CreatedBy);
    SET @ExpenseId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateExpense
    @ExpenseId INT,
    @GymId UNIQUEIDENTIFIER,
    @CategoryId INT,
    @Amount DECIMAL(18, 2),
    @ExpenseDate DATE,
    @Description NVARCHAR(1000) = NULL,
    @VendorName NVARCHAR(200) = NULL,
    @PaymentMethod NVARCHAR(50),
    @AttachmentFileId BIGINT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Expenses
    SET CategoryId = @CategoryId, Amount = @Amount, ExpenseDate = @ExpenseDate,
        [Description] = @Description, VendorName = @VendorName, PaymentMethod = @PaymentMethod,
        AttachmentFileId = @AttachmentFileId, UpdatedDate = SYSUTCDATETIME()
    WHERE ExpenseId = @ExpenseId AND GymId = @GymId AND IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteExpense
    @ExpenseId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Expenses SET IsDeleted = 1, UpdatedDate = SYSUTCDATETIME()
    WHERE ExpenseId = @ExpenseId AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetExpenseById
    @ExpenseId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT e.ExpenseId, e.GymId, e.CategoryId, ec.[Name] AS CategoryName, e.Amount, e.ExpenseDate,
           e.[Description], e.VendorName, e.PaymentMethod, e.AttachmentFileId, e.CreatedBy, e.CreatedDate, e.UpdatedDate
    FROM dbo.Expenses e
    INNER JOIN dbo.ExpenseCategories ec ON ec.CategoryId = e.CategoryId
    WHERE e.ExpenseId = @ExpenseId AND e.GymId = @GymId AND e.IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetExpensesPaged
    @GymId UNIQUEIDENTIFIER,
    @Search NVARCHAR(200) = NULL,
    @CategoryId INT = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortColumn NVARCHAR(50) = N'ExpenseDate',
    @SortDirection NVARCHAR(4) = N'DESC',
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 10;
    IF @PageSize > 5000 SET @PageSize = 5000;

    SELECT @TotalCount = COUNT(*)
    FROM dbo.Expenses e
    INNER JOIN dbo.ExpenseCategories ec ON ec.CategoryId = e.CategoryId
    WHERE e.GymId = @GymId AND e.IsDeleted = 0
      AND (@CategoryId IS NULL OR e.CategoryId = @CategoryId)
      AND (@FromDate IS NULL OR e.ExpenseDate >= @FromDate)
      AND (@ToDate IS NULL OR e.ExpenseDate <= @ToDate)
      AND (@Search IS NULL OR e.[Description] LIKE N'%' + @Search + N'%'
           OR e.VendorName LIKE N'%' + @Search + N'%' OR ec.[Name] LIKE N'%' + @Search + N'%');

    ;WITH Filtered AS (
        SELECT e.ExpenseId, e.GymId, e.CategoryId, ec.[Name] AS CategoryName, e.Amount, e.ExpenseDate,
               e.[Description], e.VendorName, e.PaymentMethod, e.AttachmentFileId, e.CreatedBy, e.CreatedDate, e.UpdatedDate
        FROM dbo.Expenses e
        INNER JOIN dbo.ExpenseCategories ec ON ec.CategoryId = e.CategoryId
        WHERE e.GymId = @GymId AND e.IsDeleted = 0
          AND (@CategoryId IS NULL OR e.CategoryId = @CategoryId)
          AND (@FromDate IS NULL OR e.ExpenseDate >= @FromDate)
          AND (@ToDate IS NULL OR e.ExpenseDate <= @ToDate)
          AND (@Search IS NULL OR e.[Description] LIKE N'%' + @Search + N'%'
               OR e.VendorName LIKE N'%' + @Search + N'%' OR ec.[Name] LIKE N'%' + @Search + N'%')
    )
    SELECT * FROM Filtered
    ORDER BY
        CASE WHEN @SortColumn = N'Amount' AND @SortDirection = N'ASC' THEN Amount END ASC,
        CASE WHEN @SortColumn = N'Amount' AND @SortDirection = N'DESC' THEN Amount END DESC,
        CASE WHEN @SortColumn = N'CategoryName' AND @SortDirection = N'ASC' THEN CategoryName END ASC,
        CASE WHEN @SortColumn = N'CategoryName' AND @SortDirection = N'DESC' THEN CategoryName END DESC,
        CASE WHEN @SortDirection = N'ASC' THEN ExpenseDate END ASC,
        ExpenseDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreatePayroll
    @GymId UNIQUEIDENTIFIER,
    @EmployeeType NVARCHAR(30),
    @EmployeeId INT,
    @EmployeeUserId UNIQUEIDENTIFIER = NULL,
    @SalaryMonth DATE,
    @BaseSalary DECIMAL(18, 2),
    @IncentiveAmount DECIMAL(18, 2) = 0,
    @CommissionAmount DECIMAL(18, 2) = 0,
    @DeductionAmount DECIMAL(18, 2) = 0,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @PayrollId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Net DECIMAL(18, 2) = @BaseSalary + @IncentiveAmount + @CommissionAmount - @DeductionAmount;
    INSERT INTO dbo.Payrolls (GymId, EmployeeType, EmployeeId, EmployeeUserId, SalaryMonth, BaseSalary, IncentiveAmount, CommissionAmount, DeductionAmount, NetSalary, [Status], CreatedBy)
    VALUES (@GymId, @EmployeeType, @EmployeeId, @EmployeeUserId, @SalaryMonth, @BaseSalary, @IncentiveAmount, @CommissionAmount, @DeductionAmount, @Net, N'Draft', @CreatedBy);
    SET @PayrollId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdatePayroll
    @PayrollId INT,
    @GymId UNIQUEIDENTIFIER,
    @BaseSalary DECIMAL(18, 2),
    @IncentiveAmount DECIMAL(18, 2),
    @CommissionAmount DECIMAL(18, 2),
    @DeductionAmount DECIMAL(18, 2)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.Payrolls WHERE PayrollId = @PayrollId AND GymId = @GymId AND [Status] = N'Paid')
        THROW 50002, 'Cannot update paid payroll.', 1;

    UPDATE dbo.Payrolls
    SET BaseSalary = @BaseSalary, IncentiveAmount = @IncentiveAmount, CommissionAmount = @CommissionAmount,
        DeductionAmount = @DeductionAmount,
        NetSalary = @BaseSalary + @IncentiveAmount + @CommissionAmount - @DeductionAmount,
        UpdatedDate = SYSUTCDATETIME()
    WHERE PayrollId = @PayrollId AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetPayrollById
    @PayrollId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.PayrollId, p.GymId, p.EmployeeType, p.EmployeeId, p.EmployeeUserId, p.SalaryMonth,
           p.BaseSalary, p.IncentiveAmount, p.CommissionAmount, p.DeductionAmount, p.NetSalary,
           p.[Status], p.PaidDate, p.CreatedDate, p.CreatedBy, p.UpdatedDate,
           COALESCE(tu.[Name], gu.[Name], N'Employee') AS EmployeeName
    FROM dbo.Payrolls p
    LEFT JOIN dbo.Trainers tr ON p.EmployeeType = N'Trainer' AND tr.TrainerId = p.EmployeeId
    LEFT JOIN dbo.Users tu ON tu.Id = tr.UserId
    LEFT JOIN dbo.Users gu ON p.EmployeeType = N'GymAdmin' AND gu.Id = p.EmployeeUserId
    WHERE p.PayrollId = @PayrollId AND p.GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetPayrollsPaged
    @GymId UNIQUEIDENTIFIER,
    @SalaryMonth DATE = NULL,
    @Status NVARCHAR(30) = NULL,
    @EmployeeType NVARCHAR(30) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF @PageNumber < 1 SET @PageNumber = 1;

    SELECT @TotalCount = COUNT(*)
    FROM dbo.Payrolls p
    WHERE p.GymId = @GymId
      AND (@SalaryMonth IS NULL OR p.SalaryMonth = @SalaryMonth)
      AND (@Status IS NULL OR p.[Status] = @Status)
      AND (@EmployeeType IS NULL OR p.EmployeeType = @EmployeeType);

    SELECT p.PayrollId, p.GymId, p.EmployeeType, p.EmployeeId, p.EmployeeUserId, p.SalaryMonth,
           p.BaseSalary, p.IncentiveAmount, p.CommissionAmount, p.DeductionAmount, p.NetSalary,
           p.[Status], p.PaidDate, p.CreatedDate, p.CreatedBy, p.UpdatedDate,
           COALESCE(tu.[Name], gu.[Name], N'Employee') AS EmployeeName
    FROM dbo.Payrolls p
    LEFT JOIN dbo.Trainers tr ON p.EmployeeType = N'Trainer' AND tr.TrainerId = p.EmployeeId
    LEFT JOIN dbo.Users tu ON tu.Id = tr.UserId
    LEFT JOIN dbo.Users gu ON p.EmployeeType = N'GymAdmin' AND gu.Id = p.EmployeeUserId
    WHERE p.GymId = @GymId
      AND (@SalaryMonth IS NULL OR p.SalaryMonth = @SalaryMonth)
      AND (@Status IS NULL OR p.[Status] = @Status)
      AND (@EmployeeType IS NULL OR p.EmployeeType = @EmployeeType)
    ORDER BY p.SalaryMonth DESC, EmployeeName
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GenerateMonthlyPayroll
    @GymId UNIQUEIDENTIFIER,
    @SalaryMonth DATE,
    @DefaultTrainerBaseSalary DECIMAL(18, 2) = 15000,
    @DefaultStaffBaseSalary DECIMAL(18, 2) = 25000,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @GeneratedCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @GeneratedCount = 0;
    DECLARE @MonthStart DATE = DATEFROMPARTS(YEAR(@SalaryMonth), MONTH(@SalaryMonth), 1);
    DECLARE @MonthEnd DATE = EOMONTH(@MonthStart);

    -- Trainers
    INSERT INTO dbo.Payrolls (GymId, EmployeeType, EmployeeId, EmployeeUserId, SalaryMonth, BaseSalary, IncentiveAmount, CommissionAmount, DeductionAmount, NetSalary, [Status], CreatedBy)
    SELECT @GymId, N'Trainer', tr.TrainerId, tr.UserId, @MonthStart, @DefaultTrainerBaseSalary, 0,
           ISNULL((SELECT SUM(tc.Amount) FROM dbo.TrainerCommissions tc
                   WHERE tc.GymId = @GymId AND tc.TrainerId = tr.TrainerId
                     AND tc.CreatedDate >= @MonthStart AND tc.CreatedDate < DATEADD(DAY, 1, @MonthEnd)), 0),
           0,
           @DefaultTrainerBaseSalary + ISNULL((SELECT SUM(tc.Amount) FROM dbo.TrainerCommissions tc
                   WHERE tc.GymId = @GymId AND tc.TrainerId = tr.TrainerId
                     AND tc.CreatedDate >= @MonthStart AND tc.CreatedDate < DATEADD(DAY, 1, @MonthEnd)), 0),
           N'Draft', @CreatedBy
    FROM dbo.Trainers tr
    WHERE tr.GymId = @GymId AND tr.IsActive = 1
      AND NOT EXISTS (
          SELECT 1 FROM dbo.Payrolls p
          WHERE p.GymId = @GymId AND p.EmployeeType = N'Trainer' AND p.EmployeeId = tr.TrainerId AND p.SalaryMonth = @MonthStart);

    SET @GeneratedCount = @GeneratedCount + @@ROWCOUNT;

    -- Gym admins
    INSERT INTO dbo.Payrolls (GymId, EmployeeType, EmployeeId, EmployeeUserId, SalaryMonth, BaseSalary, IncentiveAmount, CommissionAmount, DeductionAmount, NetSalary, [Status], CreatedBy)
    SELECT @GymId, N'GymAdmin', 0, u.Id, @MonthStart, @DefaultStaffBaseSalary, 0, 0, 0, @DefaultStaffBaseSalary, N'Draft', @CreatedBy
    FROM dbo.Users u
    INNER JOIN dbo.UserRoles ur ON ur.UserId = u.Id
    INNER JOIN dbo.Roles r ON r.RoleId = ur.RoleId AND r.RoleName = N'GymAdmin'
    WHERE u.GymId = @GymId AND u.IsActive = 1
      AND NOT EXISTS (
          SELECT 1 FROM dbo.Payrolls p
          WHERE p.GymId = @GymId AND p.EmployeeType = N'GymAdmin' AND p.EmployeeUserId = u.Id AND p.SalaryMonth = @MonthStart);

    SET @GeneratedCount = @GeneratedCount + @@ROWCOUNT;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_ApprovePayroll
    @PayrollId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Payrolls SET [Status] = N'Approved', UpdatedDate = SYSUTCDATETIME()
    WHERE PayrollId = @PayrollId AND GymId = @GymId AND [Status] = N'Draft';
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_PayPayroll
    @PayrollId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Payrolls SET [Status] = N'Paid', PaidDate = SYSUTCDATETIME(), UpdatedDate = SYSUTCDATETIME()
    WHERE PayrollId = @PayrollId AND GymId = @GymId AND [Status] IN (N'Draft', N'Approved');
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateTrainerCommission
    @GymId UNIQUEIDENTIFIER,
    @TrainerId INT,
    @MemberId INT = NULL,
    @PaymentId INT = NULL,
    @Amount DECIMAL(18, 2),
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @CommissionId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.TrainerCommissions (GymId, TrainerId, MemberId, PaymentId, Amount, CreatedBy)
    VALUES (@GymId, @TrainerId, @MemberId, @PaymentId, @Amount, @CreatedBy);
    SET @CommissionId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetExpenseDashboard
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    DECLARE @MonthStart DATE = DATEFROMPARTS(YEAR(@Today), MONTH(@Today), 1);

    SELECT
        ISNULL((SELECT SUM(e.Amount) FROM dbo.Expenses e
                WHERE e.IsDeleted = 0 AND (@GymId IS NULL OR e.GymId = @GymId)
                  AND e.ExpenseDate >= @MonthStart), 0) AS ExpensesThisMonth,
        ISNULL((SELECT SUM(e.Amount) FROM dbo.Expenses e
                WHERE e.IsDeleted = 0 AND (@GymId IS NULL OR e.GymId = @GymId)), 0) AS TotalExpenses,
        ISNULL((SELECT COUNT(*) FROM dbo.Expenses e
                WHERE e.IsDeleted = 0 AND (@GymId IS NULL OR e.GymId = @GymId)
                  AND e.ExpenseDate >= @MonthStart), 0) AS ExpenseCountThisMonth;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetPayrollDashboard
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    DECLARE @MonthStart DATE = DATEFROMPARTS(YEAR(@Today), MONTH(@Today), 1);

    SELECT
        ISNULL((SELECT SUM(p.NetSalary) FROM dbo.Payrolls p
                WHERE (@GymId IS NULL OR p.GymId = @GymId) AND p.SalaryMonth = @MonthStart), 0) AS PayrollCostThisMonth,
        ISNULL((SELECT SUM(p.NetSalary) FROM dbo.Payrolls p
                WHERE (@GymId IS NULL OR p.GymId = @GymId) AND p.[Status] IN (N'Draft', N'Approved')), 0) AS PendingSalaries,
        ISNULL((SELECT COUNT(*) FROM dbo.Payrolls p
                WHERE (@GymId IS NULL OR p.GymId = @GymId) AND p.[Status] = N'Paid'
                  AND p.SalaryMonth = @MonthStart), 0) AS PaidCountThisMonth;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetProfitLossSummary
    @GymId UNIQUEIDENTIFIER = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    IF @FromDate IS NULL SET @FromDate = DATEFROMPARTS(YEAR(@Today), MONTH(@Today), 1);
    IF @ToDate IS NULL SET @ToDate = @Today;

    DECLARE @Revenue DECIMAL(18, 2) = ISNULL((
        SELECT SUM(Amount) FROM dbo.Payments
        WHERE Status = N'Completed' AND (@GymId IS NULL OR GymId = @GymId)
          AND CAST(PaymentDate AS DATE) BETWEEN @FromDate AND @ToDate), 0);

    DECLARE @Expenses DECIMAL(18, 2) = ISNULL((
        SELECT SUM(Amount) FROM dbo.Expenses
        WHERE IsDeleted = 0 AND (@GymId IS NULL OR GymId = @GymId)
          AND ExpenseDate BETWEEN @FromDate AND @ToDate), 0);

    DECLARE @Payroll DECIMAL(18, 2) = ISNULL((
        SELECT SUM(NetSalary) FROM dbo.Payrolls
        WHERE (@GymId IS NULL OR GymId = @GymId) AND [Status] = N'Paid'
          AND SalaryMonth BETWEEN DATEFROMPARTS(YEAR(@FromDate), MONTH(@FromDate), 1)
              AND DATEFROMPARTS(YEAR(@ToDate), MONTH(@ToDate), 1)), 0);

    DECLARE @Commissions DECIMAL(18, 2) = ISNULL((
        SELECT SUM(Amount) FROM dbo.TrainerCommissions
        WHERE (@GymId IS NULL OR GymId = @GymId)
          AND CAST(CreatedDate AS DATE) BETWEEN @FromDate AND @ToDate), 0);

    SELECT @Revenue AS Revenue, @Expenses AS Expenses, @Payroll AS PayrollCost,
           @Commissions AS TrainerCommissions,
           @Revenue - @Expenses - @Payroll AS Profit,
           @FromDate AS FromDate, @ToDate AS ToDate;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMonthlyProfitTrend
    @GymId UNIQUEIDENTIFIER = NULL,
    @Months INT = 12
AS
BEGIN
    SET NOCOUNT ON;
    IF @Months < 1 SET @Months = 1;
    IF @Months > 24 SET @Months = 24;

    ;WITH MonthSeries AS (
        SELECT 0 AS N UNION ALL SELECT N + 1 FROM MonthSeries WHERE N + 1 < @Months
    )
    SELECT
        YEAR(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE))) AS [Year],
        MONTH(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE))) AS [Month],
        FORMAT(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)), 'MMM yyyy') AS MonthLabel,
        ISNULL((SELECT SUM(Amount) FROM dbo.Payments p
                WHERE p.Status = N'Completed' AND (@GymId IS NULL OR p.GymId = @GymId)
                  AND YEAR(p.PaymentDate) = YEAR(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)))
                  AND MONTH(p.PaymentDate) = MONTH(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)))), 0) AS Revenue,
        ISNULL((SELECT SUM(e.Amount) FROM dbo.Expenses e
                WHERE e.IsDeleted = 0 AND (@GymId IS NULL OR e.GymId = @GymId)
                  AND YEAR(e.ExpenseDate) = YEAR(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)))
                  AND MONTH(e.ExpenseDate) = MONTH(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)))), 0) AS Expenses,
        ISNULL((SELECT SUM(pay.NetSalary) FROM dbo.Payrolls pay
                WHERE pay.[Status] = N'Paid' AND (@GymId IS NULL OR pay.GymId = @GymId)
                  AND YEAR(pay.SalaryMonth) = YEAR(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)))
                  AND MONTH(pay.SalaryMonth) = MONTH(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)))), 0) AS PayrollCost
    FROM MonthSeries ms
    ORDER BY [Year] DESC, [Month] DESC
    OPTION (MAXRECURSION 24);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetExpenseCategoryBreakdown
    @GymId UNIQUEIDENTIFIER = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    IF @FromDate IS NULL SET @FromDate = DATEFROMPARTS(YEAR(@Today), MONTH(@Today), 1);
    IF @ToDate IS NULL SET @ToDate = @Today;

    SELECT ec.[Name], ISNULL(SUM(e.Amount), 0) AS Amount, COUNT(e.ExpenseId) AS [Count]
    FROM dbo.ExpenseCategories ec
    LEFT JOIN dbo.Expenses e ON e.CategoryId = ec.CategoryId AND e.IsDeleted = 0
        AND e.ExpenseDate BETWEEN @FromDate AND @ToDate
        AND (@GymId IS NULL OR e.GymId = @GymId)
    WHERE (@GymId IS NULL OR ec.GymId = @GymId) AND ec.IsActive = 1
    GROUP BY ec.[Name]
    HAVING ISNULL(SUM(e.Amount), 0) > 0 OR @GymId IS NOT NULL
    ORDER BY Amount DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetTrainerCommissionReport
    @GymId UNIQUEIDENTIFIER = NULL,
    @TrainerId INT = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    IF @FromDate IS NULL SET @FromDate = DATEFROMPARTS(YEAR(@Today), MONTH(@Today), 1);
    IF @ToDate IS NULL SET @ToDate = @Today;

    SELECT tc.CommissionId, tc.GymId, tc.TrainerId, tu.[Name] AS TrainerName,
           tc.MemberId, mu.[Name] AS MemberName, tc.PaymentId, tc.Amount, tc.CreatedDate
    FROM dbo.TrainerCommissions tc
    INNER JOIN dbo.Trainers tr ON tr.TrainerId = tc.TrainerId
    INNER JOIN dbo.Users tu ON tu.Id = tr.UserId
    LEFT JOIN dbo.Members m ON m.MemberId = tc.MemberId
    LEFT JOIN dbo.Users mu ON mu.Id = m.UserId
    WHERE (@GymId IS NULL OR tc.GymId = @GymId)
      AND (@TrainerId IS NULL OR tc.TrainerId = @TrainerId)
      AND CAST(tc.CreatedDate AS DATE) BETWEEN @FromDate AND @ToDate
    ORDER BY tc.CreatedDate DESC;
END
GO

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
        FORMAT(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)), 'MMM yyyy') AS MonthLabel,
        ISNULL((SELECT SUM(NetSalary) FROM dbo.Payrolls pay
                WHERE pay.[Status] = N'Paid' AND (@GymId IS NULL OR pay.GymId = @GymId)
                  AND YEAR(pay.SalaryMonth) = YEAR(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)))
                  AND MONTH(pay.SalaryMonth) = MONTH(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)))), 0) AS PayrollCost
    FROM MonthSeries ms
    ORDER BY MonthLabel
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
        FORMAT(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)), 'MMM yyyy') AS MonthLabel,
        ISNULL((SELECT SUM(Amount) FROM dbo.TrainerCommissions tc
                WHERE (@GymId IS NULL OR tc.GymId = @GymId)
                  AND YEAR(tc.CreatedDate) = YEAR(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)))
                  AND MONTH(tc.CreatedDate) = MONTH(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)))), 0) AS CommissionTotal
    FROM MonthSeries ms
    ORDER BY MonthLabel
    OPTION (MAXRECURSION 24);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetFinancialRevenueSummary
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);

    SELECT
        ISNULL((SELECT SUM(Amount) FROM dbo.Payments
                WHERE Status = N'Completed' AND (@GymId IS NULL OR GymId = @GymId)
                  AND YEAR(PaymentDate) = YEAR(@Today) AND MONTH(PaymentDate) = MONTH(@Today)), 0) AS RevenueThisMonth,
        ISNULL((SELECT SUM(Amount) FROM dbo.TrainerCommissions
                WHERE (@GymId IS NULL OR GymId = @GymId)
                  AND YEAR(CreatedDate) = YEAR(@Today) AND MONTH(CreatedDate) = MONTH(@Today)), 0) AS CommissionsThisMonth;
END
GO
