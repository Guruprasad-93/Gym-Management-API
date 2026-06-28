# Payments

## Module Overview
Payment recording, Razorpay online checkout, invoices, refunds, revenue dashboards.

## Navigation
| Page | Route |
|------|-------|
| Payments list | `/gym-admin/payments` |
| Revenue dashboard | `/gym-admin/revenue` |
| Member checkout | `/member/checkout` |

## Buttons
- Record payment (dialog)
- Download invoice
- Refund (permission-gated)
- Razorpay pay (member checkout)
- Revenue charts / export

## Tables
`Payments`, `Invoices`, `SaasSubscriptionPayments` (platform)

## APIs
`PaymentsController` — `/api/payments`, revenue sub-routes, Razorpay order/verify

## Business Rules
- Razorpay mock gateway in dev when configured
- Member checkout requires active payable membership

## Components
`payment-list`, `payment-form-dialog`, `revenue-dashboard`, `member-checkout`

## Roles
**Gym Admin** — view/create/refund; **Member** — `INITIATE_ONLINE_PAYMENT`

## SaaS Feature
`PAYMENTS`
