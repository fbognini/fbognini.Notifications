-- Schema for fbognini.Notifications.Sources.SqlServer 3.0.
-- Apply it yourself: the library never issues DDL, so a read can never rewrite your database.
-- Replace [notification] if you configured a different schema.

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'notification')
    EXEC('CREATE SCHEMA notification');
GO

IF OBJECT_ID('[notification].[Profiles]') IS NULL
CREATE TABLE [notification].[Profiles]
(
    [Channel]  NVARCHAR(64)   NOT NULL,
    [Id]       NVARCHAR(128)  NOT NULL,
    [Payload]  NVARCHAR(MAX)  NOT NULL,
    [UpdatedAt] DATETIMEOFFSET NOT NULL CONSTRAINT [DF_Profiles_UpdatedAt] DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT [PK_Profiles] PRIMARY KEY CLUSTERED ([Channel], [Id])
);
GO

IF OBJECT_ID('[notification].[Templates]') IS NULL
CREATE TABLE [notification].[Templates]
(
    [Id]      NVARCHAR(128) NOT NULL CONSTRAINT [PK_Templates] PRIMARY KEY,
    [Name]    NVARCHAR(256) NOT NULL,
    [Subject] NVARCHAR(512) NOT NULL,
    [Body]    NVARCHAR(MAX) NOT NULL
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Templates_Name')
    CREATE UNIQUE INDEX [IX_Templates_Name] ON [notification].[Templates] ([Name]);
GO

IF OBJECT_ID('[notification].[Queue]') IS NULL
CREATE TABLE [notification].[Queue]
(
    [Id]              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Queue] PRIMARY KEY,
    [Channel]         NVARCHAR(64)   NOT NULL,
    [ConfigurationId] NVARCHAR(128)  NOT NULL,
    [Address]         NVARCHAR(512)  NOT NULL,
    [Payload]         NVARCHAR(MAX)  NOT NULL,
    [CreatedAt]       DATETIMEOFFSET NOT NULL,
    [Processing]      BIT            NOT NULL CONSTRAINT [DF_Queue_Processing] DEFAULT 0,
    [ErrorRetry]      INT            NOT NULL CONSTRAINT [DF_Queue_ErrorRetry] DEFAULT 0,
    [ErrorMessage]    NVARCHAR(MAX)  NULL
);
GO

-- Coming from 2.x? Apply this file first, then migrate-2.x-to-3.0.sql.
