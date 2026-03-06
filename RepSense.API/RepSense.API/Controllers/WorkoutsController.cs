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
    public class WorkoutsController : ControllerBase
    {
        private readonly WorkoutService _workoutService;

        public WorkoutsController(WorkoutService workoutService)
        {
            _workoutService = workoutService;
        }

        /// <summary>
        /// Create a new workout session
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkout([FromBody] WorkoutSession workout)
        {
            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            try
            {
                var createdWorkout = await _workoutService.CreateWorkoutAsync(workout, userId);
                return Ok(createdWorkout);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to create workout", Error = ex.Message });
            }
        }

        /// <summary>
        /// Get all workout sessions for the current user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetWorkouts([FromQuery] int? limit = null)
        {
            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            try
            {
                var workouts = await _workoutService.GetUserWorkoutsAsync(userId, limit);
                return Ok(workouts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to retrieve workouts", Error = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific workout session by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorkout(string id)
        {
            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            try
            {
                var workout = await _workoutService.GetWorkoutByIdAsync(id, userId);
                if (workout == null)
                {
                    return NotFound(new { Message = "Workout not found" });
                }

                return Ok(workout);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to retrieve workout", Error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a workout session
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkout(string id)
        {
            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            try
            {
                var deleted = await _workoutService.DeleteWorkoutAsync(id, userId);
                if (!deleted)
                {
                    return NotFound(new { Message = "Workout not found" });
                }

                return Ok(new { Message = "Workout deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to delete workout", Error = ex.Message });
            }
        }

        /// <summary>
        /// Get workout statistics and analytics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            try
            {
                var statistics = await _workoutService.GetStatisticsAsync(userId, startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to retrieve statistics", Error = ex.Message });
            }
        }
    }
}
