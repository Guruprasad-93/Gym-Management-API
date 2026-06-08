/*
  File Management Module — Files, MemberFiles, TrainerFiles
*/

IF OBJECT_ID(N'dbo.Files', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Files
    (
        FileId BIGINT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        FileCategory NVARCHAR(50) NOT NULL,
        StorageProvider NVARCHAR(20) NOT NULL,
        StoragePath NVARCHAR(1000) NOT NULL,
        PublicUrl NVARCHAR(500) NOT NULL,
        OriginalFileName NVARCHAR(255) NOT NULL,
        ContentType NVARCHAR(100) NOT NULL,
        FileSizeBytes BIGINT NOT NULL,
        Width INT NULL,
        Height INT NULL,
        UploadedByUserId UNIQUEIDENTIFIER NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Files_CreatedAt DEFAULT (SYSUTCDATETIME()),
        IsDeleted BIT NOT NULL CONSTRAINT DF_Files_IsDeleted DEFAULT (0),
        CONSTRAINT FK_Files_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId)
    );
    CREATE INDEX IX_Files_GymId_Category ON dbo.Files (GymId, FileCategory) WHERE IsDeleted = 0;
END
GO

IF OBJECT_ID(N'dbo.MemberFiles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MemberFiles
    (
        MemberFileId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        MemberId INT NOT NULL,
        FileId BIGINT NOT NULL,
        GymId UNIQUEIDENTIFIER NOT NULL,
        FileCategory NVARCHAR(50) NOT NULL,
        DietPlanId INT NULL,
        AssignedDietPlanId INT NULL,
        WorkoutPlanId INT NULL,
        AssignedWorkoutPlanId INT NULL,
        Notes NVARCHAR(500) NULL,
        TakenAt DATE NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_MemberFiles_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_MemberFiles_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId) ON DELETE CASCADE,
        CONSTRAINT FK_MemberFiles_Files FOREIGN KEY (FileId) REFERENCES dbo.Files (FileId)
    );
    CREATE INDEX IX_MemberFiles_Member ON dbo.MemberFiles (MemberId, FileCategory);
END
GO

IF OBJECT_ID(N'dbo.TrainerFiles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TrainerFiles
    (
        TrainerFileId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        TrainerId INT NOT NULL,
        FileId BIGINT NOT NULL,
        GymId UNIQUEIDENTIFIER NOT NULL,
        FileCategory NVARCHAR(50) NOT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_TrainerFiles_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_TrainerFiles_Trainers FOREIGN KEY (TrainerId) REFERENCES dbo.Trainers (TrainerId) ON DELETE CASCADE,
        CONSTRAINT FK_TrainerFiles_Files FOREIGN KEY (FileId) REFERENCES dbo.Files (FileId)
    );
    CREATE INDEX IX_TrainerFiles_Trainer ON dbo.TrainerFiles (TrainerId, FileCategory);
END
GO

IF COL_LENGTH('dbo.Members', 'ProfilePhotoFileId') IS NULL
    ALTER TABLE dbo.Members ADD ProfilePhotoFileId BIGINT NULL;
GO
IF COL_LENGTH('dbo.Trainers', 'ProfilePhotoFileId') IS NULL
    ALTER TABLE dbo.Trainers ADD ProfilePhotoFileId BIGINT NULL;
GO
IF COL_LENGTH('dbo.Gyms', 'LogoFileId') IS NULL
    ALTER TABLE dbo.Gyms ADD LogoFileId BIGINT NULL;
GO

CREATE OR ALTER PROCEDURE dbo.sp_File_Create
    @GymId UNIQUEIDENTIFIER,
    @FileCategory NVARCHAR(50),
    @StorageProvider NVARCHAR(20),
    @StoragePath NVARCHAR(1000),
    @PublicUrl NVARCHAR(500),
    @OriginalFileName NVARCHAR(255),
    @ContentType NVARCHAR(100),
    @FileSizeBytes BIGINT,
    @Width INT = NULL,
    @Height INT = NULL,
    @UploadedByUserId UNIQUEIDENTIFIER = NULL,
    @FileId BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Files (GymId, FileCategory, StorageProvider, StoragePath, PublicUrl, OriginalFileName, ContentType, FileSizeBytes, Width, Height, UploadedByUserId)
    VALUES (@GymId, @FileCategory, @StorageProvider, @StoragePath, @PublicUrl, @OriginalFileName, @ContentType, @FileSizeBytes, @Width, @Height, @UploadedByUserId);
    SET @FileId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_File_GetById
    @FileId BIGINT,
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @GymId IS NULL
        THROW 50001, 'GymId is required.', 1;

    SELECT FileId, GymId, FileCategory, StorageProvider, StoragePath, PublicUrl, OriginalFileName, ContentType, FileSizeBytes, Width, Height, UploadedByUserId, CreatedAt
    FROM dbo.Files
    WHERE FileId = @FileId AND IsDeleted = 0
      AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_File_UpdatePublicUrl
    @FileId BIGINT,
    @GymId UNIQUEIDENTIFIER,
    @PublicUrl NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Files SET PublicUrl = @PublicUrl WHERE FileId = @FileId AND GymId = @GymId AND IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_File_SoftDelete
    @FileId BIGINT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Files SET IsDeleted = 1 WHERE FileId = @FileId AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_MemberFile_Create
    @MemberId INT,
    @FileId BIGINT,
    @GymId UNIQUEIDENTIFIER,
    @FileCategory NVARCHAR(50),
    @DietPlanId INT = NULL,
    @AssignedDietPlanId INT = NULL,
    @WorkoutPlanId INT = NULL,
    @AssignedWorkoutPlanId INT = NULL,
    @Notes NVARCHAR(500) = NULL,
    @TakenAt DATE = NULL,
    @MemberFileId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Members WHERE MemberId = @MemberId AND GymId = @GymId AND IsDeleted = 0)
        THROW 50004, 'Member not found.', 1;

    INSERT INTO dbo.MemberFiles (MemberId, FileId, GymId, FileCategory, DietPlanId, AssignedDietPlanId, WorkoutPlanId, AssignedWorkoutPlanId, Notes, TakenAt)
    VALUES (@MemberId, @FileId, @GymId, @FileCategory, @DietPlanId, @AssignedDietPlanId, @WorkoutPlanId, @AssignedWorkoutPlanId, @Notes, @TakenAt);
    SET @MemberFileId = SCOPE_IDENTITY();

    IF @FileCategory = N'MemberProfilePhoto'
    BEGIN
        UPDATE dbo.Members SET ProfilePhotoFileId = @FileId WHERE MemberId = @MemberId AND GymId = @GymId;
    END
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_TrainerFile_Create
    @TrainerId INT,
    @FileId BIGINT,
    @GymId UNIQUEIDENTIFIER,
    @FileCategory NVARCHAR(50),
    @TrainerFileId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Trainers WHERE TrainerId = @TrainerId AND GymId = @GymId)
        THROW 50020, 'Trainer not found.', 1;

    INSERT INTO dbo.TrainerFiles (TrainerId, FileId, GymId, FileCategory)
    VALUES (@TrainerId, @FileId, @GymId, @FileCategory);
    SET @TrainerFileId = SCOPE_IDENTITY();

    IF @FileCategory = N'TrainerProfilePhoto'
        UPDATE dbo.Trainers SET ProfilePhotoFileId = @FileId WHERE TrainerId = @TrainerId AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Gym_SetLogoFile
    @GymId UNIQUEIDENTIFIER,
    @FileId BIGINT,
    @PublicUrl NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Gyms SET LogoFileId = @FileId, LogoUrl = @PublicUrl WHERE GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_MemberFiles_GetByMember
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER = NULL,
    @FileCategory NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        mf.MemberFileId, mf.MemberId, mf.FileId, mf.GymId, mf.FileCategory,
        mf.DietPlanId, mf.AssignedDietPlanId, mf.WorkoutPlanId, mf.AssignedWorkoutPlanId,
        mf.Notes, mf.TakenAt, mf.CreatedAt,
        f.PublicUrl, f.OriginalFileName, f.ContentType, f.FileSizeBytes, f.Width, f.Height
    FROM dbo.MemberFiles mf
    INNER JOIN dbo.Files f ON f.FileId = mf.FileId AND f.IsDeleted = 0
    WHERE mf.MemberId = @MemberId
      AND (mf.GymId = @GymId)
      AND (@FileCategory IS NULL OR mf.FileCategory = @FileCategory)
    ORDER BY mf.CreatedAt DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_TrainerFiles_GetByTrainer
    @TrainerId INT,
    @GymId UNIQUEIDENTIFIER = NULL,
    @FileCategory NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        tf.TrainerFileId, tf.TrainerId, tf.FileId, tf.GymId, tf.FileCategory, tf.CreatedAt,
        f.PublicUrl, f.OriginalFileName, f.ContentType, f.FileSizeBytes, f.Width, f.Height
    FROM dbo.TrainerFiles tf
    INNER JOIN dbo.Files f ON f.FileId = tf.FileId AND f.IsDeleted = 0
    WHERE tf.TrainerId = @TrainerId
      AND (tf.GymId = @GymId)
      AND (@FileCategory IS NULL OR tf.FileCategory = @FileCategory)
    ORDER BY tf.CreatedAt DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_File_GetGymLogo
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT f.FileId, f.GymId, f.FileCategory, f.StorageProvider, f.StoragePath, f.PublicUrl,
           f.OriginalFileName, f.ContentType, f.FileSizeBytes, f.Width, f.Height, f.UploadedByUserId, f.CreatedAt
    FROM dbo.Gyms g
    INNER JOIN dbo.Files f ON f.FileId = g.LogoFileId AND f.IsDeleted = 0
    WHERE g.GymId = @GymId;
END
GO
