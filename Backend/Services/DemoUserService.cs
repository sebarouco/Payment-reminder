using Microsoft.EntityFrameworkCore;
using PaymentReminder.Api.Data;
using PaymentReminder.Api.Models;

namespace PaymentReminder.Api.Services
{
    public class DemoUserService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DemoUserService> _logger;

        public DemoUserService(AppDbContext context, ILogger<DemoUserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CreateDemoUserIfNotExists()
        {
            // Check if demo user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == "demo" || u.Email == "demo@paymentreminder.com");

            if (existingUser != null)
            {
                _logger.LogInformation("Demo user already exists");
                return;
            }

            // Create demo user
            var demoUser = new User
            {
                Username = "demo",
                Email = "demo@paymentreminder.com",
                PasswordHash = HashPassword("demo123"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(demoUser);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Demo user created successfully");
            _logger.LogInformation("Demo credentials: Username: demo, Password: demo123");

            // Create some demo payments
            await CreateDemoPayments(demoUser.Id);
        }

        private async Task CreateDemoPayments(int userId)
        {
            var demoPayments = new List<Payment>
            {
                new Payment
                {
                    UserId = userId,
                    ClientName = "Acme Corporation",
                    ClientEmail = "billing@acme.com",
                    ClientPhone = "+1-555-0100",
                    Amount = 1500.00m,
                    Currency = "USD",
                    Description = "Web development services - Q3 2024",
                    DueDate = DateTime.UtcNow.AddDays(7),
                    Status = PaymentStatus.Pending,
                    Priority = PaymentPriority.High,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Payment
                {
                    UserId = userId,
                    ClientName = "Tech Solutions Inc",
                    ClientEmail = "accounts@techsolutions.com",
                    ClientPhone = "+1-555-0200",
                    Amount = 3200.50m,
                    Currency = "USD",
                    Description = "Consulting services - September 2024",
                    DueDate = DateTime.UtcNow.AddDays(14),
                    Status = PaymentStatus.Pending,
                    Priority = PaymentPriority.Medium,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Payment
                {
                    UserId = userId,
                    ClientName = "Global Services Ltd",
                    ClientEmail = "finance@globalservices.com",
                    ClientPhone = "+1-555-0300",
                    Amount = 850.00m,
                    Currency = "USD",
                    Description = "Monthly maintenance fee",
                    DueDate = DateTime.UtcNow.AddDays(-2), // Overdue
                    Status = PaymentStatus.Overdue,
                    Priority = PaymentPriority.Urgent,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow
                },
                new Payment
                {
                    UserId = userId,
                    ClientName = "StartUp Ventures",
                    ClientEmail = "admin@startupventures.com",
                    ClientPhone = "+1-555-0400",
                    Amount = 5000.00m,
                    Currency = "USD",
                    Description = "Initial project setup and configuration",
                    DueDate = DateTime.UtcNow.AddDays(-5),
                    Status = PaymentStatus.Paid,
                    Priority = PaymentPriority.High,
                    PaidDate = DateTime.UtcNow.AddDays(-5),
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _context.Payments.AddRange(demoPayments);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created {demoPayments.Count} demo payments for demo user");
        }

        private string HashPassword(string password)
        {
            // Generate a random salt
            var salt = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            
            // Hash the password with the salt
            using var hmac = new System.Security.Cryptography.HMACSHA256(salt);
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            
            // Combine salt and hash
            var combined = new byte[salt.Length + hash.Length];
            System.Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
            System.Buffer.BlockCopy(hash, 0, combined, salt.Length, hash.Length);
            
            return Convert.ToBase64String(combined);
        }
    }
}