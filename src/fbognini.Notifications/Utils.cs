using Dapper;
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

        private static void EnsureTable(SqlConnection connection, string schema)
        {
            var ensure = $@"
                IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{schema}' AND  TABLE_NAME = 'EmailConfigs'))
                BEGIN
                    CREATE TABLE [notification].[EmailConfigs](
	                    [Id] [nvarchar](8) NOT NULL,
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

        private static void EnsureConfiguration(SqlConnection connection, string schema, string id)
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

        public static EmailSettings GetEmailSettings(string id, string connectionString, string schema = "notification")
        {
            using var connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();

                EnsureSchema(connection, schema);
                EnsureTable(connection, schema);
                EnsureConfiguration(connection, schema, id);

                var query = @"SELECT [UseSsl]
                                    ,[SmtpHost]
                                    ,[SmtpPort]
                                    ,[UseAuthentication]
                                    ,[SmtpUsername]
                                    ,[SmtpPassword]
                                    ,[FromEmail]
                                FROM [notification].[EmailConfigs]
                                WHERE Id = @id";
                var settings = connection.Query<EmailSettings>(query, new { id });
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
    }
}
