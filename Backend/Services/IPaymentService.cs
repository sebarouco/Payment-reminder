using PaymentReminder.Api.Models;

namespace PaymentReminder.Api.Services
{
    public interface IPaymentService
    {
        Task<Payment> CreatePaymentAsync(Payment payment);
        Task<Payment?> GetPaymentByIdAsync(int id);
        Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId);
        Task<Payment> UpdatePaymentAsync(Payment payment);
        Task DeletePaymentAsync(int id);
        Task<IEnumerable<Payment>> GetOverduePaymentsAsync();
        Task<Payment> MarkAsPaidAsync(int id);
    }
}