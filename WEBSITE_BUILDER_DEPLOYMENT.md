# Public Gym Website Builder — Deployment Guide

## Migration

Apply SQL script **`034_PublicGymWebsiteBuilderModule.sql`** (migration number 034; booking module already uses 033).

The migrator applies embedded scripts in order on application startup or via your existing migration CLI.

## Permissions

New privileges (seeded automatically):

- `VIEW_WEBSITE_BUILDER`
- `MANAGE_WEBSITE_BUILDER`
- `VIEW_WEBSITE_ANALYTICS`

Assigned to **GymAdmin** role via `DatabaseSeeder`.

## WhatsApp Templates

Migration seeds per-gym templates:

- `WebsiteLeadCreated`
- `TrialBooked`

Ensure WhatsApp notification settings are enabled for the gym.

## Public URLs

| Route | Description |
|-------|-------------|
| `/website/{gymSlug}` | Public home |
| `/website/{gymSlug}/contact` | Contact + trial booking |
| `/api/public/website/{gymSlug}` | Public API payload |
| `/api/public/website/lead` | Contact form (anonymous) |
| `/api/public/website/trial-booking` | Trial booking (anonymous) |

## Admin URLs

| Route | Permission |
|-------|------------|
| `/gym-admin/website-builder` | VIEW_WEBSITE_BUILDER |
| `/gym-admin/website-builder/pages` | MANAGE_WEBSITE_BUILDER |
| `/gym-admin/website-builder/gallery` | MANAGE_WEBSITE_BUILDER |
| `/gym-admin/website-builder/testimonials` | MANAGE_WEBSITE_BUILDER |
| `/gym-admin/website-builder/analytics` | VIEW_WEBSITE_ANALYTICS |

## Deployment Steps

1. Deploy backend with migration **034**.
2. Restart API to run seeder (privileges + WhatsApp templates).
3. Deploy Angular frontend with public `/website/:gymSlug` routes.
4. Gym admin: configure slug, branding, publish website.
5. Verify anonymous lead capture and CRM lead creation in `/gym-admin/leads`.

## Verification

```bash
dotnet test Gym.API.IntegrationTests --filter WebsiteBuilderTests
cd Frontend/gym-app && npm run build
```

## Notes

- Website slug must be unique across all gyms.
- Only **published** sites are visible publicly.
- Membership plans and trainers are read from existing modules (no duplicate data).
- Gallery images use existing `Files` table via `FileId`.
