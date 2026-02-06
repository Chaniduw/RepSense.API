using Google.Cloud.Firestore;
using RepSense.API.Models;

namespace RepSense.API.Services
{
    public class UserService
    {
        private readonly FirestoreDb _firestoreDb;
        private const string CollectionName = "users";

        public UserService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<User?> GetUserByIdAsync(string uid)
        {
            var docRef = _firestoreDb.Collection(CollectionName).Document(uid);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return null;
            }

            var user = snapshot.ConvertTo<User>();
            user.Id = snapshot.Id;
            return user;
        }

        public async Task<User> CreateUserAsync(User user, string uid)
        {
            var docRef = _firestoreDb.Collection(CollectionName).Document(uid);
            await docRef.SetAsync(user);
            user.Id = uid;
            return user;
        }
    }
}
