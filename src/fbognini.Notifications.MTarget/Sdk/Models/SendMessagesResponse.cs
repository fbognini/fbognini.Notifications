using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace fbognini.Notifications.MTarget.Sdk.Models
{
    public class SendMessagesResponse
    {
        public List<MessageResponse> Results { get; set; }
    }
}
