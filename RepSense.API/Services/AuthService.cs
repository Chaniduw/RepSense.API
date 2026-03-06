using FirebaseAdmin.Auth;
using Microsoft.IdentityModel.Tokens;
using RepSense.API.Models;
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
                        }
                    };
                    await _userService.CreateUserAsync(user, uid);
                }

                // Generate API JWT
                return GenerateJwtToken(user);
            }
            catch (Exception ex)
            {
                // Log exception
                throw new Exception("Invalid Token", ex);
            }
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
