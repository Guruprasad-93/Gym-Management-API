# Website Builder

## Module Overview
Gym marketing website CMS: settings, pages, sections, gallery, testimonials, publish, public site, lead capture, analytics.

## Navigation
| Page | Route |
|------|-------|
| Builder home | `/gym-admin/website-builder` |
| Pages | `/gym-admin/website-builder/pages` |
| Gallery | `/gym-admin/website-builder/gallery` |
| Testimonials | `/gym-admin/website-builder/testimonials` |
| Analytics | `/gym-admin/website-builder/analytics` |
| Public site | `/website/:gymSlug/*` |

## Buttons
- Save settings, publish/unpublish
- CRUD pages, sections, gallery items, testimonials
- Preview link
- Public contact form submit → lead capture

## Tables
`GymWebsiteSettings`, `GymWebsitePages`, `GymWebsiteSections`, `GymWebsiteGallery`, `GymWebsiteTestimonials`, `WebsiteLeadCaptures`

## APIs
`WebsiteController` — `/api/website/*`, `PublicWebsiteController` — `/api/public/website/*`

## Components
`website-builder`, `website-pages`, `website-gallery`, `website-testimonials`, `website-analytics`, public-website shell and page components

## Roles
**Gym Admin** — builder permissions; public anonymous read

## SaaS Feature
`WEBSITE_BUILDER`
