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
                return Unauthorized(new { Message = "Invalid Token", Error = ex.Message });
            }
        }
    }

    public class GoogleLoginRequest
    {
        public string IdToken { get; set; }
    }
}
