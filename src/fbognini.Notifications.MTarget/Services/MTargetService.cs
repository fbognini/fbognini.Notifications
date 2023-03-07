﻿using fbognini.Notifications.Interfaces;
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
    public class MTargetService : BaseMTargetService
    {
        public MTargetService(DatabaseSettings settings) : base(settings)
        {
        }

        public MTargetService(string id, DatabaseSettings settings) : base(id, settings)
        {
        }

        public override string RemoteAddress => new WSApiSmsClient().Endpoint.Address.ToString();
    }
}
