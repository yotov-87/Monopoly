# Deployment Guide - Monopoly Game

## Overview
This guide will walk you through deploying the Monopoly application using:
- **Supabase** (Free PostgreSQL Database)
- **Railway** (Backend API - $5/month)
- **Vercel** (Frontend - Free)

Total estimated cost: **$5/month**

---

## Prerequisites
- GitHub account
- Code pushed to GitHub repository
- Email address for service signups

---

## Part 1: Database Setup (Supabase)

### 1.1 Create Supabase Account
1. Go to https://supabase.com
2. Click "Start your project"
3. Sign up with GitHub

### 1.2 Create New Project
1. Click "New Project"
2. Fill in:
   - **Name**: monopoly-db (or your choice)
   - **Database Password**: Generate a strong password (save it!)
   - **Region**: Choose closest to you
   - **Pricing Plan**: Free
3. Click "Create new project" (takes ~2 minutes)

### 1.3 Get Connection String
1. Go to **Project Settings** (gear icon)
2. Click **Database** in sidebar
3. Scroll to **Connection string**
4. Select **URI** tab
5. Copy the connection string (format: `postgresql://postgres:[YOUR-PASSWORD]@[HOST]:[PORT]/postgres`)
6. Replace `[YOUR-PASSWORD]` with your actual password
7. **Save this connection string** - you'll need it for Railway

Example:
```
postgresql://postgres:YourPassword123@db.abcdefghijk.supabase.co:5432/postgres
```

---

## Part 2: Backend Deployment (Railway)

### 2.1 Create Railway Account
1. Go to https://railway.app
2. Click "Start a New Project"
3. Sign up with GitHub

### 2.2 Deploy from GitHub
1. Click "New Project"
2. Select "Deploy from GitHub repo"
3. Connect your GitHub account (authorize Railway)
4. Select your **Monopoly** repository
5. Railway will detect .NET project automatically

### 2.3 Configure Root Directory
1. After project creation, click on your service
2. Go to **Settings** tab
3. Find "Root Directory"
4. Set to: `Monopoly/Monopoly.Api`
5. Click "Save"

### 2.4 Set Environment Variables
1. Go to **Variables** tab
2. Add the following variables:

**DATABASE_URL**
```
postgresql://postgres:YourPassword123@db.abcdefghijk.supabase.co:5432/postgres
```
(Your Supabase connection string from Part 1)

**JWT_SECRET_KEY**
```
YourSuperSecretKeyThatShouldBeAtLeast32CharactersLongForProduction123456!
```
(Generate a random 64-character string for security)

**ASPNETCORE_ENVIRONMENT**
```
Production
```

**FRONTEND_URL**
```
https://your-app.vercel.app
```
(Leave empty for now, we'll update this after Vercel deployment)

3. Click "Save" after adding all variables

### 2.5 Apply Database Migrations
1. Wait for Railway deployment to complete (~3-5 minutes)
2. Go to your service → **Settings** → Find your Railway URL (e.g., `https://monopoly-api.up.railway.app`)
3. Open terminal locally:

```bash
cd /home/dimitar-yotov/Desktop/MonopolyRepo/Monopoly/Monopoly.Data

# Update connection string temporarily
export DATABASE_URL="postgresql://postgres:YourPassword@db.xxx.supabase.co:5432/postgres"

# Apply migrations
dotnet ef database update --startup-project ../Monopoly.Api
```

Alternative: Railway will auto-create tables on first run if you have `EnsureCreated()` in Program.cs

### 2.6 Get Backend URL
1. Go to **Settings** tab
2. Scroll to **Domains**
3. Click "Generate Domain"
4. Copy the generated URL (e.g., `https://monopoly-production.up.railway.app`)
5. **Save this URL** - you'll need it for Vercel

---

## Part 3: Frontend Deployment (Vercel)

### 3.1 Create Vercel Account
1. Go to https://vercel.com
2. Click "Sign Up"
3. Sign up with GitHub

### 3.2 Import Project
1. Click "Add New..." → "Project"
2. Import your **Monopoly** repository
3. Configure project:
   - **Framework Preset**: Other
   - **Root Directory**: Click "Edit" → Select `Monopoly/Monopoly.Client`
   - **Build Command**: `npm install && npm run build`
   - **Output Directory**: `dist/Monopoly.Client/browser`

### 3.3 Add Environment Variables
Before deploying, click **Environment Variables**:

**VITE_API_URL** (or handle in build script)
```
https://monopoly-production.up.railway.app/api
```
(Your Railway backend URL from Part 2)

**VITE_HUB_URL**
```
https://monopoly-production.up.railway.app/hubs
```

**Note**: Since you're using Angular's environment files, you need to update the production environment file before deployment.

### 3.4 Update Production Environment (Do this before Vercel deploy)
1. Open `Monopoly.Client/src/environments/environment.production.ts`
2. Replace with your Railway URL:

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://monopoly-production.up.railway.app/api',
  hubUrl: 'https://monopoly-production.up.railway.app/hubs'
};
```

3. Commit and push to GitHub:
```bash
git add .
git commit -m "chore: Update production environment with Railway URL"
git push
```

### 3.5 Deploy
1. Click "Deploy"
2. Wait 2-3 minutes for build
3. Once complete, Vercel will show your live URL (e.g., `https://monopoly-game.vercel.app`)

---

## Part 4: Final Configuration

### 4.1 Update Railway CORS
1. Go back to Railway
2. Go to your service → **Variables**
3. Update **FRONTEND_URL** with your Vercel URL:
```
https://monopoly-game.vercel.app
```
4. Railway will automatically redeploy

### 4.2 Test Your Application
1. Open your Vercel URL: `https://monopoly-game.vercel.app`
2. Try to register a new user
3. Create a game
4. Test gameplay

---

## Troubleshooting

### Backend Issues
- **500 Error**: Check Railway logs (Deployments → View logs)
- **Database Connection Failed**: Verify DATABASE_URL in Railway variables
- **CORS Error**: Ensure FRONTEND_URL matches your Vercel domain exactly

### Frontend Issues
- **API calls failing**: Check browser console, verify environment.production.ts URLs
- **SignalR not connecting**: Check hubUrl in environment file
- **404 on refresh**: Ensure vercel.json is present with rewrites config

### Database Issues
- **Migrations not applied**: Run `dotnet ef database update` manually with Supabase connection string
- **Connection timeout**: Check Supabase project is active and password is correct

---

## Monitoring & Maintenance

### Railway (Backend)
- Free tier: $5/month for 500 execution hours
- View logs: Deployments → Click deployment → View logs
- Auto-redeploys on GitHub push

### Vercel (Frontend)
- Free tier: Unlimited bandwidth for personal projects
- Auto-redeploys on GitHub push
- View deployment logs in dashboard

### Supabase (Database)
- Free tier: 500MB storage, unlimited API requests
- Monitor usage: Project dashboard → Database

---

## Cost Summary
- **Supabase**: $0/month (Free tier)
- **Vercel**: $0/month (Free tier)
- **Railway**: $5/month (Hobby tier)

**Total: $5/month**

---

## Next Steps After Deployment
1. Set up custom domain (optional) - Vercel supports custom domains
2. Enable HTTPS (automatic with Railway and Vercel)
3. Set up monitoring/alerts
4. Configure backup strategy for database
5. Add rate limiting and security headers

---

## Environment Variables Reference

### Railway (Backend)
```
DATABASE_URL=postgresql://postgres:password@host:5432/postgres
JWT_SECRET_KEY=your-64-character-secret-key
ASPNETCORE_ENVIRONMENT=Production
FRONTEND_URL=https://your-app.vercel.app
```

### Vercel (Frontend) - In environment.production.ts
```typescript
apiUrl: 'https://your-backend.up.railway.app/api'
hubUrl: 'https://your-backend.up.railway.app/hubs'
```

---

## Support
If you encounter issues:
1. Check Railway logs for backend errors
2. Check browser console for frontend errors
3. Verify all environment variables are set correctly
4. Ensure database migrations are applied
