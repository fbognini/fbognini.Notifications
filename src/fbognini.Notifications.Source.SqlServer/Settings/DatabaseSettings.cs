using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Notifications.Source.SqlServer.Settings
{
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; } = default!;
        public string Schema { get; set; } = "notification";
    }
}
