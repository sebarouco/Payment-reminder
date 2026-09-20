using PaymentReminder.Api.Models;

namespace PaymentReminder.Api.DTOs
{
    public class ReminderDto
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? SentDate { get; set; }
        public ReminderStatus Status { get; set; }
        public ReminderMethod Method { get; set; }
        public string? Message { get; set; }
        public int Attempts { get; set; }
        public string? Error { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateReminderRequest
    {
        public int PaymentId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public ReminderMethod Method { get; set; } = ReminderMethod.Email;
        public string? Message { get; set; }
    }
}