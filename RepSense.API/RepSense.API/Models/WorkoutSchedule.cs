using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RepSense.API.Models
{
    public class WorkoutSchedule
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("userId")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("workouts")]
        public List<ScheduledWorkout> Workouts { get; set; } = new();

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }

    public class ScheduledWorkout
    {
        [BsonElement("exerciseName")]
        public string ExerciseName { get; set; } = string.Empty;

        [BsonElement("sets")]
        public int Sets { get; set; }

        [BsonElement("reps")]
        public int Reps { get; set; }

        [BsonElement("restTimeMinutes")]
        public int RestTimeMinutes { get; set; }
    }
}
