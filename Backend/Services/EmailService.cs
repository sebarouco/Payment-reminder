using System.Net;
using System.Net.Mail;

namespace PaymentReminder.Api.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendReminderEmailAsync(string toEmail, string clientName, decimal amount, DateTime dueDate, string description)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                
                var smtpServer = emailSettings["SmtpServer"];
                var smtpPortStr = emailSettings["SmtpPort"];
                var smtpUser = emailSettings["SmtpUser"];
                var smtpPassword = emailSettings["SmtpPassword"];
                var fromEmail = emailSettings["FromEmail"];
                var fromName = emailSettings["FromName"];

                // Check if email configuration is properly set
                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUser) || 
                    string.IsNullOrEmpty(smtpPassword) || smtpUser == "your-email@gmail.com")
                {
                    _logger.LogWarning($"Email configuration not properly set. Skipping email to {toEmail}");
                    _logger.LogInformation($"EMAIL FALLBACK - To: {toEmail}, Subject: Payment Reminder: {clientName} - ${amount}");
                    return true; // Return true to not block the application
                }

                var subject = $"Payment Reminder: {clientName} - ${amount}";
                var body = $@"
Dear Client,

This is a friendly reminder about your payment:

Client: {clientName}
Amount: ${amount}
Due Date: {dueDate:MMMM dd, yyyy}
Description: {description}

Please ensure your payment is made by the due date to avoid any late fees.

Best regards,
{fromName}
";

                using var client = new SmtpClient(smtpServer, int.Parse(smtpPortStr!))
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUser, smtpPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail!, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                
                _logger.LogInformation($"Reminder email sent to {toEmail} for {clientName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email to {toEmail}: {ex.Message}");
                
                // Fallback: Log the email details
                _logger.LogInformation($"EMAIL FALLBACK - To: {toEmail}, Subject: Payment Reminder: {clientName} - ${amount}");
                
                return true; // Return true to not block the application
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string username)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                
                var smtpServer = emailSettings["SmtpServer"];
                var smtpPortStr = emailSettings["SmtpPort"];
                var smtpUser = emailSettings["SmtpUser"];
                var smtpPassword = emailSettings["SmtpPassword"];
                var fromEmail = emailSettings["FromEmail"];
                var fromName = emailSettings["FromName"];

                // Check if email configuration is properly set
                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUser) || 
                    string.IsNullOrEmpty(smtpPassword) || smtpUser == "your-email@gmail.com")
                {
                    _logger.LogWarning($"Email configuration not properly set. Skipping welcome email to {toEmail}");
                    return true; // Return true to not block the application
                }

                var subject = "Welcome to Payment Reminder";
                var body = $@"
Dear {username},

Welcome to Payment Reminder! We're excited to help you manage your payments efficiently.

You can now:
- Create and track payments
- Set up automatic reminders
- Monitor payment status
- Manage your clients

If you have any questions, feel free to reach out.

Best regards,
{fromName}
";

                using var client = new SmtpClient(smtpServer, int.Parse(smtpPortStr!))
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUser, smtpPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail!, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                
                _logger.LogInformation($"Welcome email sent to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send welcome email to {toEmail}: {ex.Message}");
                return true; // Return true to not block the application
            }
        }
    }
}