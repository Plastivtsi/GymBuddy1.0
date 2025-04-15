using BLL.Models;
using BLL.Models.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Xunit;

namespace EmailServiceTests
{
    public class EmailServiceTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<EmailService>> _loggerMock;
        private readonly EmailService _emailService;

        public EmailServiceTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<EmailService>>();
            _emailService = new EmailService(_configurationMock.Object, _loggerMock.Object);
        }


        [Fact]
        public async Task SendEmailAsync_MissingSmtpHost_ThrowsInvalidOperationException()
        {
            // Arrange
            _configurationMock.Setup(c => c["Email:SmtpHost"]).Returns((string)null);
            _configurationMock.Setup(c => c["Email:SmtpPort"]).Returns("587");
            _configurationMock.Setup(c => c["Email:SmtpUsername"]).Returns("user@example.com");
            _configurationMock.Setup(c => c["Email:SmtpPassword"]).Returns("password");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _emailService.SendEmailAsync("test@example.com", "Test Subject", "Test Message"));
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Неправильні SMTP налаштування в конфігурації")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once());
        }

        [Fact]
        public async Task SendEmailAsync_InvalidSmtpPort_ThrowsInvalidOperationException()
        {
            // Arrange
            _configurationMock.Setup(c => c["Email:SmtpHost"]).Returns("smtp.example.com");
            _configurationMock.Setup(c => c["Email:SmtpPort"]).Returns("invalid");
            _configurationMock.Setup(c => c["Email:SmtpUsername"]).Returns("user@example.com");
            _configurationMock.Setup(c => c["Email:SmtpPassword"]).Returns("password");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _emailService.SendEmailAsync("test@example.com", "Test Subject", "Test Message"));
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Неправильний формат SMTP порту")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once());
        }

        [Fact]
        public async Task SendEmailAsync_MissingSmtpUsername_ThrowsInvalidOperationException()
        {
            // Arrange
            _configurationMock.Setup(c => c["Email:SmtpHost"]).Returns("smtp.example.com");
            _configurationMock.Setup(c => c["Email:SmtpPort"]).Returns("587");
            _configurationMock.Setup(c => c["Email:SmtpUsername"]).Returns((string)null);
            _configurationMock.Setup(c => c["Email:SmtpPassword"]).Returns("password");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _emailService.SendEmailAsync("test@example.com", "Test Subject", "Test Message"));
        }

        [Fact]
        public async Task SendEmailAsync_MissingSmtpPassword_ThrowsInvalidOperationException()
        {
            // Arrange
            _configurationMock.Setup(c => c["Email:SmtpHost"]).Returns("smtp.example.com");
            _configurationMock.Setup(c => c["Email:SmtpPort"]).Returns("587");
            _configurationMock.Setup(c => c["Email:SmtpUsername"]).Returns("user@example.com");
            _configurationMock.Setup(c => c["Email:SmtpPassword"]).Returns((string)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _emailService.SendEmailAsync("test@example.com", "Test Subject", "Test Message"));
        }

        [Fact]
        public async Task SendEmailAsync_SmtpException_LogsErrorAndThrows()
        {
            // Arrange
            _configurationMock.Setup(c => c["Email:SmtpHost"]).Returns("smtp.example.com");
            _configurationMock.Setup(c => c["Email:SmtpPort"]).Returns("587");
            _configurationMock.Setup(c => c["Email:SmtpUsername"]).Returns("user@example.com");
            _configurationMock.Setup(c => c["Email:SmtpPassword"]).Returns("password");

            // Simulate SmtpException (would require mocking SmtpClient, but for simplicity, assume it throws)
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _emailService.SendEmailAsync("test@example.com", "Test Subject", "Test Message"));
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Помилка SMTP при надсиланні email")),
                    It.IsAny<SmtpException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once());
        }
    }
}