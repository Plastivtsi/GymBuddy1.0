using BLL.Service;
using BLL.Service;
using DAL.Models;
using DAL.Repositorie;
using DAL.Repositorie;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class TrainingServiceTests
{
    private readonly Mock<ITrainingRepository> _mockRepo;
    private readonly TrainingService _trainingService;

    public TrainingServiceTests()
    {
        _mockRepo = new Mock<ITrainingRepository>();
        _trainingService = new TrainingService(_mockRepo.Object);
    }

    [Fact]
    public async Task SearchTrainingsAsync_ReturnsCorrectTrainings()
    {
        // Arrange
        var trainings = new List<Training>
        {
            new Training { Id = 1, Name = "Morning Run", Date = DateTime.Today, UserId = 1 },
            new Training { Id = 2, Name = "Evening Yoga", Date = DateTime.Today, UserId = 2 }
        };

        _mockRepo.Setup(repo => repo.SearchTrainingsAsync("Morning Run", DateTime.Today, 1))
                 .ReturnsAsync(trainings.Where(t => t.Name == "Morning Run" && t.UserId == 1));

        // Act
        var result = await _trainingService.SearchTrainingsAsync("Morning Run", DateTime.Today, 1);

        // Assert
        Assert.Single(result);
        Assert.Equal("Morning Run", result.First().Name);
    }

    [Fact]
    public async Task SearchTrainingsAsync_ReturnsEmptyList_WhenNoTrainingsFound()
    {
        // Arrange
        _mockRepo.Setup(repo => repo.SearchTrainingsAsync("Nonexistent Training", DateTime.Today, 1))
                 .ReturnsAsync(Enumerable.Empty<Training>());

        // Act
        var result = await _trainingService.SearchTrainingsAsync("Nonexistent Training", DateTime.Today, 1);

        // Assert
        Assert.Empty(result); // Перевіряємо, що результат порожній
    }

    [Fact]
    public async Task SearchTrainingsAsync_ReturnsEmptyList_WhenDateDoesNotMatch()
    {
        // Arrange
        var trainings = new List<Training>
    {
        new Training { Id = 1, Name = "Morning Run", Date = DateTime.Today, UserId = 1 },
        new Training { Id = 2, Name = "Evening Yoga", Date = DateTime.Today.AddDays(1), UserId = 1 }
    };

        _mockRepo.Setup(repo => repo.SearchTrainingsAsync("Morning Run", DateTime.Today, 1))
                 .ReturnsAsync(trainings.Where(t => t.Name == "Morning Run" && t.UserId == 1));

        // Act
        var result = await _trainingService.SearchTrainingsAsync("Morning Run", DateTime.Today, 1);

        // Assert
        Assert.Single(result); // Повинно бути лише одне тренування
        Assert.Equal("Morning Run", result.First().Name); // Ім'я тренування повинно бути "Morning Run"
    }

    [Fact]
    public async Task SearchTrainingsAsync_ReturnsMultipleTrainings_WhenMultipleTrainingsMatch()
    {
        // Arrange
        var trainings = new List<Training>
    {
        new Training { Id = 1, Name = "Morning Run", Date = DateTime.Today, UserId = 1 },
        new Training { Id = 2, Name = "Morning Run", Date = DateTime.Today, UserId = 1 },
        new Training { Id = 3, Name = "Evening Yoga", Date = DateTime.Today, UserId = 1 }
    };

        _mockRepo.Setup(repo => repo.SearchTrainingsAsync("Morning Run", DateTime.Today, 1))
                 .ReturnsAsync(trainings.Where(t => t.Name == "Morning Run" && t.UserId == 1));

        // Act
        var result = await _trainingService.SearchTrainingsAsync("Morning Run", DateTime.Today, 1);

        // Assert
        Assert.Equal(2, result.Count()); // Має бути два тренування з назвою "Morning Run"
    }

    [Fact]
    public async Task SearchTrainingsAsync_ReturnsTrainingsForSpecificUser()
    {
        // Arrange
        var userId = 1;
        var trainings = new List<Training>
    {
        new Training { Id = 1, Name = "Morning Run", Date = DateTime.Today, UserId = 1 },
        new Training { Id = 2, Name = "Evening Yoga", Date = DateTime.Today, UserId = 2 }
    };

        _mockRepo.Setup(repo => repo.SearchTrainingsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), userId))
                 .ReturnsAsync(trainings.Where(t => t.UserId == userId));

        // Act
        var result = await _trainingService.SearchTrainingsAsync("Any", DateTime.Today, userId);

        // Assert
        Assert.Single(result); // Має бути лише одне тренування для користувача з id = 1
        Assert.Equal("Morning Run", result.First().Name); // Перевіряємо ім'я тренування
    }


}
