-- Migrates fbognini.Notifications 2.x data to the 3.0 schema.
--
-- Run schema.sql first. This script only moves data: it never drops a 2.x table, so you can verify the
-- result and re-run it if needed. Dropping the old tables is a manual step at the bottom.
--
-- Three things do not survive, by design:
--   * SmsConfigs.ServiceId — 2.x stored it but never sent it; MTargetIdentity has no equivalent.
--   * EmailTemplates — 3.0 has no template store. The table is left untouched; migrate it yourself if
--     templating comes back.
--   * QueueEmails — 3.0 has no queue. Pending rows stay where they are, so drain them with 2.x before
--     switching over.
--
-- EmailConfigs and SmsConfigs are migrated against the DDL 2.x created itself, so their shape is certain.

SET XACT_ABORT ON;
SET NOCOUNT ON;

DECLARE @Schema SYSNAME = N'notification';

BEGIN TRANSACTION;

-------------------------------------------------------------------------------
-- 1. Email profiles.  EmailConfigs -> Profiles (channel 'email')
-------------------------------------------------------------------------------
IF OBJECT_ID(QUOTENAME(@Schema) + '.[EmailConfigs]') IS NOT NULL
BEGIN
    INSERT INTO [notification].[Profiles] ([Channel], [Id], [Payload])
    SELECT
        'email',
        old.[Id],
        (
            SELECT
                old.[SmtpHost]                        AS SmtpHost,
                old.[SmtpPort]                        AS SmtpPort,
                CAST(old.[UseSsl] AS BIT)             AS UseSsl,
                CAST(old.[UseAuthentication] AS BIT)  AS UseAuthentication,
                old.[SmtpUsername]                    AS SmtpUsername,
                old.[SmtpPassword]                    AS SmtpPassword,
                old.[FromEmail]                       AS FromEmail
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES
        )
    FROM [notification].[EmailConfigs] AS old
    WHERE NOT EXISTS (
        SELECT 1 FROM [notification].[Profiles] AS p
        WHERE p.[Channel] = 'email' AND p.[Id] = old.[Id]);

    PRINT CONCAT('Email profiles migrated: ', @@ROWCOUNT);
END

-------------------------------------------------------------------------------
-- 2. MTarget profiles.  SmsConfigs -> Profiles (channel 'mtarget')
--    The channel is the provider, not the medium: a second SMS provider gets its own channel and its
--    own identity shape rather than colliding with this one.
-------------------------------------------------------------------------------
IF OBJECT_ID(QUOTENAME(@Schema) + '.[SmsConfigs]') IS NOT NULL
BEGIN
    INSERT INTO [notification].[Profiles] ([Channel], [Id], [Payload])
    SELECT
        'mtarget',
        old.[Id],
        (
            SELECT
                old.[Username] AS Username,
                old.[Password] AS Password,
                old.[Sender]   AS Sender
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES
        )
    FROM [notification].[SmsConfigs] AS old
    WHERE NOT EXISTS (
        SELECT 1 FROM [notification].[Profiles] AS p
        WHERE p.[Channel] = 'mtarget' AND p.[Id] = old.[Id]);

    PRINT CONCAT('MTarget profiles migrated: ', @@ROWCOUNT);

    IF EXISTS (SELECT 1 FROM [notification].[SmsConfigs] WHERE [ServiceId] IS NOT NULL AND LEN([ServiceId]) > 0)
        PRINT 'NOTE: SmsConfigs.ServiceId held values but has no 3.0 equivalent and was not migrated.';
END

COMMIT TRANSACTION;

-------------------------------------------------------------------------------
-- 3. Verify, then clean up by hand.
-------------------------------------------------------------------------------
SELECT [Channel], COUNT(*) AS Profiles FROM [notification].[Profiles] GROUP BY [Channel];

IF OBJECT_ID(QUOTENAME(@Schema) + '.[QueueEmails]') IS NOT NULL
    SELECT COUNT(*) AS PendingQueueEmailsLeftBehind
    FROM [notification].[QueueEmails] WHERE [Processing] = 0;

-- Spot-check that a payload binds to what the sink expects before trusting the migration:
-- SELECT TOP 5 [Id], JSON_VALUE([Payload], '$.SmtpHost'), JSON_VALUE([Payload], '$.FromEmail')
-- FROM [notification].[Profiles] WHERE [Channel] = 'email';

-- Once verified, and only then:
-- DROP TABLE [notification].[EmailConfigs];
-- DROP TABLE [notification].[SmsConfigs];
