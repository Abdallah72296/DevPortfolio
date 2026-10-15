using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using MyPortfolio.Application.Common.Interfaces;

namespace MyPortfolio.Infrastructure.Authentication;

public sealed class ConfiguredEmailSender(IConfiguration configuration, IHttpClientFactory httpClientFactory) : IEmailSender
{
    public async Task SendAsync(string recipient, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var resendApiKey = configuration["Email:Resend:ApiKey"];
        if (!string.IsNullOrWhiteSpace(resendApiKey))
        {
            await SendWithResendAsync(resendApiKey, recipient, subject, htmlBody, cancellationToken);
            return;
        }

        await SendWithSmtpAsync(recipient, subject, htmlBody, cancellationToken);
    }

    private async Task SendWithResendAsync(string apiKey, string recipient, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        var from = configuration["Email:Resend:From"];
        if (string.IsNullOrWhiteSpace(from))
            throw new InvalidOperationException("Resend email delivery is not configured.");

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(new
        {
            from,
            to = new[] { recipient },
            subject,
            html = htmlBody
        }), Encoding.UTF8, MediaTypeNames.Application.Json);

        using var response = await httpClientFactory.CreateClient(nameof(ConfiguredEmailSender))
            .SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Resend rejected email delivery with HTTP {(int)response.StatusCode}.");
    }

    private async Task SendWithSmtpAsync(string recipient, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        var host = configuration["Email:Smtp:Host"];
        var from = configuration["Email:Smtp:From"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
            throw new InvalidOperationException("Email delivery is not configured.");

        var port = configuration.GetValue<int?>("Email:Smtp:Port") ?? 587;
        var username = configuration["Email:Smtp:Username"];
        var password = configuration["Email:Smtp:Password"];
        var enableSsl = configuration.GetValue<bool?>("Email:Smtp:EnableSsl") ?? true;

        using var message = new MailMessage(from, recipient, subject, htmlBody) { IsBodyHtml = true };
        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 15000
        };
        if (!string.IsNullOrWhiteSpace(username))
            client.Credentials = new NetworkCredential(username, password);

        await client.SendMailAsync(message, cancellationToken);
    }
}
