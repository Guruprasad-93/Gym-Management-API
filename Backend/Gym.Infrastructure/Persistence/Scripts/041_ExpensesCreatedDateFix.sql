/*
  Legacy Expenses tables renamed CreatedAt -> CreatedDate without a DEFAULT.
  sp_CreateExpense omitted CreatedDate, causing INSERT failures.
*/

IF OBJECT_ID(N'dbo.Expenses', N'U') IS NOT NULL
   AND COL_LENGTH('dbo.Expenses', 'CreatedDate') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM sys.default_constraints dc
       INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
       INNER JOIN sys.tables t ON t.object_id = c.object_id
       INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
       WHERE s.name = N'dbo' AND t.name = N'Expenses' AND c.name = N'CreatedDate')
BEGIN
    ALTER TABLE dbo.Expenses
        ADD CONSTRAINT DF_Expenses_CreatedDate_LegacyFix DEFAULT (SYSUTCDATETIME()) FOR CreatedDate;
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

    INSERT INTO dbo.Expenses (
        GymId, CategoryId, Amount, ExpenseDate, [Description], VendorName,
        PaymentMethod, AttachmentFileId, CreatedBy, CreatedDate)
    VALUES (
        @GymId, @CategoryId, @Amount, @ExpenseDate, @Description, @VendorName,
        @PaymentMethod, @AttachmentFileId, @CreatedBy, SYSUTCDATETIME());
    SET @ExpenseId = SCOPE_IDENTITY();
END
GO
