using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JobPortal.Data;
using JobPortal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobPortal.Services
{
    public class JobAlertDispatcher : BackgroundService
    {
        private static readonly TimeSpan PollingInterval = TimeSpan.FromHours(1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEmailService _emailService;
        private readonly ILogger<JobAlertDispatcher> _logger;

        public JobAlertDispatcher(IServiceScopeFactory scopeFactory, IEmailService emailService, ILogger<JobAlertDispatcher> logger)
        {
            _scopeFactory = scopeFactory;
            _emailService = emailService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessAlertsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while processing job alerts.");
                }

                try
                {
                    await Task.Delay(PollingInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // shutting down
                }
            }
        }

        private async Task ProcessAlertsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var alerts = await context.JobAlertSubscriptions
                .Include(a => a.User)
                .Where(a => a.User.Email != null)
                .ToListAsync(cancellationToken);

            foreach (var alert in alerts)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var query = context.Jobs.AsQueryable().Where(j => j.IsActive);

                if (!string.IsNullOrWhiteSpace(alert.Keyword))
                {
                    var keyword = alert.Keyword.Trim();
                    query = query.Where(j =>
                        j.Title.Contains(keyword) ||
                        j.Description.Contains(keyword) ||
                        j.Skills.Contains(keyword) ||
                        j.CompanyName.Contains(keyword));
                }

                if (!string.IsNullOrWhiteSpace(alert.Country))
                {
                    query = query.Where(j => j.Country == alert.Country);
                }

                if (!string.IsNullOrWhiteSpace(alert.JobType))
                {
                    query = query.Where(j => j.JobType == alert.JobType);
                }

                var since = alert.LastNotifiedAt ?? DateTime.UtcNow.AddHours(-24);
                var matches = await query
                    .Where(j => j.PostedAt >= since)
                    .OrderByDescending(j => j.PostedAt)
                    .Take(15)
                    .Select(j => new
                    {
                        j.Id,
                        j.Title,
                        j.CompanyName,
                        j.Location,
                        j.JobType,
                        j.PostedAt
                    })
                    .ToListAsync(cancellationToken);

                if (!matches.Any())
                {
                    continue;
                }

                var email = alert.User.Email;
                if (string.IsNullOrWhiteSpace(email))
                {
                    continue;
                }

                var bodyBuilder = new StringBuilder();
                bodyBuilder.AppendLine("<h2 style='font-family:Inter,sans-serif;color:#111827'>New job matches</h2>");
                bodyBuilder.AppendLine("<p style='font-family:Inter,sans-serif;color:#4B5563'>Here are the latest roles matching your alert:</p>");
                bodyBuilder.AppendLine("<ul style='font-family:Inter,sans-serif;color:#111827;padding-left:16px'>");
                foreach (var match in matches)
                {
                    bodyBuilder.AppendLine($"<li style='margin-bottom:12px'><strong>{match.Title}</strong> at {match.CompanyName} · {match.Location} ({match.JobType}) · posted {match.PostedAt:MMM d}</li>");
                }
                bodyBuilder.AppendLine("</ul>");
                bodyBuilder.AppendLine("<p style='font-family:Inter,sans-serif;color:#4B5563'>Sign in to apply or manage your alerts.</p>");

                var subject = matches.Count == 1
                    ? $"1 new job matches your alert"
                    : $"{matches.Count} new jobs match your alert";

                try
                {
                    await _emailService.SendAsync(email, subject, bodyBuilder.ToString());
                    alert.LastNotifiedAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed sending job alert email to {Email}", email);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
