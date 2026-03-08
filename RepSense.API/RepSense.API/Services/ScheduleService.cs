using MongoDB.Driver;
using RepSense.API.Models;

namespace RepSense.API.Services
{
    public class ScheduleService
    {
        private readonly IMongoCollection<WorkoutSchedule> _schedules;

        public ScheduleService(IMongoDatabase database)
        {
            _schedules = database.GetCollection<WorkoutSchedule>("schedules");
        }

        public async Task<WorkoutSchedule> CreateScheduleAsync(WorkoutSchedule schedule, string userId)
        {
            schedule.UserId = userId;
            schedule.CreatedAt = DateTime.UtcNow;
            await _schedules.InsertOneAsync(schedule);
            return schedule;
        }

        public async Task<List<WorkoutSchedule>> GetUserSchedulesAsync(string userId)
        {
            var filter = Builders<WorkoutSchedule>.Filter.Eq(s => s.UserId, userId);
            var sort = Builders<WorkoutSchedule>.Sort.Descending(s => s.CreatedAt);
            
            return await _schedules
                .Find(filter)
                .Sort(sort)
                .ToListAsync();
        }

        public async Task<WorkoutSchedule?> GetScheduleByIdAsync(string scheduleId, string userId)
        {
            var filter = Builders<WorkoutSchedule>.Filter.And(
                Builders<WorkoutSchedule>.Filter.Eq(s => s.Id, scheduleId),
                Builders<WorkoutSchedule>.Filter.Eq(s => s.UserId, userId)
            );
            
            return await _schedules.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<WorkoutSchedule> UpdateScheduleAsync(WorkoutSchedule schedule, string userId)
        {
            var filter = Builders<WorkoutSchedule>.Filter.And(
                Builders<WorkoutSchedule>.Filter.Eq(s => s.Id, schedule.Id),
                Builders<WorkoutSchedule>.Filter.Eq(s => s.UserId, userId)
            );

            schedule.UpdatedAt = DateTime.UtcNow;
            
            var update = Builders<WorkoutSchedule>.Update
                .Set(s => s.Name, schedule.Name)
                .Set(s => s.Workouts, schedule.Workouts)
                .Set(s => s.UpdatedAt, schedule.UpdatedAt);

            await _schedules.UpdateOneAsync(filter, update);
            return schedule;
        }

        public async Task<bool> DeleteScheduleAsync(string scheduleId, string userId)
        {
            var filter = Builders<WorkoutSchedule>.Filter.And(
                Builders<WorkoutSchedule>.Filter.Eq(s => s.Id, scheduleId),
                Builders<WorkoutSchedule>.Filter.Eq(s => s.UserId, userId)
            );
            
            var result = await _schedules.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }
    }
}
