using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using RepSense.API.Models;
using RepSense.API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Firebase Auth Configuration (kept for Google login token verification) ---
var firebaseConfig = builder.Configuration.GetSection("Firebase");
var credentialPath = firebaseConfig["CredentialPath"];

if (!string.IsNullOrEmpty(credentialPath) && File.Exists(credentialPath))
{
    var credential = GoogleCredential.FromFile(credentialPath);

    FirebaseApp.Create(new AppOptions
    {
        Credential = credential,
    });
}
else
{
    Console.WriteLine("WARNING: Firebase Credential file not found. Firebase Auth will not work.");
}

// --- MongoDB Configuration ---
var mongoConfig = builder.Configuration.GetSection("MongoDB");
var connectionString = mongoConfig["ConnectionString"];
var databaseName = mongoConfig["DatabaseName"];

if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(databaseName))
{
    var mongoClient = new MongoClient(connectionString);
    var mongoDatabase = mongoClient.GetDatabase(databaseName);

    builder.Services.AddSingleton<IMongoClient>(mongoClient);
    builder.Services.AddSingleton(mongoDatabase);
    builder.Services.AddSingleton(mongoDatabase.GetCollection<User>("users"));
    builder.Services.AddSingleton(mongoDatabase.GetCollection<WorkoutSession>("workouts"));
    builder.Services.AddSingleton(mongoDatabase.GetCollection<WorkoutSchedule>("schedules"));
}
else
{
    Console.WriteLine("WARNING: MongoDB configuration is missing. Database services will not work.");
}

builder.Services.AddHttpClient();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<WorkoutService>();
builder.Services.AddScoped<ScheduleService>();
builder.Services.AddScoped<CoachChatService>();

// --- JWT Authentication Configuration ---
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// Configure Kestrel to use Azure's PORT environment variable
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

// Configure the HTTP request pipeline.
// Enable Swagger in all environments for testing (disable in production later)
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
