# Project Statistics

*Generated from codebase analysis. Counts reflect files and declarations in the repository.*

## Frontend (Angular)

| Metric | Count |
|--------|------:|
| Route definition files | 7 |
| Routed pages (`loadComponent` routes) | ~123 |
| Total `.component.ts` files | 149 |
| Standalone components | 156 |
| Feature areas (`src/app/features`) | 6 (auth, gym-admin, member, trainer, super-admin, public-website) |
| Menu items (Super Admin) | 9 |
| Menu items (Gym Admin) | 38 |
| Menu items (Trainer) | 7 |
| Menu items (Member) | 13 |
| Frontend permission constants | 107 |

## Backend (ASP.NET Core)

| Metric | Count |
|--------|------:|
| Controller `.cs` files | 36 |
| Controller classes | 53–55 |
| HTTP endpoint actions (approx.) | 372 |
| Application services | 40+ |
| FluentValidation validator classes | 20+ |
| Integration test classes | 15+ |

## Database (SQL Server)

| Metric | Count |
|--------|------:|
| SQL migration scripts | 76 |
| Unique table names (in scripts) | 110 |
| `CREATE OR ALTER PROCEDURE` declarations | 619 |
| Unique stored procedure names (approx.) | 497 |
| Views | 0 |
| Triggers | 0 |
| Functions | 2 (`fn_Saas_CalculatePeriodEnd`, `fn_CalculateSubscriptionPeriodEnd`) |

## Security & Access

| Metric | Count |
|--------|------:|
| Roles | 4 (SuperAdmin, GymAdmin, Trainer, Member) |
| Backend permissions | 98 |
| SaaS system features | 20+ (see subscription/plan modules) |

## Modules Documented

| Category | Count |
|----------|------:|
| Fully implemented module docs | 22 |
| Partially implemented areas called out | 5 (AI provider, push, WhatsApp, TrainerAvailability API, legacy schema tables) |
| Intentionally not documented | Features with no implementation |
