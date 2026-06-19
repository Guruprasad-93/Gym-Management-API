/*
  Prevent duplicate branch codes from surfacing as HTTP 500.
*/

CREATE OR ALTER PROCEDURE dbo.sp_CreateBranch
    @GymId UNIQUEIDENTIFIER,
    @BranchName NVARCHAR(200),
    @BranchCode NVARCHAR(20) = NULL,
    @Address NVARCHAR(500) = NULL,
    @City NVARCHAR(100) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @Email NVARCHAR(256) = NULL,
    @BranchId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @BranchName = LTRIM(RTRIM(@BranchName));
    SET @BranchCode = NULLIF(LTRIM(RTRIM(@BranchCode)), N'');

    IF @BranchName = N''
        THROW 50001, N'Branch name is required.', 1;

    IF @BranchCode IS NOT NULL AND EXISTS (
        SELECT 1
        FROM dbo.Branches
        WHERE GymId = @GymId
          AND IsDeleted = 0
          AND BranchCode = @BranchCode
    )
        THROW 50002, N'A branch with this code already exists. The default branch uses code MAIN — choose a different code.', 1;

    INSERT INTO dbo.Branches (GymId, BranchName, BranchCode, Address, City, Phone, Email)
    VALUES (@GymId, @BranchName, @BranchCode, @Address, @City, @Phone, @Email);

    SET @BranchId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateBranch
    @BranchId INT, @GymId UNIQUEIDENTIFIER,
    @BranchName NVARCHAR(200), @BranchCode NVARCHAR(20) = NULL,
    @Address NVARCHAR(500) = NULL, @City NVARCHAR(100) = NULL,
    @Phone NVARCHAR(20) = NULL, @Email NVARCHAR(256) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @BranchName = LTRIM(RTRIM(@BranchName));
    SET @BranchCode = NULLIF(LTRIM(RTRIM(@BranchCode)), N'');

    IF @BranchName = N''
        THROW 50001, N'Branch name is required.', 1;

    IF @BranchCode IS NOT NULL AND EXISTS (
        SELECT 1
        FROM dbo.Branches
        WHERE GymId = @GymId
          AND IsDeleted = 0
          AND BranchCode = @BranchCode
          AND BranchId <> @BranchId
    )
        THROW 50002, N'A branch with this code already exists. Choose a different code.', 1;

    UPDATE dbo.Branches SET BranchName = @BranchName, BranchCode = @BranchCode, Address = @Address,
        City = @City, Phone = @Phone, Email = @Email, UpdatedDate = SYSUTCDATETIME()
    WHERE BranchId = @BranchId AND GymId = @GymId AND IsDeleted = 0;

    IF @@ROWCOUNT = 0 THROW 50410, 'Branch not found.', 1;
END
GO
