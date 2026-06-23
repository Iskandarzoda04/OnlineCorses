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
        using var client = new SmtpClient(settings.Host, settings.Port)
        {
            EnableSsl = settings.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(settings.Username))
            client.Credentials = new NetworkCredential(settings.Username, settings.Password);

        using var mail = new MailMessage
        {
            From = new MailAddress(settings.From, settings.DisplayName),
            Subject = message.Subject,
            Body = message.Body,
            IsBodyHtml = message.IsHtml
        };
        mail.To.Add(message.To);

        await client.SendMailAsync(mail);
        logger.LogInformation("Email sent to {Email}", message.To);
    }
}
