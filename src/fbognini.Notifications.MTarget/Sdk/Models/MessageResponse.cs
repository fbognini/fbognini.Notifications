using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace fbognini.Notifications.Sinks.MTarget.Sdk.Models
{
    public class MessageResponse
    {
        public string MSISDNS { get; set; }
        [JsonPropertyName("smscount")]
        public string SmsCount { get; set; }
        public string Code { get; set; }
        public string Reason { get; set; }
        public string Ticket { get; set; }
    }
}
