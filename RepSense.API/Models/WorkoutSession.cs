using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RepSense.API.Models
{
    public class WorkoutSession
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("userId")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("exerciseName")]
        public string ExerciseName { get; set; } = string.Empty;

        [BsonElement("startTime")]
        public DateTime StartTime { get; set; }

        [BsonElement("endTime")]
        public DateTime? EndTime { get; set; }

        [BsonElement("totalReps")]
        public int TotalReps { get; set; }

        [BsonElement("correctReps")]
        public int CorrectReps { get; set; }

        [BsonElement("incorrectReps")]
        public int IncorrectReps { get; set; }

        [BsonElement("accuracy")]
        public double Accuracy { get; set; }

        [BsonElement("duration")]
        public int Duration { get; set; } // in seconds

        [BsonElement("results")]
        public List<ExerciseResult> Results { get; set; } = new();

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ExerciseResult
    {
        [BsonElement("exerciseName")]
        public string ExerciseName { get; set; } = string.Empty;

        [BsonElement("isCorrectForm")]
        public bool IsCorrectForm { get; set; }

        [BsonElement("confidence")]
        public double Confidence { get; set; }

        [BsonElement("issues")]
        public List<string> Issues { get; set; } = new();

        [BsonElement("metrics")]
        public Dictionary<string, double> Metrics { get; set; } = new();

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
