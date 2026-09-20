using System.ComponentModel.DataAnnotations;

namespace PaymentReminder.Api.Models
{
    public class Payment
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        public User? User { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ClientName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string ClientEmail { get; set; } = string.Empty;
        
        [Phone]
        [StringLength(20)]
        public string ClientPhone { get; set; } = string.Empty;
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "USD";
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public PaymentPriority Priority { get; set; } = PaymentPriority.Medium;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
    }

    public enum PaymentStatus
    {
        Pending,
        PartiallyPaid,
        Paid,
        Overdue,
        Cancelled
    }

    public enum PaymentPriority
    {
        Low,
        Medium,
        High,
        Urgent
    }
}