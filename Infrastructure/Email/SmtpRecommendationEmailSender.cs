using System.Globalization;
using System.Text;
using ElectricityPlanner.Application.DTOs;
using ElectricityPlanner.Application.Options;
using ElectricityPlanner.Application.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ElectricityPlanner.Infrastructure.Email;

public class SmtpRecommendationEmailSender : IRecommendationEmailSender
{
    private readonly SmtpOptions _smtp;

    public SmtpRecommendationEmailSender(IOptions<SmtpOptions> smtp)
    {
        _smtp = smtp.Value;
    }

    public async Task SendRecommendationAsync(
        string toEmail,
        decimal kwh,
        string taxGroupName,
        RecommendationResponse response,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_smtp.Host))
            throw new InvalidOperationException(
                "SMTP is not configured. Set Smtp:Host, Smtp:Username, Smtp:Password (and From) via User Secrets or environment variables.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            string.IsNullOrWhiteSpace(_smtp.FromName) ? "Electricity Planner" : _smtp.FromName,
            string.IsNullOrWhiteSpace(_smtp.FromEmail) ? throw new InvalidOperationException("Smtp:FromEmail is required.") : _smtp.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = $"Electricity plan recommendation ({response.Recommended.PlanName})";

        var body = BuildBody(kwh, taxGroupName, response);
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        var socketOptions = _smtp.Port == 465
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        await client.ConnectAsync(_smtp.Host, _smtp.Port, socketOptions, cancellationToken);

        if (!string.IsNullOrEmpty(_smtp.Username))
            await client.AuthenticateAsync(_smtp.Username, _smtp.Password, cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    private static string F(decimal d) => d.ToString("F2", CultureInfo.InvariantCulture);

    private static string BuildBody(decimal kwh, string taxGroupName, RecommendationResponse r)
    {
        var sb = new StringBuilder();
        sb.Append("<html><body style=\"font-family:sans-serif\">");
        sb.Append("<h2>Your electricity plan recommendation</h2>");
        sb.Append("<p><strong>Consumption:</strong> ").Append(F(kwh)).Append(" kWh / month<br/>");
        sb.Append("<strong>Tax group:</strong> ").Append(System.Net.WebUtility.HtmlEncode(taxGroupName)).Append("</p>");
        sb.Append("<h3>Recommended: ").Append(System.Net.WebUtility.HtmlEncode(r.Recommended.PlanName)).Append("</h3>");
        sb.Append("<p><strong>Grand total:</strong> ").Append(F(r.Recommended.Costs.GrandTotal)).Append("</p>");
        sb.Append("<h4>All plans</h4><table border=\"1\" cellpadding=\"6\" cellspacing=\"0\"><tr><th>Plan</th><th>Energy</th><th>After discount</th><th>Eco</th><th>VAT</th><th>Total</th></tr>");
        foreach (var p in r.AllPlans)
        {
            sb.Append("<tr>");
            sb.Append("<td>").Append(System.Net.WebUtility.HtmlEncode(p.PlanName)).Append("</td>");
            sb.Append("<td>").Append(F(p.Costs.EnergySubtotal)).Append("</td>");
            sb.Append("<td>").Append(F(p.Costs.EnergyAfterDiscount)).Append("</td>");
            sb.Append("<td>").Append(F(p.Costs.EcoTaxTotal)).Append("</td>");
            sb.Append("<td>").Append(F(p.Costs.VatAmount)).Append("</td>");
            sb.Append("<td><strong>").Append(F(p.Costs.GrandTotal)).Append("</strong></td>");
            sb.Append("</tr>");
        }
        sb.Append("</table><p><small>Sent by Electricity Planner API.</small></p></body></html>");
        return sb.ToString();
    }
}
