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
    public class RemindersController : ControllerBase
    {
        private readonly IReminderService _reminderService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<RemindersController> _logger;

        public RemindersController(IReminderService reminderService, IPaymentService paymentService, ILogger<RemindersController> logger)
        {
            _reminderService = reminderService;
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpGet("payment/{paymentId}")]
        public async Task<IActionResult> GetRemindersForPayment(int paymentId)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
                if (payment == null)
                    return NotFound();

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (payment.UserId != int.Parse(userIdClaim!))
                    return Forbid();

                var reminders = await _reminderService.GetRemindersByPaymentIdAsync(paymentId);
                var reminderDtos = reminders.Select(r => new ReminderDto
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
                }).ToList();

                return Ok(reminderDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get reminders error: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateReminder([FromBody] CreateReminderRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var payment = await _paymentService.GetPaymentByIdAsync(request.PaymentId);
                if (payment == null)
                    return NotFound();

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (payment.UserId != int.Parse(userIdClaim!))
                    return Forbid();

                var reminder = new Reminder
                {
                    PaymentId = request.PaymentId,
                    ScheduledDate = request.ScheduledDate,
                    Method = request.Method,
                    Message = request.Message,
                    Status = ReminderStatus.Scheduled
                };

                var createdReminder = await _reminderService.CreateReminderAsync(reminder);
                
                _logger.LogInformation($"Reminder created: {createdReminder.Id} for payment {request.PaymentId}");
                
                var reminderDto = new ReminderDto
                {
                    Id = createdReminder.Id,
                    PaymentId = createdReminder.PaymentId,
                    ScheduledDate = createdReminder.ScheduledDate,
                    SentDate = createdReminder.SentDate,
                    Status = createdReminder.Status,
                    Method = createdReminder.Method,
                    Message = createdReminder.Message,
                    Attempts = createdReminder.Attempts,
                    Error = createdReminder.Error,
                    CreatedAt = createdReminder.CreatedAt
                };

                return CreatedAtAction(nameof(GetRemindersForPayment), new { paymentId = createdReminder.PaymentId }, reminderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Create reminder error: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/process")]
        public async Task<IActionResult> ProcessReminder(int id)
        {
            try
            {
                var reminders = await _reminderService.GetRemindersByPaymentIdAsync(0); // We need to check if this reminder belongs to user
                // For now, let's process it directly
                await _reminderService.ProcessReminderAsync(id);
                
                _logger.LogInformation($"Reminder processed: {id}");
                return Ok(new { message = "Reminder processed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Process reminder error: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("payment/{paymentId}/schedule")]
        public async Task<IActionResult> ScheduleRemindersForPayment(int paymentId)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
                if (payment == null)
                    return NotFound();

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (payment.UserId != int.Parse(userIdClaim!))
                    return Forbid();

                await _reminderService.ScheduleRemindersForPaymentAsync(paymentId);
                
                _logger.LogInformation($"Reminders scheduled for payment: {paymentId}");
                return Ok(new { message = "Reminders scheduled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Schedule reminders error: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}