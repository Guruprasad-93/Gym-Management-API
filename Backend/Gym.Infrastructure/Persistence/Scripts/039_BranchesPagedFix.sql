/*
  Fix CTE scope in branch paged procedures.
  A CTE is only valid for the immediately following statement; the original
  sp_GetBranchesPaged / sp_GetBranchTransferHistory tried to reuse Filtered twice.
*/

CREATE OR ALTER PROCEDURE dbo.sp_GetBranchesPaged
    @GymId UNIQUEIDENTIFIER,
    @Search NVARCHAR(100) = NULL,
    @IncludeInactive BIT = 0,
    @PageNumber INT = 1, @PageSize INT = 20,
    @SortColumn NVARCHAR(50) = N'BranchName', @SortDirection NVARCHAR(4) = N'ASC',
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 20;

    ;WITH Filtered AS (
        SELECT b.BranchId, b.GymId, b.BranchName, b.BranchCode, b.Address, b.City, b.Phone, b.Email,
               b.IsActive, b.CreatedDate, b.UpdatedDate,
               bm.UserId AS ManagerUserId, u.Name AS ManagerName,
               (SELECT COUNT(*) FROM dbo.Members m WHERE m.BranchId = b.BranchId AND m.IsDeleted = 0) AS MemberCount,
               (SELECT COUNT(*) FROM dbo.Trainers t WHERE t.BranchId = b.BranchId AND t.IsActive = 1) AS TrainerCount
        FROM dbo.Branches b
        LEFT JOIN dbo.BranchManagers bm ON bm.BranchId = b.BranchId AND bm.GymId = b.GymId AND bm.IsActive = 1
        LEFT JOIN dbo.Users u ON u.Id = bm.UserId
        WHERE b.GymId = @GymId AND b.IsDeleted = 0
          AND (@IncludeInactive = 1 OR b.IsActive = 1)
          AND (@Search IS NULL OR b.BranchName LIKE N'%' + @Search + N'%' OR b.BranchCode LIKE N'%' + @Search + N'%')
    )
    SELECT @TotalCount = COUNT(*) FROM Filtered;

    ;WITH Filtered AS (
        SELECT b.BranchId, b.GymId, b.BranchName, b.BranchCode, b.Address, b.City, b.Phone, b.Email,
               b.IsActive, b.CreatedDate, b.UpdatedDate,
               bm.UserId AS ManagerUserId, u.Name AS ManagerName,
               (SELECT COUNT(*) FROM dbo.Members m WHERE m.BranchId = b.BranchId AND m.IsDeleted = 0) AS MemberCount,
               (SELECT COUNT(*) FROM dbo.Trainers t WHERE t.BranchId = b.BranchId AND t.IsActive = 1) AS TrainerCount
        FROM dbo.Branches b
        LEFT JOIN dbo.BranchManagers bm ON bm.BranchId = b.BranchId AND bm.GymId = b.GymId AND bm.IsActive = 1
        LEFT JOIN dbo.Users u ON u.Id = bm.UserId
        WHERE b.GymId = @GymId AND b.IsDeleted = 0
          AND (@IncludeInactive = 1 OR b.IsActive = 1)
          AND (@Search IS NULL OR b.BranchName LIKE N'%' + @Search + N'%' OR b.BranchCode LIKE N'%' + @Search + N'%')
    )
    SELECT * FROM Filtered
    ORDER BY
        CASE WHEN @SortColumn = N'BranchName' AND @SortDirection = N'ASC' THEN BranchName END ASC,
        CASE WHEN @SortColumn = N'BranchName' AND @SortDirection = N'DESC' THEN BranchName END DESC,
        BranchName ASC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetBranchTransferHistory
    @GymId UNIQUEIDENTIFIER, @EntityType NVARCHAR(20) = NULL, @BranchId INT = NULL,
    @PageNumber INT = 1, @PageSize INT = 20, @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 20;

    ;WITH Filtered AS (
        SELECT t.*, fb.BranchName AS FromBranchName, tb.BranchName AS ToBranchName, u.Name AS TransferredByName,
               CASE t.EntityType
                   WHEN N'Member' THEN (SELECT mu.Name FROM dbo.Members m INNER JOIN dbo.Users mu ON mu.Id = m.UserId WHERE m.MemberId = t.EntityId)
                   WHEN N'Trainer' THEN (SELECT tu.Name FROM dbo.Trainers tr INNER JOIN dbo.Users tu ON tu.Id = tr.UserId WHERE tr.TrainerId = t.EntityId)
               END AS EntityName
        FROM dbo.BranchTransferHistory t
        LEFT JOIN dbo.Branches fb ON fb.BranchId = t.FromBranchId
        LEFT JOIN dbo.Branches tb ON tb.BranchId = t.ToBranchId
        LEFT JOIN dbo.Users u ON u.Id = t.TransferredByUserId
        WHERE t.GymId = @GymId
          AND (@EntityType IS NULL OR t.EntityType = @EntityType)
          AND (@BranchId IS NULL OR t.FromBranchId = @BranchId OR t.ToBranchId = @BranchId)
    )
    SELECT @TotalCount = COUNT(*) FROM Filtered;

    ;WITH Filtered AS (
        SELECT t.*, fb.BranchName AS FromBranchName, tb.BranchName AS ToBranchName, u.Name AS TransferredByName,
               CASE t.EntityType
                   WHEN N'Member' THEN (SELECT mu.Name FROM dbo.Members m INNER JOIN dbo.Users mu ON mu.Id = m.UserId WHERE m.MemberId = t.EntityId)
                   WHEN N'Trainer' THEN (SELECT tu.Name FROM dbo.Trainers tr INNER JOIN dbo.Users tu ON tu.Id = tr.UserId WHERE tr.TrainerId = t.EntityId)
               END AS EntityName
        FROM dbo.BranchTransferHistory t
        LEFT JOIN dbo.Branches fb ON fb.BranchId = t.FromBranchId
        LEFT JOIN dbo.Branches tb ON tb.BranchId = t.ToBranchId
        LEFT JOIN dbo.Users u ON u.Id = t.TransferredByUserId
        WHERE t.GymId = @GymId
          AND (@EntityType IS NULL OR t.EntityType = @EntityType)
          AND (@BranchId IS NULL OR t.FromBranchId = @BranchId OR t.ToBranchId = @BranchId)
    )
    SELECT * FROM Filtered ORDER BY TransferDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO
