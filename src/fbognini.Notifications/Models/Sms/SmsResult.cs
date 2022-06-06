using System;
using System.Collections.Generic;
using System.Text;

namespace fbognini.Notifications.Models.Sms
{
    public class SmsResult
    {
        public SmsResult(bool success = true, string message = null)
        {
            Success = success;
            Message = message;
        }

        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
