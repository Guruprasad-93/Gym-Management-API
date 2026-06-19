/*
  Fix sp_SearchPushNotificationCampaigns: CTE was out of scope on the paginated SELECT.
*/

CREATE OR ALTER PROCEDURE dbo.sp_SearchPushNotificationCampaigns
    @GymId UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        NotificationType,
        Title,
        Message,
        MIN(CreatedDate) AS SentDate,
        COUNT(*) AS RecipientCount,
        SUM(CASE WHEN [Status] = N'Failed' THEN 1 ELSE 0 END) AS FailedCount,
        SUM(CASE WHEN [Status] IN (N'Sent', N'Delivered', N'Opened', N'Clicked') THEN 1 ELSE 0 END) AS SentCount
    INTO #Campaigns
    FROM dbo.PushNotifications
    WHERE GymId = @GymId AND NotificationType = N'ManualCampaign'
    GROUP BY NotificationType, Title, Message, CAST(CreatedDate AS DATE);

    SELECT @TotalCount = COUNT(*) FROM #Campaigns;

    SELECT
        NotificationType,
        Title,
        Message,
        SentDate,
        RecipientCount,
        FailedCount,
        SentCount
    FROM #Campaigns
    ORDER BY SentDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;

    DROP TABLE #Campaigns;
END
GO
