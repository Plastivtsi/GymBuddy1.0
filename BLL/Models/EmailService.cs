using BLL.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using Microsoft.Extensions.Logging;

namespace BLL.Models
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }


        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"];
                var smtpPortString = _configuration["Email:SmtpPort"];
                var smtpUsername = _configuration["Email:SmtpUsername"];
                var smtpPassword = _configuration["Email:SmtpPassword"];
                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpPortString) ||
                    string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogError("Неправильні SMTP налаштування в конфігурації");
                    throw new InvalidOperationException("SMTP налаштування відсутні або неповні");
                }

                if (!int.TryParse(smtpPortString, out var smtpPort))
                {
                    _logger.LogError("Неправильний формат SMTP порту: {SmtpPort}", smtpPortString);
                    throw new InvalidOperationException("Неправильний формат SMTP порту");
                }

                _logger.LogInformation("Надсилання email до {Email} з темою {Subject}", email, subject);

                using var client = new System.Net.Mail.SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = true
                };

                var mailMessage = new System.Net.Mail.MailMessage
                {
                    From = new System.Net.Mail.MailAddress(smtpUsername),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email успішно надіслано до {Email}", email);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "Помилка SMTP при надсиланні email до {Email}: {Message}", email, ex.Message);
                throw new InvalidOperationException("Помилка SMTP при надсиланні email", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при надсиланні email до {Email}: {Message}", email, ex.Message);
                throw new InvalidOperationException("Не вдалося надіслати email", ex);
            }
        }
    }
}
