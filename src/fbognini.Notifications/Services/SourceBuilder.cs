using fbognini.Notifications.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace fbognini.Notifications.Services
{
    public abstract class SourceBuilder
    {
        public IServiceCollection Services { get; set; }

        public SourceBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}
