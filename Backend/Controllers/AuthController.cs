using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentReminder.Api.DTOs;
using PaymentReminder.Api.Services;
using System.Security.Claims;

namespace PaymentReminder.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var token = await _authService.RegisterAsync(request.Username, request.Email, request.Password);
                var user = await _authService.GetUserByUsernameAsync(request.Username);

                var response = new AuthResponse
                {
                    Token = token,
                    User = new UserDto
                    {
                        Id = user!.Id,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role,
                        CreatedAt = user.CreatedAt,
                        LastLogin = user.LastLogin
                    }
                };

                _logger.LogInformation($"User registered: {request.Username}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Registration error: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var token = await _authService.LoginAsync(request.Username, request.Password);
                var user = await _authService.GetUserByUsernameAsync(request.Username);

                var response = new AuthResponse
                {
                    Token = token,
                    User = new UserDto
                    {
                        Id = user!.Id,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role,
                        CreatedAt = user.CreatedAt,
                        LastLogin = user.LastLogin
                    }
                };

                _logger.LogInformation($"User logged in: {request.Username}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized();

                var user = await _authService.GetUserByIdAsync(int.Parse(userIdClaim));
                if (user == null)
                    return NotFound();

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt,
                    LastLogin = user.LastLogin
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get current user error: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("demo")]
        public IActionResult GetDemoCredentials()
        {
            return Ok(new
            {
                message = "Demo mode enabled. Use these credentials to test the application:",
                username = "demo",
                password = "demo123",
                note = "This is a demo account with pre-loaded data for testing purposes."
            });
        }
    }
}