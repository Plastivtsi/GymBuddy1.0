using Xunit;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL.Models;
using BLL.Service;
using DAL.Repositorie;
using Microsoft.Extensions.Logging;

namespace Tests.ServicesTests
{
    public class TrainingHistoryServiceTests
    {
        private readonly Mock<ITrainingRepository> _trainingRepositoryMock;
        private readonly Mock<ILogger<TrainingService>> _loggerMock;
        private readonly TrainingService _trainingService;

        public TrainingHistoryServiceTests()
        {
            _trainingRepositoryMock = new Mock<ITrainingRepository>();
            _loggerMock = new Mock<ILogger<TrainingService>>();
            _trainingService = new TrainingService(_trainingRepositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetUserTrainingHistoryAsync_ShouldReturnTrainingsWithNonZeroWeightExercises()
        {
            // Arrange
            var userId = 1;
            var trainings = new List<Training>
            {
                new Training
                {
                    Id = 1,
                    UserId = userId,
                    Exercises = new List<Exercise>
                    {
                        new Exercise { Name = "Жим", Weight = 50, Repetitions = 10 },
                        new Exercise { Name = "Присідання", Weight = 0, Repetitions = 10 }
                    }
                },
                new Training
                {
                    Id = 2,
                    UserId = userId,
                    Exercises = new List<Exercise>
                    {
                        new Exercise { Name = "Тяга", Weight = 0, Repetitions = 12 },
                        new Exercise { Name = "Станова", Weight = 0, Repetitions = 8 }
                    }
                }
            };

            _trainingRepositoryMock.Setup(repo => repo.GetTrainingsByUserId(userId))
                .ReturnsAsync(trainings);

            // Act
            var result = await _trainingService.GetUserTrainingHistoryAsync(userId);

            // Assert
            Assert.Equal(1, result.Count); // Only one training has an exercise with Weight > 0
            Assert.All(result, t => Assert.True(t.Exercises.Any(e => e.Weight > 0)));
            Assert.Equal(1, result.First().Id); // Verify the correct training is returned
        }
    }
}