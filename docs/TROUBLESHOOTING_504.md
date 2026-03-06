# Troubleshooting 504 Gateway Timeout Error

A 504 Gateway Timeout means Azure can't reach your application. Here's how to fix it:

---

## 🔍 Step 1: Check if App Service is Running

1. Go to Azure Portal → Your App Service (`repsense-api-lk`)
2. On the **Overview** page, check the status:
   - Should show **Running** (green)
   - If it shows **Stopped**, click **Start**

---

## 🔍 Step 2: Check Application Logs

### Option A: Log Stream (Real-time)

1. Azure Portal → Your App Service → **Log stream**
2. Watch for errors when the app starts
3. Common errors:
   - Missing Firebase credentials
   - MongoDB connection failures
   - Missing environment variables

### Option B: Logs (Detailed)

1. Azure Portal → Your App Service → **Logs**
2. Click **Download** to get log files
3. Look for startup errors

---

## 🔍 Step 3: Check Startup Time

Azure has a default startup timeout. Your app might be taking too long to start.

### Check Startup Command:

1. Azure Portal → Your App Service → **Configuration** → **General settings**
2. Look for **Startup Command**
3. For .NET 8, it should be:
   ```
   dotnet RepSense.API.dll
   ```
   Or leave it empty (Azure auto-detects)

---

## 🔍 Step 4: Verify Firebase Credentials

**This is the most common cause!**

### Check if Firebase file exists:

1. Azure Portal → Your App Service → **SSH** or **Console**
2. Navigate to: `/home/site/wwwroot/`
3. Check if `serviceAccountKey.json` exists

### If missing, verify GitHub Secret:

1. GitHub → Your Repo → **Settings** → **Secrets** → **Actions**
2. Check `FIREBASE_SERVICE_ACCOUNT_KEY` exists
3. Verify the JSON is valid

---

## 🔍 Step 5: Check Environment Variables

1. Azure Portal → Your App Service → **Configuration** → **Environment variables**
2. Verify these exist:
   - `MongoDB__ConnectionString`
   - `MongoDB__DatabaseName`
   - `Jwt__Key`
   - `Jwt__Issuer`
   - `Jwt__Audience`
   - `Firebase__CredentialPath`

---

## 🔍 Step 6: Check Application Insights

1. Azure Portal → Your App Service → **Application Insights**
2. Check **Failures** tab
3. Look for error details

---

## 🔧 Common Fixes

### Fix 1: Restart App Service

1. Azure Portal → Your App Service → **Overview**
2. Click **Restart**
3. Wait 2-3 minutes
4. Try accessing the URL again

### Fix 2: Check Deployment Status

1. Azure Portal → Your App Service → **Deployment Center**
2. Check if latest deployment succeeded
3. If failed, check GitHub Actions logs

### Fix 3: Verify Port Configuration

For Linux App Service, check:

1. Azure Portal → Your App Service → **Configuration** → **General settings**
2. **Always On**: Should be **On**
3. **HTTP Version**: Should be **2.0**

### Fix 4: Check Startup Timeout

1. Azure Portal → Your App Service → **Configuration** → **General settings**
2. Increase **Startup timeout** if needed (default is 230 seconds)

---

## 🐛 Debugging Steps

### Step 1: Check Log Stream

```bash
# In Azure Portal → Log stream
# Look for errors like:
- "Firebase credential file not found"
- "MongoDB connection failed"
- "Application startup exception"
```

### Step 2: Test Locally

```bash
cd RepSense.API/RepSense.API
dotnet run
```

If it works locally but not in Azure, it's a configuration issue.

### Step 3: Check Deployment Logs

1. GitHub → **Actions** → Latest workflow run
2. Check if deployment succeeded
3. Look for warnings or errors

---

## ✅ Quick Checklist

- [ ] App Service is **Running** (not Stopped)
- [ ] Firebase `serviceAccountKey.json` exists in deployment
- [ ] All environment variables are set
- [ ] MongoDB connection string is correct
- [ ] App Service has been restarted after configuration changes
- [ ] Latest deployment succeeded
- [ ] Checked Log Stream for errors

---

## 🚨 Most Common Causes

1. **Missing Firebase credentials** (90% of cases)
2. **App Service is Stopped**
3. **Missing environment variables**
4. **MongoDB connection string incorrect**
5. **App crashing on startup**

---

## 📞 Still Not Working?

1. Check **Log Stream** for specific error messages
2. Share the error message from logs
3. Verify all secrets are set in GitHub
4. Check if deployment completed successfully
