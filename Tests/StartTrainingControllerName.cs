using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using System;
using DAL.Models;
using BLL.Interfaces;
using PL.Controllers;
using Microsoft.AspNetCore.Http;

public class StartTrainingControllerTests
{
    private readonly Mock<ITrainingService> _trainingServiceMock;
    private readonly Mock<ILogger<StartTrainingController>> _loggerMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly StartTrainingController _controller;

    public StartTrainingControllerTests()
    {
        _trainingServiceMock = new Mock<ITrainingService>();
        _loggerMock = new Mock<ILogger<StartTrainingController>>();

        // Мокування UserManager
        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

        _controller = new StartTrainingController(
            _trainingServiceMock.Object,
            _loggerMock.Object,
            _userManagerMock.Object);

        // Налаштування контролера для імітації автентифікованого користувача
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "1") // Use an int as a string
                }, "mock"))
            }
        };
    }

    [Fact]
    public async Task Index_TrainingExists_ReturnsViewWithTraining()
    {
        // Arrange
        int trainingId = 1;
        var training = new Training { Id = trainingId, Name = "Test Training" };
        _trainingServiceMock.Setup(s => s.SearchTrainingsAsync(null, null, null))
            .ReturnsAsync(new List<Training> { training });

        // Act
        var result = await _controller.Index(trainingId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("~/Views/Training/StartTraining.cshtml", viewResult.ViewName);
        Assert.Equal(training, viewResult.Model);
    }

    [Fact]
    public async Task Index_TrainingNotFound_ReturnsNotFound()
    {
        // Arrange
        int trainingId = 1;
        _trainingServiceMock.Setup(s => s.SearchTrainingsAsync(null, null, null))
            .ReturnsAsync(new List<Training>());

        // Act
        var result = await _controller.Index(trainingId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Training with ID {trainingId} not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once());
    }

    [Fact]
    public async Task Complete_ValidData_RedirectsToTrainingIndex()
    {
        // Arrange
        int trainingId = 1;
        var user = new User { Id = 1 }; // Use an int for User.Id
        var originalTraining = new Training { Id = trainingId, Name = "Original Training", Template = true };
        var updatedTraining = new Training { Id = trainingId, Exercises = new List<Exercise> { new Exercise { Name = "Push-up" } } };

        _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user); // Fixed: getUserAsync -> GetUserAsync
        _trainingServiceMock.Setup(s => s.SearchTrainingsAsync(null, null, null))
            .ReturnsAsync(new List<Training> { originalTraining });
        _trainingServiceMock.Setup(s => s.CreateTrainingAsync(It.IsAny<Training>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Complete(trainingId, updatedTraining);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Training", redirectResult.ControllerName);
        _trainingServiceMock.Verify(s => s.CreateTrainingAsync(It.IsAny<Training>()), Times.Once());
    }

    [Fact]
    public async Task Complete_IdMismatch_ReturnsNotFound()
    {
        // Arrange
        int trainingId = 1;
        var updatedTraining = new Training { Id = 2 };

        // Act
        var result = await _controller.Complete(trainingId, updatedTraining);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Complete_UserNotAuthenticated_RedirectsToLogin()
    {
        // Arrange
        int trainingId = 1;
        var updatedTraining = new Training { Id = trainingId };
        _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((User)null);

        // Act
        var result = await _controller.Complete(trainingId, updatedTraining);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirectResult.ActionName);
        Assert.Equal("Account", redirectResult.ControllerName);
    }

    [Fact]
    public async Task Complete_TrainingNotFound_ReturnsNotFound()
    {
        // Arrange
        int trainingId = 1;
        var user = new User { Id = 1 }; // Use an int for User.Id
        var updatedTraining = new Training { Id = trainingId };

        _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _trainingServiceMock.Setup(s => s.SearchTrainingsAsync(null, null, null))
            .ReturnsAsync(new List<Training>());

        // Act
        var result = await _controller.Complete(trainingId, updatedTraining);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Complete_Exception_ReturnsViewWithError()
    {
        // Arrange
        int trainingId = 1;
        var user = new User { Id = 1 }; // Use an int for User.Id
        var originalTraining = new Training { Id = trainingId };
        var updatedTraining = new Training { Id = trainingId };

        _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _trainingServiceMock.Setup(s => s.SearchTrainingsAsync(null, null, null))
            .ReturnsAsync(new List<Training> { originalTraining });
        _trainingServiceMock.Setup(s => s.CreateTrainingAsync(It.IsAny<Training>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.Complete(trainingId, updatedTraining);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("~/Views/Training/StartTraining.cshtml", viewResult.ViewName);
        Assert.False(_controller.ModelState.IsValid);
        Assert.Equal(updatedTraining, viewResult.Model);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once());
    }
}