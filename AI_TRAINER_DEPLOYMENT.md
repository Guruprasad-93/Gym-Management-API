# AI Trainer & Recommendation Engine — Deployment Guide

## 1. Run Migration

```bash
cd Backend/Gym.API
dotnet run -- migrate
```

Or apply `032_AiTrainerRecommendationModule.sql` manually against your SQL Server database.

## 2. Configuration

Add to User Secrets or environment variables:

| Setting | Description |
|---------|-------------|
| `AI__Enabled` | `true` to enable AI text generation |
| `AI__Provider` | `Mock` (dev) or `OpenAI` (production) |
| `AI__ApiKey` | OpenAI API key |
| `AI__Model` | e.g. `gpt-4o-mini` |
| `AI__BackgroundJobIntervalHours` | Default 24 |
| `AI__MaxMembersPerRun` | Members processed per gym per job run |

Example `appsettings.Production.json`:

```json
{
  "AI": {
    "Enabled": true,
    "Provider": "OpenAI",
    "ApiKey": "sk-...",
    "Model": "gpt-4o-mini",
    "BackgroundJobIntervalHours": 24,
    "MaxMembersPerRun": 500
  }
}
```

## 3. OpenAI Setup

1. Create an OpenAI project and API key at https://platform.openai.com
2. Set billing limits appropriate for your member volume
3. Use `gpt-4o-mini` for cost-effective recommendations
4. Monitor token usage via `/api/ai/analytics`

## 4. Mock Provider (Development)

With `Provider: "Mock"` and `Enabled: false`, the system still calculates risk scores and lead scores using rule-based logic. Recommendations are generated via `MockAiProvider` when `Enabled: true`.

## 5. Permissions & Seeding

Re-run database seed or restart with `Database:RunSeedOnStartup: true` to assign:

- GymAdmin: `VIEW_AI_INSIGHTS`, `VIEW_AI_RECOMMENDATIONS`
- Trainer: `VIEW_AI_RECOMMENDATIONS`

Demo users must re-login after seed for JWT permission updates.

## 6. Background Job

`AiRecommendationBackgroundJob` runs daily and:

- Calculates member risk scores for all active members
- Generates AI recommendations (when enabled)
- Creates business insights
- Sends high churn / renewal risk alerts (WhatsApp + push)

## 7. Frontend Routes

- Gym Admin: `/gym-admin/ai`, `/gym-admin/ai/insights`
- Trainer: `/trainer/ai-recommendations`

## 8. Production Checklist

- [ ] Migration 032 applied
- [ ] OpenAI API key configured (or Mock for staging)
- [ ] Firebase configured for push alerts (see MOBILE_PUSH_DEPLOYMENT.md)
- [ ] WhatsApp provider configured for churn alerts
- [ ] Permissions seeded for GymAdmin and Trainer roles
- [ ] Background job interval reviewed for member volume
- [ ] Integration tests passing: `dotnet test --filter AiRecommendationTests`

## 9. Monitoring

- Review `/api/ai/analytics` for acceptance rate and token usage
- Watch application logs for `AiRecommendationBackgroundJob` completion
- Track high-risk member count on AI dashboard
