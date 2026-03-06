# Fix Firebase Secret Issue

## Problem
The `FIREBASE_SERVICE_ACCOUNT_KEY` secret exists in GitHub, but the Firebase credential file is not being created during deployment.

## Solution

### Step 1: Verify Secret is Not Empty

GitHub Secrets never show their values (for security), but you can verify it's set:

1. Go to GitHub → Your Repo → **Settings** → **Secrets** → **Actions**
2. Find `FIREBASE_SERVICE_ACCOUNT_KEY`
3. Click **Edit**
4. If the field appears empty, it might actually be empty or just hidden
5. **Re-add the secret** to be sure:

### Step 2: Re-add Firebase Secret

1. Open your local `serviceAccountKey.json` file
2. **Copy ALL content** (Ctrl+A, Ctrl+C)
3. In GitHub → **Settings** → **Secrets** → **Actions**
4. Click **Edit** on `FIREBASE_SERVICE_ACCOUNT_KEY`
5. **Delete the old value** (select all and delete)
6. **Paste the entire JSON** from your file
7. Click **Update secret**

### Step 3: Check Which Workflow is Running

Azure Deployment Center might have created its own workflow file. Check:

1. Go to GitHub → **Actions** tab
2. Look at the workflow file name being used
3. If it's `main_repsense-api-lk.yml` or similar (Azure-generated), you need to update that file

### Step 4: Update Azure-Generated Workflow (If Needed)

If Azure created its own workflow file:

1. Go to GitHub → **Actions** → Click on a workflow run
2. Click on the workflow file name (e.g., `main_repsense-api-lk.yml`)
3. Click **Edit** (pencil icon)
4. Add this step **before** the deployment step:

```yaml
- name: Create Firebase credential file
  run: |
    if [ -z "${{ secrets.FIREBASE_SERVICE_ACCOUNT_KEY }}" ]; then
      echo "ERROR: FIREBASE_SERVICE_ACCOUNT_KEY secret is empty or not set!"
      exit 1
    fi
    echo '${{ secrets.FIREBASE_SERVICE_ACCOUNT_KEY }}' > ./publish/serviceAccountKey.json
    echo "Firebase credential file created successfully"
    ls -la ./publish/serviceAccountKey.json
  shell: bash
```

5. Commit the changes

### Step 5: Trigger New Deployment

After updating the secret or workflow:

1. Go to GitHub → **Actions** tab
2. Select your workflow
3. Click **Run workflow** → **Run workflow**
4. Or make a small change and push:
   ```bash
   git commit --allow-empty -m "Trigger deployment"
   git push
   ```

### Step 6: Verify in Logs

After deployment, check:

1. **GitHub Actions logs**: Look for "Firebase credential file created successfully"
2. **Azure Log Stream**: Should NOT see "WARNING: Firebase Credential file not found"

---

## Alternative: Manual File Upload

If the workflow approach doesn't work, you can manually upload the file:

1. Azure Portal → Your App Service → **SSH** or **Console**
2. Navigate to `/home/site/wwwroot/`
3. Create `serviceAccountKey.json` file
4. Paste your Firebase JSON content
5. Save

**Note**: This file will be overwritten on next deployment, so you still need to fix the workflow.

---

## Quick Test

To verify the secret is set correctly, check the GitHub Actions logs:

1. Go to GitHub → **Actions** → Latest workflow run
2. Expand "Create Firebase credential file" step
3. Look for:
   - ✅ "Firebase credential file created successfully"
   - ❌ "ERROR: FIREBASE_SERVICE_ACCOUNT_KEY secret is empty"

If you see the error, the secret is empty and needs to be re-added.
