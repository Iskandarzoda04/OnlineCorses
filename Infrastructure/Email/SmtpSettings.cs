namespace Infrastructure.Email;

public class SmtpSettings
{
    public string Host { get; set; } ="";
    public string SmtpServer { get; set; } = "";
    public int Port { get; set; } = 25;
    public string Username { get; set; } ="";
    public string Email { get; set; } = "";
    public string Password { get; set; } ="";
    public string From { get; set; } = "";
    public string DisplayName { get; set; } ="";
    public bool EnableSsl { get; set; }
}
