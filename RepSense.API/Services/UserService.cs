using MongoDB.Driver;
using RepSense.API.Models;

namespace RepSense.API.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IMongoCollection<User> users)
        {
            _users = users;
        }

        public async Task<User?> GetUserByIdAsync(string uid)
        {
            return await _users.Find(u => u.Id == uid).FirstOrDefaultAsync();
        }

        public async Task<User> CreateUserAsync(User user, string uid)
        {
            user.Id = uid;
            await _users.ReplaceOneAsync(
                u => u.Id == uid,
                user,
                new ReplaceOptions { IsUpsert = true }
            );
            return user;
        }
    }
}
