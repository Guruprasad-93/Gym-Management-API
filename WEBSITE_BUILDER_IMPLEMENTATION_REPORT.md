# Public Gym Website Builder — Implementation Report

## Overview

Full Public Gym Website Builder module integrated into the Gym Management SaaS platform following existing Dapper + stored procedure, multi-tenant, permission, CRM, and notification patterns.

**Migration file:** `034_PublicGymWebsiteBuilderModule.sql`  
(Requested filename was `033_`; slot 033 is used by Booking & Slot Reservation.)

## Database

### Tables

| Table | Purpose |
|-------|---------|
| `GymWebsiteSettings` | Branding, SEO, contact, publish state, unique `WebsiteSlug` |
| `GymWebsitePages` | Custom CMS pages |
| `GymWebsiteSections` | Hero, About, CTA, etc. |
| `GymWebsiteTestimonials` | Member reviews |
| `GymWebsiteGallery` | Gallery linked to `Files` |
| `WebsiteLeadCaptures` | Website-specific lead tracking linked to CRM |

### Stored Procedures

Settings, pages, sections, testimonials, gallery CRUD; lead capture/search/convert; public site by slug; analytics overview; notification recipients; WhatsApp template seeding.

## Backend

| Layer | Files |
|-------|-------|
| DTOs | `Gym.Application/DTOs/Website/WebsiteDtos.cs` |
| Constants | `Gym.Application/Constants/WebsiteConstants.cs` |
| Repository | `Gym.Infrastructure/Repositories/WebsiteRepository.cs` |
| Service | `Gym.Application/Services/WebsiteService.cs` |
| Exporter | `Gym.Infrastructure/Services/WebsiteReportExporter.cs` |
| Controllers | `Gym.API/Controllers/WebsiteController.cs` |
| SP names | `StoredProcedureNames.cs` (Website block) |

## Features

- Gym admin branding, pages, sections, gallery, testimonials
- Publish / unpublish with audit logging
- Public site with membership plans + trainers from existing modules
- Contact form → CRM lead + `WebsiteLeadCapture` + WhatsApp (`WebsiteLeadCreated`)
- Free trial booking → CRM lead (`TrialScheduled`) + trial schedule + WhatsApp (`TrialBooked`)
- SEO meta fields, sitemap, robots.txt endpoints
- Analytics dashboard with Chart.js (daily leads, conversion KPIs)
- PDF/Excel lead export
- Tenant isolation via `GymId` on all tables

## Permissions

| Permission | GymAdmin |
|------------|----------|
| VIEW_WEBSITE_BUILDER | ✓ |
| MANAGE_WEBSITE_BUILDER | ✓ |
| VIEW_WEBSITE_ANALYTICS | ✓ |

## Frontend (Angular Standalone)

| Route | Component |
|-------|-----------|
| `/gym-admin/website-builder` | Settings + publish |
| `/gym-admin/website-builder/pages` | Page management |
| `/gym-admin/website-builder/gallery` | Gallery |
| `/gym-admin/website-builder/testimonials` | Testimonials |
| `/gym-admin/website-builder/analytics` | Chart.js analytics |
| `/website/:gymSlug/*` | Public Ahana-style site |

## Integration Tests

`WebsiteBuilderTests.cs` — 10 tests covering settings CRUD, publish, public access, lead capture, trial booking, analytics, auth, sitemap.

## Audit Events

- Website published / unpublished
- Page created / updated
- Lead captured (website + CRM)

## CRM Integration

Uses `ILeadRepository` directly for public endpoints (no auth required) with `LeadSource = Website`, activity logging, and trial scheduling via existing lead SPs.
