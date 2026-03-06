# RepSense API

.NET 8.0 Web API for RepSense gym workout tracking application.

## 🚀 Quick Start

### Prerequisites

- .NET 8.0 SDK
- MongoDB Atlas account (or local MongoDB)
- Firebase project with service account key

### Local Development

1. Clone the repository:
   ```bash
   git clone https://github.com/YOUR_USERNAME/RepSense-API.git
   cd RepSense-API
   ```

2. Copy `appsettings.json.example` to `appsettings.json`:
   ```bash
   cp RepSense.API/RepSense.API/appsettings.json.example RepSense.API/RepSense.API/appsettings.json
   ```

3. Add your Firebase service account key:
   - Place `serviceAccountKey.json` in `RepSense.API/RepSense.API/` folder
   - **⚠️ Never commit this file to Git!**

4. Update `appsettings.json` with your configuration:
   - MongoDB connection string
   - JWT secret key
   - Other settings

5. Run the API:
   ```bash
   cd RepSense.API/RepSense.API
   dotnet run
   ```

6. Access Swagger UI:
   - Navigate to `https://localhost:5241/swagger` (or the port shown in console)

## 📚 Documentation

- [Deployment Guide](docs/DEPLOYMENT_GUIDE.md) - Deploy to Azure using GitHub Actions
- [Architecture Documentation](../AIGymTrackerUOB/docs/ARCHITECTURE.md) - Complete system architecture

## 🔧 Configuration

### App Settings

All configuration is in `appsettings.json`. For Azure deployment, configure these in Azure Portal → App Service → Configuration:

- `MongoDB__ConnectionString` - MongoDB Atlas connection string
- `MongoDB__DatabaseName` - Database name (default: RepSenseDb)
- `Jwt__Key` - Secret key for JWT token generation (min 32 characters)
- `Jwt__Issuer` - JWT issuer (default: RepSense.API)
- `Jwt__Audience` - JWT audience (default: RepSense.App)
- `Firebase__CredentialPath` - Path to Firebase service account key (default: serviceAccountKey.json)

### Environment Variables

The API uses the following environment variables (can override appsettings.json):

- `ASPNETCORE_ENVIRONMENT` - Set to `Production` for production deployments
- `MongoDB__ConnectionString` - MongoDB connection string
- `MongoDB__DatabaseName` - Database name
- `Jwt__Key` - JWT secret key
- `Jwt__Issuer` - JWT issuer
- `Jwt__Audience` - JWT audience

## 🔐 Security

- **Never commit** `serviceAccountKey.json` to Git
- Use strong JWT keys (minimum 32 characters)
- Keep MongoDB connection strings secure
- Use HTTPS in production
- Configure CORS appropriately for your Flutter app domain

## 📡 API Endpoints

### Authentication
- `POST /api/auth/google` - Google OAuth login

### Users
- `GET /api/users/profile` - Get user profile (requires authentication)
- `PUT /api/users/profile` - Update user profile (requires authentication)

### Workouts
- `POST /api/workouts` - Create workout session (requires authentication)
- `GET /api/workouts` - Get user's workouts (requires authentication)
- `GET /api/workouts/statistics` - Get workout statistics (requires authentication)
- `DELETE /api/workouts/{id}` - Delete workout session (requires authentication)

## 🚢 Deployment

See [Deployment Guide](docs/DEPLOYMENT_GUIDE.md) for detailed instructions on deploying to Azure using GitHub Actions.

### Quick Deploy

1. Push code to GitHub `main` branch
2. GitHub Actions automatically deploys to Azure
3. Check deployment status in GitHub → Actions tab

## 🛠️ Development

### Project Structure

```
RepSense.API/
├── RepSense.API/
│   ├── Controllers/      # API controllers
│   ├── Models/          # Data models
│   ├── Services/        # Business logic services
│   ├── Program.cs       # Application entry point
│   └── appsettings.json # Configuration
├── .github/
│   └── workflows/       # GitHub Actions workflows
└── docs/                # Documentation
```

### Adding New Endpoints

1. Create controller in `Controllers/` folder
2. Add service in `Services/` folder if needed
3. Register service in `Program.cs`
4. Test locally with Swagger UI
5. Commit and push to trigger deployment

## 📝 License

[Your License Here]

## 👥 Contributors

[Your Name/Team]

---

**For detailed deployment instructions, see [Deployment Guide](docs/DEPLOYMENT_GUIDE.md)**
