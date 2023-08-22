using Dapper;
using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Models;
using fbognini.Notifications.Settings;
using fbognini.Notifications.Source.SqlServer.Settings;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Notifications.Source.SqlServer
{
    internal class SqlServerSettingsProvider : ISettingsProvider
    {
        private readonly DatabaseSettings databaseSettings;

        public SqlServerSettingsProvider(DatabaseSettings databaseSettings)
        {
            this.databaseSettings = databaseSettings;
        }

        public EmailConfig GetEmailSettings(string id)
        {
            using var connection = new SqlConnection(databaseSettings.ConnectionString);
            try
            {
                connection.Open();

                EnsureSchema(connection, databaseSettings.Schema);
                EnsureEmailTable(connection, databaseSettings.Schema);
                EnsureEmailConfiguration(connection, databaseSettings.Schema, id);

                var query = $@"SELECT [UseSsl]
                                    ,[SmtpHost]
                                    ,[SmtpPort]
                                    ,[UseAuthentication]
                                    ,[SmtpUsername]
                                    ,[SmtpPassword]
                                    ,[FromEmail]
                                FROM [{databaseSettings.Schema}].[EmailConfigs]
                                WHERE Id = @id";
                var settings = connection.Query<EmailConfig>(query, new { id });
                if (settings.Count() != 1)
                    throw new Exception($"{settings.Count()} email configurations founded!");

                connection.Close();
                return settings.FirstOrDefault();
            }
            catch (Exception ex)
            {
                connection.Close();
                throw;
            }
        }

        public SmsConfig GetSmsSettings(string id)
        {
            using var connection = new SqlConnection(databaseSettings.ConnectionString);
            try
            {
                connection.Open();

                EnsureSchema(connection, databaseSettings.Schema);
                EnsureSmsTable(connection, databaseSettings.Schema);
                EnsureSmsConfiguration(connection, databaseSettings.Schema, id);

                var query = $@"SELECT [Username]
                                    ,[Password]
                                    ,[Sender]
                                    ,[ServiceId]
                                FROM [{databaseSettings.Schema}].[SmsConfigs]
                                WHERE Id = @id";
                var settings = connection.Query<SmsConfig>(query, new { id });
                if (settings.Count() != 1)
                    throw new Exception($"{settings.Count()} sms configurations founded!");

                connection.Close();
                return settings.FirstOrDefault();
            }
            catch (Exception ex)
            {
                connection.Close();
                throw;
            }

        }

        private static void EnsureSchema(SqlConnection connection, string schema)
        {
            var ensure = $@"
                    IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = '{schema}')
                    BEGIN
	                    EXEC('CREATE SCHEMA {schema}')
                    END
                    ";

            connection.Execute(ensure);
        }

        private static void EnsureEmailTable(SqlConnection connection, string schema)
        {
            var ensure = $@"
                IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{schema}' AND  TABLE_NAME = 'EmailConfigs'))
                BEGIN
                    CREATE TABLE [notification].[EmailConfigs](
	                    [Id] [nvarchar](50) NOT NULL,
	                    [UseSsl] [bit] NOT NULL,
	                    [SmtpHost] [nvarchar](100) NOT NULL,
	                    [SmtpPort] [int] NOT NULL,
	                    [UseAuthentication] [bit] NOT NULL,
	                    [SmtpUsername] [nvarchar](100) NULL,
	                    [SmtpPassword] [nvarchar](100) NULL,
	                    [FromEmail] [nvarchar](100) NOT NULL
                    ) ON [PRIMARY]
                END
                ";

            connection.Execute(ensure);
        }

        private static void EnsureSmsTable(SqlConnection connection, string schema)
        {
            var ensure = $@"
                IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{schema}' AND  TABLE_NAME = 'SmsConfigs'))
                BEGIN
                    CREATE TABLE [notification].[SmsConfigs](
	                    [Id] [nvarchar](50) NOT NULL,
	                    [Username] [nvarchar](100) NULL,
	                    [Password] [nvarchar](100) NULL,
	                    [Sender] [nvarchar](100) NULL,
	                    [ServiceId] [nvarchar](100) NULL
                    ) ON [PRIMARY]
                END
                ";

            connection.Execute(ensure);
        }

        private static void EnsureEmailConfiguration(SqlConnection connection, string schema, string id)
        {
            var ensure = $@"
                IF (NOT EXISTS (SELECT * FROM [{schema}].[EmailConfigs] WHERE Id = @id))
                BEGIN
                    
                    INSERT INTO [{schema}].[EmailConfigs]
                               ([Id]
                               ,[UseSsl]
                               ,[SmtpHost]
                               ,[SmtpPort]
                               ,[UseAuthentication]
                               ,[SmtpUsername]
                               ,[SmtpPassword]
                               ,[FromEmail])
                         VALUES
                               (@id
                               ,0
                               ,'localhost'
                               ,25
                               ,0
                               ,NULL
                               ,NULL
                               ,'from@email.com')
                END
                ";

            connection.Execute(ensure, new { id });
        }

        private static void EnsureSmsConfiguration(SqlConnection connection, string schema, string id)
        {
            var ensure = $@"
                IF (NOT EXISTS (SELECT * FROM [{schema}].[SmsConfigs] WHERE Id = @id))
                BEGIN
                    
                    INSERT INTO [{schema}].[SmsConfigs]
                               ([Id]
                               ,[Username]
                               ,[Password]
                               ,[Sender]
                               ,[ServiceId])
                         VALUES
                               (@id
                               ,''
                               ,''
                               ,''
                               ,'')
                END
                ";

            connection.Execute(ensure, new { id });
        }
    }
}
