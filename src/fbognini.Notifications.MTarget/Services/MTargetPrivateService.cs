using fbognini.Notifications.Interfaces;
using fbognini.Notifications.Models;
using fbognini.Notifications.Models.Sms;
using fbognini.Notifications.Services;
using fbognini.Notifications.Settings;
using MTarget;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace fbognini.Notifications.MTarget.Services
{
    public class MTargetPrivateService : BaseMTargetService
    {
        public MTargetPrivateService(DatabaseSettings settings) : base(settings)
        {
        }

        public MTargetPrivateService(string id, DatabaseSettings settings) : base(id, settings)
        {
        }

        public override string RemoteAddress => new MTargetPrivate.WSApiSmsClient().Endpoint.Address.ToString();
    }
}
