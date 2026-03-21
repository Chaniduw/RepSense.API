using FirebaseAdmin.Auth;
using Microsoft.IdentityModel.Tokens;
using RepSense.API.Models;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RepSense.API.Services
{
    public class AuthService
    {
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;

        public AuthService(UserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        public async Task<string> VerifyFirebaseTokenAndLoginAsync(string idToken)
        {
            try
            {
                // Verify the ID token using Firebase Admin SDK
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                string uid = decodedToken.Uid;
                string email = decodedToken.Claims.ContainsKey("email") ? decodedToken.Claims["email"].ToString() ?? "" : "";
                string name = decodedToken.Claims.ContainsKey("name") ? decodedToken.Claims["name"].ToString() ?? "" : "";
                string picture = decodedToken.Claims.ContainsKey("picture") ? decodedToken.Claims["picture"].ToString() ?? "" : "";

                // Get or create user
                var user = await _userService.GetUserByIdAsync(uid);
                if (user == null)
                {
                    user = new User
                    {
                        Profile = new UserProfile
                        {
                            Email = email,
                            Name = name,
                            PhotoUrl = picture,
                            CreatedAt = DateTime.UtcNow
                        },
                        Auth = new UserAuth { Provider = "google" }
                    };
                    await _userService.CreateUserAsync(user, uid);
                }

                // Generate API JWT
                return GenerateJwtToken(user);
            }
            catch (Exception ex)
            {
                // Log the actual Firebase error
                Console.WriteLine($"Firebase token verification error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                // Preserve the original exception message
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                throw new Exception($"Invalid Token: {errorMessage}", ex);
            }
        }

        public async Task<string> RegisterWithEmailPasswordAsync(string email, string password, string? name, string? phone)
        {
            var normalizedEmail = email.Trim().ToLower();
            var existing = await _userService.GetUserByEmailAsync(normalizedEmail);
            if (existing != null)
            {
                throw new InvalidOperationException("An account already exists with this email.");
            }

            var saltBytes = RandomNumberGenerator.GetBytes(16);
            var hash = HashPassword(password, saltBytes);
            var userId = Guid.NewGuid().ToString("N");

            var user = new User
            {
                Id = userId,
                Profile = new UserProfile
                {
                    Email = normalizedEmail,
                    Name = name,
                    Program = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                Auth = new UserAuth
                {
                    Provider = "email",
                    PasswordSalt = Convert.ToBase64String(saltBytes),
                    PasswordHash = hash
                }
            };

            await _userService.CreateUserAsync(user, userId);
            return GenerateJwtToken(user);
        }

        public async Task<string> LoginWithEmailPasswordAsync(string email, string password)
        {
            var normalizedEmail = email.Trim().ToLower();
            var user = await _userService.GetUserByEmailAsync(normalizedEmail);
            if (user == null || string.IsNullOrEmpty(user.Auth?.PasswordHash) || string.IsNullOrEmpty(user.Auth?.PasswordSalt))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var saltBytes = Convert.FromBase64String(user.Auth.PasswordSalt);
            var computedHash = HashPassword(password, saltBytes);
            if (!CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(computedHash),
                Convert.FromBase64String(user.Auth.PasswordHash)))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            return GenerateJwtToken(user);
        }

        private static string HashPassword(string password, byte[] saltBytes)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100_000, HashAlgorithmName.SHA256);
            var hashBytes = pbkdf2.GetBytes(32);
            return Convert.ToBase64String(hashBytes);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

            var claims = new List<Claim>
            {
                new Claim("id", user.Id)
            };

            if (!string.IsNullOrEmpty(user.Profile.Email))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Profile.Email));
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Profile.Email));
            }

            // Still useful to have UID in token if needed by API
            claims.Add(new Claim("firebase_uid", user.Id));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
