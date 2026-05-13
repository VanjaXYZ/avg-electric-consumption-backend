using ElectricityPlanner.Application.DTOs;

namespace ElectricityPlanner.Application.Services;

public interface IRecommendationEmailSender
{
    Task SendRecommendationAsync(
        string toEmail,
        decimal kwh,
        string taxGroupName,
        RecommendationResponse response,
        CancellationToken cancellationToken = default);
}
