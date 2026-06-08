# AI Trainer & Recommendation Engine — Implementation Report

## Overview

Module `032_AiTrainerRecommendationModule` adds AI-powered workout/diet recommendations, churn and renewal prediction, member health scoring, CRM lead scoring, and business insights.

## Database

**Migration:** `Backend/Gym.Infrastructure/Persistence/Scripts/032_AiTrainerRecommendationModule.sql`

| Table | Purpose |
|-------|---------|
| `AiRecommendations` | Member workout/diet recommendations with confidence and acceptance tracking |
| `AiInsights` | Gym-level business insights |
| `MemberRiskScores` | Churn risk, attendance risk, renewal probability, health score |
| `AiGenerationLogs` | Provider usage and token tracking |

## Backend

| Layer | Files |
|-------|-------|
| DTOs | `Gym.Application/DTOs/Ai/AiDtos.cs` |
| Constants | `AiConstants.cs`, `Permissions.cs`, `PushNotificationTypes.cs` |
| Repository | `AiRecommendationRepository.cs` |
| Service | `AiRecommendationService.cs` |
| Providers | `MockAiProvider.cs`, `OpenAiProvider.cs` |
| Controller | `AiController.cs` |
| Background job | `AiRecommendationBackgroundJob.cs` (daily) |

## API Endpoints

| Method | Route | Permission |
|--------|-------|------------|
| GET | `/api/ai/dashboard` | VIEW_AI_INSIGHTS |
| GET | `/api/ai/recommendations` | VIEW_AI_RECOMMENDATIONS |
| PUT | `/api/ai/recommendations/accept` | VIEW_AI_RECOMMENDATIONS |
| GET | `/api/ai/member-risk` | VIEW_AI_INSIGHTS |
| GET | `/api/ai/lead-scoring` | VIEW_AI_INSIGHTS |
| GET | `/api/ai/business-insights` | VIEW_AI_INSIGHTS |
| GET | `/api/ai/analytics` | VIEW_AI_INSIGHTS |

## Permissions

- **GymAdmin:** VIEW_AI_INSIGHTS, VIEW_AI_RECOMMENDATIONS
- **Trainer:** VIEW_AI_RECOMMENDATIONS

## Frontend

| Route | Component |
|-------|-----------|
| `/gym-admin/ai` | AI dashboard with Chart.js churn/renewal charts |
| `/gym-admin/ai/insights` | Business insights + lead scoring |
| `/trainer/ai-recommendations` | Trainer recommendation cards |

## Notifications

High churn risk and low renewal probability trigger WhatsApp + push notifications (`ChurnRiskAlert`, `RenewalRiskAlert`).

## Analytics

Tracks recommendation acceptance rate, churn predictions, token usage, and generation counts via `sp_GetAiAnalytics`.

## Tests

`AiRecommendationTests.cs` — 8 tests covering dashboard, recommendations, risk, lead scoring, tenant auth, and risk calculation logic.

## Configuration

```json
"AI": {
  "Enabled": false,
  "Provider": "Mock",
  "ApiKey": "",
  "Model": "gpt-4o-mini",
  "BackgroundJobIntervalHours": 24,
  "MaxMembersPerRun": 200
}
```

Set `Provider` to `"OpenAI"` and `Enabled: true` for production OpenAI integration.
