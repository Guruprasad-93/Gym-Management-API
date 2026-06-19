/*
  Fix CTE scope in sp_GetLeadsPaged.
  A CTE is only valid for the immediately following statement; the original
  procedure tried to reuse Filtered for both COUNT and paged SELECT.
*/

CREATE OR ALTER PROCEDURE dbo.sp_GetLeadsPaged
    @GymId UNIQUEIDENTIFIER = NULL,
    @TrainerId INT = NULL,
    @Search NVARCHAR(200) = NULL,
    @Status NVARCHAR(30) = NULL,
    @LeadSource NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortColumn NVARCHAR(50) = N'CreatedDate',
    @SortDirection NVARCHAR(4) = N'DESC',
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 10;
    IF @PageSize > 500 SET @PageSize = 500;

    DECLARE @SearchPattern NVARCHAR(202) = NULL;
    IF @Search IS NOT NULL AND LEN(LTRIM(RTRIM(@Search))) > 0
        SET @SearchPattern = N'%' + LTRIM(RTRIM(@Search)) + N'%';

    ;WITH Filtered AS (
        SELECT l.LeadId, l.GymId, l.FullName, l.MobileNumber, l.Email, l.Gender, l.Age, l.[Address],
               l.LeadSource, l.InterestedPlanId, mp.PlanName AS InterestedPlanName,
               l.[Status], l.AssignedTrainerId, tu.Name AS AssignedTrainerName,
               l.Notes, l.ConvertedMemberId, l.CreatedDate, l.CreatedBy, l.UpdatedDate
        FROM dbo.Leads l
        LEFT JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = l.InterestedPlanId
        LEFT JOIN dbo.Trainers tr ON tr.TrainerId = l.AssignedTrainerId
        LEFT JOIN dbo.Users tu ON tu.Id = tr.UserId
        WHERE l.IsDeleted = 0
          AND (@GymId IS NULL OR l.GymId = @GymId)
          AND (@TrainerId IS NULL OR l.AssignedTrainerId = @TrainerId)
          AND (@Status IS NULL OR l.[Status] = @Status)
          AND (@LeadSource IS NULL OR l.LeadSource = @LeadSource)
          AND (@SearchPattern IS NULL OR l.FullName LIKE @SearchPattern OR l.MobileNumber LIKE @SearchPattern
               OR l.Email LIKE @SearchPattern OR tu.Name LIKE @SearchPattern)
    )
    SELECT @TotalCount = COUNT(*) FROM Filtered;

    ;WITH Filtered AS (
        SELECT l.LeadId, l.GymId, l.FullName, l.MobileNumber, l.Email, l.Gender, l.Age, l.[Address],
               l.LeadSource, l.InterestedPlanId, mp.PlanName AS InterestedPlanName,
               l.[Status], l.AssignedTrainerId, tu.Name AS AssignedTrainerName,
               l.Notes, l.ConvertedMemberId, l.CreatedDate, l.CreatedBy, l.UpdatedDate
        FROM dbo.Leads l
        LEFT JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = l.InterestedPlanId
        LEFT JOIN dbo.Trainers tr ON tr.TrainerId = l.AssignedTrainerId
        LEFT JOIN dbo.Users tu ON tu.Id = tr.UserId
        WHERE l.IsDeleted = 0
          AND (@GymId IS NULL OR l.GymId = @GymId)
          AND (@TrainerId IS NULL OR l.AssignedTrainerId = @TrainerId)
          AND (@Status IS NULL OR l.[Status] = @Status)
          AND (@LeadSource IS NULL OR l.LeadSource = @LeadSource)
          AND (@SearchPattern IS NULL OR l.FullName LIKE @SearchPattern OR l.MobileNumber LIKE @SearchPattern
               OR l.Email LIKE @SearchPattern OR tu.Name LIKE @SearchPattern)
    )
    SELECT * FROM Filtered
    ORDER BY
        CASE WHEN @SortColumn = N'FullName' AND @SortDirection = N'ASC' THEN FullName END ASC,
        CASE WHEN @SortColumn = N'FullName' AND @SortDirection = N'DESC' THEN FullName END DESC,
        CASE WHEN @SortColumn = N'Status' AND @SortDirection = N'ASC' THEN [Status] END ASC,
        CASE WHEN @SortColumn = N'Status' AND @SortDirection = N'DESC' THEN [Status] END DESC,
        CASE WHEN @SortColumn = N'LeadSource' AND @SortDirection = N'ASC' THEN LeadSource END ASC,
        CASE WHEN @SortColumn = N'LeadSource' AND @SortDirection = N'DESC' THEN LeadSource END DESC,
        CASE WHEN @SortColumn = N'CreatedDate' AND @SortDirection = N'ASC' THEN CreatedDate END ASC,
        CASE WHEN @SortColumn = N'CreatedDate' AND @SortDirection = N'DESC' THEN CreatedDate END DESC,
        LeadId DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO
