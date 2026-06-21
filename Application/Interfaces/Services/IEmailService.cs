using Application.DTOs.Email;

namespace Application.Interfaces.Services;

public interface IEmailService
{
    Task SendAsync(EmailMessageDto message);
}
