using AuthService.Application.Interfaces;

namespace AuthService.Application.Services;

public class FakeEmailService : IEmailService
{
    public Task SendEmailAsync(string toEmail, string subject, string body)
    {
        Console.WriteLine("========== FAKE EMAIL ==========");
        Console.WriteLine($"To      : {toEmail}");
        Console.WriteLine($"Subject : {subject}");
        Console.WriteLine($"Body    : {body}");
        Console.WriteLine("================================");
        return Task.CompletedTask;
    }
}