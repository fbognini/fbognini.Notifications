-- Migrates fbognini.Notifications 2.x data to the 3.0 schema.
--
-- Run schema.sql first. This script only moves data: it never drops a 2.x table, so you can verify the
-- result and re-run it if needed. Dropping the old tables is a manual step at the bottom.
--
-- Two things do not survive, by design:
--   * SmsConfigs.ServiceId — 2.x stored it but never sent it; MTargetIdentity has no equivalent.
--   * QueueEmails rows have no profile, because 2.x had no such concept. Set @DefaultConfigurationId
--     below to the profile those pending emails should be sent as.
--
-- EmailConfigs and SmsConfigs are migrated against the DDL 2.x created itself, so their shape is certain.
-- EmailTemplates and QueueEmails were never created by 2.x — you made them by hand — so the columns below
-- are the ones its queries used. Adjust those two sections if you named yours differently.

SET XACT_ABORT ON;
SET NOCOUNT ON;

DECLARE @Schema                 SYSNAME       = N'notification';
DECLARE @DefaultConfigurationId NVARCHAR(128) = N'SUPPORT';   -- <-- review before running

IF @DefaultConfigurationId IS NULL OR LEN(@DefaultConfigurationId) = 0
BEGIN
    RAISERROR('Set @DefaultConfigurationId to the profile pending queued emails belong to.', 16, 1);
    RETURN;
END

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

-------------------------------------------------------------------------------
-- 3. Templates.  EmailTemplates -> Templates
-------------------------------------------------------------------------------
IF OBJECT_ID(QUOTENAME(@Schema) + '.[EmailTemplates]') IS NOT NULL
BEGIN
    INSERT INTO [notification].[Templates] ([Id], [Name], [Subject], [Body])
    SELECT old.[Id], old.[Name], old.[Subject], old.[Body]
    FROM [notification].[EmailTemplates] AS old
    WHERE NOT EXISTS (
        SELECT 1 FROM [notification].[Templates] AS t WHERE t.[Id] = old.[Id]);

    PRINT CONCAT('Templates migrated: ', @@ROWCOUNT);
END

-------------------------------------------------------------------------------
-- 4. Pending queue.  QueueEmails -> Queue
--    Only rows that were never picked up are worth carrying over.
-------------------------------------------------------------------------------
IF OBJECT_ID(QUOTENAME(@Schema) + '.[QueueEmails]') IS NOT NULL
BEGIN
    INSERT INTO [notification].[Queue]
        ([Channel], [ConfigurationId], [Address], [Payload], [CreatedAt], [Processing], [ErrorRetry], [ErrorMessage])
    SELECT
        'email',
        @DefaultConfigurationId,
        ISNULL(old.[To], ''),
        (
            SELECT
                @DefaultConfigurationId   AS ConfigurationId,
                old.[To]                  AS [To],
                old.[Cc]                  AS Cc,
                old.[Bcc]                 AS Bcc,
                old.[Subject]             AS [Subject],
                old.[Body]                AS Body,
                CAST(old.[IsHtml] AS BIT) AS IsHtml,
                JSON_QUERY(
                    CASE
                        WHEN old.[Attachments] IS NULL OR LEN(old.[Attachments]) = 0 THEN NULL
                        ELSE (
                            SELECT LTRIM(RTRIM(value))
                            FROM STRING_SPLIT(old.[Attachments], ';')
                            WHERE LEN(LTRIM(RTRIM(value))) > 0
                            FOR JSON PATH
                        )
                    END
                ) AS Attachments
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES
        ),
        CAST(old.[InsertionDate] AS DATETIMEOFFSET),
        0,
        ISNULL(old.[ErrorRetry], 0),
        old.[ErrorMessage]
    FROM [notification].[QueueEmails] AS old
    WHERE old.[Processing] = 0;

    PRINT CONCAT('Queued emails migrated: ', @@ROWCOUNT);
END

COMMIT TRANSACTION;

-------------------------------------------------------------------------------
-- 5. Verify, then clean up by hand.
-------------------------------------------------------------------------------
SELECT [Channel], COUNT(*) AS Profiles FROM [notification].[Profiles] GROUP BY [Channel];
SELECT COUNT(*) AS Templates FROM [notification].[Templates];
SELECT COUNT(*) AS QueuedNotifications FROM [notification].[Queue];

-- Spot-check that a payload binds to what the sink expects before trusting the migration:
-- SELECT TOP 5 [Id], JSON_VALUE([Payload], '$.SmtpHost'), JSON_VALUE([Payload], '$.FromEmail')
-- FROM [notification].[Profiles] WHERE [Channel] = 'email';

-- Once verified, and only then:
-- DROP TABLE [notification].[EmailConfigs];
-- DROP TABLE [notification].[SmsConfigs];
-- DROP TABLE [notification].[EmailTemplates];
-- DROP TABLE [notification].[QueueEmails];
