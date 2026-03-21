using Microsoft.AspNetCore.Mvc;
using RepSense.API.Services;

namespace RepSense.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.IdToken))
            {
                return BadRequest("ID Token is required.");
            }

            try
            {
                var token = await _authService.VerifyFirebaseTokenAndLoginAsync(request.IdToken);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                // Get the actual Firebase error from inner exception
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine($"Firebase token verification failed: {errorMessage}");
                Console.WriteLine($"Full exception: {ex}");
                return Unauthorized(new { Message = "Invalid Token", Error = errorMessage });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { Message = "Email and password are required." });
            }

            if (request.Password.Length < 6)
            {
                return BadRequest(new { Message = "Password must be at least 6 characters." });
            }

            try
            {
                var token = await _authService.RegisterWithEmailPasswordAsync(
                    request.Email,
                    request.Password,
                    request.Name,
                    request.Phone);
                return Ok(new { Token = token });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Registration failed.", Error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] EmailLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { Message = "Email and password are required." });
            }

            try
            {
                var token = await _authService.LoginWithEmailPasswordAsync(request.Email, request.Password);
                return Ok(new { Token = token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Login failed.", Error = ex.Message });
            }
        }
    }

    public class GoogleLoginRequest
    {
        public required string IdToken { get; set; }
    }

    public class RegisterRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
    }

    public class EmailLoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
