# Settings

## Module Overview
Configuration surfaces for gym branding, SaaS subscription, booking rules, notification preferences, and white-label.

## Navigation Paths
| Setting area | Route |
|--------------|-------|
| Gym branding (logo/colors) | `/gym-admin/settings/branding` |
| SaaS subscription | `/gym-admin/subscription/*` |
| Booking rules | `/gym-admin/schedules/settings` |
| Notification settings | `/gym-admin/notifications` (settings tab/API) |
| White label | `/gym-admin/branding` or `/gym-admin/white-label` |
| White label preview | `/gym-admin/white-label/preview` |
| Mobile notification preferences | `/api/mobile/preferences` (member device) |

## Gym Branding (`GymBrandingComponent`)
- Upload logo, set primary colors
- API: gym branding endpoints / file upload

## SaaS Subscription (`/gym-admin/subscription`)
| Sub-page | Purpose |
|----------|---------|
| overview | Current plan status |
| catalog | Available plans |
| compare | Feature comparison |
| checkout | Upgrade/purchase (Razorpay) |
| my-features | Enabled feature codes |

## Booking Settings (`BookingSettingsComponent`)
- Max bookings per day, cancellation window, reminder minutes, waitlist toggle
- `GET/PUT /api/bookings/settings`

## Notification Settings
- Template management, channel settings via `NotificationsController`

## White Label (`WhiteLabelSettingsComponent`)
- Subdomain/custom domain, email templates, mobile app branding
- APIs under `/api/white-label`

## Tables
`BookingSettings`, `NotificationSettings`, `WhiteLabelSettings`, `WhiteLabelEmailTemplates`, `WhiteLabelMobileSettings`, `SaasSubscriptionPlans`, `PlanFeatures`

## Roles
**Gym Admin** with `MANAGE_GYM_BRANDING`, `MANAGE_SAAS_SUBSCRIPTION`, `MANAGE_SCHEDULES`, `MANAGE_WHITE_LABEL` as applicable

## SaaS Features
`CUSTOM_BRANDING`, `SUBSCRIPTIONS`, `BOOKINGS`, `WHITE_LABEL`

## Application Configuration (not UI)
`appsettings.json` — JWT, cookies, Razorpay, AI, Firebase, WhatsApp, connection strings (`appsettings.Development.json` for local secrets)
