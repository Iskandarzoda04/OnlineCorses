using System.Net;
using System.Net.Mail;
using Application.DTOs.Email;
using Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Email;

public class EmailService(IOptions<SmtpSettings> options, ILogger<EmailService> logger) : IEmailService
{
    public async Task SendAsync(EmailMessageDto message)
    {
        var settings = options.Value;
        var host = string.IsNullOrWhiteSpace(settings.Host) ? settings.SmtpServer : settings.Host;
        var username = string.IsNullOrWhiteSpace(settings.Username) ? settings.Email : settings.Username;
        var from = string.IsNullOrWhiteSpace(settings.From) ? settings.Email : settings.From;

        using var client = new SmtpClient(host, settings.Port)
        {
            EnableSsl = settings.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(username))
            client.Credentials = new NetworkCredential(username, settings.Password);

        using var mail = new MailMessage
        {
            From = new MailAddress(from, settings.DisplayName),
            Subject = message.Subject,
            Body = message.Body,
            IsBodyHtml = message.IsHtml
        };
        mail.To.Add(message.To);

        await client.SendMailAsync(mail);
        logger.LogInformation("Email sent to {Email}", message.To);
    }
}
