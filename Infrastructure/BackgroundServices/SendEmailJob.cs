using System.Net.Mail;
using Application.DTOs.Email;
using Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundServices;

public class SendEmailJob(IEmailService emailService, ILogger<SendEmailJob> logger)
{
    public async Task ExecuteAsync()
    {
        var recipients = new[]
        {
            "hadyatulloiskandarzoda@gmail.com"
        };

        const string subject = "Test Email from Hangfire";
        const string body = "This is a test email sent from Hangfire";

        foreach (var recipient in recipients)
        {
            try
            {
                await emailService.SendAsync(new EmailMessageDto
                {
                    To = recipient,
                    Subject = subject,
                    Body = body,
                    IsHtml = false
                });
                logger.LogInformation("Test email sent to {Recipient}.", recipient);
            }
            catch (SmtpException ex)
            {
                logger.LogWarning(ex, "Test email was not sent to {Recipient}. Check EmailSettings and Gmail App Password.", recipient);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "Test email was not sent to {Recipient}. EmailSettings are not configured correctly.", recipient);
            }
            catch (FormatException ex)
            {
                logger.LogWarning(ex, "Test email was not sent to {Recipient}. Email address format is invalid.", recipient);
            }
        }
    }
}
