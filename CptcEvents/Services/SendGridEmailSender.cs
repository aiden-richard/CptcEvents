using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text.RegularExpressions;

namespace CptcEvents.Services;

/// <summary>
/// Email sender implementation using SendGrid for sending transactional emails.
/// Implements ASP.NET Identity's <see cref="IEmailSender"/> interface for account confirmation and other notifications.
/// </summary>
public class SendGridEmailSender : IEmailSender
{
    private readonly IConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="SendGridEmailSender"/> class.
    /// </summary>
    /// <param name="config">The application configuration containing SendGrid API key and sender settings.</param>
    public SendGridEmailSender(IConfiguration config)
    {
        _config = config;
    }

    /// <inheritdoc/>
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        string? apiKey = _config["SendGrid:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("SendGrid:ApiKey is not configured.");

        string? fromEmail = _config["SendGrid:FromEmail"];
        string fromName = _config["SendGrid:FromName"] ?? "";
        if (string.IsNullOrWhiteSpace(fromEmail))
            throw new InvalidOperationException("SendGrid:FromEmail is not configured.");

        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(fromEmail, fromName);
        var to = new EmailAddress(email);

        // Create message with both plain text and HTML
        string plainText = StripHtml(htmlMessage);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainText, htmlMessage);

        var response = await client.SendEmailAsync(msg);

        // Throw a helpful error if SendGrid returns failure so callers (Identity) can surface it or be logged.
        if ((int)response.StatusCode >= 400)
        {
            string body = await response.Body.ReadAsStringAsync();
            throw new InvalidOperationException($"SendGrid failed to send email. StatusCode={(int)response.StatusCode}. Body={body}");
        }
    }

    /// <summary>
    /// Removes HTML tags from a string to create plain text version.
    /// </summary>
    /// <param name="input">The HTML string to clean.</param>
    /// <returns>The input string with all HTML tags removed.</returns>
    private static string StripHtml(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
        return Regex.Replace(input, "<.*?>", string.Empty);
    }
}
