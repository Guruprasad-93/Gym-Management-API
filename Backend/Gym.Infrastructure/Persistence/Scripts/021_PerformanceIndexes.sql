/*
  Sprint 3 — Performance indexes for attendance, membership, payment, and audit.
  Idempotent: only creates indexes when missing.
*/

/* ========== ATTENDANCE ========== */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MemberAttendance_Gym_Date' AND object_id = OBJECT_ID(N'dbo.MemberAttendance'))
    CREATE NONCLUSTERED INDEX IX_MemberAttendance_Gym_Date
        ON dbo.MemberAttendance (GymId, AttendanceDate DESC)
        INCLUDE (MemberId, AttendanceStatusId, CheckInAt, CheckOutAt, TrainerId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MemberAttendance_Gym_Member_Date' AND object_id = OBJECT_ID(N'dbo.MemberAttendance'))
    CREATE NONCLUSTERED INDEX IX_MemberAttendance_Gym_Member_Date
        ON dbo.MemberAttendance (GymId, MemberId, AttendanceDate DESC)
        INCLUDE (AttendanceStatusId, CheckInAt, CheckOutAt);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MemberAttendance_OpenSession' AND object_id = OBJECT_ID(N'dbo.MemberAttendance'))
    CREATE NONCLUSTERED INDEX IX_MemberAttendance_OpenSession
        ON dbo.MemberAttendance (GymId, MemberId)
        WHERE CheckOutAt IS NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TrainerAttendance_Gym_Trainer_Open' AND object_id = OBJECT_ID(N'dbo.TrainerAttendance'))
    CREATE NONCLUSTERED INDEX IX_TrainerAttendance_Gym_Trainer_Open
        ON dbo.TrainerAttendance (GymId, TrainerId)
        WHERE CheckOutAt IS NULL;
GO

/* ========== MEMBERSHIP ========== */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Memberships_Gym_Status_EndDate' AND object_id = OBJECT_ID(N'dbo.Memberships'))
    CREATE NONCLUSTERED INDEX IX_Memberships_Gym_Status_EndDate
        ON dbo.Memberships (GymId, Status, EndDate DESC)
        INCLUDE (MemberId, MembershipPlanId, StartDate, Amount);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Memberships_Gym_Member_Status' AND object_id = OBJECT_ID(N'dbo.Memberships'))
    CREATE NONCLUSTERED INDEX IX_Memberships_Gym_Member_Status
        ON dbo.Memberships (GymId, MemberId, Status)
        INCLUDE (EndDate, MembershipPlanId);
GO

/* ========== PAYMENT ========== */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Payments_Gym_PaymentDate' AND object_id = OBJECT_ID(N'dbo.Payments'))
    CREATE NONCLUSTERED INDEX IX_Payments_Gym_PaymentDate
        ON dbo.Payments (GymId, PaymentDate DESC)
        INCLUDE (MemberId, MembershipId, Amount, Status, PaymentMethod);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Payments_Gym_Member' AND object_id = OBJECT_ID(N'dbo.Payments'))
    CREATE NONCLUSTERED INDEX IX_Payments_Gym_Member
        ON dbo.Payments (GymId, MemberId, PaymentDate DESC)
        INCLUDE (Amount, Status, MembershipId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Payments_Gym_Membership' AND object_id = OBJECT_ID(N'dbo.Payments'))
    CREATE NONCLUSTERED INDEX IX_Payments_Gym_Membership
        ON dbo.Payments (GymId, MembershipId);
GO

/* ========== AUDIT (supplemental) ========== */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLogs_Gym_Action_CreatedAt' AND object_id = OBJECT_ID(N'dbo.AuditLogs'))
    CREATE NONCLUSTERED INDEX IX_AuditLogs_Gym_Action_CreatedAt
        ON dbo.AuditLogs (GymId, Action, CreatedAt DESC)
        INCLUDE (UserId, EntityName, EntityId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLogs_Gym_Entity_CreatedAt' AND object_id = OBJECT_ID(N'dbo.AuditLogs'))
    CREATE NONCLUSTERED INDEX IX_AuditLogs_Gym_Entity_CreatedAt
        ON dbo.AuditLogs (GymId, EntityName, CreatedAt DESC)
        INCLUDE (EntityId, UserId, Action);
GO
