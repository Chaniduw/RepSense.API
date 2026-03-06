using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepSense.API.Models;
using RepSense.API.Services;
using System.Security.Claims;

namespace RepSense.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get current user's profile
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to retrieve profile", Error = ex.Message });
            }
        }

        /// <summary>
        /// Update current user's profile
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                // Update profile fields
                if (request.Name != null) user.Profile.Name = request.Name;
                if (request.Email != null) user.Profile.Email = request.Email;
                if (request.PhotoUrl != null) user.Profile.PhotoUrl = request.PhotoUrl;
                if (request.Gender != null) user.Profile.Gender = request.Gender;
                if (request.DateOfBirth.HasValue) user.Profile.DateOfBirth = request.DateOfBirth.Value;
                if (request.Height.HasValue) user.Profile.Height = request.Height.Value;
                if (request.Weight.HasValue) user.Profile.Weight = request.Weight.Value;
                if (request.Program != null) user.Profile.Program = request.Program;
                if (request.NotificationsEnabled.HasValue) user.Profile.NotificationsEnabled = request.NotificationsEnabled.Value;

                user.Profile.UpdatedAt = DateTime.UtcNow;

                await _userService.UpdateUserProfileAsync(user);

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to update profile", Error = ex.Message });
            }
        }
    }

    public class UpdateProfileRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public double? Height { get; set; }
        public double? Weight { get; set; }
        public string? Program { get; set; }
        public bool? NotificationsEnabled { get; set; }
    }
}
