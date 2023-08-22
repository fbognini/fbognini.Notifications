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
    internal class SqlServerTemplateService : ITemplateService
    {
        private readonly DatabaseSettings settings;

        public SqlServerTemplateService(DatabaseSettings settings)
        {
            this.settings = settings;
        }

        public EmailTemplate GetTemplate(string name)
        {
            using var connection = new SqlConnection(settings.ConnectionString);
            connection.Open();

            var query = @$"SELECT [Id]
                              ,[Name]
                              ,[Subject]
                              ,[Body]
                        FROM [{settings.Schema}].[EmailTemplates]
                        WHERE Name LIKE @Name";

            return connection.QueryFirstOrDefault<EmailTemplate>(query, new { name });
        }

        public EmailTemplate GetTemplateById(string id)
        {
            using var connection = new SqlConnection(settings.ConnectionString);
            connection.Open();

            var query = @$"SELECT [Id]
                              ,[Name]
                              ,[Subject]
                              ,[Body]
                        FROM [{settings.Schema}].[EmailTemplates]
                        WHERE Id = @Id";

            return connection.QueryFirstOrDefault<EmailTemplate>(query, new { id });
        }
    }
}
