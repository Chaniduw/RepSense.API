using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RepSense.API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Firebase & Firestore Configuration ---
var firebaseConfig = builder.Configuration.GetSection("Firebase");
var credentialPath = firebaseConfig["CredentialPath"];
var projectId = firebaseConfig["ProjectId"];

// Only initialize if config is present (Robustness for "I'll create project later")
if (!string.IsNullOrEmpty(credentialPath) && File.Exists(credentialPath))
{
    var credential = GoogleCredential.FromFile(credentialPath);
    
    FirebaseApp.Create(new AppOptions
    {
        Credential = credential,
        ProjectId = projectId
    });

    builder.Services.AddSingleton(provider =>
    {
        var firestoreBuilder = new FirestoreDbBuilder
        {
            ProjectId = projectId,
            Credential = credential
        };
        return firestoreBuilder.Build();
    });
}
else
{
    Console.WriteLine("WARNING: Firebase Credential file not found. Firebase services will not work.");
    // Register a dummy or allow DI to fail at runtime if services are used?
    // For now, we won't register FirestoreDb, so the app might fail to start if it eagerly resolves dependent services,
    // but Controller resolution is lazy.
}

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // Ensure this is before Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();
//hello
