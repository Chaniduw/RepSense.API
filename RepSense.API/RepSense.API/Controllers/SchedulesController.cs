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
    public class SchedulesController : ControllerBase
    {
        private readonly ScheduleService _scheduleService;

        public SchedulesController(ScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        /// <summary>
        /// Create a new workout schedule
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSchedule([FromBody] WorkoutSchedule schedule)
        {
            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            try
            {
                var createdSchedule = await _scheduleService.CreateScheduleAsync(schedule, userId);
                return Ok(createdSchedule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to create schedule", Error = ex.Message });
            }
        }

        /// <summary>
        /// Get all workout schedules for the current user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSchedules()
        {
            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            try
            {
                var schedules = await _scheduleService.GetUserSchedulesAsync(userId);
                return Ok(schedules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to retrieve schedules", Error = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific workout schedule by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSchedule(string id)
        {
            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            try
            {
                var schedule = await _scheduleService.GetScheduleByIdAsync(id, userId);
                if (schedule == null)
                {
                    return NotFound(new { Message = "Schedule not found" });
                }

                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to retrieve schedule", Error = ex.Message });
            }
        }

        /// <summary>
        /// Update a workout schedule
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchedule(string id, [FromBody] WorkoutSchedule schedule)
        {
            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            try
            {
                // Ensure the schedule ID matches
                schedule.Id = id;
                
                var updatedSchedule = await _scheduleService.UpdateScheduleAsync(schedule, userId);
                return Ok(updatedSchedule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to update schedule", Error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a workout schedule
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(string id)
        {
            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            try
            {
                var deleted = await _scheduleService.DeleteScheduleAsync(id, userId);
                if (!deleted)
                {
                    return NotFound(new { Message = "Schedule not found" });
                }

                return Ok(new { Message = "Schedule deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to delete schedule", Error = ex.Message });
            }
        }
    }
}
