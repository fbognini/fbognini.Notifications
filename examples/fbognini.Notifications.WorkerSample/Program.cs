using fbognini.Notifications;
using fbognini.Notifications.Sinks.Email;
using fbognini.Notifications.Sinks.Telegram;
using fbognini.Notifications.Sources.AppSettings;
using fbognini.Notifications.WorkerSample;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();

builder.Services
    .AddNotifications(
        configuration => configuration.DynamicCacheTtl = TimeSpan.FromMinutes(2),
        dispatcher => dispatcher.MaxAttempts = 3)
    .AddEmail()
    .AddTelegram(o => o.ParseMode = TelegramParseMode.Html)
    .FromAppSettings(builder.Configuration);

await builder.Build().RunAsync();
