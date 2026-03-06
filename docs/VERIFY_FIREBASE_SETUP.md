# Verify Firebase Credentials Setup

## Step 1: Check GitHub Secret

1. Go to: `https://github.com/Chaniduw/RepSense-API/settings/secrets/actions`
2. Look for `FIREBASE_SERVICE_ACCOUNT_KEY`
3. If it exists, verify it's not empty
4. If it doesn't exist, add it (see below)

## Step 2: Add Firebase Secret (If Missing)

1. Open your local `serviceAccountKey.json` file
2. Copy **ALL** the JSON content (entire file)
3. In GitHub → **Settings** → **Secrets and variables** → **Actions**
4. Click **New repository secret**
5. Name: `FIREBASE_SERVICE_ACCOUNT_KEY`
6. Value: Paste the entire JSON content
7. Click **Add secret**

## Step 3: Redeploy

After adding the secret, trigger a new deployment:

1. Go to GitHub → **Actions** tab
2. Select the workflow
3. Click **Run workflow** → **Run workflow**
4. Or make a small change and push:
   ```bash
   git commit --allow-empty -m "Trigger deployment"
   git push
   ```

## Step 4: Verify in Logs

After redeployment, check Azure Log Stream:
- Should NOT see: `WARNING: Firebase Credential file not found`
- Should see Firebase initialized successfully
