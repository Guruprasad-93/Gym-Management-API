/*
  Allow Super Admin to list all gym admins when @GymId is omitted.
*/

CREATE OR ALTER PROCEDURE dbo.sp_GymAdmin_GetAll
    @GymId UNIQUEIDENTIFIER = NULL,
    @Search NVARCHAR(200) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortColumn NVARCHAR(50) = N'Name',
    @SortDirection NVARCHAR(4) = N'ASC',
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 10;
    IF @PageSize > 100 SET @PageSize = 100;

    DECLARE @SearchPattern NVARCHAR(202) = NULL;
    IF @Search IS NOT NULL AND LEN(LTRIM(RTRIM(@Search))) > 0
        SET @SearchPattern = N'%' + LTRIM(RTRIM(@Search)) + N'%';

    SELECT
        u.Id AS UserId,
        u.Name,
        u.Email,
        u.GymId,
        g.Name AS GymName,
        ISNULL(u.IsActive, 1) AS IsActive,
        ISNULL(u.MustChangePassword, 0) AS MustChangePassword,
        u.CreatedDate
    INTO #Filtered
    FROM dbo.Users u
    INNER JOIN dbo.UserRoles ur ON u.Id = ur.UserId
    INNER JOIN dbo.Roles r ON ur.RoleId = r.RoleId AND r.RoleName = N'GymAdmin'
    LEFT JOIN dbo.Gyms g ON g.GymId = u.GymId
    WHERE (@GymId IS NULL OR u.GymId = @GymId)
      AND (@SearchPattern IS NULL OR u.Name LIKE @SearchPattern OR u.Email LIKE @SearchPattern OR g.Name LIKE @SearchPattern);

    SET @TotalCount = (SELECT COUNT(*) FROM #Filtered);

    SELECT UserId, Name, Email, GymId, GymName, IsActive, MustChangePassword, CreatedDate
    FROM #Filtered
    ORDER BY
        CASE WHEN @SortDirection = N'ASC' AND @SortColumn = N'Name' THEN Name END ASC,
        CASE WHEN @SortDirection = N'DESC' AND @SortColumn = N'Name' THEN Name END DESC,
        CASE WHEN @SortDirection = N'ASC' AND @SortColumn = N'Email' THEN Email END ASC,
        CASE WHEN @SortDirection = N'DESC' AND @SortColumn = N'Email' THEN Email END DESC,
        CASE WHEN @SortDirection = N'ASC' AND @SortColumn = N'CreatedDate' THEN CreatedDate END ASC,
        CASE WHEN @SortDirection = N'DESC' AND @SortColumn = N'CreatedDate' THEN CreatedDate END DESC,
        Name ASC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    DROP TABLE #Filtered;
END
GO
