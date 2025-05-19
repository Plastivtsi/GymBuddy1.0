using BLL.Service;
using DAL.Models;
using DAL.Repositorie;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Tests.ServicesTests
{
    public class TrainingServiceTests
    {
        private readonly Mock<ITrainingRepository> _mockRepo;
        private readonly Mock<ILogger<TrainingService>> _loggerMock;
        private readonly TrainingService _trainingService;

        public TrainingServiceTests()
        {
            _mockRepo = new Mock<ITrainingRepository>();
            _loggerMock = new Mock<ILogger<TrainingService>>();
            _trainingService = new TrainingService(_mockRepo.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task SearchTrainingsAsync_ReturnsCorrectTrainings()
        {
            // Arrange
            var userId = 1;
            var searchDate = DateTime.Today;
            var trainings = new List<Training>
            {
                new Training { Id = 1, Name = "Morning Run", Date = searchDate, UserId = userId },
                new Training { Id = 2, Name = "Evening Yoga", Date = searchDate, UserId = 2 }
            };

            _mockRepo.Setup(repo => repo.SearchTrainingsAsync("Morning Run", searchDate, userId))
                .ReturnsAsync(trainings.Where(t => t.Name == "Morning Run" && t.UserId == userId && t.Date == searchDate).ToList());

            // Act
            var result = await _trainingService.SearchTrainingsAsync("Morning Run", searchDate, userId);

            // Assert
            Assert.Equal(1, result.Count());
            Assert.Equal("Morning Run", result.First().Name);
            Assert.Equal(userId, result.First().UserId);
        }

        [Fact]
        public async Task SearchTrainingsAsync_ReturnsEmptyList_WhenNoTrainingsFound()
        {
            // Arrange
            var userId = 1;
            var searchDate = DateTime.Today;
            _mockRepo.Setup(repo => repo.SearchTrainingsAsync("Nonexistent Training", searchDate, userId))
                .ReturnsAsync(new List<Training>());

            // Act
            var result = await _trainingService.SearchTrainingsAsync("Nonexistent Training", searchDate, userId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchTrainingsAsync_ReturnsEmptyList_WhenDateDoesNotMatch()
        {
            // Arrange
            var userId = 1;
            var searchDate = DateTime.Today;
            var trainings = new List<Training>
            {
                new Training { Id = 1, Name = "Morning Run", Date = searchDate.AddDays(1), UserId = userId },
                new Training { Id = 2, Name = "Evening Yoga", Date = searchDate.AddDays(1), UserId = userId }
            };

            _mockRepo.Setup(repo => repo.SearchTrainingsAsync("Morning Run", searchDate, userId))
                .ReturnsAsync(trainings.Where(t => t.Name == "Morning Run" && t.UserId == userId && t.Date == searchDate).ToList());

            // Act
            var result = await _trainingService.SearchTrainingsAsync("Morning Run", searchDate, userId);

            // Assert
            Assert.Empty(result); // No trainings match the date
        }

        [Fact]
        public async Task SearchTrainingsAsync_ReturnsMultipleTrainings_WhenMultipleTrainingsMatch()
        {
            // Arrange
            var userId = 1;
            var searchDate = DateTime.Today;
            var trainings = new List<Training>
            {
                new Training { Id = 1, Name = "Morning Run", Date = searchDate, UserId = userId },
                new Training { Id = 2, Name = "Morning Run", Date = searchDate, UserId = userId },
                new Training { Id = 3, Name = "Evening Yoga", Date = searchDate, UserId = userId }
            };

            _mockRepo.Setup(repo => repo.SearchTrainingsAsync("Morning Run", searchDate, userId))
                .ReturnsAsync(trainings.Where(t => t.Name == "Morning Run" && t.UserId == userId && t.Date == searchDate).ToList());

            // Act
            var result = await _trainingService.SearchTrainingsAsync("Morning Run", searchDate, userId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, t => Assert.Equal("Morning Run", t.Name));
        }

        [Fact]
        public async Task SearchTrainingsAsync_ReturnsTrainingsForSpecificUser()
        {
            // Arrange
            var userId = 1;
            var searchDate = DateTime.Today;
            var trainings = new List<Training>
            {
                new Training { Id = 1, Name = "Morning Run", Date = searchDate, UserId = userId },
                new Training { Id = 2, Name = "Evening Yoga", Date = searchDate, UserId = 2 }
            };

            _mockRepo.Setup(repo => repo.SearchTrainingsAsync(null, null, userId))
                .ReturnsAsync(trainings.Where(t => t.UserId == userId).ToList());

            // Act
            var result = await _trainingService.SearchTrainingsAsync(null, null, userId);

            // Assert
            Assert.Equal(1, result.Count());
            Assert.Equal("Morning Run", result.First().Name);
            Assert.Equal(userId, result.First().UserId);
        }
    }
}