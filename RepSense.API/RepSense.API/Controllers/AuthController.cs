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
    }

    public class GoogleLoginRequest
    {
        public required string IdToken { get; set; }
    }
}
