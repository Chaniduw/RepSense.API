# Quick Start: Deploy to Azure with GitHub Actions

This is a condensed guide to get you deploying in 10 minutes.

## ⚡ Quick Steps

### 1. Create Azure App Service (5 min)

1. Go to [Azure Portal](https://portal.azure.com)
2. **Create** → **Web App**
3. Fill in:
   - Name: `repsense-api-xyz` (change to your unique name)
   - Runtime: `.NET 8 (LTS)`
   - OS: `Windows` or `Linux`
4. Click **Create**

### 2. Configure Azure App Settings (2 min)

In Azure Portal → Your App Service → **Configuration** → **Application settings**:

Add these (click **+ New application setting**):

```
MongoDB__ConnectionString = [Your MongoDB connection string]
MongoDB__DatabaseName = RepSenseDb
Jwt__Key = [Generate a random 32+ character string]
Jwt__Issuer = RepSense.API
Jwt__Audience = RepSense.App
Firebase__CredentialPath = serviceAccountKey.json
```

Click **Save** → **Continue**

### 3. Push Code to GitHub (2 min)

```bash
cd RepsenseAPI
git init
git add .
git commit -m "Initial commit"
git remote add origin https://github.com/YOUR_USERNAME/RepSense-API.git
git push -u origin main
```

### 4. Configure GitHub Secrets (2 min)

1. Go to GitHub → Your Repo → **Settings** → **Secrets and variables** → **Actions**
2. Add these secrets:

   **Secret 1: `AZURE_WEBAPP_PUBLISH_PROFILE`**
   - In Azure Portal → Your App Service → Click **Get publish profile** (top toolbar)
   - Download the `.PublishSettings` file
   - Open it and copy **ALL** the XML content
   - Paste into GitHub secret

   **Secret 2: `FIREBASE_SERVICE_ACCOUNT_KEY`**
   - Open your `serviceAccountKey.json` file
   - Copy **ALL** the JSON content
   - Paste into GitHub secret

### 5. Update Workflow File (1 min)

1. Open `.github/workflows/azure-deploy.yml`
2. Change line 11:
   ```yaml
   AZURE_WEBAPP_NAME: repsense-api-xyz  # Change to YOUR App Service name
   ```
3. Commit and push:
   ```bash
   git add .github/workflows/azure-deploy.yml
   git commit -m "Configure deployment"
   git push
   ```

### 6. Watch It Deploy! 🚀

1. Go to GitHub → **Actions** tab
2. You should see the workflow running
3. Wait 2-3 minutes
4. ✅ Green checkmark = Success!

### 7. Test Your API

Your API is now live at:
```
https://repsense-api-xyz.azurewebsites.net/swagger
```

---

## 🔄 Making Changes

After initial setup, deploying changes is super easy:

```bash
# Make your code changes
git add .
git commit -m "Your changes"
git push origin main
```

That's it! GitHub Actions will automatically deploy your changes.

---

## ❓ Troubleshooting

**Workflow fails?**
- Check GitHub → Actions → Click failed workflow → Check logs
- Verify secrets are set correctly
- Make sure App Service name matches in workflow file

**API returns 500 errors?**
- Check Azure Portal → App Service → **Log stream**
- Verify all App Settings are configured
- Check MongoDB connection string is correct

**Need help?**
See [Full Deployment Guide](DEPLOYMENT_GUIDE.md) for detailed instructions.

---

**That's it! You're now deploying automatically! 🎉**
