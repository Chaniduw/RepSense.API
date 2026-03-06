# Post-Deployment Checklist ✅

Your API has been successfully deployed to Azure! Follow these steps to verify everything is working.

---

## ✅ Step 1: Test Your API Endpoints

Your API is now live at:
```
https://repsense-api-lk.azurewebsites.net
```

### Test Swagger UI (if enabled):
```
https://repsense-api-lk.azurewebsites.net/swagger
```

### Test Health Endpoint:
Open in browser or use curl:
```bash
curl https://repsense-api-lk.azurewebsites.net/api/auth/google
```

**Expected**: Should return an error about missing token (which is correct - it means the API is running!)

---

## ✅ Step 2: Verify Firebase Credentials

The deployment workflow creates `serviceAccountKey.json` from GitHub secrets. Verify it exists:

1. Go to Azure Portal → Your App Service → **SSH** or **Console**
2. Navigate to `/home/site/wwwroot/`
3. Check if `serviceAccountKey.json` exists

**If Firebase credentials are missing**, the API will fail when trying to verify tokens.

---

## ✅ Step 3: Update Flutter App Base URL

Update your Flutter app to use the Azure URL:

1. Open `lib/services/dio_client.dart`
2. Update the base URL:

```dart
final baseUrl = 'https://repsense-api-lk.azurewebsites.net';
```

3. Test the connection from your Flutter app

---

## ✅ Step 4: Verify App Settings in Azure

Check that all required settings are configured:

1. Go to Azure Portal → Your App Service → **Configuration** → **Environment variables**
2. Verify these settings exist:
   - `MongoDB__ConnectionString`
   - `MongoDB__DatabaseName`
   - `Jwt__Key`
   - `Jwt__Issuer`
   - `Jwt__Audience`
   - `Firebase__CredentialPath`

---

## ✅ Step 5: Check Logs

If something isn't working:

1. Go to Azure Portal → Your App Service → **Log stream**
2. Watch for errors when making API calls
3. Check **Logs** section for detailed error messages

---

## ✅ Step 6: Test Authentication Flow

1. **From Flutter App:**
   - Try logging in with Google
   - Check if you receive a JWT token
   - Verify the token works for subsequent API calls

2. **From Browser/Postman:**
   - Test `POST /api/auth/google` with a Firebase ID token
   - Should return `{ "Token": "your-jwt-token" }`

---

## ✅ Step 7: Future Deployments

Now that everything is set up, deploying changes is easy:

1. Make code changes locally
2. Commit and push:
   ```bash
   git add .
   git commit -m "Your changes"
   git push origin main
   ```
3. GitHub Actions will automatically deploy!
4. Check **Actions** tab to see deployment progress

---

## 🐛 Troubleshooting

### API returns 500 errors:
- Check Azure **Log stream** for error details
- Verify Firebase credentials are set correctly
- Check MongoDB connection string

### Authentication fails:
- Verify Firebase `serviceAccountKey.json` exists in Azure
- Check JWT settings match between Azure and Flutter app
- Verify MongoDB user exists

### Deployment fails:
- Check GitHub Actions logs
- Verify all secrets are set in GitHub
- Check workflow file syntax

---

## 🎉 Success!

If all steps pass, your API is fully deployed and ready to use!

**Next Steps:**
- Monitor usage in Azure Portal
- Set up Application Insights for better monitoring
- Configure custom domain (optional)
- Set up SSL certificate (already included with Azure)

---

**Your API URL:** `https://repsense-api-lk.azurewebsites.net`
