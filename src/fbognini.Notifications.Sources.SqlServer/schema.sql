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

-- Coming from 2.x? Apply this file first, then migrate-2.x-to-3.0.sql.
