using FreightSystem.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FreightSystem.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string to, string subject, string body)
        {
            _logger.LogInformation("[Hangfire Email] From={From}; To={To}; Subject={Subject}; Body={Body}", "no-reply@freightsystem.local", to, subject, body);
            // TODO: integrate real SMTP provider by reading config (mocked in this project)
            return Task.CompletedTask;
        }

        public Task SendSmsAsync(string to, string message)
        {
            _logger.LogInformation("[Hangfire SMS] To={To}; Message={Message}", to, message);
            // TODO: integrate real SMS provider (mocked in this project)
            return Task.CompletedTask;
        }
    }
}
