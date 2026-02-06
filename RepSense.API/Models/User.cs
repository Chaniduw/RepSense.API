using Google.Cloud.Firestore;

namespace RepSense.API.Models
{
    [FirestoreData]
    public class User
    {
        public string Id { get; set; } = string.Empty; // Document ID (usually Auth UID)

        [FirestoreProperty("profile")]
        public UserProfile Profile { get; set; } = new UserProfile();
    }

    [FirestoreData]
    public class UserProfile
    {
        [FirestoreProperty("name")]
        public string? Name { get; set; }

        [FirestoreProperty("email")]
        public string? Email { get; set; }

        [FirestoreProperty("photoUrl")]
        public string? PhotoUrl { get; set; }

        [FirestoreProperty("gender")]
        public string? Gender { get; set; }

        [FirestoreProperty("dateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        [FirestoreProperty("height")]
        public double? Height { get; set; }

        [FirestoreProperty("weight")]
        public double? Weight { get; set; }

        [FirestoreProperty("program")]
        public string? Program { get; set; }

        [FirestoreProperty("notificationsEnabled")]
        public bool? NotificationsEnabled { get; set; } = true;

        [FirestoreProperty("createdAt")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [FirestoreProperty("updatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }
}
