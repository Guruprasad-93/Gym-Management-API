/*
  Refresh token reuse detection for rotation hardening.
*/

CREATE OR ALTER PROCEDURE dbo.sp_RefreshToken_GetRevokedByToken
    @Token NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (1) UserId
    FROM dbo.RefreshTokens
    WHERE Token = @Token AND RevokedAt IS NOT NULL;
END
GO
