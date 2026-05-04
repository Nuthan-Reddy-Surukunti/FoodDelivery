using OrderService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace OrderService.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var smtpServer = emailSettings["SmtpServer"] ?? "localhost";
            var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "1025");
            var senderEmail = emailSettings["SenderEmail"] ?? "noreply@quickbite.com";
            var senderPassword = emailSettings["SenderPassword"] ?? "";
            var senderName = emailSettings["SenderName"] ?? "QuickBite App";

            using (var client = new SmtpClient(smtpServer, smtpPort))
            {
                // Mailhog usually doesn't require SSL or credentials by default if running locally
                if (!string.IsNullOrEmpty(senderPassword))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(senderEmail, senderPassword);
                }

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);
                await client.SendMailAsync(mailMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email to {ToEmail}.", toEmail);
            // Don't throw here to avoid crashing the order flow if email fails
        }
    }
}
