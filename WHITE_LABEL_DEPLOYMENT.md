# White Label SaaS Module — Deployment Guide

## Overview

Migration `035_WhiteLabelSaasModule.sql` adds white label branding, domain configuration, email templates, mobile settings, and platform analytics.

## Prerequisites

- Gym Management SaaS platform running with migrations 001–034 applied
- SQL Server access
- Gym Admin users with `VIEW_WHITE_LABEL` and `MANAGE_WHITE_LABEL` privileges (seeded automatically)

## Database Deployment

1. Deploy the application or run migrations via startup (`Database:RunMigrationsOnStartup=true`), or execute manually:

```sql
-- From Backend/Gym.Infrastructure/Persistence/Scripts/035_WhiteLabelSaasModule.sql
```

2. Verify tables exist:

```sql
SELECT * FROM dbo.WhiteLabelSettings;
SELECT * FROM dbo.WhiteLabelEmailTemplates;
SELECT * FROM dbo.WhiteLabelMobileSettings;
```

3. Confirm privileges were seeded:

```sql
SELECT * FROM dbo.Privileges WHERE PrivilegeName IN ('VIEW_WHITE_LABEL', 'MANAGE_WHITE_LABEL');
```

## API Endpoints

| Endpoint | Auth | Description |
|----------|------|-------------|
| `GET/PUT /api/white-label/settings` | Gym Admin | Brand settings CRUD |
| `PUT /api/white-label/domain` | Gym Admin | Subdomain / custom domain |
| `GET/POST/PUT/DELETE /api/white-label/email-templates` | Gym Admin | Branded email templates |
| `GET/PUT /api/white-label/mobile-settings` | Gym Admin | Future mobile branding |
| `GET /api/white-label/preview` | Gym Admin | Login / website / mobile preview |
| `GET /api/public/white-label/login-branding` | Anonymous | Login page branding |
| `GET /api/platform/white-label/dashboard` | Super Admin | Platform analytics |

## Frontend Routes

- `/gym-admin/branding` — Branding entry (alias)
- `/gym-admin/white-label` — Full white label configuration
- `/gym-admin/white-label/preview` — Preview before publish
- `/super-admin/white-label` — Platform dashboard with Chart.js

## Subdomain & Custom Domain (Future)

Subdomains are stored normalized (lowercase). Examples:

- `fitzone.gymsaas.com`
- `powerhouse.gymsaas.com`

Custom domains (e.g. `www.fitzonegym.com`) are stored for future DNS/reverse-proxy routing. Point DNS CNAME to your platform ingress when ready.

## Login Branding

The login page loads branding from:

1. Query params: `?subDomain=fitzone` or `?customDomain=www.example.com`
2. Hostname pattern: `{subdomain}.gymsaas.com`

## Website Builder Integration

Published public websites inherit white label logo, colors, and brand name when website-specific values are not set.

## Notifications

WhatsApp/event notifications automatically receive `brandName` and related variables when white label is enabled for the gym.

## Verification Checklist

- [ ] Migration 035 applied successfully
- [ ] Gym Admin can save white label settings
- [ ] Subdomain uniqueness enforced (SQL THROW 50001)
- [ ] Login page shows gym branding when enabled
- [ ] Public website inherits branding
- [ ] Super Admin dashboard loads at `/super-admin/white-label`
- [ ] Integration tests pass: `dotnet test --filter WhiteLabelTests`

## Rollback

Drop objects in reverse order if required (only when no production data):

```sql
DROP PROCEDURE IF EXISTS dbo.sp_GetWhiteLabelPlatformDashboard;
-- ... other procedures ...
DROP TABLE IF EXISTS dbo.WhiteLabelMobileSettings;
DROP TABLE IF EXISTS dbo.WhiteLabelEmailTemplates;
DROP TABLE IF EXISTS dbo.WhiteLabelSettings;
```
