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
    internal class SqlServerEmailQueueService : IEmailQueueService
    {
        private readonly DatabaseSettings settings;

        public SqlServerEmailQueueService(DatabaseSettings settings)
        {
            this.settings = settings;
        }

        private string Table => $"[{settings.Schema}].[QueueEmails]";

        public int InsertQueueEmails(List<Email> emails)
        {
            using var connection = new SqlConnection(settings.ConnectionString);
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
