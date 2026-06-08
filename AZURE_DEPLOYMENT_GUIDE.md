# Azure Deployment Guide — Gym Management SaaS

This guide covers deploying the Gym Management API and Angular frontend to Azure after Production Hardening Sprint 2.

---

## Architecture Overview

```
[Browser] → [Azure Front Door / SWA] → [App Service - API]
                                              ↓
                                    [Azure SQL Database]
                                    [Azure Blob Storage]
                                    [Application Insights]
```

---

## Prerequisites

- Azure subscription
- Azure CLI (`az login`)
- Docker (optional, for container deploy)
- .NET 8 SDK (for migrations)
- Node.js 20+ (for Angular build)

---

## 1. Azure SQL Database

1. Create a resource group: `rg-gym-prod`
2. Create **Azure SQL Server** + **SQL Database** (S1 or higher for production).
3. Configure firewall: allow Azure services; restrict dev IPs as needed.
4. Set connection string:
   ```
   Server=tcp:{server}.database.windows.net,1433;Initial Catalog=GymDb;User ID={user};Password={password};Encrypt=True;TrustServerCertificate=False;
   ```

### Run migrations (CI/CD — do NOT rely on app startup)

```bash
export ConnectionStrings__DefaultConnection="..."
export Jwt__Secret="your-production-secret-min-32-chars"

dotnet run --project Backend/Gym.API -- migrate
```

Verify scripts in `dbo.SchemaVersions`.

---

## 2. Azure Blob Storage (files)

1. Create Storage Account (GRS recommended for production).
2. Create container `gym-files` (private access).
3. Enable **soft delete** for blobs (7–30 days) and containers.
4. Configure App Service settings:
   - `FileStorage__Provider=Azure`
   - `FileStorage__AzureConnectionString` (prefer Key Vault reference)
   - `FileStorage__AzureContainerName=gym-files`
   - `FileStorage__UrlSigningSecret` (unique 32+ char secret)

---

## 3. App Service (API)

### Option A — Container deploy

```bash
az acr create --name gymacr --resource-group rg-gym-prod --sku Basic
az acr build --registry gymacr --image gym-api:latest -f Dockerfile .
az appservice plan create --name plan-gym-api --resource-group rg-gym-prod --is-linux --sku B2
az webapp create --name gym-api-prod --resource-group rg-gym-prod --plan plan-gym-api --deployment-container-image-name gymacr.azurecr.io/gym-api:latest
```

### Option B — Zip deploy / GitHub Actions

Publish with `dotnet publish -c Release` and deploy to Linux App Service (.NET 8 runtime).

### Required App Settings

| Setting | Value |
|---------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | Azure SQL connection string |
| `Jwt__Secret` | Key Vault / secure setting (32+ chars) |
| `Jwt__Issuer` / `Jwt__Audience` | Your production values |
| `Cors__AllowedOrigins__0` | `https://your-frontend-domain.com` |
| `AuthCookies__UseCookieAuth` | `true` |
| `Database__RunMigrationsOnStartup` | **`false`** |
| `Database__RunSeedOnStartup` | **`false`** |
| `Demo__Enabled` | **`false`** |
| `ApplicationInsights__Enabled` | `true` |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | From App Insights resource |
| `WEBSITE_HTTPLOGGING_RETENTION_DAYS` | `7` |

### Cookie auth behind Azure

- App Service terminates TLS; **`ForwardedHeaders`** (Sprint 1) is already configured.
- Set **`AuthCookies`** secure cookies automatically when `IsProduction()` is true.
- Frontend origin **must exactly match** CORS allowed origin (include `https://`, no trailing slash mismatch).
- Enable **ARR Affinity** if using multiple instances (session validation is server-side via DB — affinity optional but helps debugging).

### Health probe

Configure App Service health check path: **`/health`**

---

## 4. Application Insights

```bash
az monitor app-insights component create --app gym-insights --location eastus --resource-group rg-gym-prod
```

Copy connection string to `ApplicationInsights__ConnectionString`. Serilog forwards traces when enabled.

---

## 5. Angular Frontend

### Build

```bash
cd Frontend/gym-app
npm ci
npm run build -- --configuration production
```

Output: `Frontend/gym-app/dist/gym-app/browser`

### Option A — Azure Static Web Apps

1. Create SWA linked to your repo or upload `dist`.
2. Configure `staticwebapp.config.json` to proxy `/api/*` to App Service backend.
3. Ensure API CORS includes SWA URL.

### Option B — Same App Service (sub-path)

Serve static files from a separate App Service or CDN; API remains on `api.yourdomain.com`.

### Production requirements

- Frontend and API on **same site** (path-based) **or** cross-origin with CORS + credentials.
- `useCookieAuth: true` in `environment.prod.ts` (already set).
- HTTPS only in production.

---

## 6. CI/CD Pipeline (recommended stages)

```yaml
# Conceptual pipeline
stages:
  - build_test:
      - dotnet build
      - npm test (when available)
  - migrate:
      - dotnet run --project Backend/Gym.API -- migrate
      # Run ONLY against target environment DB
  - deploy_api:
      - docker build / zip deploy to App Service
  - deploy_frontend:
      - npm run build → SWA deploy
  - smoke_test:
      - curl https://api.example.com/health
      - login E2E (optional)
```

**Critical:** Run migrations **before** deploying new API version. Never set `RunMigrationsOnStartup=true` in Production.

---

## 7. Secrets management

Use **Azure Key Vault references** in App Service:

```
@Microsoft.KeyVault(SecretUri=https://vault.vault.azure.net/secrets/JwtSecret/)
```

Map to `Jwt__Secret`, `ConnectionStrings__DefaultConnection`, `FileStorage__AzureConnectionString`.

---

## 8. Post-deploy verification

1. `GET /health` → `Healthy`, SQL check passes.
2. Login from production frontend → cookies set, no tokens in localStorage.
3. CSRF: mutating request without header → 403.
4. Upload file → blob or local path per config.
5. Application Insights → traces appear with `CorrelationId`, `GymId`.

---

## 9. Local Docker (pre-Azure smoke test)

```bash
docker compose up --build
curl http://localhost:5088/health
```

Angular dev server with proxy:
```bash
cd Frontend/gym-app && npm start
```

---

## Troubleshooting

| Issue | Fix |
|-------|-----|
| CORS error on login | Add exact frontend origin; ensure `AllowCredentials` (automatic with cookie auth) |
| 403 on API calls | Fetch CSRF token; ensure `X-XSRF-TOKEN` header sent |
| Cookies not set | HTTPS in prod; check SameSite + domain alignment |
| Health unhealthy | Verify SQL firewall + connection string |
| 401 after deploy | Users must re-login (refresh token hashing change) |

---

## Related documents

- `PRODUCTION_HARDENING_SPRINT2.md` — implementation details
- `BACKUP_DISASTER_RECOVERY.md` — backup and restore procedures
- `PRODUCTION_HARDENING_SPRINT1.md` — signed URLs, tenant isolation, rate limiting
