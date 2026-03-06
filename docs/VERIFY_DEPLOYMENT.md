# Verify Deployment - Next Steps

## ✅ Deployment Completed Successfully!

Your GitHub Actions workflow has successfully deployed the API with Firebase credentials.

## Step 1: Check Azure Log Stream

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to your App Service: `repsense-api-lk`
3. In the left menu, go to **Monitoring** → **Log stream**
4. Look for the application startup logs
5. **Expected Result:** You should **NOT** see the warning:
   ```
   WARNING: Firebase Credential file not found. Firebase Auth will not work.
   ```
6. **Good Sign:** You should see:
   ```
   Now listening on: http://0.0.0.0:8080
   Application started. Press Ctrl+C to shut down.
   ```

## Step 2: Test Swagger UI

1. Open: `https://repsense-api-lk-e3h9dwdvd7gbe3dn.indonesiacentral-01.azurewebsites.net/swagger`
2. Verify all endpoints are visible
3. Try expanding the `/api/Auth/google` endpoint

## Step 3: Test Authentication Endpoint

### Option A: Test via Swagger
1. In Swagger UI, find `POST /api/Auth/google`
2. Click "Try it out"
3. Enter a test request body:
   ```json
   {
     "idToken": "test-token"
   }
   ```
4. Click "Execute"
5. **Note:** This will fail with an invalid token, but it should **NOT** fail with "Firebase credential not found"

### Option B: Test from Flutter App
1. Open your Flutter app
2. Make sure `dio_client.dart` points to your Azure URL
3. Try logging in with Google Sign-In
4. Check if authentication works

## Step 4: Verify Firebase Credential File

The workflow created `serviceAccountKey.json` (2382 bytes) during deployment. This file should now be present in your Azure App Service.

## Step 5: Test Protected Endpoints

After authentication works:
1. Get a JWT token from `/api/Auth/google`
2. Use that token to test protected endpoints like:
   - `GET /api/Users/profile`
   - `GET /api/Workouts`
   - `POST /api/Workouts`

## Troubleshooting

### If Firebase warning still appears:
1. Check GitHub Secrets: Ensure `FIREBASE_SERVICE_ACCOUNT_KEY` exists
2. Check the workflow logs: Look for "Create Firebase credential file" step
3. Verify the file was created (should show 2382 bytes)

### If authentication fails:
1. Check Azure Log Stream for detailed error messages
2. Verify the Firebase service account key is valid
3. Check that the key has the correct permissions in Firebase Console

## Success Indicators

✅ No Firebase credential warning in Azure logs  
✅ Swagger UI loads correctly  
✅ Authentication endpoint responds (even if token is invalid)  
✅ Flutter app can authenticate users  

## Next Steps After Verification

Once everything is verified:
1. Update your Flutter app's `dio_client.dart` to use the Azure URL
2. Test end-to-end: Login → Create Workout → View Analytics
3. Monitor Azure Log Stream for any errors
4. Set up alerts in Azure for production monitoring
