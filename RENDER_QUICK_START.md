# Quick Start Guide - Render Deployment

## Files Created for Render.com Deployment:

### Core Configuration
1. **`render.yaml`** - Main blueprint file (defines all services)
2. **`RENDER_DEPLOYMENT.md`** - Detailed deployment instructions

### Backend (.NET API)
3. **`Monopoly.Api/Dockerfile`** - Docker configuration
4. **`Monopoly.Api/.dockerignore`** - Docker ignore rules
5. **`Monopoly.Api/appsettings.Production.json`** - Production settings

### Frontend (Angular)
6. **`build-frontend.sh`** - Build script
7. **`Monopoly.Client/src/environments/environment.ts`** - Production environment
8. **`Monopoly.Client/src/environments/environment.development.ts`** - Dev environment

### Updated Files
9. **`Monopoly.Api/Program.cs`** - Added CORS for Render URLs + health check endpoint

---

## Ready to Deploy! 🚀

### Quick Steps:

1. **Commit & Push to GitHub:**
   ```bash
   cd /home/dimitar-yotov/Desktop/MonopolyRepo/Monopoly
   git add .
   git commit -m "Add Render deployment configuration"
   git push origin main_initial_Azure
   ```

2. **Go to Render.com:**
   - Sign up/login at https://render.com
   - Click "New +" → "Blueprint"
   - Connect your GitHub repo: `yotov-87/Monopoly`
   - Select branch: `main_initial_Azure`
   - Click "Apply"

3. **Wait for deployment** (5-10 minutes)
   - All services deploy automatically
   - All environment variables configured automatically
   - **No manual configuration needed!**

4. **Test:**
   - Visit your API health endpoint: `https://monopoly-api.onrender.com/health`
   - Open frontend: `https://monopoly-frontend.onrender.com`

---

## What Gets Deployed:

✅ **PostgreSQL Database** (free, 90-day trial)  
✅ **Backend API** (.NET 8, Dockerized)  
✅ **Frontend** (Angular, static site)  
✅ **Automatic SSL/HTTPS**  
✅ **Auto-deploy on git push**

---

## Important Notes:

⚠️ **Free tier limitations:**
- Services sleep after 15 min inactivity
- First request takes 30-60 sec to wake up
- Database expires after 90 days

💡 **No credit card required for free tier!**

---

For detailed instructions, see **RENDER_DEPLOYMENT.md**
