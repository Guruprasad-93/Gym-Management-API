# Gym Management System - Manual Test Cases

**Generated:** 2026-06-07
**Total test cases:** 239

---

## 1. How to Use This Document

| Column | Purpose |
|--------|---------|
| **TC_ID** | Unique test case identifier |
| **Module** | AUTH, REG, PUB, SA, GA, TR, MB, XCUT |
| **Page / Route** | UI page and URL path |
| **Event** | User action being tested |
| **Description** | What this test validates |
| **Preconditions** | Required setup before testing |
| **Steps** | Actions to perform |
| **Expected Result** | Pass criteria |
| **Priority** | High / Medium / Low |
| **Status** | Pass / Fail / Blocked / Not Run |

**Tracking:** Use `MANUAL_TEST_CASES.csv` in Excel/Google Sheets to mark Status, Tester, Test_Date, and Notes.

---

## 2. Test Environment

| Item | Value |
|------|-------|
| Frontend URL | http://localhost:4200 |
| Backend API | http://localhost:5088 |
| Browser | Chrome / Edge (latest) |
| Database | GymDb (local SQL Server) |

### Demo Credentials

| Role | Email | Password |
|------|-------|----------|
| Super Admin | superadmin@gym.com | SuperAdmin@123 |
| Gym Admin | admin@fitzone-demo.com | Demo@123 |
| Trainer | (demo trainer account) | Demo@123 |
| Member | (demo member account) | Demo@123 |

---

## 3. Module Summary

| Module | Description | Test Cases |
|--------|-------------|------------|
| AUTH | Login, forgot/reset/change password | 12 |
| REG | Gym owner registration | 4 |
| PUB | Public gym website | 8 |
| SA | Super Admin portal | 29 |
| GA | Gym Admin portal | 143 |
| TR | Trainer portal | 14 |
| MB | Member portal | 20 |
| XCUT | Cross-cutting (auth, layout, guards) | 9 |

---

## 4. Test Cases by Module

### Module: AUTH

#### TC-AUTH-001 - Login - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/auth/login` |
| **Description** | Verify login page renders |
| **Preconditions** | App running |
| **Steps** | Open /auth/login |
| **Expected Result** | Email, password fields and Sign In visible |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-AUTH-002 - Login - Valid Login - Super Admin

| Field | Value |
|-------|-------|
| **Route** | `/auth/login` |
| **Description** | Super admin can sign in |
| **Preconditions** | Valid super admin account |
| **Steps** | Enter superadmin@gym.com / SuperAdmin@123; click Sign In |
| **Expected Result** | Redirect to /super-admin; sidebar visible |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-AUTH-003 - Login - Valid Login - Gym Admin

| Field | Value |
|-------|-------|
| **Route** | `/auth/login` |
| **Description** | Gym admin can sign in |
| **Preconditions** | Valid gym admin account |
| **Steps** | Enter admin@fitzone-demo.com / Demo@123; Sign In |
| **Expected Result** | Redirect to /gym-admin/dashboard |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-AUTH-004 - Login - Invalid Credentials

| Field | Value |
|-------|-------|
| **Route** | `/auth/login` |
| **Description** | Wrong password rejected |
| **Preconditions** | None |
| **Steps** | Enter valid email + wrong password; Sign In |
| **Expected Result** | Error message; stay on login |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-AUTH-005 - Login - Empty Form Validation

| Field | Value |
|-------|-------|
| **Route** | `/auth/login` |
| **Description** | Required fields enforced |
| **Preconditions** | None |
| **Steps** | Click Sign In with empty fields |
| **Expected Result** | Validation errors shown |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-AUTH-006 - Login - Show/Hide Password

| Field | Value |
|-------|-------|
| **Route** | `/auth/login` |
| **Description** | Password visibility toggle |
| **Preconditions** | On login page |
| **Steps** | Click eye icon on password field |
| **Expected Result** | Password toggles masked/visible |
| **Priority** | Low |
| **Status** | [ ] Not Run |
#### TC-AUTH-007 - Login - Navigate Forgot Password

| Field | Value |
|-------|-------|
| **Route** | `/auth/login` |
| **Description** | Link to forgot password |
| **Preconditions** | On login page |
| **Steps** | Click Forgot Password link |
| **Expected Result** | Navigate to /auth/forgot-password |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-AUTH-008 - Forgot Password - Submit Email

| Field | Value |
|-------|-------|
| **Route** | `/auth/forgot-password` |
| **Description** | Request password reset |
| **Preconditions** | On forgot password page |
| **Steps** | Enter registered email; Send Reset Instructions |
| **Expected Result** | Success message; email flow triggered |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-AUTH-009 - Forgot Password - Back to Login

| Field | Value |
|-------|-------|
| **Route** | `/auth/forgot-password` |
| **Description** | Return to login |
| **Preconditions** | On forgot password page |
| **Steps** | Click Back to login |
| **Expected Result** | Navigate to /auth/login |
| **Priority** | Low |
| **Status** | [ ] Not Run |
#### TC-AUTH-010 - Reset Password - Reset with Token

| Field | Value |
|-------|-------|
| **Route** | `/auth/reset-password` |
| **Description** | Reset password using token |
| **Preconditions** | Valid reset token in URL/email |
| **Steps** | Enter email, token, new password, confirm; Reset Password |
| **Expected Result** | Success; redirect to login |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-AUTH-011 - Reset Password - Password Mismatch

| Field | Value |
|-------|-------|
| **Route** | `/auth/reset-password` |
| **Description** | Confirm password validation |
| **Preconditions** | On reset page |
| **Steps** | Enter mismatched passwords; submit |
| **Expected Result** | Validation error shown |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-AUTH-012 - Change Password - Forced Change

| Field | Value |
|-------|-------|
| **Route** | `/auth/change-password` |
| **Description** | Logged-in user changes password |
| **Preconditions** | User flagged must-change-password |
| **Steps** | Enter current + new + confirm; Update Password |
| **Expected Result** | Password updated; access granted |
| **Priority** | High |
| **Status** | [ ] Not Run |

### Module: REG

#### TC-REG-013 - Register - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/register` |
| **Description** | Registration page renders |
| **Preconditions** | None |
| **Steps** | Open /register |
| **Expected Result** | Gym signup form visible |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-REG-014 - Register - Valid Registration

| Field | Value |
|-------|-------|
| **Route** | `/register` |
| **Description** | New gym owner signup |
| **Preconditions** | Unique email/mobile |
| **Steps** | Fill gym name, owner, mobile, email, address, password; Start free trial |
| **Expected Result** | Account created; redirect/login success |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-REG-015 - Register - Duplicate Email

| Field | Value |
|-------|-------|
| **Route** | `/register` |
| **Description** | Duplicate email rejected |
| **Preconditions** | Email already exists |
| **Steps** | Register with existing email |
| **Expected Result** | Error message displayed |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-REG-016 - Register - Back to Login

| Field | Value |
|-------|-------|
| **Route** | `/register` |
| **Description** | Navigate to login |
| **Preconditions** | On register page |
| **Steps** | Click Back to Login |
| **Expected Result** | Navigate to /auth/login |
| **Priority** | Low |
| **Status** | [ ] Not Run |

### Module: PUB

#### TC-PUB-017 - Home - Nav links work

| Field | Value |
|-------|-------|
| **Route** | `/website/{slug}` |
| **Description** | Open public site home |
| **Preconditions** | Published gym website exists |
| **Steps** | All nav links visible |
| **Expected Result** |  |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-PUB-018 - About - Page load

| Field | Value |
|-------|-------|
| **Route** | `/website/{slug}/about` |
| **Description** | Click About in nav |
| **Preconditions** | Published gym website exists |
| **Steps** | About content displayed |
| **Expected Result** |  |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-PUB-019 - Plans - Plans list

| Field | Value |
|-------|-------|
| **Route** | `/website/{slug}/plans` |
| **Description** | Open Plans page |
| **Preconditions** | Published gym website exists |
| **Steps** | Membership plans listed |
| **Expected Result** |  |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-PUB-020 - Trainers - Trainers list

| Field | Value |
|-------|-------|
| **Route** | `/website/{slug}/trainers` |
| **Description** | Open Trainers page |
| **Preconditions** | Published gym website exists |
| **Steps** | Trainer cards displayed |
| **Expected Result** |  |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-PUB-021 - Gallery - Gallery load

| Field | Value |
|-------|-------|
| **Route** | `/website/{slug}/gallery` |
| **Description** | Open Gallery page |
| **Preconditions** | Published gym website exists |
| **Steps** | Images displayed |
| **Expected Result** |  |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-PUB-022 - Contact - Contact form submit

| Field | Value |
|-------|-------|
| **Route** | `/website/{slug}/contact` |
| **Description** | Fill enquiry form; Send Enquiry |
| **Preconditions** | Published gym website exists |
| **Steps** | Success confirmation |
| **Expected Result** |  |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-PUB-023 - Contact - Book trial submit

| Field | Value |
|-------|-------|
| **Route** | `/website/{slug}/contact` |
| **Description** | Fill trial form; Book Trial |
| **Preconditions** | Published gym website exists |
| **Steps** | Trial booking submitted |
| **Expected Result** |  |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-PUB-024 - Home - Book Free Trial CTA

| Field | Value |
|-------|-------|
| **Route** | `/website/{slug}` |
| **Description** | Click hero CTA |
| **Preconditions** | Published gym website exists |
| **Steps** | Navigate to contact/trial section |
| **Expected Result** |  |
| **Priority** | Medium |
| **Status** | [ ] Not Run |

### Module: SA

#### TC-SA-025 - Dashboard - Load

| Field | Value |
|-------|-------|
| **Route** | `/super-admin` |
| **Description** | Load |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | KPI cards and charts render |
| **Expected Result** | High |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-SA-026 - Dashboard - Quick Actions

| Field | Value |
|-------|-------|
| **Route** | `/super-admin` |
| **Description** | Quick Actions |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Click quick action links |
| **Expected Result** | Navigate to target pages |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-SA-027 - Gyms - List Load

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/gyms` |
| **Description** | List Load |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Gyms table loads |
| **Expected Result** | Gyms listed |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-SA-028 - Gyms - Add Gym

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/gyms` |
| **Description** | Add Gym |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Open Add Gym; fill form; save |
| **Expected Result** | New gym appears in list |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-SA-029 - Gyms - Edit Gym

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/gyms` |
| **Description** | Edit Gym |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Edit existing gym; save |
| **Expected Result** | Changes reflected |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-SA-030 - Gyms - Activate/Deactivate

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/gyms` |
| **Description** | Activate/Deactivate |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Toggle gym status |
| **Expected Result** | Status badge updates |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-SA-031 - Gyms - Delete Gym

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/gyms` |
| **Description** | Delete Gym |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Delete gym with confirmation |
| **Expected Result** | Gym removed from list |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-SA-032 - Gym Admins - List Load

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/gym-admins` |
| **Description** | List Load |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Gym admins table loads |
| **Expected Result** | Admins listed |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-SA-033 - Gym Admins - Add Admin

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/gym-admins` |
| **Description** | Add Admin |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Create gym admin |
| **Expected Result** | Admin created |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-SA-034 - Gym Admins - Edit Admin

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/gym-admins` |
| **Description** | Edit Admin |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Edit admin details |
| **Expected Result** | Changes saved |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-SA-035 - Gym Admins - Resend Temp Password

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/gym-admins` |
| **Description** | Resend Temp Password |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Click resend temp password |
| **Expected Result** | Success notification |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-SA-036 - Gym Admins - Activate/Deactivate

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/gym-admins` |
| **Description** | Activate/Deactivate |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Toggle admin status |
| **Expected Result** | Status updates |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-SA-037 - Roles - List Load

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/roles` |
| **Description** | List Load |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Roles table loads |
| **Expected Result** | Roles listed |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-SA-038 - Roles - Add Role

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/roles` |
| **Description** | Add Role |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Create new role |
| **Expected Result** | Role added |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-SA-039 - Roles - Edit Role

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/roles` |
| **Description** | Edit Role |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Edit role name/details |
| **Expected Result** | Changes saved |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-SA-040 - Roles - Delete Role

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/roles` |
| **Description** | Delete Role |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Delete role |
| **Expected Result** | Role removed |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-SA-041 - Privileges - List Load

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/privileges` |
| **Description** | List Load |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Privileges table loads |
| **Expected Result** | Privileges listed |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-SA-042 - Privileges - Add Privilege

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/privileges` |
| **Description** | Add Privilege |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Create privilege |
| **Expected Result** | Privilege added |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-SA-043 - Privileges - Delete Privilege

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/privileges` |
| **Description** | Delete Privilege |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Delete privilege |
| **Expected Result** | Privilege removed |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-SA-044 - Role Matrix - Matrix Load

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/role-matrix` |
| **Description** | Matrix Load |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Permission matrix renders |
| **Expected Result** | Matrix displayed |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-SA-045 - Role Matrix - Toggle Permission

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/role-matrix` |
| **Description** | Toggle Permission |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Check/uncheck role-privilege cell |
| **Expected Result** | Permission assigned/removed |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-SA-046 - Audit Logs - List Load

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/audit` |
| **Description** | List Load |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Audit logs load |
| **Expected Result** | Logs displayed |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-SA-047 - Audit Logs - Search Filter

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/audit` |
| **Description** | Search Filter |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Search by user/entity/action |
| **Expected Result** | Filtered results |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-SA-048 - Audit Logs - Date Filter

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/audit` |
| **Description** | Date Filter |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Apply date range filter |
| **Expected Result** | Results within range |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-SA-049 - Audit Logs - Export PDF

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/audit` |
| **Description** | Export PDF |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Click Export PDF |
| **Expected Result** | PDF downloads |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-SA-050 - Audit Logs - Export Excel

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/audit` |
| **Description** | Export Excel |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Click Export Excel |
| **Expected Result** | Excel downloads |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-SA-051 - White Label - Dashboard Load

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/white-label` |
| **Description** | Dashboard Load |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | SaaS white-label dashboard loads |
| **Expected Result** | KPIs and table shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-SA-052 - White Label - Search

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/white-label` |
| **Description** | Search |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Search premium customers |
| **Expected Result** | Filtered table |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-SA-053 - White Label - Status Filter

| Field | Value |
|-------|-------|
| **Route** | `/super-admin/white-label` |
| **Description** | Status Filter |
| **Preconditions** | Logged in as Super Admin |
| **Steps** | Filter by subscription status |
| **Expected Result** | Filtered results |
| **Priority** | Medium |
| **Status** | [ ] Not Run |

### Module: GA

#### TC-GA-054 - Dashboard - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/dashboard` |
| **Description** | Dashboard loads with KPIs and charts |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open sidebar Dashboard |
| **Expected Result** | KPI cards and charts visible |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-055 - Dashboard - Export PDF

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/dashboard` |
| **Description** | Export dashboard PDF |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Export PDF |
| **Expected Result** | PDF file downloads |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-056 - Dashboard - Export Excel

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/dashboard` |
| **Description** | Export dashboard Excel |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Export Excel |
| **Expected Result** | Excel file downloads |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-057 - Dashboard - Quick Nav

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/dashboard` |
| **Description** | Quick action strip navigation |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click quick nav links |
| **Expected Result** | Navigate to linked modules |
| **Priority** | Low |
| **Status** | [ ] Not Run |
#### TC-GA-058 - Revenue Analytics - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/analytics/revenue` |
| **Description** | Revenue analytics page loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Revenue Analytics |
| **Expected Result** | Charts and KPIs visible |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-059 - Revenue Analytics - Export

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/analytics/revenue` |
| **Description** | Export revenue report |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Export PDF/Excel |
| **Expected Result** | Files download |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-060 - Member Analytics - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/analytics/members` |
| **Description** | Member analytics loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Member Analytics |
| **Expected Result** | Member charts visible |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-061 - Member Analytics - Export

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/analytics/members` |
| **Description** | Export member analytics |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Export PDF/Excel |
| **Expected Result** | Files download |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-062 - Attendance Analytics - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/analytics/attendance` |
| **Description** | Attendance analytics loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Attendance Analytics |
| **Expected Result** | Charts visible |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-063 - Trainer Analytics - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/analytics/trainers` |
| **Description** | Trainer analytics loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Trainer Analytics |
| **Expected Result** | Charts visible |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-064 - Members List - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/members` |
| **Description** | Members list loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Members |
| **Expected Result** | Table with members shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-065 - Members List - Search

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/members` |
| **Description** | Search members |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Enter name/email in search |
| **Expected Result** | Filtered results |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-066 - Members List - Add Member

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/members` |
| **Description** | Create member via dialog |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Add Member; fill form; save |
| **Expected Result** | Member appears in list |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-067 - Members List - Edit Member

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/members` |
| **Description** | Edit member from row action |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Edit; update; save |
| **Expected Result** | Changes reflected |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-068 - Members List - Assign Trainer

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/members` |
| **Description** | Assign trainer to member |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Assign trainer; select; save |
| **Expected Result** | Trainer assigned |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-069 - Members List - Delete Member

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/members` |
| **Description** | Delete/deactivate member |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Delete; confirm |
| **Expected Result** | Member removed/deactivated |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-070 - Members List - View Detail

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/members` |
| **Description** | Navigate to member detail |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click member row/view |
| **Expected Result** | Open /gym-admin/members/:id |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-071 - Member Detail - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/members/:id` |
| **Description** | Member detail loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open member detail |
| **Expected Result** | Profile info displayed |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-072 - Member Detail - Edit

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/members/:id` |
| **Description** | Edit from detail page |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Edit; save changes |
| **Expected Result** | Changes saved |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-073 - Member Detail - View Diet

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/members/:id` |
| **Description** | Navigate to member diet |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Diet link |
| **Expected Result** | Open member diet view |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-074 - Member Detail - View Workout

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/members/:id` |
| **Description** | Navigate to member workout |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Workout link |
| **Expected Result** | Open member workout view |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-075 - Member Diet View - Assign Plan

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/members/:id/diet` |
| **Description** | Assign diet plan to member |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Select plan; assign |
| **Expected Result** | Plan assigned |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-076 - Member Diet View - Export PDF

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/members/:id/diet` |
| **Description** | Export diet plan PDF |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Export PDF |
| **Expected Result** | PDF downloads |
| **Priority** | Low |
| **Status** | [ ] Not Run |
#### TC-GA-077 - Member Workout View - Assign Plan

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/members/:id/workout` |
| **Description** | Assign workout plan |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Select plan; assign |
| **Expected Result** | Plan assigned |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-078 - Trainers List - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/trainers` |
| **Description** | Trainers list loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Trainers |
| **Expected Result** | Table displayed |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-079 - Trainers List - Search

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/trainers` |
| **Description** | Search trainers |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Enter search text |
| **Expected Result** | Filtered results |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-080 - Trainers List - Add Trainer

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/trainers` |
| **Description** | Create trainer |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Add Trainer dialog; save |
| **Expected Result** | Trainer in list |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-081 - Trainers List - Edit Trainer

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/trainers` |
| **Description** | Edit trainer |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Edit row; save |
| **Expected Result** | Changes saved |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-082 - Trainers List - Deactivate

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/trainers` |
| **Description** | Deactivate trainer |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Deactivate; confirm |
| **Expected Result** | Status inactive |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-083 - Trainer Detail - Assign Members

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/trainers/:id` |
| **Description** | Assign members to trainer |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Assign members; save |
| **Expected Result** | Members linked |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-084 - Trainer Detail - Unassign Member

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/trainers/:id` |
| **Description** | Remove member assignment |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Unassign from table |
| **Expected Result** | Member unassigned |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-085 - Leads List - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/leads` |
| **Description** | Leads list loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Leads & CRM |
| **Expected Result** | Leads table/kanban shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-086 - Leads List - Add Lead

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/leads` |
| **Description** | Navigate to create lead |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Add Lead |
| **Expected Result** | Open create form |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-087 - Leads List - Search

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/leads` |
| **Description** | Search leads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Enter search |
| **Expected Result** | Filtered leads |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-088 - Leads List - Status Filter

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/leads` |
| **Description** | Filter by status |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Select status filter |
| **Expected Result** | Filtered results |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-089 - Leads List - Kanban Toggle

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/leads` |
| **Description** | Switch list/kanban view |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Toggle view mode |
| **Expected Result** | View switches |
| **Priority** | Low |
| **Status** | [ ] Not Run |
#### TC-GA-090 - Lead Create - Create Lead

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/leads/create` |
| **Description** | Submit new lead form |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Fill form; Create lead |
| **Expected Result** | Lead created; redirect/list update |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-091 - Lead Edit - Edit Lead

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/leads/edit/:id` |
| **Description** | Update lead details |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Modify form; Save changes |
| **Expected Result** | Lead updated |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-092 - Lead Detail - View Detail

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/leads/:id` |
| **Description** | Lead detail page loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open lead from list |
| **Expected Result** | Detail shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-093 - Lead Detail - Convert to Member

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/leads/:id` |
| **Description** | Convert lead to member |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Convert; fill modal; submit |
| **Expected Result** | Member created |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-094 - Lead Followups - List Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/leads/followups` |
| **Description** | Pending follow-ups load |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Follow-ups |
| **Expected Result** | Follow-up list shown |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-095 - Lead Trials - List Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/leads/trials` |
| **Description** | Today trials load |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Trials |
| **Expected Result** | Trial list shown |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-096 - Lead Analytics - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/leads/analytics` |
| **Description** | Lead analytics loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Lead Analytics |
| **Expected Result** | Charts visible |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-097 - Lead Analytics - Export

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/leads/analytics` |
| **Description** | Export lead analytics |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Export PDF/Excel |
| **Expected Result** | Files download |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-098 - Membership Plans - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/membership-plans` |
| **Description** | Plans list loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Membership Plans |
| **Expected Result** | Plans table shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-099 - Membership Plans - Create Plan

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/membership-plans` |
| **Description** | Create membership plan |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Create plan dialog; save |
| **Expected Result** | Plan added |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-100 - Membership Plans - Edit Plan

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/membership-plans` |
| **Description** | Edit plan |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Edit; save |
| **Expected Result** | Plan updated |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-101 - Membership Plans - Deactivate Plan

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/membership-plans` |
| **Description** | Deactivate plan |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Deactivate action |
| **Expected Result** | Plan inactive |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-102 - Memberships - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/memberships` |
| **Description** | Active memberships load |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Memberships |
| **Expected Result** | Memberships listed |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-103 - Memberships - Create Membership

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/memberships` |
| **Description** | Assign membership to member |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Create membership; save |
| **Expected Result** | Membership created |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-104 - Memberships - Renew

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/memberships` |
| **Description** | Renew membership |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Renew; confirm |
| **Expected Result** | Membership extended |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-105 - Memberships - Cancel

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/memberships` |
| **Description** | Cancel membership |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Cancel; confirm |
| **Expected Result** | Membership cancelled |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-106 - Expired Memberships - List Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/memberships/expired` |
| **Description** | Expired memberships load |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open expired link |
| **Expected Result** | Expired list shown |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-107 - Payments - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/payments` |
| **Description** | Payments list loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Payments |
| **Expected Result** | Payments table shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-108 - Payments - Record Payment

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/payments` |
| **Description** | Record manual payment |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Record payment dialog; save |
| **Expected Result** | Payment recorded |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-109 - Payments - Download Invoice

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/payments` |
| **Description** | Download payment invoice |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Download invoice |
| **Expected Result** | Invoice file downloads |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-110 - Payments - Refund

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/payments` |
| **Description** | Refund payment |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Refund; confirm |
| **Expected Result** | Payment refunded |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-111 - Revenue - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/revenue` |
| **Description** | Revenue dashboard loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Revenue |
| **Expected Result** | Revenue KPIs shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-112 - Expenses - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/expenses` |
| **Description** | Expenses list loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Expenses |
| **Expected Result** | Expense table shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-113 - Expenses - Add Expense

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/expenses` |
| **Description** | Create expense |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Add expense modal; save |
| **Expected Result** | Expense added |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-114 - Expenses - Edit Expense

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/expenses` |
| **Description** | Edit expense |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Edit; save |
| **Expected Result** | Expense updated |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-115 - Expenses - Delete Expense

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/expenses` |
| **Description** | Delete expense |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Delete; confirm |
| **Expected Result** | Expense removed |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-116 - Expenses - Export Excel

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/expenses` |
| **Description** | Export expenses |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Export Excel |
| **Expected Result** | File downloads |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-117 - Payroll - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/payroll` |
| **Description** | Payroll list loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Payroll |
| **Expected Result** | Payroll table shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-118 - Payroll - Generate Payroll

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/payroll` |
| **Description** | Generate payroll run |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Generate modal; submit |
| **Expected Result** | Payroll entries created |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-119 - Payroll - Approve

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/payroll` |
| **Description** | Approve payroll |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Approve |
| **Expected Result** | Status approved |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-120 - Payroll - Mark Paid

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/payroll` |
| **Description** | Mark payroll paid |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Mark paid |
| **Expected Result** | Status paid |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-121 - Payroll - Export PDF

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/payroll` |
| **Description** | Export payroll PDF |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Export PDF |
| **Expected Result** | File downloads |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-122 - Financial Dashboard - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/financial` |
| **Description** | Financial dashboard loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Financial |
| **Expected Result** | KPIs/charts shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-123 - Financial Dashboard - Export

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/financial` |
| **Description** | Export financial report |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Export PDF/Excel |
| **Expected Result** | Files download |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-124 - Branches List - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/branches` |
| **Description** | Branches page loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Branches |
| **Expected Result** | Branch list/form shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-125 - Branches List - Create Branch

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/branches` |
| **Description** | Add new branch |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Fill branch form; Add Branch |
| **Expected Result** | Branch appears in table |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-126 - Branches List - Nav Dashboard

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/branches` |
| **Description** | Navigate to branch dashboard |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Dashboard link |
| **Expected Result** | Open branches/dashboard |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-127 - Branch Dashboard - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/branches/dashboard` |
| **Description** | Branch dashboard loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Branch Dashboard |
| **Expected Result** | Branch KPI cards shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-128 - Branch Dashboard - Sidebar Highlight

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/branches/dashboard` |
| **Description** | Only one menu item active |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Branch Dashboard |
| **Expected Result** | Only Branch Dashboard highlighted |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-129 - Branch Analytics - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/branches/analytics` |
| **Description** | Branch analytics loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Branch Analytics |
| **Expected Result** | Analytics charts shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-130 - Branch Transfers - Transfer Member

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/branches/transfers` |
| **Description** | Transfer member between branches |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Select member + target branch; Submit |
| **Expected Result** | Transfer recorded |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-131 - Branch Transfers - Transfer Trainer

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/branches/transfers` |
| **Description** | Transfer trainer between branches |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Select trainer + branch; Submit |
| **Expected Result** | Transfer recorded |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-132 - Branch Targets - Set Target

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/branches/targets` |
| **Description** | Save branch monthly target |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Fill target form; Submit |
| **Expected Result** | Target saved |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-133 - Attendance Hub - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/attendance` |
| **Description** | Attendance dashboard loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Attendance |
| **Expected Result** | Hub links visible |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-134 - Attendance List - List Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/attendance/list` |
| **Description** | Attendance records load |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open List |
| **Expected Result** | Records table shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-135 - Check In - Member Check In

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/attendance/check-in` |
| **Description** | Check in member |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Select member; Check In |
| **Expected Result** | Session opened |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-136 - Check Out - Member Check Out

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/attendance/check-out` |
| **Description** | Check out member |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Select open session; Check Out |
| **Expected Result** | Session closed |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-137 - Member History - History Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/attendance/members/:id/history` |
| **Description** | Member attendance history |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open member history |
| **Expected Result** | History table shown |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-138 - Member History - Export Excel

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/attendance/members/:id/history` |
| **Description** | Export history |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Export Excel |
| **Expected Result** | File downloads |
| **Priority** | Low |
| **Status** | [ ] Not Run |
#### TC-GA-139 - Attendance Reports - Daily Report

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/attendance/reports` |
| **Description** | Load daily report |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Select daily tab; Load |
| **Expected Result** | Report data shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-140 - Attendance Reports - Monthly Report

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/attendance/reports` |
| **Description** | Load monthly report |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Select monthly tab; Load |
| **Expected Result** | Report data shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-141 - Attendance Reports - Export

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/attendance/reports` |
| **Description** | Export attendance report |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Export PDF/Excel |
| **Expected Result** | Files download |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-142 - Trainer Attendance - Trainer Check In

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/attendance/trainers` |
| **Description** | Check in trainer |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Select trainer; Check In |
| **Expected Result** | Trainer checked in |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-143 - Trainer Attendance - Trainer Check Out

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/attendance/trainers` |
| **Description** | Check out trainer |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Check Out |
| **Expected Result** | Trainer checked out |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-144 - Diet Plans - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/diet-plans` |
| **Description** | Diet plans list loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Diet Plans |
| **Expected Result** | Plans table shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-145 - Diet Plans - Search

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/diet-plans` |
| **Description** | Search diet plans |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Enter search |
| **Expected Result** | Filtered plans |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-146 - Diet Plans - New Plan

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/diet-plans` |
| **Description** | Navigate to create |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click New plan |
| **Expected Result** | Open editor |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-147 - Diet Plans - Assign

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/diet-plans` |
| **Description** | Assign plan to member |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Assign action; select member |
| **Expected Result** | Assignment success |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-148 - Diet Plans - Clone

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/diet-plans` |
| **Description** | Clone diet plan |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Clone |
| **Expected Result** | Duplicate plan created |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-149 - Diet Plans - Delete

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/diet-plans` |
| **Description** | Delete diet plan |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Delete; confirm |
| **Expected Result** | Plan removed |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-150 - Diet Plan Editor - Create Plan

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/diet-plans/new` |
| **Description** | Save new diet plan |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Fill plan + items; Save |
| **Expected Result** | Plan created |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-151 - Diet Plan Editor - Edit Plan

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/diet-plans/:id/edit` |
| **Description** | Update diet plan |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Modify; Save plan |
| **Expected Result** | Plan updated |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-152 - Workout Plans - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/workout-plans` |
| **Description** | Workout plans list loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Workout Plans |
| **Expected Result** | Plans listed |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-153 - Workout Plans - New Plan

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/workout-plans` |
| **Description** | Create workout plan |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | New Plan; save |
| **Expected Result** | Plan created |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-154 - Workout Plans - Exercise Library Link

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/workout-plans` |
| **Description** | Open exercise library |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Exercise Library |
| **Expected Result** | Open exercises page |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-155 - Exercise Library - Add Exercise

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/workout-plans/exercises` |
| **Description** | Create exercise |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Add Exercise; save |
| **Expected Result** | Exercise added |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-156 - Exercise Library - Edit Exercise

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/workout-plans/exercises` |
| **Description** | Edit exercise |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Edit; save |
| **Expected Result** | Exercise updated |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-157 - Exercise Library - Delete Exercise

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/workout-plans/exercises` |
| **Description** | Delete exercise |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Delete; confirm |
| **Expected Result** | Exercise removed |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-158 - Workout Plan Editor - Create Plan

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/workout-plans/new` |
| **Description** | Save workout plan |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Add exercises; Save |
| **Expected Result** | Plan created |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-159 - Workout Plan Editor - Edit Plan

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/workout-plans/:id/edit` |
| **Description** | Update workout plan |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Modify; Save |
| **Expected Result** | Plan updated |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-160 - Bookings - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/bookings` |
| **Description** | Bookings list loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Bookings |
| **Expected Result** | Bookings table shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-161 - Schedules - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/schedules` |
| **Description** | Class schedules load |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Class Schedules |
| **Expected Result** | Schedule cards shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-162 - Schedules - Cancel Schedule

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/schedules` |
| **Description** | Cancel a schedule |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Cancel schedule; confirm |
| **Expected Result** | Schedule cancelled |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-163 - Booking Analytics - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/booking-analytics` |
| **Description** | Booking analytics loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Booking Analytics |
| **Expected Result** | Analytics shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-164 - Booking Analytics - Export

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/booking-analytics` |
| **Description** | Export booking analytics |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Export PDF/Excel |
| **Expected Result** | Files download |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-165 - Notifications Hub - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/notifications` |
| **Description** | Notification dashboard loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Notifications |
| **Expected Result** | Quick links shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-166 - Notification Templates - List Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/notifications/templates` |
| **Description** | Templates load |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Templates |
| **Expected Result** | Templates listed |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-167 - Notification Templates - Create Template

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/notifications/templates` |
| **Description** | Create template |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Create; save |
| **Expected Result** | Template added |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-168 - Notification Templates - Edit Template

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/notifications/templates` |
| **Description** | Edit template |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Edit; save |
| **Expected Result** | Template updated |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-169 - Notification Templates - Delete Template

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/notifications/templates` |
| **Description** | Delete template |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Delete; confirm |
| **Expected Result** | Template removed |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-170 - Notification History - List Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/notifications/history` |
| **Description** | History loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open History |
| **Expected Result** | Sent logs shown |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-171 - Notification Test - Send Test

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/notifications/test` |
| **Description** | Send test notification |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Fill form; Send |
| **Expected Result** | Test sent success |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-172 - Mobile Push - Send Push

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/mobile-notifications` |
| **Description** | Send mobile push notification |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Fill form; Send |
| **Expected Result** | Push sent |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-173 - Mobile Analytics - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/mobile-analytics` |
| **Description** | Mobile analytics loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Mobile Analytics |
| **Expected Result** | Stats shown |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-174 - AI Dashboard - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/ai` |
| **Description** | AI dashboard loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open AI Dashboard |
| **Expected Result** | Overview shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-175 - AI Insights - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/ai/insights` |
| **Description** | AI insights loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open AI Insights |
| **Expected Result** | Insights/tabs shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-176 - AI Insights - Tab Switch

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/ai/insights` |
| **Description** | Switch insights tabs |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Toggle Insights/Leads tabs |
| **Expected Result** | Tab content changes |
| **Priority** | Low |
| **Status** | [ ] Not Run |
#### TC-GA-177 - Website Builder - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/website-builder` |
| **Description** | Website builder loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Website Builder |
| **Expected Result** | Settings form shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-178 - Website Builder - Save Settings

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/website-builder` |
| **Description** | Save site settings |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Edit settings; Save |
| **Expected Result** | Settings saved |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-179 - Website Builder - Publish/Unpublish

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/website-builder` |
| **Description** | Toggle publish state |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Publish or Unpublish |
| **Expected Result** | Status updates |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-180 - Website Pages - Add Page

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/website-builder/pages` |
| **Description** | Add website page |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Add Page; save |
| **Expected Result** | Page added |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-181 - Website Pages - Delete Page

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/website-builder/pages` |
| **Description** | Delete page |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Delete; confirm |
| **Expected Result** | Page removed |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-182 - Website Gallery - Add Image

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/website-builder/gallery` |
| **Description** | Upload gallery image |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Add Image; save |
| **Expected Result** | Image added |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-183 - Website Gallery - Remove Image

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/website-builder/gallery` |
| **Description** | Remove gallery image |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Remove; confirm |
| **Expected Result** | Image removed |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-184 - Website Testimonials - Add Testimonial

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/website-builder/testimonials` |
| **Description** | Add testimonial |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Fill form; Add |
| **Expected Result** | Testimonial added |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-185 - Website Analytics - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/website-builder/analytics` |
| **Description** | Website analytics loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Website Analytics |
| **Expected Result** | Traffic stats shown |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-186 - White Label Settings - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/white-label` |
| **Description** | White label settings load |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open White Label |
| **Expected Result** | Branding form shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-187 - White Label Settings - Save Branding

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/white-label` |
| **Description** | Save white label settings |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Edit; Save |
| **Expected Result** | Settings saved |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-188 - White Label Preview - Preview Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/white-label/preview` |
| **Description** | Preview branding |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Preview |
| **Expected Result** | Preview renders |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-189 - Gym Branding - Save Branding

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/settings/branding` |
| **Description** | Save gym logo/receipt branding |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Upload logo; Save |
| **Expected Result** | Branding saved |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-190 - Branding Route - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/branding` |
| **Description** | Branding alias route loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open /gym-admin/branding |
| **Expected Result** | Same as white-label settings |
| **Priority** | Low |
| **Status** | [ ] Not Run |
#### TC-GA-191 - Subscription - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/subscription` |
| **Description** | Subscription page loads |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Subscription |
| **Expected Result** | Plan info shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-192 - Subscription - Upgrade Monthly

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/subscription` |
| **Description** | Upgrade to monthly plan |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Upgrade Monthly |
| **Expected Result** | Subscription updated |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-193 - Subscription - Cancel Subscription

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/subscription` |
| **Description** | Cancel SaaS subscription |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Click Cancel; confirm |
| **Expected Result** | Subscription cancelled |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-194 - Audit Logs - List Load

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/audit` |
| **Description** | Gym audit logs load |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Open Audit Logs |
| **Expected Result** | Logs displayed |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-GA-195 - Audit Logs - Filter Search

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/audit` |
| **Description** | Filter audit logs |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Apply search/filters |
| **Expected Result** | Filtered results |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-GA-196 - Audit Logs - Export

| Field | Value |
|-------|-------|
| **Route** | `/gym-admin/audit` |
| **Description** | Export audit logs |
| **Preconditions** | Logged in as Gym Admin |
| **Steps** | Export PDF/Excel |
| **Expected Result** | Files download |
| **Priority** | Medium |
| **Status** | [ ] Not Run |

### Module: TR

#### TC-TR-197 - Dashboard - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/trainer` |
| **Description** | Trainer dashboard loads |
| **Preconditions** | Logged in as Trainer |
| **Steps** | Open Dashboard |
| **Expected Result** | KPIs and quick links shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-TR-198 - My Members - List Load

| Field | Value |
|-------|-------|
| **Route** | `/trainer/members` |
| **Description** | Assigned members load |
| **Preconditions** | Logged in as Trainer |
| **Steps** | Open My Members |
| **Expected Result** | Members table shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-TR-199 - My Members - View Workout

| Field | Value |
|-------|-------|
| **Route** | `/trainer/members` |
| **Description** | Open member workout |
| **Preconditions** | Logged in as Trainer |
| **Steps** | Click View workout |
| **Expected Result** | Member workout page opens |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-TR-200 - Leads - List Load

| Field | Value |
|-------|-------|
| **Route** | `/trainer/leads` |
| **Description** | Trainer leads list loads |
| **Preconditions** | Logged in as Trainer |
| **Steps** | Navigate to /trainer/leads |
| **Expected Result** | Leads list shown |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-TR-201 - Attendance Hub - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/trainer/attendance` |
| **Description** | Attendance hub loads |
| **Preconditions** | Logged in as Trainer |
| **Steps** | Open Attendance |
| **Expected Result** | Hub links shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-TR-202 - Check In - Check In Member

| Field | Value |
|-------|-------|
| **Route** | `/trainer/attendance/check-in` |
| **Description** | Trainer checks in member |
| **Preconditions** | Logged in as Trainer |
| **Steps** | Select member; Check In |
| **Expected Result** | Check-in success |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-TR-203 - Check Out - Check Out Member

| Field | Value |
|-------|-------|
| **Route** | `/trainer/attendance/check-out` |
| **Description** | Trainer checks out member |
| **Preconditions** | Logged in as Trainer |
| **Steps** | Select session; Check Out |
| **Expected Result** | Check-out success |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-TR-204 - Workout Plans - List Load

| Field | Value |
|-------|-------|
| **Route** | `/trainer/workout-plans` |
| **Description** | Workout plans list |
| **Preconditions** | Logged in as Trainer |
| **Steps** | Open Workout Plans |
| **Expected Result** | Plans listed |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-TR-205 - Member Workout - Assign Workout

| Field | Value |
|-------|-------|
| **Route** | `/trainer/members/:id/workout` |
| **Description** | Assign workout to member |
| **Preconditions** | Logged in as Trainer |
| **Steps** | Select plan; assign |
| **Expected Result** | Plan assigned |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-TR-206 - AI Recommendations - List Load

| Field | Value |
|-------|-------|
| **Route** | `/trainer/ai-recommendations` |
| **Description** | AI recommendations load |
| **Preconditions** | Logged in as Trainer |
| **Steps** | Open AI Recommendations |
| **Expected Result** | Recommendations listed |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-TR-207 - AI Recommendations - Mark Accepted

| Field | Value |
|-------|-------|
| **Route** | `/trainer/ai-recommendations` |
| **Description** | Accept recommendation |
| **Preconditions** | Logged in as Trainer |
| **Steps** | Click Mark accepted |
| **Expected Result** | Status updated |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-TR-208 - Schedule - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/trainer/schedule` |
| **Description** | Trainer schedule loads |
| **Preconditions** | Logged in as Trainer |
| **Steps** | Open My Schedule |
| **Expected Result** | Schedule shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-TR-209 - Bookings - List Load

| Field | Value |
|-------|-------|
| **Route** | `/trainer/bookings` |
| **Description** | Class bookings load |
| **Preconditions** | Logged in as Trainer |
| **Steps** | Open Class Bookings |
| **Expected Result** | Bookings listed |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-TR-210 - Bookings - Search Filter

| Field | Value |
|-------|-------|
| **Route** | `/trainer/bookings` |
| **Description** | Filter bookings |
| **Preconditions** | Logged in as Trainer |
| **Steps** | Search/status filter |
| **Expected Result** | Filtered results |
| **Priority** | Medium |
| **Status** | [ ] Not Run |

### Module: MB

#### TC-MB-211 - Dashboard - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/member/dashboard` |
| **Description** | Member dashboard loads |
| **Preconditions** | Logged in as Member |
| **Steps** | Open Dashboard |
| **Expected Result** | KPIs and quick actions shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-MB-212 - Profile - Page Load

| Field | Value |
|-------|-------|
| **Route** | `/member/profile` |
| **Description** | Profile page loads |
| **Preconditions** | Logged in as Member |
| **Steps** | Open My Profile |
| **Expected Result** | Tabs and profile info shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-MB-213 - Profile - Tab Navigation

| Field | Value |
|-------|-------|
| **Route** | `/member/profile` |
| **Description** | Switch profile tabs |
| **Preconditions** | Logged in as Member |
| **Steps** | Click each tab |
| **Expected Result** | Tab content loads |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-MB-214 - Profile - Pay Membership Link

| Field | Value |
|-------|-------|
| **Route** | `/member/profile` |
| **Description** | Navigate to checkout |
| **Preconditions** | Logged in as Member |
| **Steps** | Click Pay Membership |
| **Expected Result** | Open checkout page |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-MB-215 - Goals - Add Goal

| Field | Value |
|-------|-------|
| **Route** | `/member/goals` |
| **Description** | Create fitness goal |
| **Preconditions** | Logged in as Member |
| **Steps** | Fill Add Goal form; submit |
| **Expected Result** | Goal appears in list |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-MB-216 - Goals - Mark Complete

| Field | Value |
|-------|-------|
| **Route** | `/member/goals` |
| **Description** | Complete a goal |
| **Preconditions** | Logged in as Member |
| **Steps** | Click Mark Complete |
| **Expected Result** | Goal marked complete |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-MB-217 - Progress - Log Progress

| Field | Value |
|-------|-------|
| **Route** | `/member/progress` |
| **Description** | Log body progress |
| **Preconditions** | Logged in as Member |
| **Steps** | Fill Log Progress form; submit |
| **Expected Result** | Progress logged |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-MB-218 - Progress - Export PDF

| Field | Value |
|-------|-------|
| **Route** | `/member/progress` |
| **Description** | Export progress PDF |
| **Preconditions** | Logged in as Member |
| **Steps** | Export PDF |
| **Expected Result** | File downloads |
| **Priority** | Low |
| **Status** | [ ] Not Run |
#### TC-MB-219 - Workouts - Log Workout

| Field | Value |
|-------|-------|
| **Route** | `/member/workouts` |
| **Description** | Log workout session |
| **Preconditions** | Logged in as Member |
| **Steps** | Fill workout log; submit |
| **Expected Result** | Workout logged |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-MB-220 - Workouts - Mark Complete

| Field | Value |
|-------|-------|
| **Route** | `/member/workouts` |
| **Description** | Mark workout complete |
| **Preconditions** | Logged in as Member |
| **Steps** | Mark Complete |
| **Expected Result** | Status updated |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-MB-221 - Diets - Log Diet

| Field | Value |
|-------|-------|
| **Route** | `/member/diets` |
| **Description** | Log daily diet |
| **Preconditions** | Logged in as Member |
| **Steps** | Fill diet log; Log Today |
| **Expected Result** | Diet logged |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-MB-222 - Water - Save Intake

| Field | Value |
|-------|-------|
| **Route** | `/member/water` |
| **Description** | Log water intake |
| **Preconditions** | Logged in as Member |
| **Steps** | Enter amount; Save |
| **Expected Result** | Intake saved |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-MB-223 - Referrals - Copy Code

| Field | Value |
|-------|-------|
| **Route** | `/member/referrals` |
| **Description** | Copy referral code |
| **Preconditions** | Logged in as Member |
| **Steps** | Click Copy Code |
| **Expected Result** | Code copied to clipboard |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-MB-224 - Feedback - Submit Feedback

| Field | Value |
|-------|-------|
| **Route** | `/member/feedback` |
| **Description** | Submit gym feedback |
| **Preconditions** | Logged in as Member |
| **Steps** | Rate + comment; Submit Feedback |
| **Expected Result** | Feedback submitted |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-MB-225 - My Diet Plan - View Plan

| Field | Value |
|-------|-------|
| **Route** | `/member/diet` |
| **Description** | View assigned diet plan |
| **Preconditions** | Logged in as Member |
| **Steps** | Open My Diet Plan |
| **Expected Result** | Plan details shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-MB-226 - My Workout Plan - View Plan

| Field | Value |
|-------|-------|
| **Route** | `/member/workout` |
| **Description** | View assigned workout plan |
| **Preconditions** | Logged in as Member |
| **Steps** | Open My Workout Plan |
| **Expected Result** | Plan details shown |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-MB-227 - Checkout - Select Plan Pay

| Field | Value |
|-------|-------|
| **Route** | `/member/checkout` |
| **Description** | Pay for membership online |
| **Preconditions** | Logged in as Member |
| **Steps** | Select plan; Pay |
| **Expected Result** | Payment flow initiated/completed |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-MB-228 - Bookings - Book Class

| Field | Value |
|-------|-------|
| **Route** | `/member/bookings` |
| **Description** | Book available class slot |
| **Preconditions** | Logged in as Member |
| **Steps** | Select slot; Book |
| **Expected Result** | Booking confirmed |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-MB-229 - Bookings - Join Waitlist

| Field | Value |
|-------|-------|
| **Route** | `/member/bookings` |
| **Description** | Join class waitlist |
| **Preconditions** | Logged in as Member |
| **Steps** | Click Join Waitlist |
| **Expected Result** | Added to waitlist |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-MB-230 - Booking History - History Load

| Field | Value |
|-------|-------|
| **Route** | `/member/bookings/history` |
| **Description** | Past bookings load |
| **Preconditions** | Logged in as Member |
| **Steps** | Open booking history |
| **Expected Result** | History listed |
| **Priority** | Medium |
| **Status** | [ ] Not Run |

### Module: XCUT

#### TC-XCUT-231 - Sidebar - Menu Navigation

| Field | Value |
|-------|-------|
| **Route** | `All portals` |
| **Description** | Sidebar links navigate correctly |
| **Preconditions** | Logged in user |
| **Steps** | Click each visible sidebar item |
| **Expected Result** | Correct page opens |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-XCUT-232 - Sidebar - Active Highlight

| Field | Value |
|-------|-------|
| **Route** | `All portals` |
| **Description** | Only relevant menu item highlighted |
| **Preconditions** | On nested route e.g. branches/dashboard |
| **Steps** | Observe sidebar active state |
| **Expected Result** | Only child route highlighted |
| **Priority** | Medium |
| **Status** | [ ] Not Run |
#### TC-XCUT-233 - Header - Logout

| Field | Value |
|-------|-------|
| **Route** | `All portals` |
| **Description** | User can logout |
| **Preconditions** | Logged in |
| **Steps** | Open profile menu; Logout |
| **Expected Result** | Redirect to login; session cleared |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-XCUT-234 - Header - Sidenav Toggle

| Field | Value |
|-------|-------|
| **Route** | `All portals` |
| **Description** | Toggle sidebar collapse |
| **Preconditions** | Logged in |
| **Steps** | Click menu toggle |
| **Expected Result** | Sidebar opens/closes |
| **Priority** | Low |
| **Status** | [ ] Not Run |
#### TC-XCUT-235 - Auth Guard - Unauthenticated Access

| Field | Value |
|-------|-------|
| **Route** | `All protected routes` |
| **Description** | Blocked without login |
| **Preconditions** | Logged out |
| **Steps** | Open /gym-admin/dashboard directly |
| **Expected Result** | Redirect to login |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-XCUT-236 - Role Guard - Role isolation

| Field | Value |
|-------|-------|
| **Route** | `Wrong portal` |
| **Description** | Trainer cannot access gym-admin |
| **Preconditions** | Logged in as Trainer |
| **Steps** | Open /gym-admin/dashboard |
| **Expected Result** | Access denied/redirect |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-XCUT-237 - Permission Guard - Permission enforcement

| Field | Value |
|-------|-------|
| **Route** | `Restricted route` |
| **Description** | User lacks permission |
| **Preconditions** | Login without specific permission |
| **Steps** | Open restricted URL directly |
| **Expected Result** | Access blocked |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-XCUT-238 - API Errors - Error Toast

| Field | Value |
|-------|-------|
| **Route** | `All data pages` |
| **Description** | API failure shows notification |
| **Preconditions** | Simulate API down/500 |
| **Steps** | Load data page |
| **Expected Result** | Error message shown; no infinite spinner |
| **Priority** | High |
| **Status** | [ ] Not Run |
#### TC-XCUT-239 - Responsive - Mobile Layout

| Field | Value |
|-------|-------|
| **Route** | `Key pages` |
| **Description** | Pages usable on mobile width |
| **Preconditions** | Browser dev tools mobile |
| **Steps** | Open dashboard, members, leads |
| **Expected Result** | Layout readable; no overlap |
| **Priority** | Low |
| **Status** | [ ] Not Run |

---

## 5. Suggested Test Execution Order

1. **AUTH + REG** â€” Authentication and registration flows
2. **XCUT** â€” Guards, logout, sidebar navigation
3. **SA** â€” Super Admin setup (gyms, admins, roles)
4. **GA Core** â€” Dashboard, Members, Trainers, Memberships, Payments
5. **GA Operations** â€” Attendance, Leads, Branches, Financial
6. **GA Advanced** â€” Diet/Workout, Bookings, Notifications, AI, Website
7. **TR** â€” Trainer portal workflows
8. **MB** â€” Member self-service workflows
9. **PUB** â€” Public website (after website builder publish)

---

## 6. Notes

- Dialog-based CRUD (members, trainers, payments) is tested via list page events.
- `/gym-admin/branding` and `/gym-admin/white-label` share the same component.
- Trainer `/trainer/leads` exists but is not in the trainer sidebar menu.
- Replace `{slug}` in public website routes with your gym's published slug.
- For parameterized routes (`:id`), use a valid record ID from your test data.
