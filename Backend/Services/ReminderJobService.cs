using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PaymentReminder.Api.Data;
using PaymentReminder.Api.Hubs;
using PaymentReminder.Api.Models;

namespace PaymentReminder.Api.Services
{
    public class ReminderJobService
    {
        private readonly AppDbContext _context;
        private readonly IReminderService _reminderService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<ReminderJobService> _logger;

        public ReminderJobService(
            AppDbContext context,
            IReminderService reminderService,
            IHubContext<NotificationHub> hubContext,
            ILogger<ReminderJobService> logger)
        {
            _context = context;
            _reminderService = reminderService;
            _hubContext = hubContext;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task ProcessPendingReminders()
        {
            _logger.LogInformation("Processing pending reminders");
            
            var pendingReminders = await _context.Reminders
                .Include(r => r.Payment)
                .ThenInclude(p => p.User)
                .Where(r => r.Status == ReminderStatus.Scheduled && r.ScheduledDate <= DateTime.UtcNow)
                .ToListAsync();

            _logger.LogInformation($"Found {pendingReminders.Count} pending reminders");

            foreach (var reminder in pendingReminders)
            {
                try
                {
                    await _reminderService.ProcessReminderAsync(reminder.Id);
                    
                    // Send real-time notification
                    if (reminder.Payment?.User != null)
                    {
                        await _hubContext.Clients.Group($"user_{reminder.Payment.UserId}")
                            .SendAsync("ReceiveNotification", 
                                $"Reminder sent for {reminder.Payment.ClientName} - ${reminder.Payment.Amount}", 
                                "reminder");
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue processing other reminders
                    _logger.LogError($"Error processing reminder {reminder.Id}: {ex.Message}");
                }
            }
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task UpdateOverduePayments()
        {
            _logger.LogInformation("Updating overdue payments");
            
            var overduePayments = await _context.Payments
                .Where(p => p.DueDate < DateTime.UtcNow && p.Status == PaymentStatus.Pending)
                .ToListAsync();

            _logger.LogInformation($"Found {overduePayments.Count} overdue payments");

            foreach (var payment in overduePayments)
            {
                payment.Status = PaymentStatus.Overdue;
                payment.UpdatedAt = DateTime.UtcNow;
                
                // Send notification
                await _hubContext.Clients.Group($"user_{payment.UserId}")
                    .SendAsync("ReceiveNotification", 
                        $"Payment for {payment.ClientName} is now overdue - ${payment.Amount}", 
                        "warning");
            }

            await _context.SaveChangesAsync();
        }
    }

    public static class HangfireJobScheduler
    {
        public static void ScheduleRecurringJobs(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            
            // Process reminders every 5 minutes
            RecurringJob.AddOrUpdate<ReminderJobService>("process-reminders", 
                service => service.ProcessPendingReminders(), 
                Cron.Minutely);

            // Update overdue payments every hour
            RecurringJob.AddOrUpdate<ReminderJobService>("update-overdue", 
                service => service.UpdateOverduePayments(), 
                Cron.Hourly);
        }
    }
}