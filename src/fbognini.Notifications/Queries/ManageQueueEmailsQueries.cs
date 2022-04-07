using Dapper;
using fbognini.Notifications.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace fbognini.Notifications.Queries
{
    public class ManageQueueEmailsQueries
    {
        private readonly string connectionString;
        private readonly string schema;

        public ManageQueueEmailsQueries(string connectionString, string schema)
        {
            this.connectionString = connectionString;
            this.schema = schema;
        }

        private string Table => $"[{schema}].[QueueEmails]";

        public int InsertQueueEmails(List<Email> emails)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                var query = $"INSERT INTO {Table} ([To], Cc, Bcc, Subject, Body, IsHtml, Attachments, InsertionDate, Processing) VALUES (@To, @Cc, @Bcc, @Subject, @Body, @IsHtml, @Attachments, @InsertionDate, @Processing)";

                var queryparams = emails.Select(x => new
                {
                    x.To,
                    x.Cc,
                    x.Bcc,
                    x.Subject,
                    x.Body,
                    x.IsHtml,
                    x.Attachments,
                    x.InsertionDate,
                    x.Processing
                }).ToList();

                return connection.Execute(query, queryparams);
            }
        }

        
    }
}
