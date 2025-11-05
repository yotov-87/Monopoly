# Monopoly Azure Deployment Guide

## 🎯 Deployed Resources

### 1. PostgreSQL Database
- **Server**: monopoly-db-server.postgres.database.azure.com
- **Database**: monopoly
- **Tier**: Burstable B1ms (1 vCore, 2GB RAM)
- **Location**: North Europe
- **Cost**: ~$12-15/month

### 2. Backend API (Container App)
- **URL**: https://monopoly-api.proudisland-741dd732.northeurope.azurecontainerapps.io
- **Registry**: monopolygameregistry.azurecr.io
- **Image**: monopoly-api:latest
- **Location**: North Europe
- **Cost**: ~$0-2/month (Consumption-based)

### 3. Angular Client (Static Web App)
- **URL**: https://blue-pebble-0156b5403.3.azurestaticapps.net
- **Tier**: Free
- **Location**: West Europe
- **Cost**: $0/month

### 4. Container Registry
- **Name**: monopolygameregistry.azurecr.io
- **Tier**: Basic
- **Cost**: ~$5/month

## 💰 Total Monthly Cost: ~$17-22/month

---

## 🔄 Redeploy Backend API

1. Build new Docker image:
```bash
cd /path/to/Monopoly
sudo docker build -t monopolygameregistry.azurecr.io/monopoly-api:latest -f Monopoly.Api/Dockerfile .
```

2. Push to registry:
```bash
sudo docker push monopolygameregistry.azurecr.io/monopoly-api:latest
```

3. Update Container App:
```bash
az containerapp update --name monopoly-api --resource-group monopoly-rg --image monopolygameregistry.azurecr.io/monopoly-api:latest
```

---

## 🔄 Redeploy Angular Client

1. Build production version:
```bash
cd Monopoly.Client
npm run build
```

**Note**: The build automatically includes `staticwebapp.config.json` for proper routing.

2. Deploy to Static Web App:
```bash
swa deploy ./dist/Monopoly.Client/browser --env production --deployment-token "YOUR_TOKEN"
```

Get token:
```bash
az staticwebapp secrets list --name monopoly-client --resource-group monopoly-rg --query "properties.apiKey" -o tsv
```

---

## 🔧 Routing Configuration

The `staticwebapp.config.json` file ensures that Angular routing works correctly with deep links and page refreshes. All non-file routes are redirected to `/index.html` with a 200 status code.

---

## 🔐 Database Connection

**Connection String**:
```
Host=monopoly-db-server.postgres.database.azure.com;Database=monopoly;Username=monopolyadmin;Password=Monopoly2025!SecurePass;SSL Mode=Require
```

---

## 🧪 Test Endpoints

### Register new user:
```bash
curl -X POST https://monopoly-api.proudisland-741dd732.northeurope.azurecontainerapps.io/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"Test123!"}'
```

### Login:
```bash
curl -X POST https://monopoly-api.proudisland-741dd732.northeurope.azurecontainerapps.io/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"Test123!"}'
```

---

## 📝 Environment Variables (Container App)

- `ConnectionStrings__DefaultConnection`: PostgreSQL connection string
- `JwtSettings__SecretKey`: JWT secret key
- `JwtSettings__Issuer`: MonopolyApi
- `JwtSettings__Audience`: MonopolyClient
- `ASPNETCORE_URLS`: http://+:8080

---

## 🔧 Update CORS Origins

If you need to add new origins, edit `Monopoly.Api/Program.cs`:

```csharp
builder.WithOrigins(
    "http://localhost:4200",
    "https://blue-pebble-0156b5403.3.azurestaticapps.net",
    "https://your-new-domain.com"
)
```

Then rebuild and redeploy.

---

## 📊 Resource Group

All resources are in: **monopoly-rg** (North Europe & West Europe)

To list all resources:
```bash
az resource list --resource-group monopoly-rg --output table
```

To delete everything:
```bash
az group delete --name monopoly-rg --yes --no-wait
```
