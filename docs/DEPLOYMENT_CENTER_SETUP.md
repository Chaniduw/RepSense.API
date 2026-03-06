# Deploy Using Azure Deployment Center (Easiest Method)

If you can't download the publish profile, use Azure Portal's **Deployment Center** to connect GitHub directly. This is actually easier!

---

## Step-by-Step Guide

### Step 1: Go to Deployment Center in Azure Portal

1. In Azure Portal, go to your App Service (`repsense-api-lk`)
2. In the left menu, scroll down to **Deployment**
3. Click **Deployment Center**

### Step 2: Connect GitHub Repository

1. In the **Source** tab:
   - **Source**: Select **GitHub**
   - Click **Authorize** if prompted (sign in with your GitHub account)
   - **Organization**: Select your GitHub username
   - **Repository**: Select `RepSense-API` (or your repo name)
   - **Branch**: Select `main` (or your default branch)
   - **Build provider**: Select **GitHub Actions** (recommended) or **App Service build service**

2. Click **Save**

### Step 3: Azure Will Create the Workflow

Azure will automatically:
- Create a GitHub Actions workflow file (`.github/workflows/azure-webapps-deploy.yml`)
- Add necessary secrets to your GitHub repository
- Set up the deployment pipeline

### Step 4: Verify the Workflow

1. Go to your GitHub repository
2. Check `.github/workflows/` folder - you should see a new workflow file
3. Go to **Actions** tab - you should see a workflow running

### Step 5: Update the Workflow (if needed)

If Azure created a workflow, you might need to update it to match your project structure. Check if it needs:
- Correct project path
- Firebase credential setup (the workflow we created earlier)

---

## Alternative: Use Azure CLI to Get Publish Profile

If Deployment Center doesn't work, try getting the publish profile via CLI:

```powershell
# Get publish profile via Azure CLI
az webapp deployment list-publishing-profiles --name repsense-api-lk --resource-group RepSense-RG --xml
```

This will output the XML directly in the terminal - copy it and use it as the GitHub secret.

---

## Alternative: Manual Deployment via Visual Studio

If all else fails, you can deploy manually:

1. Open `RepSense.API.sln` in Visual Studio
2. Right-click `RepSense.API` project → **Publish**
3. Select **Azure** → **Azure App Service (Windows)** or **Azure App Service (Linux)**
4. Select your App Service (`repsense-api-lk`)
5. Click **Publish**

This will deploy directly without needing GitHub Actions.

---

**Try the Deployment Center method first - it's the easiest!**
