# Azure Deployment Guide - GitHub Actions CI/CD

This guide explains how to deploy your RepSense API to Azure App Service using GitHub Actions for continuous deployment.

---

## 📋 Table of Contents

1. [Prerequisites](#prerequisites)
2. [Initial Azure Setup](#initial-azure-setup)
3. [GitHub Repository Setup](#github-repository-setup)
4. [Configure GitHub Secrets](#configure-github-secrets)
5. [Configure Azure App Service](#configure-azure-app-service)
6. [Deployment Workflow](#deployment-workflow)
7. [Manual Deployment Options](#manual-deployment-options)
8. [Troubleshooting](#troubleshooting)

---

## Prerequisites

- ✅ Azure subscription
- ✅ GitHub account
- ✅ Azure App Service created (see [Initial Azure Setup](#initial-azure-setup))
- ✅ Firebase `serviceAccountKey.json` file

---

## Initial Azure Setup

### Step 1: Create Azure App Service

1. Go to [Azure Portal](https://portal.azure.com)
2. Click **Create a resource** → **Web App**
3. Fill in:
   - **Subscription**: Your subscription
   - **Resource Group**: Create new (e.g., `RepSense-RG`)
   - **Name**: `repsense-api-xyz` (must be unique globally)
   - **Publish**: `Code`
   - **Runtime stack**: `.NET 8 (LTS)`
   - **Operating System**: `Windows` or `Linux` (both work)
   - **Region**: Choose closest to your users
4. Click **Review + create** → **Create**
5. Wait for deployment to complete

### Step 2: Configure App Settings in Azure

1. In Azure Portal, go to your App Service
2. Navigate to **Configuration** → **Application settings**
3. Add these settings (click **+ New application setting** for each):

   ```
   MongoDB__ConnectionString = mongodb+srv://your-connection-string
   MongoDB__DatabaseName = RepSenseDb
   Jwt__Key = your-super-secret-jwt-key-min-32-characters
   Jwt__Issuer = RepSense.API
   Jwt__Audience = RepSense.App
   Firebase__CredentialPath = serviceAccountKey.json
   ASPNETCORE_ENVIRONMENT = Production
   ```

4. Click **Save** → **Continue** (restart app)

**Important**: The `__` (double underscore) is used by .NET Configuration to represent nested JSON sections.

---

## GitHub Repository Setup

### Step 1: Initialize Git Repository (if not already done)

```bash
cd RepsenseAPI
git init
git add .
git commit -m "Initial commit"
```

### Step 2: Create GitHub Repository

1. Go to [GitHub](https://github.com) → **New repository**
2. Name it: `RepSense-API` (or your preferred name)
3. **Don't** initialize with README (you already have files)
4. Click **Create repository**

### Step 3: Push Code to GitHub

```bash
git remote add origin https://github.com/YOUR_USERNAME/RepSense-API.git
git branch -M main
git push -u origin main
```

**⚠️ Important**: Make sure `serviceAccountKey.json` is in `.gitignore` (already added) and never commit it!

---

## Configure GitHub Secrets

GitHub Secrets store sensitive information securely. You need to add these:

### Step 1: Get Azure Publish Profile

1. In Azure Portal, go to your App Service (`repsense-api-lk`)
2. **Make sure you're on the Overview page** (click **Overview** in the left menu if you're not there)
3. Look at the **top toolbar** (above the main content area) - you'll see buttons like:
   - **Stop** / **Start** / **Restart**
   - **Download publish profile** ← **This is the button you need!**
4. Click **Download publish profile** button
5. **If you see a dialog saying "Basic authentication is disabled"**: 
   - Click **OK** (this is just informational, the file should still download)
   - Check your browser's **Downloads folder** for a `.PublishSettings` file
6. If the file didn't download automatically:
   - Try right-clicking the button and selecting **"Save link as..."**
   - Or use the **Alternative Method** below (Service Principal)
7. Open the downloaded `.PublishSettings` file in a text editor (Notepad, VS Code, etc.)
8. Copy the **entire XML content** (select all → copy)

**Note**: The "Download publish profile" button is only visible on the **Overview** page, not in Environment variables or other settings pages.

**Troubleshooting**: If you can't download the publish profile or get authentication errors, use the **Alternative Method** below instead.

### Step 2: Get Azure Service Principal (Alternative Method)

If you prefer using service principal instead of publish profile:

```bash
# Install Azure CLI if not installed
az login

# Create service principal
az ad sp create-for-rbac --name "RepSense-API-Deploy" --role contributor \
  --scopes /subscriptions/YOUR_SUBSCRIPTION_ID/resourceGroups/RepSense-RG \
  --sdk-auth

# Copy the JSON output - you'll need it for AZURE_CREDENTIALS secret
```

### Step 3: Get Firebase Service Account Key

1. Open your `serviceAccountKey.json` file
2. Copy the entire JSON content

### Step 4: Add Secrets to GitHub

1. Go to your GitHub repository
2. Click **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret** and add:

   | Secret Name | Value | Description |
   |------------|-------|-------------|
   | `AZURE_WEBAPP_PUBLISH_PROFILE` | Paste the entire XML from `.PublishSettings` file | Azure deployment credentials |
   | `FIREBASE_SERVICE_ACCOUNT_KEY` | Paste the entire JSON from `serviceAccountKey.json` | Firebase Admin SDK credentials |
   | `AZURE_CREDENTIALS` | (Optional) JSON from `az ad sp create-for-rbac` | Alternative authentication method |

---

## Configure Azure App Service

### Update Workflow File

1. Open `.github/workflows/azure-deploy.yml`
2. Update the `AZURE_WEBAPP_NAME` environment variable:

   ```yaml
   env:
     AZURE_WEBAPP_NAME: repsense-api-xyz  # Change to your actual App Service name
   ```

3. Commit and push:

   ```bash
   git add .github/workflows/azure-deploy.yml
   git commit -m "Configure Azure deployment workflow"
   git push
   ```

---

## Deployment Workflow

### Automatic Deployment

Once configured, **every push to `main` branch will automatically deploy**:

1. Make changes to your code
2. Commit and push:

   ```bash
   git add .
   git commit -m "Your change description"
   git push origin main
   ```

3. Go to GitHub → **Actions** tab
4. Watch the workflow run:
   - ✅ Green checkmark = Deployment successful
   - ❌ Red X = Deployment failed (check logs)

### Manual Deployment

You can also trigger deployment manually:

1. Go to GitHub → **Actions** tab
2. Select **Deploy to Azure App Service** workflow
3. Click **Run workflow** → **Run workflow**

### Deployment Branches

The workflow is configured to deploy:
- `main` branch → Production
- `develop` branch → Staging (optional)

To change this, edit `.github/workflows/azure-deploy.yml`:

```yaml
on:
  push:
    branches:
      - main      # Change to your production branch
      - develop   # Add/remove branches as needed
```

---

## Manual Deployment Options

If you need to deploy manually without GitHub Actions:

### Option 1: Visual Studio Publish

1. Open `RepSense.API.sln` in Visual Studio
2. Right-click `RepSense.API` project → **Publish**
3. Select your Azure App Service
4. Click **Publish**

### Option 2: Azure CLI

```bash
# Login to Azure
az login

# Navigate to project folder
cd RepSense.API/RepSense.API

# Publish
dotnet publish -c Release -o ./publish
cd publish
zip -r ../repsense-api.zip .

# Deploy
az webapp deployment source config-zip \
  --resource-group RepSense-RG \
  --name repsense-api-xyz \
  --src ../repsense-api.zip
```

### Option 3: Azure Portal (Kudu)

1. Go to Azure Portal → Your App Service
2. Navigate to **Development Tools** → **Advanced Tools (Kudu)** → **Go**
3. Click **Debug console** → **CMD**
4. Navigate to `site/wwwroot`
5. Upload your published files via drag-and-drop or FTP

---

## Troubleshooting

### Issue: Workflow fails with "Publish profile not found"

**Solution**: 
- Verify `AZURE_WEBAPP_PUBLISH_PROFILE` secret is set correctly
- Make sure you copied the entire XML content from `.PublishSettings` file

### Issue: Firebase credential file not found

**Solution**:
- Verify `FIREBASE_SERVICE_ACCOUNT_KEY` secret contains valid JSON
- Check that the workflow step creates the file correctly:
  ```yaml
  - name: Create Firebase credential file
    run: |
      echo '${{ secrets.FIREBASE_SERVICE_ACCOUNT_KEY }}' > ./publish/serviceAccountKey.json
  ```

### Issue: App starts but returns 500 errors

**Solution**:
- Check Azure App Service **Log stream** for errors
- Verify all App Settings are configured correctly in Azure Portal
- Ensure MongoDB connection string is correct
- Check JWT settings match between Azure and your Flutter app

### Issue: Changes not reflecting after deployment

**Solution**:
- Check deployment logs in GitHub Actions
- Verify the workflow completed successfully
- Restart the App Service manually in Azure Portal
- Clear browser cache if testing via browser

### Issue: Build fails with "project not found"

**Solution**:
- Verify `PROJECT_PATH` in workflow file matches your project structure
- Check that `.csproj` file exists at the specified path

---

## Best Practices

### 1. Environment-Specific Deployments

Create separate workflows for different environments:

- `.github/workflows/deploy-production.yml` → `main` branch
- `.github/workflows/deploy-staging.yml` → `develop` branch

### 2. Deployment Slots

Use Azure Deployment Slots for zero-downtime deployments:

1. Create a staging slot in Azure Portal
2. Deploy to staging first
3. Swap staging ↔ production after testing

### 3. Secrets Management

- ✅ Never commit secrets to Git
- ✅ Use GitHub Secrets for sensitive data
- ✅ Rotate secrets regularly
- ✅ Use different secrets for different environments

### 4. Monitoring

- Set up Application Insights in Azure
- Monitor deployment logs in GitHub Actions
- Set up alerts for failed deployments

### 5. Rollback Strategy

If deployment fails:
1. Check GitHub Actions logs
2. Fix the issue
3. Redeploy, or
4. Use Azure Portal → **Deployment Center** → **Redeploy** previous successful deployment

---

## Quick Reference

### Update App Settings in Azure

```bash
az webapp config appsettings set \
  --resource-group RepSense-RG \
  --name repsense-api-xyz \
  --settings MongoDB__ConnectionString="your-connection-string"
```

### View Deployment Logs

```bash
az webapp log tail \
  --resource-group RepSense-RG \
  --name repsense-api-xyz
```

### Restart App Service

```bash
az webapp restart \
  --resource-group RepSense-RG \
  --name repsense-api-xyz
```

---

## Next Steps

1. ✅ Set up monitoring with Application Insights
2. ✅ Configure custom domain
3. ✅ Set up SSL certificate
4. ✅ Configure auto-scaling
5. ✅ Set up backup strategy

---

**Last Updated**: 2024
**Version**: 1.0
