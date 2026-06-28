# Deployment Guide — Gym Management SaaS v1.0.0-RC1

This guide covers deploying the Gym Management SaaS platform to production: backend API, Angular frontend, SQL Server database, and supporting services.

**Repositories:**
- **API:** `GymManagementSystem` (this repo) — `Backend/Gym.API`
- **UI:** `GymManagementSystem-UI` — Angular SPA

---

## Architecture Overview

```
[Browser / Mobile Web]
        │
        ▼ HTTPS
[CDN / Static Web App / Nginx]  ← Angular build (GymManagementSystem-UI)
        │
        ▼ HTTPS  /api/*
[Load Balancer / Reverse Proxy]
        │
        ▼
[ASP.NET Core 8 API]  ← Gym.API (Docker or App Service)
        │
        ├──► [SQL Server]          (Azure SQL or self-hosted)
        ├──► [Blob Storage]        (Azure Blob or local disk)
        ├──► [Application Insights]
        └──► [External APIs]       (Razorpay, WhatsApp, Firebase, OpenAI — optional)
```

---

## Prerequisites

| Requirement | Version |
|-------------|---------|
| .NET SDK | 8.0+ |
| Node.js | 20 LTS+ |
| SQL Server | 2019+ or Azure SQL |
| SSL certificate | Required for production (TLS 1.2+) |
| Reverse proxy | Nginx, IIS, Azure Front Door, or App Service built-in |

---

## 1. SQL Server Deployment

### 1.1 Create database

```sql
CREATE DATABASE GymDb;
-- Use General Purpose tier (Azure SQL) for PITR and production workloads
```

### 1.2 Run migrations (recommended: CI/CD, not app startup)

```powershell
cd Backend

# Set connection string and JWT secret (required for migrator host)
$env:ConnectionStrings__DefaultConnection = "Server=...;Database=GymDb;..."
$env:Jwt__Secret = "your-production-secret-minimum-32-characters"

dotnet run --project Gym.API -- migrate
```

This executes:
1. EF Core migrations (`ApplicationDbContext`)
2. Embedded SQL scripts `001` through `052` in order
3. Critical schema validation

### 1.3 Verify migration

```sql
SELECT * FROM dbo.SchemaVersions ORDER BY AppliedAt;
-- Expect 52 applied script rows (001–052) after RC1
```

### 1.4 Bootstrap Super Admin (first deploy only)

Set environment variables before first API start with seed enabled, **once**:

```json
{
  "Bootstrap": {
    "SuperAdminEmail": "admin@yourcompany.com",
    "SuperAdminPassword": "CHANGE_ME_STRONG_PASSWORD"
  },
  "Database": {
    "RunSeedOnStartup": true
  }
}
```

After first successful start, set `RunSeedOnStartup=false` permanently.

> **Production:** Never enable `Demo__Enabled` or demo seed passwords.

---

## 2. Backend (API) Deployment

### 2.1 Build

```powershell
cd Backend
dotnet publish Gym.API/Gym.API.csproj -c Release -o ./publish
```

### 2.2 Docker (recommended)

```powershell
docker build -t gym-api:1.0.0-rc1 .
docker run -d -p 443:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="..." \
  -e Jwt__Secret="..." \
  -e Database__RunMigrationsOnStartup=false \
  -e Database__RunSeedOnStartup=false \
  -e Demo__Enabled=false \
  -e Cors__AllowedOrigins__0="https://app.yourdomain.com" \
  -e AuthCookies__UseCookieAuth=true \
  -v gym-uploads:/app/uploads \
  gym-api:1.0.0-rc1
```

Or use `docker compose up` for local/staging (see root `docker-compose.yml`).

### 2.3 Environment variables

| Variable | Required | Description |
|----------|----------|-------------|
| `ConnectionStrings__DefaultConnection` | Yes | SQL Server connection string |
| `Jwt__Secret` | Yes | Min 32 characters; store in Key Vault |
| `Jwt__Issuer` | Yes | Token issuer (e.g. `GymManagementSystem`) |
| `Jwt__Audience` | Yes | Token audience |
| `ASPNETCORE_ENVIRONMENT` | Yes | `Production` |
| `Cors__AllowedOrigins__0` | Yes | Frontend origin (HTTPS) |
| `AuthCookies__UseCookieAuth` | Yes | `true` for browser SPA |
| `Database__RunMigrationsOnStartup` | Yes | **`false`** in production |
| `Database__RunSeedOnStartup` | Yes | **`false`** after bootstrap |
| `Demo__Enabled` | Yes | **`false`** in production |
| `PasswordReset__FrontendBaseUrl` | Yes | `https://app.yourdomain.com` |
| `FileStorage__Provider` | Yes | `Azure` or `Local` |
| `FileStorage__AzureConnectionString` | If Azure | Blob storage connection |
| `FileStorage__AzureContainerName` | If Azure | e.g. `gym-files` |
| `FileStorage__UrlSigningSecret` | If Azure | 32+ char signing secret |
| `ApplicationInsights__Enabled` | Recommended | `true` |
| `ApplicationInsights__ConnectionString` | Recommended | App Insights connection |
| `Razorpay__Enabled` | Optional | Online payments |
| `Razorpay__KeyId` / `KeySecret` | Optional | Razorpay credentials |
| `WhatsApp__Enabled` | Optional | WhatsApp notifications |
| `WhatsApp__ApiKey` | Optional | Interakt or provider key |
| `Firebase__Enabled` | Optional | Push notifications |
| `AI__Enabled` | Optional | AI recommendations |
| `AI__ApiKey` | Optional | OpenAI API key |

Copy `Backend/Gym.API/appsettings.Example.json` as reference. **Never commit secrets.**

### 2.4 SSL / HTTPS requirements

- Terminate TLS at load balancer or reverse proxy.
- API must be served over **HTTPS** in production (HSTS enabled when `IsProduction()`).
- Cookie auth requires `Secure` flag — automatic in Production environment.
- Configure **forwarded headers** (already in `Program.cs`) when behind proxy:
  - `X-Forwarded-For`, `X-Forwarded-Proto`, `X-Forwarded-Host`
- Minimum TLS 1.2 on all endpoints.

### 2.5 Health check

```http
GET https://api.yourdomain.com/health
GET https://api.yourdomain.com/api/health
```

Expect HTTP 200 with `"status": "Healthy"`.

---

## 3. Angular Frontend Deployment

### 3.1 Build for production

```powershell
cd GymManagementSystem-UI
npm ci
ng build --configuration production
```

Output: `dist/gym-app/browser/` (static files).

### 3.2 Configure API URL

Production builds must proxy API calls to the backend. Options:

**Option A — Same-origin reverse proxy (recommended)**

Nginx/App Service routes `/api/*` to backend; Angular uses relative `/api` paths (default with dev proxy pattern).

**Option B — Environment file**

Set `apiUrl` in `src/environments/environment.prod.ts` to `https://api.yourdomain.com`.

### 3.3 Deploy static files

| Platform | Steps |
|----------|-------|
| **Azure Static Web Apps** | Deploy `dist/gym-app/browser`; configure API proxy in `staticwebapp.config.json` |
| **Nginx** | Serve static files; proxy `/api` to backend upstream |
| **Azure App Service** | Separate Web App for UI or combined with API gateway |
| **S3 + CloudFront** | Upload build; configure CloudFront behavior for `/api/*` |

Example Nginx snippet:

```nginx
server {
    listen 443 ssl http2;
    server_name app.yourdomain.com;

    ssl_certificate     /etc/ssl/certs/app.crt;
    ssl_certificate_key /etc/ssl/private/app.key;

    root /var/www/gym-app;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    location /api/ {
        proxy_pass https://api.yourdomain.com;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cookie_path / /;
    }
}
```

### 3.4 CORS and cookies

- Add frontend origin to API `Cors__AllowedOrigins`.
- Cookies are set on API domain; same-site deployment (single domain with `/api` proxy) avoids cross-site cookie issues.
- If API and UI are on different domains, configure `SameSite=None; Secure` (review cookie settings for your topology).

---

## 4. Post-Deployment Verification

```powershell
# API smoke
curl https://api.yourdomain.com/health

# LoginIdentifier login (replace credentials)
curl -X POST https://api.yourdomain.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"loginIdentifier":"superadmin","password":"..."}'

# Run QA runner against staging/production (with care)
cd Backend/scripts
./e2e-qa-runner.ps1 -BaseUrl https://api.yourdomain.com
```

---

## 5. Backup Procedures

See [BACKUP_DISASTER_RECOVERY.md](./BACKUP_DISASTER_RECOVERY.md) for full detail.

| Asset | Method | RPO | RTO |
|-------|--------|-----|-----|
| SQL Database | Azure automated backups / PITR | 1 hour | 4 hours |
| File uploads | Azure Blob GRS + soft delete | 24 hours | 8 hours |
| Configuration | Key Vault / IaC in git | N/A | 1 hour |

**Monthly:** Restore SQL to staging and run health + login smoke test.  
**Daily:** Verify backup job success alerts.

---

## 6. Rollback Procedures

### 6.1 Application rollback

```bash
# Docker
docker pull gym-api:1.0.0-rc1-previous
docker stop gym-api && docker run ... gym-api:1.0.0-rc1-previous

# App Service
az webapp deployment slot swap --name gym-api-prod --resource-group rg-gym-prod \
  --slot staging --target-slot production
```

Deploy previous Angular build artifact from CI retention.

### 6.2 Database rollback

SQL scripts are **forward-only**. Rollback strategy:

1. **Preferred:** Restore database to pre-deployment PITR snapshot (new DB, swap connection string).
2. **Emergency:** Deploy previous API version compatible with current schema (RC1 schema is backward-compatible with RC1 API only).
3. **Never** run down-migration scripts in production without DBA review.

```bash
az sql db restore \
  --dest-name GymDb-pre-deploy \
  --resource-group rg-gym-prod \
  --server gym-sql-prod \
  --source-database GymDb \
  --time "2026-06-20T02:00:00Z"
```

### 6.3 Rollback checklist

- [ ] Stop traffic to new version (swap slot / update load balancer)
- [ ] Restore DB if schema/data migration caused issues
- [ ] Deploy previous API + UI artifacts
- [ ] Verify `/health`, login, tenant-scoped query
- [ ] Notify stakeholders; document incident

---

## 7. Staging vs Production Settings

| Setting | Staging | Production |
|---------|---------|------------|
| `ASPNETCORE_ENVIRONMENT` | Staging | Production |
| `Demo__Enabled` | true (optional) | **false** |
| `Database__RunMigrationsOnStartup` | false | **false** |
| `ReturnResetTokenInDevelopment` | false | **false** |
| `ApplicationInsights__Enabled` | true | true |
| SSL | Required | Required |
| Swagger | Disabled | Disabled |

---

## Related Documents

- [AZURE_DEPLOYMENT_GUIDE.md](./AZURE_DEPLOYMENT_GUIDE.md) — Azure-specific steps
- [BACKUP_DISASTER_RECOVERY.md](./BACKUP_DISASTER_RECOVERY.md) — DR runbook
- [PRODUCTION_CHECKLIST.md](./PRODUCTION_CHECKLIST.md) — Pre-launch checklist
