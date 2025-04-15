using BLL.Models;
using DAL;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class TrainingCalendarTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetMonthlyTrainingCountsAsync_ReturnsCorrectCounts()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var loggerMock = new Mock<ILogger<TrainingCalendar>>();
            var service = new TrainingCalendar(context, loggerMock.Object);

            int userId = 1;
            int currentYear = DateTime.UtcNow.Year;
            int previousYear = currentYear - 1;

            // Test data with Description
            context.Trainings.AddRange(
                new Training
                {
                    UserId = userId,
                    Name = "Training Jan This Year",
                    Date = new DateTime(currentYear, 1, 10),
                    Description = "Test Description",
                    Exercises = new List<Exercise>()
                },
                new Training
                {
                    UserId = userId,
                    Name = "Training Jan Last Year",
                    Date = new DateTime(previousYear, 1, 15),
                    Description = "Test Description",
                    Exercises = new List<Exercise>()
                },
                new Training
                {
                    UserId = userId,
                    Name = "Training Feb This Year",
                    Date = new DateTime(currentYear, 2, 20),
                    Description = "Test Description",
                    Exercises = new List<Exercise>()
                }
            );

            await context.SaveChangesAsync();

            // Act
            var result = await service.GetMonthlyTrainingCountsAsync(userId, currentYear);

            // Assert
            Assert.Equal(2, result[1].CurrentYearCount + result[1].PreviousYearCount); // January: 1 this year + 1 last year
            Assert.Equal(1, result[2].CurrentYearCount); // February: 1 this year
            Assert.Equal(0, result[2].PreviousYearCount); // February: 0 last year
        }
    }
}
