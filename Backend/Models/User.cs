using System.ComponentModel.DataAnnotations;

namespace PaymentReminder.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        public string Role { get; set; } = "User";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
        
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}