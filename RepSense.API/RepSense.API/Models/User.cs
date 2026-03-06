using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RepSense.API.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = string.Empty; // Document ID (usually Auth UID)

        [BsonElement("profile")]
        public UserProfile Profile { get; set; } = new UserProfile();
    }

    public class UserProfile
    {
        [BsonElement("name")]
        public string? Name { get; set; }

        [BsonElement("email")]
        public string? Email { get; set; }

        [BsonElement("photoUrl")]
        public string? PhotoUrl { get; set; }

        [BsonElement("gender")]
        public string? Gender { get; set; }

        [BsonElement("dateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        [BsonElement("height")]
        public double? Height { get; set; }

        [BsonElement("weight")]
        public double? Weight { get; set; }

        [BsonElement("program")]
        public string? Program { get; set; }

        [BsonElement("notificationsEnabled")]
        public bool? NotificationsEnabled { get; set; } = true;

        [BsonElement("createdAt")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }
}
