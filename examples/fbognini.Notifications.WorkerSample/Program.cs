using fbognini.Notifications;
using fbognini.Notifications.Sinks.Email;
using fbognini.Notifications.Source.AppSettings;
using fbognini.Notifications.WorkerSample;
using Microsoft.Extensions.Configuration;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddHostedService<Worker>();

        services.AddNotifications()
            .AddEmailService("SUPPORT")
            .FromAppSettings(ctx.Configuration);
    })
    .Build();

host.Run();
