# Backup & Disaster Recovery — Gym Management SaaS

**Last updated:** Sprint 2  
**Owner:** Platform / DevOps

---

## Objectives

| Metric | Target | Notes |
|--------|--------|-------|
| **RPO** (Recovery Point Objective) | **1 hour** (SQL) / **24 hours** (files) | SQL via PITR; blobs via daily replication + soft delete |
| **RTO** (Recovery Time Objective) | **4 hours** (database) / **8 hours** (full platform) | Includes verification and DNS cutover |

---

## 1. Azure SQL Database

### Backup strategy

Azure SQL provides **automated backups** without additional configuration:

| Backup type | Retention (default) | Use case |
|-------------|---------------------|----------|
| Full | Weekly | Base restore |
| Differential | Every 12–24 hours | Faster restore |
| Transaction log | Every 5–10 min | Point-in-time restore (PITR) |

**Production recommendations:**
- Use **General Purpose** or **Business Critical** tier (not Basic) for reliable PITR.
- Set backup redundancy to **Geo-Redundant (GRS/RA-GRS)** for regional disaster recovery.
- Retention: **30 days** minimum (configure via `backup-storage-redundancy` and retention policies).

### Restore procedures

#### Point-in-time restore (PITR)

1. Azure Portal → SQL Database → **Restore**.
2. Select timestamp (within retention window).
3. Restore to **new database** (e.g. `GymDb-restored-YYYYMMDD`).
4. Validate data (row counts, latest `SchemaVersions`, sample tenant).
5. Update App Service connection string or swap database name.
6. Run smoke tests (`/health`, login, tenant-scoped query).

```bash
az sql db restore \
  --dest-name GymDb-restored \
  --edition GeneralPurpose \
  --service-objective GP_Gen5_2 \
  --resource-group rg-gym-prod \
  --server gym-sql-prod \
  --source-database GymDb \
  --time "2026-05-29T10:00:00Z"
```

#### Geo-restore (regional outage)

1. Fail over to geo-replicated secondary (if configured) or restore from geo-redundant backup.
2. Update DNS / connection strings to new region.
3. Re-deploy App Service in target region if needed.

### Verification

- **Monthly:** restore to staging database and run `/health` + login test.
- **Quarterly:** full DR drill with documented timeline vs RTO.

---

## 2. Azure Blob Storage (file attachments)

### Backup strategy

| Control | Setting | Purpose |
|---------|---------|---------|
| Soft delete (blobs) | 14–30 days | Recover accidental deletes |
| Soft delete (containers) | 7 days | Container recovery |
| Versioning | Optional | Overwrite protection |
| Replication | **GRS** or **GZRS** | Cross-region durability |
| Immutable storage | Optional | Compliance / ransomware hardening |

Container: `gym-files` (private). Files stored as `{gymId}/{category}/{guid}.ext`.

### Restore procedures

#### Single blob restore

1. Portal → Storage Account → Blob container → **Show deleted blobs**.
2. Select blob → **Undelete**.
3. If versioned, restore previous version.

#### Container-level disaster

1. Restore from geo-redundant copy (Microsoft support / secondary region read access).
2. Or restore from **AzCopy** backup if periodic export is configured:

```bash
azcopy copy "https://source.blob.core.windows.net/gym-files" "https://backup.blob.core.windows.net/gym-files-backup" --recursive
```

3. Reconcile `dbo.Files` table — ensure `StoragePath` values match restored blob names.

### RPO note

Without continuous blob backup to a secondary account, RPO for files is **up to 24 hours** if relying on GRS replication lag. For stricter RPO, schedule **AzCopy** every 1–4 hours to a backup storage account.

---

## 3. Application configuration & secrets

| Asset | Backup method |
|-------|---------------|
| App Service settings | Export ARM/Bicep template; document in Key Vault |
| Key Vault secrets | Soft delete + purge protection enabled |
| `SchemaVersions` | Part of SQL backup |

**Restore:** redeploy infrastructure from IaC; re-bind Key Vault references.

---

## 4. Disaster scenarios & runbooks

### Scenario A — Database corruption / bad migration

1. Stop API (prevent further writes).
2. PITR restore to last known good time (before migration).
3. Fix migration script; re-run `dotnet run -- migrate` on staging first.
4. Resume API.

### Scenario B — Accidental tenant data delete

1. Identify delete time from audit logs (`AuditLogs` table).
2. PITR restore to isolated DB; export affected rows.
3. Merge into production with scoped SQL scripts (Super Admin review).

### Scenario C — Region failure

1. Activate geo-restored SQL + storage in secondary region.
2. Deploy API from container image (ACR geo-replicated).
3. Update Front Door / DNS to secondary region.
4. Communicate downtime to customers.

### Scenario D — Security incident (token compromise)

1. Rotate `Jwt__Secret` and `FileStorage__UrlSigningSecret`.
2. Increment all user token versions or force global password reset.
3. Clear refresh tokens: `UPDATE RefreshTokens SET RevokedAt = SYSUTCDATETIME() WHERE RevokedAt IS NULL`.
4. Review audit logs; redeploy with new secrets.

---

## 5. RPO / RTO summary

| Component | RPO | RTO | Method |
|-----------|-----|-----|--------|
| Azure SQL (transactional data) | **≤ 1 hour** | **4 hours** | PITR + geo-redundant backup |
| Blob files | **24 hours** (GRS) / **1–4 hours** (with AzCopy) | **8 hours** | Soft delete + geo-restore / AzCopy |
| App Service config | **0** (IaC) | **2 hours** | Redeploy from pipeline |
| Full platform | **1 hour** (data) | **8 hours** | Combined runbook |

---

## 6. Responsibilities

| Role | Responsibility |
|------|----------------|
| DevOps | Backup retention, restore drills, Key Vault |
| DBA / Backend | Migration rollback, SQL PITR validation |
| Support | Customer communication during outage |
| Security | Incident response, secret rotation |

---

## 7. Checklist (go-live)

- [ ] Azure SQL backup retention ≥ 30 days, geo-redundant
- [ ] Blob soft delete enabled
- [ ] Storage replication GRS/GZRS
- [ ] Key Vault purge protection on
- [ ] `Database__RunMigrationsOnStartup=false` in Production
- [ ] Monthly restore test scheduled
- [ ] DR contacts and escalation documented

---

## Related documents

- `AZURE_DEPLOYMENT_GUIDE.md`
- `PRODUCTION_HARDENING_SPRINT2.md`
