using fbognini.Notifications.Interfaces;

namespace fbognini.Notifications.WorkerSample
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IEmailService emailService;

        public Worker(ILogger<Worker> logger, IEmailService emailService)
        {
            _logger = logger;
            this.emailService = emailService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await emailService.SendAsync("test@test.it", "test", "body", cancellationToken: stoppingToken);

                _logger.LogInformation("Email sent at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}