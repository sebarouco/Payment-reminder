namespace PaymentReminder.Api.Models
{
    public class Reminder
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public Payment? Payment { get; set; }
        
        public DateTime ScheduledDate { get; set; }
        public DateTime? SentDate { get; set; }
        
        public ReminderStatus Status { get; set; } = ReminderStatus.Scheduled;
        public ReminderMethod Method { get; set; } = ReminderMethod.Email;
        
        public string? Message { get; set; }
        public int Attempts { get; set; } = 0;
        public string? Error { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum ReminderStatus
    {
        Scheduled,
        Sent,
        Failed,
        Cancelled
    }

    public enum ReminderMethod
    {
        Email,
        SMS,
        Notification,
        All
    }
}