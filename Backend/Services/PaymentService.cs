using Microsoft.EntityFrameworkCore;
using PaymentReminder.Api.Data;
using PaymentReminder.Api.Models;

namespace PaymentReminder.Api.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> CreatePaymentAsync(Payment payment)
        {
            payment.CreatedAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;
            
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            
            return payment;
        }

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Reminders)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId)
        {
            return await _context.Payments
                .Include(p => p.Reminders)
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.DueDate)
                .ToListAsync();
        }

        public async Task<Payment> UpdatePaymentAsync(Payment payment)
        {
            payment.UpdatedAt = DateTime.UtcNow;
            
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
            
            return payment;
        }

        public async Task DeletePaymentAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Payment>> GetOverduePaymentsAsync()
        {
            return await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Reminders)
                .Where(p => p.DueDate < DateTime.UtcNow && p.Status != PaymentStatus.Paid)
                .ToListAsync();
        }

        public async Task<Payment> MarkAsPaidAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                throw new Exception("Payment not found");

            payment.Status = PaymentStatus.Paid;
            payment.PaidDate = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;

            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            return payment;
        }
    }
}