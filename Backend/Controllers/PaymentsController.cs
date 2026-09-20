using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentReminder.Api.DTOs;
using PaymentReminder.Api.Models;
using PaymentReminder.Api.Services;
using System.Security.Claims;

namespace PaymentReminder.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IReminderService _reminderService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentService paymentService, IReminderService reminderService, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _reminderService = reminderService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyPayments()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized();

                var payments = await _paymentService.GetPaymentsByUserIdAsync(int.Parse(userIdClaim));
                var paymentDtos = payments.Select(MapToDto).ToList();
                return Ok(paymentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get payments error: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(int id)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(id);
                if (payment == null)
                    return NotFound();

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (payment.UserId != int.Parse(userIdClaim!))
                    return Forbid();

                return Ok(MapToDto(payment));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get payment error: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized();

                var payment = new Payment
                {
                    UserId = int.Parse(userIdClaim),
                    ClientName = request.ClientName,
                    ClientEmail = request.ClientEmail,
                    ClientPhone = request.ClientPhone,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Description = request.Description,
                    DueDate = request.DueDate,
                    Priority = request.Priority,
                    Status = PaymentStatus.Pending
                };

                var createdPayment = await _paymentService.CreatePaymentAsync(payment);
                
                // Schedule reminders for the new payment
                await _reminderService.ScheduleRemindersForPaymentAsync(createdPayment.Id);
                
                _logger.LogInformation($"Payment created: {createdPayment.Id} for user {userIdClaim}");
                return CreatedAtAction(nameof(GetPayment), new { id = createdPayment.Id }, MapToDto(createdPayment));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Create payment error: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] UpdatePaymentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingPayment = await _paymentService.GetPaymentByIdAsync(id);
                if (existingPayment == null)
                    return NotFound();

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (existingPayment.UserId != int.Parse(userIdClaim!))
                    return Forbid();

                var payment = new Payment
                {
                    Id = id,
                    UserId = existingPayment.UserId,
                    ClientName = request.ClientName,
                    ClientEmail = request.ClientEmail,
                    ClientPhone = request.ClientPhone,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Description = request.Description,
                    DueDate = request.DueDate,
                    Priority = request.Priority,
                    Status = request.Status,
                    PaidDate = existingPayment.PaidDate,
                    CreatedAt = existingPayment.CreatedAt
                };

                var updatedPayment = await _paymentService.UpdatePaymentAsync(payment);
                
                _logger.LogInformation($"Payment updated: {id}");
                return Ok(MapToDto(updatedPayment));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Update payment error: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            try
            {
                var existingPayment = await _paymentService.GetPaymentByIdAsync(id);
                if (existingPayment == null)
                    return NotFound();

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (existingPayment.UserId != int.Parse(userIdClaim!))
                    return Forbid();

                await _paymentService.DeletePaymentAsync(id);
                
                _logger.LogInformation($"Payment deleted: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Delete payment error: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/mark-paid")]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            try
            {
                var existingPayment = await _paymentService.GetPaymentByIdAsync(id);
                if (existingPayment == null)
                    return NotFound();

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (existingPayment.UserId != int.Parse(userIdClaim!))
                    return Forbid();

                var payment = await _paymentService.MarkAsPaidAsync(id);
                
                _logger.LogInformation($"Payment marked as paid: {id}");
                return Ok(MapToDto(payment));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Mark as paid error: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("overdue")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetOverduePayments()
        {
            try
            {
                var overduePayments = await _paymentService.GetOverduePaymentsAsync();
                var paymentDtos = overduePayments.Select(MapToDto).ToList();
                return Ok(paymentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get overdue payments error: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        private PaymentDto MapToDto(Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                UserId = payment.UserId,
                ClientName = payment.ClientName,
                ClientEmail = payment.ClientEmail,
                ClientPhone = payment.ClientPhone,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Description = payment.Description,
                DueDate = payment.DueDate,
                PaidDate = payment.PaidDate,
                Status = payment.Status,
                Priority = payment.Priority,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt,
                Reminders = payment.Reminders?.Select(r => new ReminderDto
                {
                    Id = r.Id,
                    PaymentId = r.PaymentId,
                    ScheduledDate = r.ScheduledDate,
                    SentDate = r.SentDate,
                    Status = r.Status,
                    Method = r.Method,
                    Message = r.Message,
                    Attempts = r.Attempts,
                    Error = r.Error,
                    CreatedAt = r.CreatedAt
                }).ToList() ?? new List<ReminderDto>()
            };
        }
    }
}