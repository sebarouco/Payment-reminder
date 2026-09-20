using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PaymentReminder.Api.Data;
using PaymentReminder.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PaymentReminder.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context, IConfiguration configuration, IEmailService emailService, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<string> RegisterAsync(string username, string email, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Username == username))
                throw new Exception("Username already exists");

            if (await _context.Users.AnyAsync(u => u.Email == email))
                throw new Exception("Email already exists");

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = HashPassword(password),
                CreatedAt = DateTime.UtcNow,
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Send welcome email
            try
            {
                await _emailService.SendWelcomeEmailAsync(email, username);
                _logger.LogInformation($"Welcome email sent to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to send welcome email to {email}: {ex.Message}");
                // Don't fail registration if email fails
            }

            return GenerateJwtToken(user);
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == username);

            if (user == null)
                throw new Exception("Invalid credentials");

            if (!VerifyPassword(password, user.PasswordHash))
                throw new Exception("Invalid credentials");

            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return GenerateJwtToken(user);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == username);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationInMinutes"]!)),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(secretKey),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
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
            using var hmac = new HMACSHA256(salt);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            
            // Combine salt and hash
            var combined = new byte[salt.Length + hash.Length];
            Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
            Buffer.BlockCopy(hash, 0, combined, salt.Length, hash.Length);
            
            return Convert.ToBase64String(combined);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var combined = Convert.FromBase64String(storedHash);
            
            // Extract salt (first 32 bytes) and hash
            var salt = new byte[32];
            var hash = new byte[combined.Length - 32];
            
            Buffer.BlockCopy(combined, 0, salt, 0, salt.Length);
            Buffer.BlockCopy(combined, salt.Length, hash, 0, hash.Length);
            
            // Hash the password with the extracted salt
            using var hmac = new HMACSHA256(salt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            
            return computedHash.SequenceEqual(hash);
        }
    }
}