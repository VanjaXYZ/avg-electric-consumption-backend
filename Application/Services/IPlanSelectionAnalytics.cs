using ElectricityPlanner.Application.DTOs;

namespace ElectricityPlanner.Application.Services;

public interface IPlanSelectionAnalytics
{
    Task RecordRecommendationAsync(
        RecommendationRequest request,
        int taxGroupId,
        PlanComparisonDto recommended,
        CancellationToken cancellationToken = default);
}