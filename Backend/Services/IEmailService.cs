namespace PaymentReminder.Api.Services
{
    public interface IEmailService
    {
        Task<bool> SendReminderEmailAsync(string toEmail, string clientName, decimal amount, DateTime dueDate, string description);
        Task<bool> SendWelcomeEmailAsync(string toEmail, string username);
    }
}