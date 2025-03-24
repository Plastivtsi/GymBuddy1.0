using Xunit;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL.Models;
using BLL.Service;
using DAL.Repositorie;

namespace Tests.ServicesTests
{
    public class TrainingServiceTests
    {
        private readonly Mock<ITrainingRepository> _trainingRepositoryMock;
        private readonly TrainingService _trainingService;

        public TrainingServiceTests()
        {
            _trainingRepositoryMock = new Mock<ITrainingRepository>();
            _trainingService = new TrainingService(_trainingRepositoryMock.Object);
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

            // Упевнимося, що метод GetTrainingsByUserIdAsync існує в ITrainingRepository
            _trainingRepositoryMock.Setup(repo => repo.GetTrainingsByUserId(userId))
                .ReturnsAsync(trainings);

            // Act
            var result = await _trainingService.GetUserTrainingHistory(userId);

            // Assert
            Assert.Single(result); // Має залишитися тільки одне тренування
            Assert.All(result, t => Assert.Contains(t.Exercises, e => e.Weight > 0));
        }
    }
}
