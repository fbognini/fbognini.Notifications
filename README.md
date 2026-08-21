# fbognini.Notifications

Multi-channel notifications for .NET, composed from independent packages: one per **sink** (a channel
you send through) and one per **source** (where configuration comes from).

Adding a channel is a new package. It does not require a change to the core, which is the whole point.

```csharp
builder.Services.AddNotifications()
    .AddEmail()
    .AddTelegram()
    .FromAppSettings(builder.Configuration);
```

```csharp
await email.SendAsync(new EmailMessage
{
    ConfigurationId = "SUPPORT",
    To = "customer@example.com",
    Subject = "Your order shipped",
    Body = "<p>It's on its way.</p>",
    IsHtml = true,
}, cancellationToken);
```

Targets `net8.0` and `net10.0`.

## Packages

| Package | What it gives you |
|---|---|
| `fbognini.Notifications` | Contracts, configuration resolution, dispatcher. Required. |
| `fbognini.Notifications.Sinks.Email` | SMTP, via MailKit |
| `fbognini.Notifications.Sinks.Telegram` | Telegram Bot API |
| `fbognini.Notifications.Sinks.MTarget` | MTarget SMS |
| `fbognini.Notifications.Sources.AppSettings` | Static profiles bound from `IConfiguration` |
| `fbognini.Notifications.Sources.SqlServer` | Dynamic profiles on SQL Server |

You need the core, at least one sink, and at least one source. A composition missing any of these fails
when the host starts, not on the first send.

---

## The two names you will mix up

| | What it is | Where you set it |
|---|---|---|
| **Sink name** | *Which implementation.* `"email"`, `"telegram"`, `"backup-smtp"`. | `AddEmail(o => o.Name = "backup-smtp")` — only needed when you register two sinks on the same channel. |
| **`ConfigurationId`** | *Which credentials.* `"SUPPORT"`, `"BILLING"`, `"TENANT-42"`. | On every send. |

One sink serves any number of profiles. Nothing on a sink instance holds a profile, so concurrent sends
for different tenants cannot pick up each other's credentials.

---

## Configuring a channel

Every profile lives under `Notifications:{channel}:{id}`, and its shape is the identity type the sink
declares. Get the shape wrong and the host tells you at startup.

### Email — channel `email`

```json
{
  "Notifications": {
    "email": {
      "SUPPORT": {
        "SmtpHost": "smtp.example.com",
        "SmtpPort": 587,
        "UseSsl": true,
        "UseAuthentication": true,
        "SmtpUsername": "support@example.com",
        "SmtpPassword": "…",
        "FromEmail": "support@example.com",
        "FromName": "Example Support",
        "ReplyToEmail": "help@example.com"
      }
    }
  }
}
```

| Field | Required | Notes |
|---|---|---|
| `SmtpHost` | yes | |
| `SmtpPort` | no | defaults to 25 |
| `UseSsl` | no | STARTTLS when true |
| `UseAuthentication` | no | when true, username and password are both required |
| `SmtpUsername`, `SmtpPassword` | conditional | |
| `FromEmail` | yes | |
| `FromName`, `ReplyToEmail` | no | |

```csharp
builder.Services.AddNotifications()
    .AddEmail()
    .FromAppSettings(builder.Configuration);
```

`IEmailSender` gives you the full channel: cc, bcc, attachments and HTML.

### Telegram — channel `telegram`

```json
{
  "Notifications": {
    "telegram": {
      "ALERTS": { "BotToken": "123456:ABC-DEF…" }
    }
  }
}
```

```csharp
builder.Services.AddNotifications()
    .AddTelegram(o => o.ParseMode = TelegramParseMode.Html)
    .FromAppSettings(builder.Configuration);
```

A Telegram address is a **chat id**, not something you can derive from a user: it exists only once that
person has started your bot. Store the mapping yourself, or implement `IRecipientDirectory`.

Telegram's `429` with `retry_after` is honoured: the dispatcher waits exactly as long as the API asked.

### MTarget — channel `mtarget`

```json
{
  "Notifications": {
    "mtarget": {
      "SUPPORT": {
        "Username": "…",
        "Password": "…",
        "Sender": "EXAMPLE"
      }
    }
  }
}
```

```csharp
builder.Services.AddNotifications()
    .AddMTarget(o => o.Environment = MTargetEnvironment.Private)
    .FromAppSettings(builder.Configuration);
```

```csharp
await mtarget.SendAsync("SUPPORT", [phoneNumber], $"Your code is {code}", cancellationToken);
```

The channel is named after the provider, not the medium. A second SMS provider would be its own channel
with its own identity shape, instead of fighting this one over the same configuration key.

### Secrets

The static layer is plain `IConfiguration`, so user secrets, environment variables and Key Vault all work
without any extra code. Override a single value with a double underscore:

```
Notifications__email__SUPPORT__SmtpPassword=…
```

---

## Sending

### Through the channel you chose

Take a dependency on the channel's own interface when you need what makes that channel different:
`IEmailSender`, `ITelegramSender`, `IMTargetSender`.

### Through the dispatcher, without choosing

```csharp
var report = await dispatcher.DispatchAsync(NotificationRequest.To(
    "SUPPORT",
    "Your order shipped.",
    Recipient.For("email", "customer@example.com"),
    Recipient.For("telegram", "123456789")), cancellationToken);
```

Fan-out is **best effort**: each channel reports its own outcome, and one failing does not stop the
others. All-or-nothing across channels cannot be honest — an email the SMTP server already accepted
cannot be recalled because Telegram then refused.

```csharp
if (!report.AllSucceeded)
{
    foreach (var failure in report.Failures)
    {
        logger.LogWarning("{Channel}: {Reason}", failure.Channel, failure.Result.FailureReason);
    }
}
```

Transient failures are retried (default 3 attempts, exponential backoff, or the wait the channel asked
for). Permanent ones — a refused mailbox, a malformed address — are not retried at all.

### Two sinks on one channel

```csharp
builder.Services.AddNotifications()
    .AddEmail()
    .AddEmail(o => o.Name = "backup-smtp")
    .FromAppSettings(builder.Configuration);
```

Both receive. To reach one specifically, resolve it by key:

```csharp
var backup = serviceProvider.GetRequiredKeyedService<INotificationSink>("backup-smtp");
```

---

## Static and dynamic profiles

Two layers, one entry point. Callers never know which one answered.

**Static** — `FromAppSettings(...)`. Read once at startup and never reloaded: appsettings is treated as
fixed for the life of the process.

**Dynamic** — `FromSqlServer(...)`. Read at runtime and cached, for profiles that are created while the
application is running: one per tenant, typically.

```csharp
builder.Services.AddNotifications(configuration => configuration.DynamicCacheTtl = TimeSpan.FromMinutes(2))
    .AddEmail()
    .FromAppSettings(builder.Configuration)
    .FromSqlServer(o => o.ConnectionString = connectionString);
```

Dynamic profiles are stored as JSON in exactly the shape documented above:

```sql
INSERT INTO [notification].[Profiles] (Channel, Id, Payload) VALUES
('email', 'TENANT-42', N'{"SmtpHost":"smtp.tenant42.it","SmtpPort":587,"UseSsl":true,
  "UseAuthentication":true,"SmtpUsername":"noreply@tenant42.it","SmtpPassword":"…",
  "FromEmail":"noreply@tenant42.it"}');
```

Rules worth knowing:

- **If an id exists in both layers, the static one wins.** Silently — checking for a conflict would mean
  a database round trip on every read.
- **An unknown id throws.** No silent fallback: sending from the wrong profile is worse than failing.
- **When the database is unreachable, the last known value is served** rather than failing the send.
  Turn it off with `ServeStaleOnSourceFailure = false`.
- **The cache is bounded and evicts** (`DynamicCacheCapacity`, 512 by default), so tenant ids created at
  runtime cannot grow it without limit.

Changed a profile from elsewhere in your application? Drop the cached copy:

```csharp
configurationProvider.Invalidate("email", "TENANT-42");
```

Otherwise the old value stands until the TTL lapses (5 minutes by default).

### Setting up the database

Apply [`schema.sql`](src/fbognini.Notifications.Sources.SqlServer/schema.sql) yourself — both scripts
also ship inside the package under `sql/`. The library never issues DDL, so reading a configuration can
never rewrite your database.

---

## Writing your own sink

The core has no list of channels, so nothing here needs its permission.

1. **An identity type** — whatever per-profile configuration your channel needs. A plain class with
   settable properties; it gets bound from `IConfiguration` and deserialised from JSON.
2. **An options type** — what is fixed for the whole process (endpoints, defaults, timeouts) plus a
   `Name`. This is the Serilog half: it arrives as an argument to your extension method, not from a
   configuration source.
3. **A sink** implementing `INotificationSink`:

```csharp
internal sealed class PigeonSink(INotificationConfigurationProvider configuration, PigeonOptions options)
    : INotificationSink
{
    public string Channel => "carrier-pigeon";

    public string Name => options.Name;

    public async Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken ct)
    {
        var identity = await configuration.GetAsync<PigeonIdentity>(Channel, request.ConfigurationId, ct);

        var addresses = request.Recipients.Where(r => r.Channel == Channel).Select(r => r.Address).ToArray();
        if (addresses.Length == 0)
        {
            return NotificationResult.Skipped("Nothing addressed to this channel.");
        }

        // …send, then classify the outcome honestly:
        return NotificationResult.Sent();
        // NotificationResult.TransientFailure(reason, retryAfter)  -> the dispatcher will retry
        // NotificationResult.PermanentFailure(reason)              -> it will not
    }
}
```

4. **An extension method** on `NotificationsBuilder`:

```csharp
public static NotificationsBuilder AddPigeon(this NotificationsBuilder builder, Action<PigeonOptions>? configure = null)
{
    var options = new PigeonOptions();
    configure?.Invoke(options);

    return builder.AddSink("carrier-pigeon", options.Name,
        sp => new PigeonSink(sp.GetRequiredService<INotificationConfigurationProvider>(), options),
        typeof(PigeonIdentity));
}
```

Passing the identity type is what lets the host validate every profile of your channel at startup
instead of failing on the first send.

Two rules the dispatcher relies on: **a sink must be stateless and thread-safe** — resolve the profile on
each send, never hold it — and **it should not throw**; return a `NotificationResult` instead. A sink
that throws anyway is contained, but it loses the transient/permanent distinction and will not be
retried.

## Writing your own source

Implement whichever capabilities you can serve and register only those:

| Capability | Interface | Registered with |
|---|---|---|
| Static profiles | `IStaticConfigurationStore` | `AddStaticConfigurationStore(...)` |
| Dynamic profiles | `INotificationConfigurationSource` | `AddDynamicConfigurationSource(...)` |
| Recipient lookup | `IRecipientDirectory` | register in `Services` |

`INotificationConfigurationSource` returns the stored payload as a string and never deserialises it —
which is exactly why a new sink does not require a new version of your source.

---

## Upgrading from 2.x

3.0 is a rewrite with no compatibility layer.

### The source packages were renamed

`Source.` became `Sources.`, matching `Sinks.` which was already plural. These are new package ids, so
**updating will not find them** — you have to change the reference:

| 2.x package | 3.0 package |
|---|---|
| `fbognini.Notifications.Source.AppSettings` | `fbognini.Notifications.Sources.AppSettings` |
| `fbognini.Notifications.Source.SqlServer` | `fbognini.Notifications.Sources.SqlServer` |

The old ids stop at 2.0.0 and will be unlisted once 3.0 is stable. The sink packages kept their names.

### What else changed, and why

| 2.x | 3.0 |
|---|---|
| `ChangeId(id)` mutating a shared instance | `ConfigurationId` on each send. The instance holds nothing, so concurrent tenants cannot collide. |
| `ISettingsProvider` with `GetEmailSettings` / `GetSmsSettings` | One generic lookup. The core no longer enumerates channels, so a new sink is a new package. |
| One config table per channel | One `Profiles` table keyed by channel and id, payload as JSON. |
| DDL executed on every configuration read | `schema.sql`, applied by whoever owns the database. |
| `System.Data.SqlClient` | `Microsoft.Data.SqlClient` |
| `IEmailService`, `ISmsService` | `IEmailSender`, `IMTargetSender`, plus `INotificationDispatcher` for channel-agnostic sends. |

Data migration: apply `schema.sql`, then
[`migrate-2.x-to-3.0.sql`](src/fbognini.Notifications.Sources.SqlServer/migrate-2.x-to-3.0.sql). It moves
profiles without dropping anything, so you can verify before cleaning up. Read the header first — three
things have no 3.0 equivalent and are left behind: `EmailTemplates`, pending `QueueEmails` rows (drain
them with 2.x before switching over) and `SmsConfigs.ServiceId`.

## Licence

MIT. See [LICENSE](LICENSE).
