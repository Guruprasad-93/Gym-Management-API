/* Post-MVP tables – created empty for future phases */

IF OBJECT_ID(N'dbo.UserLoginSessions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserLoginSessions
    (
        UserLoginSessionId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        LoginSessionGuid UNIQUEIDENTIFIER NOT NULL,
        DeviceInfo NVARCHAR(256) NULL,
        IpAddress NVARCHAR(50) NULL,
        LoginAt DATETIME2 NOT NULL,
        LogoutAt DATETIME2 NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_UserLoginSessions_IsActive DEFAULT (1),
        CONSTRAINT FK_UserLoginSessions_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE,
        CONSTRAINT UQ_UserLoginSessions_LoginSessionGuid UNIQUE (LoginSessionGuid)
    );
END
GO

IF OBJECT_ID(N'dbo.GymBranches', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.GymBranches
    (
        GymBranchId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        BranchName NVARCHAR(200) NOT NULL,
        Address NVARCHAR(500) NULL,
        Phone NVARCHAR(20) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_GymBranches_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_GymBranches_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.GymSubscriptions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.GymSubscriptions
    (
        GymSubscriptionId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        PlanName NVARCHAR(100) NOT NULL,
        StartDate DATE NOT NULL,
        EndDate DATE NOT NULL,
        Amount DECIMAL(18, 2) NOT NULL,
        Status NVARCHAR(20) NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_GymSubscriptions_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.MemberProgress', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MemberProgress
    (
        MemberProgressId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NOT NULL,
        RecordedDate DATE NOT NULL,
        Notes NVARCHAR(1000) NULL,
        CreatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_MemberProgress_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.MemberAttendance', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MemberAttendance
    (
        MemberAttendanceId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NOT NULL,
        CheckInAt DATETIME2 NOT NULL,
        CheckOutAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_MemberAttendance_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.PaymentMethods', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PaymentMethods
    (
        PaymentMethodId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MethodName NVARCHAR(50) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_PaymentMethods_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_PaymentMethods_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.Invoices', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Invoices
    (
        InvoiceId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        PaymentId INT NULL,
        MemberId INT NOT NULL,
        InvoiceNumber NVARCHAR(50) NOT NULL,
        Amount DECIMAL(18, 2) NOT NULL,
        IssuedAt DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_Invoices_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_Invoices_Payments FOREIGN KEY (PaymentId) REFERENCES dbo.Payments (PaymentId),
        CONSTRAINT FK_Invoices_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId)
    );
END
GO

IF OBJECT_ID(N'dbo.DietPlanFiles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DietPlanFiles
    (
        DietPlanFileId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        DietPlanId INT NOT NULL,
        FileUploadId INT NULL,
        FileName NVARCHAR(256) NOT NULL,
        FilePath NVARCHAR(500) NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_DietPlanFiles_DietPlans FOREIGN KEY (DietPlanId) REFERENCES dbo.DietPlans (DietPlanId) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.Exercises', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Exercises
    (
        ExerciseId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        ExerciseName NVARCHAR(200) NOT NULL,
        Category NVARCHAR(100) NULL,
        Description NVARCHAR(1000) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Exercises_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_Exercises_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.WorkoutExercises', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WorkoutExercises
    (
        WorkoutExerciseId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        WorkoutPlanId INT NOT NULL,
        ExerciseId INT NOT NULL,
        Sets INT NULL,
        Reps INT NULL,
        DurationMinutes INT NULL,
        Notes NVARCHAR(500) NULL,
        SortOrder INT NOT NULL CONSTRAINT DF_WorkoutExercises_SortOrder DEFAULT (0),
        CONSTRAINT FK_WorkoutExercises_Plans FOREIGN KEY (WorkoutPlanId) REFERENCES dbo.WorkoutPlans (WorkoutPlanId) ON DELETE CASCADE,
        CONSTRAINT FK_WorkoutExercises_Exercises FOREIGN KEY (ExerciseId) REFERENCES dbo.Exercises (ExerciseId)
    );
END
GO

IF OBJECT_ID(N'dbo.ProgressPhotos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProgressPhotos
    (
        ProgressPhotoId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NOT NULL,
        PhotoPath NVARCHAR(500) NOT NULL,
        TakenAt DATE NOT NULL,
        Notes NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_ProgressPhotos_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.WeightHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WeightHistory
    (
        WeightHistoryId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NOT NULL,
        WeightKg DECIMAL(6, 2) NOT NULL,
        RecordedAt DATE NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_WeightHistory_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.BMIRecords', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BMIRecords
    (
        BMIRecordId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NOT NULL,
        BmiValue DECIMAL(5, 2) NOT NULL,
        RecordedAt DATE NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_BMIRecords_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.Feedbacks', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Feedbacks
    (
        FeedbackId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NOT NULL,
        TrainerId INT NULL,
        Rating INT NOT NULL,
        Comments NVARCHAR(1000) NULL,
        CreatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_Feedbacks_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.Notifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notifications
    (
        NotificationId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        GymId UNIQUEIDENTIFIER NULL,
        Title NVARCHAR(200) NOT NULL,
        Message NVARCHAR(1000) NOT NULL,
        IsRead BIT NOT NULL CONSTRAINT DF_Notifications_IsRead DEFAULT (0),
        CreatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.Announcements', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Announcements
    (
        AnnouncementId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Content NVARCHAR(2000) NOT NULL,
        PublishedAt DATETIME2 NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Announcements_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_Announcements_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.FileUploads', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FileUploads
    (
        FileUploadId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        UploadedByUserId UNIQUEIDENTIFIER NOT NULL,
        FileName NVARCHAR(256) NOT NULL,
        FilePath NVARCHAR(500) NOT NULL,
        ContentType NVARCHAR(100) NULL,
        FileSizeBytes BIGINT NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_FileUploads_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_FileUploads_Users FOREIGN KEY (UploadedByUserId) REFERENCES dbo.Users (Id)
    );
END
GO

IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs
    (
        AuditLogId BIGINT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NULL,
        UserId UNIQUEIDENTIFIER NULL,
        EntityName NVARCHAR(100) NOT NULL,
        EntityId NVARCHAR(50) NOT NULL,
        Action NVARCHAR(50) NOT NULL,
        OldValues NVARCHAR(MAX) NULL,
        NewValues NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.ActivityLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ActivityLogs
    (
        ActivityLogId BIGINT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NULL,
        UserId UNIQUEIDENTIFIER NULL,
        ActivityType NVARCHAR(100) NOT NULL,
        Description NVARCHAR(1000) NULL,
        CreatedAt DATETIME2 NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.SystemSettings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SystemSettings
    (
        SystemSettingId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NULL,
        SettingKey NVARCHAR(100) NOT NULL,
        SettingValue NVARCHAR(2000) NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT UQ_SystemSettings_Key_Gym UNIQUE (SettingKey, GymId)
    );
END
GO

IF OBJECT_ID(N'dbo.Coupons', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Coupons
    (
        CouponId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        Code NVARCHAR(50) NOT NULL,
        DiscountPercent DECIMAL(5, 2) NULL,
        DiscountAmount DECIMAL(18, 2) NULL,
        ValidFrom DATE NOT NULL,
        ValidTo DATE NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Coupons_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_Coupons_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.Expenses', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Expenses
    (
        ExpenseId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        Category NVARCHAR(100) NOT NULL,
        Amount DECIMAL(18, 2) NOT NULL,
        ExpenseDate DATE NOT NULL,
        Description NVARCHAR(500) NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_Expenses_IsDeleted DEFAULT (0),
        CreatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_Expenses_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.Expenses', N'U') IS NOT NULL AND COL_LENGTH('dbo.Expenses', 'IsDeleted') IS NULL
    ALTER TABLE dbo.Expenses ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Expenses_IsDeleted_004 DEFAULT (0) WITH VALUES;
GO

IF OBJECT_ID(N'dbo.RevenueReports', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RevenueReports
    (
        RevenueReportId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        ReportPeriodStart DATE NOT NULL,
        ReportPeriodEnd DATE NOT NULL,
        TotalRevenue DECIMAL(18, 2) NOT NULL,
        GeneratedAt DATETIME2 NOT NULL,
        ReportData NVARCHAR(MAX) NULL,
        CONSTRAINT FK_RevenueReports_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.SupportTickets', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SupportTickets
    (
        SupportTicketId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NULL,
        UserId UNIQUEIDENTIFIER NOT NULL,
        Subject NVARCHAR(200) NOT NULL,
        Description NVARCHAR(2000) NOT NULL,
        Status NVARCHAR(20) NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_SupportTickets_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id)
    );
END
GO

IF OBJECT_ID(N'dbo.EmailTemplates', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmailTemplates
    (
        EmailTemplateId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        TemplateKey NVARCHAR(100) NOT NULL,
        Subject NVARCHAR(200) NOT NULL,
        Body NVARCHAR(MAX) NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        CONSTRAINT UQ_EmailTemplates_TemplateKey UNIQUE (TemplateKey)
    );
END
GO

IF OBJECT_ID(N'dbo.SmsTemplates', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SmsTemplates
    (
        SmsTemplateId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        TemplateKey NVARCHAR(100) NOT NULL,
        Message NVARCHAR(500) NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        CONSTRAINT UQ_SmsTemplates_TemplateKey UNIQUE (TemplateKey)
    );
END
GO
