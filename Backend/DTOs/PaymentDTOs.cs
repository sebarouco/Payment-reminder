using System.ComponentModel.DataAnnotations;
using PaymentReminder.Api.Models;

namespace PaymentReminder.Api.DTOs
{
    public class CreatePaymentRequest
    {
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

        public PaymentPriority Priority { get; set; } = PaymentPriority.Medium;
    }

    public class UpdatePaymentRequest
    {
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

        public PaymentStatus Status { get; set; }
        public PaymentPriority Priority { get; set; }
    }

    public class PaymentDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public PaymentStatus Status { get; set; }
        public PaymentPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ReminderDto> Reminders { get; set; } = new List<ReminderDto>();
    }
}