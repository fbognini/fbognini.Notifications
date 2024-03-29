﻿using fbognini.Notifications.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace fbognini.Notifications.Services
{
    public class NotificationsBuilder : SinkBuilder
    {
        internal NotificationsBuilder(IServiceCollection services)
            :base(services)
        {
        }
    }
}
