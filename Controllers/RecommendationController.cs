using ElectricityPlanner.Application.DTOs;
using ElectricityPlanner.Application.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace ElectricityPlanner.Api.Controllers;

[ApiController]
[Route("recommendation")]
public class RecommendationController : ControllerBase
{
    private readonly IPlanSelectionAnalytics _analytics;
    private readonly RecommendationService _recommendation;
    private readonly IRecommendationEmailSender _email;

    public RecommendationController(
        IPlanSelectionAnalytics analytics,
        RecommendationService recommendation,
        IRecommendationEmailSender email)
    {
        _analytics = analytics;
        _recommendation = recommendation;
        _email = email;
    }

    [HttpPost]
    [ProducesResponseType(typeof(RecommendationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RecommendationResponse>> Recommend([FromBody] RecommendationRequest request, CancellationToken cancellationToken)
    {
        var (response, taxGroupId, error) = await _recommendation.BuildAsync(request.Kwh, request.TaxGroup, cancellationToken);
        if (error is not null) return BadRequest(new { error });

        await _analytics.RecordRecommendationAsync(request, taxGroupId!.Value, response!.Recommended, cancellationToken);

        return Ok(response);
    }

    [HttpPost("email")]
    [ProducesResponseType(typeof(RecommendationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<RecommendationResponse>> RecommendAndEmail(
        [FromBody] RecommendationEmailRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ToEmail)) return BadRequest(new { error = "ToEmail is required." });

        try
        {
            _ = MailboxAddress.Parse(request.ToEmail.Trim());
        }
        catch (ParseException)
        {
            return BadRequest(new { error = "ToEmail is not a valid email address." });
        }

        var (response, taxGroupId, error) = await _recommendation.BuildAsync(request.Kwh, request.TaxGroup, cancellationToken);
        if (error is not null) return BadRequest(new { error });

        try
        {
            await _email.SendRecommendationAsync(
                request.ToEmail.Trim(),
                request.Kwh,
                request.TaxGroup.Trim(),
                response!,
                cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (AuthenticationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "Failed to send email.", detail = ex.Message });
        }
        catch (SmtpCommandException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "Failed to send email.", detail = ex.Message });
        }
        catch (SmtpProtocolException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "Failed to send email.", detail = ex.Message });
        }

        await _analytics.RecordRecommendationAsync(
            new RecommendationRequest { Kwh = request.Kwh, TaxGroup = request.TaxGroup },
            taxGroupId!.Value,
            response!.Recommended,
            cancellationToken);

        return Ok(response);
    }
}
