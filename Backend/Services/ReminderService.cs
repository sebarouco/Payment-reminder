using Microsoft.EntityFrameworkCore;
using PaymentReminder.Api.Data;
using PaymentReminder.Api.Models;

namespace PaymentReminder.Api.Services
{
    public class ReminderService : IReminderService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public ReminderService(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<Reminder> CreateReminderAsync(Reminder reminder)
        {
            reminder.CreatedAt = DateTime.UtcNow;
            
            _context.Reminders.Add(reminder);
            await _context.SaveChangesAsync();
            
            return reminder;
        }

        public async Task<IEnumerable<Reminder>> GetRemindersByPaymentIdAsync(int paymentId)
        {
            return await _context.Reminders
                .Where(r => r.PaymentId == paymentId)
                .OrderBy(r => r.ScheduledDate)
                .ToListAsync();
        }

        public async Task ProcessReminderAsync(int reminderId)
        {
            var reminder = await _context.Reminders
                .Include(r => r.Payment)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(r => r.Id == reminderId);

            if (reminder == null)
                throw new Exception("Reminder not found");

            try
            {
                reminder.Attempts++;
                
                if (reminder.Payment == null || reminder.Payment.User == null)
                {
                    reminder.Status = ReminderStatus.Failed;
                    reminder.Error = "Payment or User not found";
                    await _context.SaveChangesAsync();
                    return;
                }
                
                var success = await _emailService.SendReminderEmailAsync(
                    reminder.Payment.User.Email,
                    reminder.Payment.ClientName,
                    reminder.Payment.Amount,
                    reminder.Payment.DueDate,
                    reminder.Payment.Description);

                if (success)
                {
                    reminder.Status = ReminderStatus.Sent;
                    reminder.SentDate = DateTime.UtcNow;
                }
                else
                {
                    reminder.Status = ReminderStatus.Failed;
                    reminder.Error = "Failed to send email";
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                reminder.Status = ReminderStatus.Failed;
                reminder.Error = ex.Message;
                reminder.Attempts++;
                await _context.SaveChangesAsync();
                throw;
            }
        }

        public async Task ScheduleRemindersForPaymentAsync(int paymentId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
                throw new Exception("Payment not found");

            // Create reminders at different intervals before due date
            var reminderDates = new[]
            {
                payment.DueDate.AddDays(-7),  // 1 week before
                payment.DueDate.AddDays(-3),  // 3 days before
                payment.DueDate.AddDays(-1),  // 1 day before
                payment.DueDate.AddDays(1)    // 1 day after (if overdue)
            };

            foreach (var date in reminderDates)
            {
                if (date > DateTime.UtcNow)
                {
                    var reminder = new Reminder
                    {
                        PaymentId = paymentId,
                        ScheduledDate = date,
                        Status = ReminderStatus.Scheduled,
                        Method = ReminderMethod.Email,
                        Message = $"Payment reminder for {payment.ClientName}"
                    };

                    await CreateReminderAsync(reminder);
                }
            }
        }
    }
}