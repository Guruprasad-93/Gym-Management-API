# Login & Authentication

## Module Overview
Cookie-based session authentication with JWT tokens, CSRF protection, refresh tokens, and role-based access. Supports Super Admin, Gym Admin, Trainer, and Member portals.

## Purpose
Secure login, session management, password lifecycle, and permission/feature loading for the Angular SPA.

## Navigation Paths
| Page | Route |
|------|-------|
| Login | `/auth/login` |
| Register gym | `/register` |
| Forgot password | `/auth/forgot-password` |
| Reset password | `/auth/reset-password` |
| Change password | `/auth/change-password` |

## Screen Description

### Login (`LoginComponent`)
- Login identifier (username/email) and password fields
- Link to forgot password and register

### Buttons & Actions
| Button | Action |
|--------|--------|
| Sign in | `POST /api/auth/login` after `GET /api/auth/csrf` |
| Forgot password link | Navigate to forgot-password |
| Register link | Navigate to `/register` |

### Validation
- Required login identifier and password (client-side)
- Server validates credentials; generic error on failure

### Business Rules
- HTTP-only cookies: `gym_access_token`, refresh token on `/api/auth` path
- CSRF cookie `XSRF-TOKEN` required for mutating requests
- Session exposes roles, permissions, `enabledFeatureCodes`, `gymId`
- Super Admin has no `gymId`; tenant users scoped by gym

### Workflow
1. App fetches CSRF token
2. User submits login → cookies set
3. Redirect by role (`getDefaultRouteForUser`)
4. On 401, interceptor attempts refresh; logout on failure

### Database Tables
`Users`, `RefreshTokens`, `Roles`, `Privileges`, `RolePrivileges`, `UserRoles`, `UserLoginSessions`

### Stored Procedures
`sp_LoginUser`, `sp_User_*`, `sp_RefreshToken_*`, password reset SPs

### APIs
| Method | Endpoint | Auth |
|--------|----------|------|
| GET | `/api/auth/csrf` | Anonymous |
| POST | `/api/auth/login` | Anonymous |
| POST | `/api/auth/logout` | Authorized |
| POST | `/api/auth/refresh` | Cookie refresh |
| GET | `/api/auth/validate` | Authorized |
| GET | `/api/auth/session` | Authorized |
| POST | `/api/auth/change-password` | Authorized |
| POST | `/api/auth/forgot-password` | Anonymous |
| POST | `/api/auth/reset-password` | Anonymous |
| POST | `/api/auth/register` | Anonymous (onboarding) |

### Angular Components
`login.component.ts`, `forgot-password-page`, `reset-password-page`, `change-password-page`, `register.component`

### Roles
All roles use login; registration creates Gym Admin + gym

### Messages
- Success: redirect after login
- Error: invalid credentials, CSRF validation failed, session expired toast

### Dependencies
`AuthService`, `csrfInterceptor`, `credentialsInterceptor`, `authInterceptor`

### Limitations
JWT header auth supported as legacy; production uses cookies (`useCookieAuth: true`)
