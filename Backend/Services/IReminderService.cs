using PaymentReminder.Api.Models;

namespace PaymentReminder.Api.Services
{
    public interface IReminderService
    {
        Task<Reminder> CreateReminderAsync(Reminder reminder);
        Task<IEnumerable<Reminder>> GetRemindersByPaymentIdAsync(int paymentId);
        Task ProcessReminderAsync(int reminderId);
        Task ScheduleRemindersForPaymentAsync(int paymentId);
    }
}