using Gym.Application.Constants;
using Gym.Application.DTOs.Ai;
using Gym.Application.Interfaces;

namespace Gym.Infrastructure.Services.Ai;

public class MockAiProvider : IAiProvider
{
    public Task<AiProviderResult> GenerateMemberRecommendationsAsync(MemberAiAnalysisContextDto context, CancellationToken cancellationToken = default)
    {
        var recommendations = new List<AiGeneratedRecommendation>();
        var attendanceDrop = context.AttendancePrev30Days > 0
            && context.AttendanceLast30Days < context.AttendancePrev30Days * 0.7;
        var workoutLow = context.AvgWorkoutCompletion is null or < 50;
        var dietLow = context.AvgDietCompliance is null or < 50;
        var goal = context.PrimaryGoal ?? "General Fitness";

        if (workoutLow || attendanceDrop)
        {
            recommendations.Add(new AiGeneratedRecommendation
            {
                RecommendationType = AiRecommendationTypes.WorkoutAdjustment,
                RecommendationText = $"{context.FullName}: Based on your {goal} goal and recent activity, increase weekly sessions from {Math.Max(2, context.AttendanceLast30Days / 4)} to {Math.Max(3, context.AttendanceLast30Days / 3)} with compound movements and 10% progressive overload.",
                ConfidenceScore = attendanceDrop ? 88 : 75
            });
        }

        recommendations.Add(new AiGeneratedRecommendation
        {
            RecommendationType = AiRecommendationTypes.WorkoutIntensity,
            RecommendationText = workoutLow
                ? "Start with moderate intensity (RPE 6-7) for 2 weeks, then increase to RPE 7-8 as completion improves."
                : "Maintain current intensity and add one high-intensity interval session per week.",
            ConfidenceScore = workoutLow ? 82 : 70
        });

        recommendations.Add(new AiGeneratedRecommendation
        {
            RecommendationType = AiRecommendationTypes.WeeklyWorkoutPlan,
            RecommendationText = $"Suggested weekly split: 3 strength days, 2 cardio days, 2 active recovery days aligned with your {goal} goal.",
            ConfidenceScore = 78
        });

        var weightDelta = context.LatestWeight.HasValue && context.Weight30DaysAgo.HasValue
            ? context.LatestWeight.Value - context.Weight30DaysAgo.Value
            : 0;
        var calorieTarget = context.Weight.HasValue ? (int)(context.Weight.Value * 24) : 2000;

        recommendations.Add(new AiGeneratedRecommendation
        {
            RecommendationType = AiRecommendationTypes.DietCalorie,
            RecommendationText = weightDelta > 1
                ? $"Calorie target: {calorieTarget - 300} kcal/day to support fat loss while preserving muscle."
                : weightDelta < -1
                    ? $"Calorie target: {calorieTarget + 300} kcal/day to support healthy weight gain."
                    : $"Maintain approximately {calorieTarget} kcal/day based on current weight trend.",
            ConfidenceScore = 80
        });

        if (dietLow)
        {
            recommendations.Add(new AiGeneratedRecommendation
            {
                RecommendationType = AiRecommendationTypes.DietMealAdjustment,
                RecommendationText = "Increase meal prep consistency: plan 5 weekday meals in advance and track compliance daily.",
                ConfidenceScore = 76
            });
        }

        recommendations.Add(new AiGeneratedRecommendation
        {
            RecommendationType = AiRecommendationTypes.ProteinTarget,
            RecommendationText = context.Weight.HasValue
                ? $"Protein target: {Math.Round(context.Weight.Value * 1.8m, 0)}g/day ({Math.Round(context.Weight.Value * 0.8m, 0)}g per main meal)."
                : "Protein target: 1.6-2.0g per kg body weight spread across 4 meals.",
            ConfidenceScore = 85
        });

        return Task.FromResult(new AiProviderResult
        {
            Recommendations = recommendations,
            TokensUsed = recommendations.Sum(r => r.RecommendationText.Length / 4)
        });
    }

    public Task<AiProviderResult> GenerateBusinessInsightsAsync(
        BusinessAiContextDto context,
        IReadOnlyList<BranchAttendanceAiDto> branches,
        CancellationToken cancellationToken = default)
    {
        var insights = new List<AiGeneratedInsight>();

        if (context.RevenuePrevMonth > 0)
        {
            var change = (context.RevenueThisMonth - context.RevenuePrevMonth) / context.RevenuePrevMonth * 100;
            if (change <= -5)
            {
                insights.Add(new AiGeneratedInsight
                {
                    InsightType = AiInsightTypes.RevenueDecline,
                    InsightText = $"Revenue down {Math.Abs(Math.Round(change, 1))}% this month compared to last month.",
                    Severity = change <= -12 ? AiInsightSeverities.Critical : AiInsightSeverities.Warning
                });
            }
        }

        if (context.ActiveMembersPrev30Days > 0
            && context.ActiveMembersLast30Days < context.ActiveMembersPrev30Days * 0.85)
        {
            insights.Add(new AiGeneratedInsight
            {
                InsightType = AiInsightTypes.AttendanceDecline,
                InsightText = "Gym-wide attendance declined more than 15% in the last 30 days.",
                Severity = AiInsightSeverities.Warning
            });
        }

        foreach (var branch in branches.Where(b => b.AttendancePrev30Days > 0 && b.AttendanceLast30Days < b.AttendancePrev30Days * 0.8))
        {
            insights.Add(new AiGeneratedInsight
            {
                InsightType = AiInsightTypes.BranchAttendanceDecline,
                InsightText = $"Attendance declining in {branch.BranchName} ({branch.AttendanceLast30Days} vs {branch.AttendancePrev30Days} visits).",
                Severity = AiInsightSeverities.Warning
            });
        }

        if (context.ActiveTrainers > 0)
        {
            var utilization = context.MembersWithTrainer / (decimal)context.ActiveTrainers;
            if (utilization < 8)
            {
                insights.Add(new AiGeneratedInsight
                {
                    InsightType = AiInsightTypes.TrainerUtilization,
                    InsightText = $"Trainer utilization below target ({Math.Round(utilization, 1)} members per trainer). Consider rebalancing assignments.",
                    Severity = AiInsightSeverities.Info
                });
            }
        }

        if (context.HighChurnMembers >= 5)
        {
            insights.Add(new AiGeneratedInsight
            {
                InsightType = AiInsightTypes.ChurnRiskIncrease,
                InsightText = $"High churn risk members increasing: {context.HighChurnMembers} members flagged as high risk.",
                Severity = AiInsightSeverities.Critical
            });
        }

        return Task.FromResult(new AiProviderResult
        {
            Insights = insights,
            TokensUsed = insights.Sum(i => i.InsightText.Length / 4)
        });
    }
}
