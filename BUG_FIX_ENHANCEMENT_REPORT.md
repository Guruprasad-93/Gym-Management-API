# Bug Fix & Enhancement Report

**Date:** 2026-06-13  
**Scope:** Password validation, phone validation, password visibility, tooltips, financial chart ordering, revenue chart layout

---

## 1. Change Password Validation

### Root cause
- **Frontend:** Mismatch error was attached to the **form group** (`passwordMatchValidator` on the group), but `mat-error` was placed inside the confirm-password `mat-form-field`. Angular Material only displays control-level errors for the bound control, so the message never appeared.
- **Backend:** `ChangePasswordDto` had no `ConfirmPassword` field; server could not reject mismatched passwords.

### Fix
- Field-level `confirmPasswordValidator('newPassword')` on the confirm control with message: **"Confirm Password does not match New Password."**
- Re-validates confirm when new password changes.
- Backend `ConfirmPassword` property + FluentValidation `Equal` rule.

### Files modified
- `Frontend/gym-app/src/app/features/auth/change-password/change-password-page.component.ts`
- `Frontend/gym-app/src/app/shared/validators/password.validators.ts` (new)
- `Frontend/gym-app/src/app/shared/components/password-field/password-field.component.ts` (new)
- `Frontend/gym-app/src/app/core/services/auth.service.ts`
- `Backend/Gym.Application/DTOs/Auth/ChangePasswordDto.cs`
- `Backend/Gym.Application/Validators/ChangePasswordDtoValidator.cs`

### Test results
- Angular build: **PASS** (development)
- Manual: mismatch on confirm field shows error; submit blocked until match

---

## 2. International Phone Number Validation

### Root cause
- Only `MaximumLength(20)` was enforced; no E.164 format validation on backend or frontend.
- Plain text inputs allowed letters and invalid lengths.

### Fix
- **Backend:** Shared `PhoneNumberRules` (E.164: `+[1-9]` + 7–15 digits) + `PhoneValidationExtensions` applied to members, leads, SaaS registration, public website leads/trials.
- **Frontend:** `PhoneFieldComponent` with country code selector, numeric-only national input, E.164 output; `phoneValidator()` for plain inputs (register, public contact).

### Files modified
- `Backend/Gym.Application/Validation/PhoneNumberRules.cs` (new)
- `Backend/Gym.Application/Validation/PhoneValidationExtensions.cs` (new)
- `Backend/Gym.Application/Validators/CreateMemberDtoValidator.cs`
- `Backend/Gym.Application/Validators/UpdateMemberDtoValidator.cs`
- `Backend/Gym.Application/Validators/LeadDtoValidators.cs`
- `Backend/Gym.Application/Validators/RegisterGymDtoValidator.cs`
- `Backend/Gym.Application/Validators/PublicWebsiteLeadDtoValidator.cs` (new)
- `Backend/Gym.Application/Validators/PublicTrialBookingDtoValidator.cs` (new)
- `Frontend/gym-app/src/app/shared/utils/phone.util.ts` (new)
- `Frontend/gym-app/src/app/shared/validators/phone.validators.ts` (new)
- `Frontend/gym-app/src/app/shared/components/phone-field/phone-field.component.ts` (new)
- Member, lead, register, public contact forms updated

### Test results
- Valid: `+91 9876543210`, `+1 5551234567` — accepted
- Invalid characters / short numbers — rejected with clear message

---

## 3. Password Visibility Toggle

### Root cause
- All password inputs used `type="password"` with no toggle control.

### Fix
- Reusable `PasswordFieldComponent` with eye / eye-off icon on all Material-based password fields.
- Register page uses Show/Hide button for custom template styling.

### Pages updated
- Login, Change Password, Reset Password
- Create Member, Create Trainer, Create Gym Admin

### Files modified
- `Frontend/gym-app/src/app/shared/components/password-field/password-field.component.ts` (new)
- Auth pages, member/trainer/gym-admin dialogs, register component

---

## 4. Tooltip Issues

### Root cause
- Tooltips clipped by table `overflow` and low overlay stacking in some layouts.
- Long audit tooltips had no max-width / wrap rules.

### Fix
- Global tooltip styles in `styles.css` (z-index, max-width, pre-wrap).
- `.audit-tooltip` wider max-width.
- `.row-actions` set to `overflow: visible`.

### Files modified
- `Frontend/gym-app/src/styles.css`
- `Frontend/gym-app/src/app/shared/styles/saas-data-table.css`

---

## 5. Financial Dashboard — Monthly Chart Order

### Root cause
- SQL procedures `sp_GetPayrollCostTrend` and `sp_GetCommissionTrend` used `ORDER BY MonthLabel` (alphabetical: Apr before Jan).
- Payroll/commission repository did not reverse to chronological order (profit trend already reversed).

### Fix
- Migration `050_FinancialTrendOrderingFix.sql` — order by Year/Month DESC.
- `PayrollRepository` reverses results to chronological ascending.
- Frontend `sortMonthlyChronologically()` as safety net on financial dashboard charts.

### Files modified
- `Backend/Gym.Infrastructure/Persistence/Scripts/050_FinancialTrendOrderingFix.sql` (new)
- `Backend/Gym.Infrastructure/Repositories/PayrollRepository.cs`
- `Backend/Gym.Infrastructure/Repositories/PaymentRepository.cs`
- `Frontend/gym-app/src/app/shared/utils/chart.util.ts` (new)
- `Frontend/gym-app/src/app/features/gym-admin/financial/financial-dashboard.component.ts`

---

## 6. Revenue Page — Chart Layout

### Root cause
- All three charts were in one 2-column grid; the wide trend chart and two breakdown charts competed for the same row, causing uneven layout.

### Fix
- Split into two rows: **primary** (full-width trend, taller chart) and **secondary** (plan + payment method side-by-side).
- Responsive: single column on mobile.
- Monthly trend data sorted chronologically.

### Files modified
- `Frontend/gym-app/src/app/features/gym-admin/analytics/analytics-revenue.component.html`
- `Frontend/gym-app/src/app/features/gym-admin/analytics/analytics-revenue.component.css`
- `Frontend/gym-app/src/app/features/gym-admin/analytics/analytics-revenue.component.ts`
- `Frontend/gym-app/src/app/features/gym-admin/payments/revenue-dashboard.component.ts`

---

## Screenshots

Before/after screenshots were not captured in this environment. Verify visually at:
- `/auth/change-password` — mismatch validation
- `/gym-admin/leads/new` — phone country selector
- `/gym-admin/analytics/revenue` — chart layout
- `/gym-admin/financial` — month order on charts
- Gym Admin list / Audit — tooltips on action buttons

---

## Build & Test Summary

| Check | Result |
|-------|--------|
| Angular `ng build` (development) | **PASS** |
| Backend `Gym.Application` compile | **PASS** |
| Full API build | Blocked by running `Gym.API` process (file lock); restart API to pick up changes |

---

## Confirmation

All six requested issues have been addressed in code. Restart the API to apply migration `050` and backend validator changes.
