using fbognini.Notifications.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace fbognini.Notifications.Services
{
    public abstract class SinkBuilder
    {
        public IServiceCollection Services { get; set; }

        public SinkBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}
