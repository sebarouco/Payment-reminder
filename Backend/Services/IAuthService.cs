using PaymentReminder.Api.Models;

namespace PaymentReminder.Api.Services
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(string username, string email, string password);
        Task<string> LoginAsync(string username, string password);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByUsernameAsync(string username);
    }
}