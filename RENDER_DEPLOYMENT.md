# Deploying Monopoly to Render.com (100% Free)

This guide walks you through deploying the Monopoly game to Render.com using their free tier. The setup includes:
- PostgreSQL Database (Free)
- .NET 8 Backend API (Free)
- Angular Frontend (Free Static Site)

## Prerequisites

- A GitHub account
- A Render.com account (sign up at https://render.com)
- Your code pushed to a GitHub repository

## Step 1: Push Code to GitHub

Make sure all your code is committed and pushed to GitHub:

```bash
cd /home/dimitar-yotov/Desktop/MonopolyRepo/Monopoly
git add .
git commit -m "Prepare for Render deployment"
git push origin main_initial_Azure
```

## Step 2: Sign Up / Log In to Render

1. Go to https://render.com
2. Sign up or log in (recommend using GitHub OAuth)
3. Authorize Render to access your GitHub repositories

## Step 3: Create a New Blueprint

Render will automatically detect the `render.yaml` file and set up all services.

1. From the Render dashboard, click **"New +"** → **"Blueprint"**
2. Connect your GitHub repository
3. Select the repository: `yotov-87/Monopoly`
4. Select the branch: `main_initial_Azure` (or your main branch)
5. Click **"Apply"**

Render will automatically create:
- **monopoly-db** - PostgreSQL database
- **monopoly-api** - Backend API
- **monopoly-frontend** - Frontend static site

## Step 4: Configure Environment Variables

After the services are created, you'll need to verify/add some environment variables:

### For `monopoly-api` (Backend):

The following are auto-configured by `render.yaml`:
- ✅ `ASPNETCORE_ENVIRONMENT` = Production
- ✅ `ASPNETCORE_URLS` = http://0.0.0.0:5000
- ✅ `ConnectionStrings__DefaultConnection` (auto-linked from database)
- ✅ `JwtSettings__Issuer` = MonopolyApi
- ✅ `JwtSettings__Audience` = MonopolyClient
- ✅ `FRONTEND_URL` (auto-linked from frontend)

**Manual step:** 
- Set `JwtSettings__SecretKey` to a strong random value (32+ characters)
  - In Render dashboard, go to `monopoly-api` → Environment
  - Update `JwtSettings__SecretKey` with a secure random string

### For `monopoly-frontend` (Frontend):

- ✅ `API_URL` (auto-linked from backend)

## Step 5: Monitor Deployment

1. Watch the **Logs** tab for each service
2. Backend API should build using Docker
3. Frontend should build with npm
4. Database should initialize automatically

**Expected deployment time:** 5-10 minutes

## Step 6: Verify Deployment

After deployment completes:

1. **Check Backend Health:**
   - Go to your API URL (e.g., `https://monopoly-api.onrender.com`)
   - Visit `/health` endpoint
   - Should return: `{"status":"healthy","timestamp":"..."}`

2. **Check Frontend:**
   - Go to your frontend URL (e.g., `https://monopoly-frontend.onrender.com`)
   - The Angular app should load

3. **Test SignalR:**
   - The frontend should connect to the backend via WebSocket
   - Check browser console for any connection errors

## Important Notes About Render Free Tier

### ⚠️ Cold Starts
- Free tier services spin down after 15 minutes of inactivity
- First request after inactivity takes 30-60 seconds to "wake up"
- Subsequent requests are fast

### Database Limitations
- Free PostgreSQL expires after 90 days
- Limited to 1 GB storage
- Consider upgrading to paid tier for production

### Service URLs
Your services will have URLs like:
- Backend: `https://monopoly-api.onrender.com`
- Frontend: `https://monopoly-frontend.onrender.com`
- Database: Internal connection string (not publicly accessible)

## Architecture

```
┌─────────────────┐
│   Frontend      │
│   (Static)      │  https://monopoly-frontend.onrender.com
└────────┬────────┘
         │ HTTP/WebSocket
         ▼
┌─────────────────┐
│   Backend API   │
│   (Docker)      │  https://monopoly-api.onrender.com
└────────┬────────┘
         │ PostgreSQL
         ▼
┌─────────────────┐
│   PostgreSQL    │
│   (Database)    │  (Internal)
└─────────────────┘
```

## Configuration Files Created

1. **`render.yaml`** - Blueprint configuration for all services
2. **`Monopoly.Api/Dockerfile`** - Docker container for backend
3. **`Monopoly.Api/.dockerignore`** - Excludes unnecessary files from Docker
4. **`build-frontend.sh`** - Build script for Angular
5. **`appsettings.Production.json`** - Production settings
6. **`environments/environment.ts`** - Angular environment config

## Updating Your App

To deploy changes:

```bash
git add .
git commit -m "Update feature X"
git push origin main_initial_Azure
```

Render automatically deploys on every push to your branch!

## Troubleshooting

### Backend won't start
- Check logs in Render dashboard
- Verify environment variables are set correctly
- Ensure `JwtSettings__SecretKey` is configured

### Frontend can't connect to backend
- Check CORS settings in `Program.cs`
- Verify `FRONTEND_URL` environment variable
- Check browser console for CORS errors

### Database connection issues
- Verify `ConnectionStrings__DefaultConnection` is auto-populated
- Check database service is running
- Review API logs for connection errors

### SignalR connection fails
- Ensure WebSocket support is enabled (it is by default on Render)
- Check CORS allows credentials
- Verify hub endpoint `/hubs/game` is accessible

## Monitoring & Logs

Access logs for each service:
1. Go to Render dashboard
2. Select your service
3. Click **Logs** tab
4. View real-time logs and errors

## Cost Estimate

**Total monthly cost: $0 (100% Free!)**

- PostgreSQL Database: Free (90-day limit)
- Backend API: Free (750 hours/month)
- Frontend Static Site: Free (100 GB bandwidth/month)

## Next Steps

1. Set up custom domain (optional)
2. Enable SSL (automatic on Render)
3. Set up monitoring/alerts
4. Consider upgrading database to paid tier for production

## Support

- Render Documentation: https://render.com/docs
- Render Community: https://community.render.com
- GitHub Issues: https://github.com/yotov-87/Monopoly/issues

---

**Happy deploying! 🚀**
