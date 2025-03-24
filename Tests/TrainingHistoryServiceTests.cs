using Xunit;
using Moq;
using DAL.Repositorie;
using BLL.Services;
using System.Collections.Generic;
using System.Linq;
using DAL.Models;

public class TrainingHistoryServiceTests
{
    [Fact]
    public void GetUserTrainingHistory_ShouldFilterOutZeroWeightExercises()
    {
        // Arrange
        var mockRepo = new Mock<ITrainingHistoryRepository>();
        mockRepo.Setup(repo => repo.GetUserTrainingHistory(It.IsAny<int>()))
            .Returns(new List<TrainingHistory>
            {
                new TrainingHistory
                {
                    Id = 1, UserId = 1, TrainingDate = DateTime.Now, Duration = TimeSpan.FromMinutes(60),
                    Exercises = new List<Exercise>
                    {
                        new Exercise { Name = "Жим", Weight = 50, Repetitions = 10 },
                        new Exercise { Name = "Присідання", Weight = 0, Repetitions = 10 }
                    }
                },
                new TrainingHistory
                {
                    Id = 2, UserId = 1, TrainingDate = DateTime.Now, Duration = TimeSpan.FromMinutes(45),
                    Exercises = new List<Exercise>
                    {
                        new Exercise { Name = "Тяга", Weight = 0, Repetitions = 12 },
                        new Exercise { Name = "Станова", Weight = 0, Repetitions = 8 }
                    }
                }
            });

        var service = new TrainingHistoryService(mockRepo.Object);

        // Act
        var result = service.GetUserTrainingHistory(1).ToList();

        // Assert
        Assert.Single(result);
        Assert.DoesNotContain(result, t => t.Exercises.All(e => e.Weight == 0));
    }
}
