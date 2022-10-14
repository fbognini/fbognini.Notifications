using Dapper;
using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace fbognini.Notifications.Services
{
    internal class TemplateService : ITemplateService
    {
        public string ConnectionString { get; private set; }
        public string Schema { get; private set; }

        public void LoadConfiguration(string connectionString, string schema)
        {
            ConnectionString = connectionString;
            Schema = schema;
        }

        public EmailTemplate GetTemplate(string name)
        {
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();

            var query = @$"SELECT [Id]
                              ,[Name]
                              ,[Subject]
                              ,[Body]
                        FROM [{Schema}].[EmailTemplates]
                        WHERE Name LIKE @Name";

            return connection.QueryFirstOrDefault<EmailTemplate>(query, new { name });
        }
        
        public EmailTemplate GetTemplateById(string id)
        {
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();

            var query = @$"SELECT [Id]
                              ,[Name]
                              ,[Subject]
                              ,[Body]
                        FROM [{Schema}].[EmailTemplates]
                        WHERE Id = @Id";

            return connection.QueryFirstOrDefault<EmailTemplate>(query, new { id });
        }

    }
}
