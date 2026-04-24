using MongoDB.Driver;
using RepSense.API.Models;

namespace RepSense.API.Services
{
    // Research compliance note:
    // Parts of workout analytics aggregation and method structuring were refined
    // with AI assistance using Codex 5.3 .
    // Final testing and domain validation were performed by the researcher.
    public class WorkoutService
    {
        private readonly IMongoCollection<WorkoutSession> _workouts;

        public WorkoutService(IMongoDatabase database)
        {
            _workouts = database.GetCollection<WorkoutSession>("workouts");
        }

        public async Task<WorkoutSession> CreateWorkoutAsync(WorkoutSession workout, string userId)
        {
            workout.UserId = userId;
            workout.CreatedAt = DateTime.UtcNow;
            await _workouts.InsertOneAsync(workout);
            return workout;
        }

        public async Task<List<WorkoutSession>> GetUserWorkoutsAsync(string userId, int? limit = null)
        {
            var filter = Builders<WorkoutSession>.Filter.Eq(w => w.UserId, userId);
            var sort = Builders<WorkoutSession>.Sort.Descending(w => w.StartTime);
            
            if (limit.HasValue)
            {
                return await _workouts
                    .Find(filter)
                    .Sort(sort)
                    .Limit(limit.Value)
                    .ToListAsync();
            }
            
            return await _workouts
                .Find(filter)
                .Sort(sort)
                .ToListAsync();
        }

        public async Task<WorkoutSession?> GetWorkoutByIdAsync(string workoutId, string userId)
        {
            var filter = Builders<WorkoutSession>.Filter.And(
                Builders<WorkoutSession>.Filter.Eq(w => w.Id, workoutId),
                Builders<WorkoutSession>.Filter.Eq(w => w.UserId, userId)
            );
            
            return await _workouts.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteWorkoutAsync(string workoutId, string userId)
        {
            var filter = Builders<WorkoutSession>.Filter.And(
                Builders<WorkoutSession>.Filter.Eq(w => w.Id, workoutId),
                Builders<WorkoutSession>.Filter.Eq(w => w.UserId, userId)
            );
            
            var result = await _workouts.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        public async Task<WorkoutStatistics> GetStatisticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var filter = Builders<WorkoutSession>.Filter.Eq(w => w.UserId, userId);
            
            if (startDate.HasValue)
            {
                filter &= Builders<WorkoutSession>.Filter.Gte(w => w.StartTime, startDate.Value);
            }
            
            if (endDate.HasValue)
            {
                filter &= Builders<WorkoutSession>.Filter.Lte(w => w.StartTime, endDate.Value);
            }

            var workouts = await _workouts.Find(filter).ToListAsync();

            var totalSessions = workouts.Count;
            var totalReps = workouts.Sum(w => w.TotalReps);
            var correctReps = workouts.Sum(w => w.CorrectReps);
            var incorrectReps = workouts.Sum(w => w.IncorrectReps);
            var totalDuration = workouts.Sum(w => w.Duration);
            var averageAccuracy = workouts.Count > 0 
                ? workouts.Average(w => w.Accuracy) 
                : 0;

            // Exercise counts
            var exerciseCounts = workouts
                .GroupBy(w => w.ExerciseName)
                .ToDictionary(g => g.Key, g => g.Count());

            // Daily progress (for graph)
            var dailyProgress = workouts
                .GroupBy(w => w.StartTime.Date)
                .Select(g => new DailyProgress
                {
                    Date = g.Key,
                    TotalReps = g.Sum(w => w.TotalReps),
                    CorrectReps = g.Sum(w => w.CorrectReps),
                    IncorrectReps = g.Sum(w => w.IncorrectReps),
                    Accuracy = g.Sum(w => w.TotalReps) > 0
                        ? (g.Sum(w => w.CorrectReps) / (double)g.Sum(w => w.TotalReps)) * 100
                        : 0,
                    Sessions = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Weekly progress (for graph)
            var weeklyProgress = workouts
                .GroupBy(w => GetWeekStart(w.StartTime))
                .Select(g => new WeeklyProgress
                {
                    WeekStart = g.Key,
                    TotalReps = g.Sum(w => w.TotalReps),
                    CorrectReps = g.Sum(w => w.CorrectReps),
                    IncorrectReps = g.Sum(w => w.IncorrectReps),
                    Accuracy = g.Sum(w => w.TotalReps) > 0
                        ? (g.Sum(w => w.CorrectReps) / (double)g.Sum(w => w.TotalReps)) * 100
                        : 0,
                    Sessions = g.Count()
                })
                .OrderBy(w => w.WeekStart)
                .ToList();

            return new WorkoutStatistics
            {
                TotalSessions = totalSessions,
                TotalReps = totalReps,
                CorrectReps = correctReps,
                IncorrectReps = incorrectReps,
                OverallAccuracy = totalReps > 0 ? (correctReps / (double)totalReps) * 100 : 0,
                AverageAccuracy = averageAccuracy,
                TotalDuration = totalDuration,
                AverageDuration = totalSessions > 0 ? totalDuration / totalSessions : 0,
                ExerciseCounts = exerciseCounts,
                DailyProgress = dailyProgress,
                WeeklyProgress = weeklyProgress
            };
        }

        private DateTime GetWeekStart(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
    }

    public class WorkoutStatistics
    {
        public int TotalSessions { get; set; }
        public int TotalReps { get; set; }
        public int CorrectReps { get; set; }
        public int IncorrectReps { get; set; }
        public double OverallAccuracy { get; set; }
        public double AverageAccuracy { get; set; }
        public int TotalDuration { get; set; }
        public int AverageDuration { get; set; }
        public Dictionary<string, int> ExerciseCounts { get; set; } = new();
        public List<DailyProgress> DailyProgress { get; set; } = new();
        public List<WeeklyProgress> WeeklyProgress { get; set; } = new();
    }

    public class DailyProgress
    {
        public DateTime Date { get; set; }
        public int TotalReps { get; set; }
        public int CorrectReps { get; set; }
        public int IncorrectReps { get; set; }
        public double Accuracy { get; set; }
        public int Sessions { get; set; }
    }

    public class WeeklyProgress
    {
        public DateTime WeekStart { get; set; }
        public int TotalReps { get; set; }
        public int CorrectReps { get; set; }
        public int IncorrectReps { get; set; }
        public double Accuracy { get; set; }
        public int Sessions { get; set; }
    }
}
