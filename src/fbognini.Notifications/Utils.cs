using Dapper;
using fbognini.Notifications.Models;
using fbognini.Notifications.Queries;
using fbognini.Notifications.Services;
using fbognini.Notifications.Settings;
using System;
using System.Data.SqlClient;
using System.Linq;

namespace fbognini.Notifications
{
    public static class Utils
    {
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

        public static EmailConfig GetEmailSettings(string id, string connectionString, string schema = "notification")
        {
            using var connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();

                EnsureSchema(connection, schema);
                EnsureEmailTable(connection, schema);
                EnsureEmailConfiguration(connection, schema, id);

                var query = $@"SELECT [UseSsl]
                                    ,[SmtpHost]
                                    ,[SmtpPort]
                                    ,[UseAuthentication]
                                    ,[SmtpUsername]
                                    ,[SmtpPassword]
                                    ,[FromEmail]
                                FROM [{schema}].[EmailConfigs]
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

        public static SmsConfig GetSmsSettings(string id, string connectionString, string schema = "notification")
        {
            using var connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();

                EnsureSchema(connection, schema);
                EnsureSmsTable(connection, schema);
                EnsureSmsConfiguration(connection, schema, id);

                var query = $@"SELECT [Username]
                                    ,[Password]
                                    ,[Sender]
                                    ,[ServiceId]
                                FROM [{schema}].[SmsConfigs]
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
    }
}
