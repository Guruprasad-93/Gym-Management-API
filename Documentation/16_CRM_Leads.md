# CRM / Leads

## Module Overview
Lead pipeline: capture, follow-ups, trials, conversion to member, analytics.

## Navigation
| Page | Route |
|------|-------|
| Lead list | `/gym-admin/leads` |
| Create / edit | `/gym-admin/leads/create`, `/leads/edit/:id` |
| Detail | `/gym-admin/leads/:id` |
| Follow-ups | `/gym-admin/leads/followups` |
| Trials | `/gym-admin/leads/trials` |
| Analytics | `/gym-admin/leads/analytics` |

## Buttons
- Create lead, update status, assign trainer
- Schedule trial, record feedback
- Add follow-up
- Convert to member
- Export report

## Tables
`Leads`, `LeadFollowUps`, `LeadTrials`, `LeadActivities`

## APIs
`LeadsController` — `/api/leads/*`

## Components
`lead-list`, `lead-form`, `lead-detail`, `lead-followups`, `lead-trials`, `lead-analytics`

## Roles
**Gym Admin**; trainer route `/trainer/leads` exists (shared list) but not in trainer menu

## SaaS Feature
`CRM`
