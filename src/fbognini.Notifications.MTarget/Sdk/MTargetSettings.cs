using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Notifications.Sinks.MTarget.Sdk
{
    public enum MTargetEnvironment
    {
        PRIVATE,
        PUBLIC
    }

    public class MTargetSettings
    {
        private readonly Dictionary<MTargetEnvironment, string> Urls = new()
        {
            [MTargetEnvironment.PRIVATE] = "https://api-2.mtarget.fr/",
            [MTargetEnvironment.PUBLIC] = "https://api-public-2.mtarget.fr/",
        };

        public string Username { get; set; }
        public string Password { get; set; }
        public string Sender { get; set; }
        public MTargetEnvironment? Environment { get; set; }

        internal string GetUrl() => Urls[Environment ?? MTargetEnvironment.PRIVATE];
    }
}
