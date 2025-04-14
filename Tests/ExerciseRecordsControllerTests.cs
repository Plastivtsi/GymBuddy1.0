using DAL.Models;
using GymBuddy1._0.Controllers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GymBuddy1._0.Tests
{
    public class ExerciseRecordsControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly ExerciseRecordsController _controller;

        public ExerciseRecordsControllerTests()
        {
            // Налаштування In-Memory бази даних
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new ExerciseRecordsController(_context);
        }

        private void SeedData()
        {
            // Додаємо тестові дані
            var trainings = new List<Training>
            {
                new Training
                {
                    Id = 1,
                    UserId = 1, // Змінено з "1" на 1
                    Template = false,
                    Exercises = new List<Exercise>
                    {
                        new Exercise { Id = 1, Name = "Squat", Weight = 100, Repetitions = 10, TrainingId = 1 },
                        new Exercise { Id = 2, Name = "Squat", Weight = 120, Repetitions = 8, TrainingId = 1 },
                        new Exercise { Id = 3, Name = "Bench Press", Weight = 80, Repetitions = 12, TrainingId = 1 }
                    }
                },
                new Training
                {
                    Id = 2,
                    UserId = 1, // Змінено з "1" на 1
                    Template = true, // Це тренування має бути виключене
                    Exercises = new List<Exercise>
                    {
                        new Exercise { Id = 4, Name = "Squat", Weight = 150, Repetitions = 5, TrainingId = 2 }
                    }
                },
                new Training
                {
                    Id = 3,
                    UserId = 2, // Змінено з "2" на 2
                    Template = false,
                    Exercises = new List<Exercise>
                    {
                        new Exercise { Id = 5, Name = "Squat", Weight = 200, Repetitions = 3, TrainingId = 3 }
                    }
                }
            };

            _context.Trainings.AddRange(trainings);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetExerciseRecords_ReturnsCorrectRecords()
        {
            // Arrange
            SeedData();
            var userId = "1"; // Залишаємо string, оскільки метод приймає string

            // Act
            var result = await _controller.GetExerciseRecords(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Має бути 2 унікальні вправи (Squat і Bench Press)

            var squatRecord = result.First(r => ((dynamic)r).ExerciseName == "Squat");
            Assert.Equal(120, ((dynamic)squatRecord).MaxWeight);
            Assert.Equal(10, ((dynamic)squatRecord).MaxReps);

            var benchRecord = result.First(r => ((dynamic)r).ExerciseName == "Bench Press");
            Assert.Equal(80, ((dynamic)benchRecord).MaxWeight);
            Assert.Equal(12, ((dynamic)benchRecord).MaxReps);
        }

        [Fact]
        public async Task GetExerciseRecords_ReturnsEmptyList_WhenNoTrainings()
        {
            // Arrange
            var userId = "999"; // Користувач без тренувань

            // Act
            var result = await _controller.GetExerciseRecords(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetExerciseRecords_ExcludesTemplateTrainings()
        {
            // Arrange
            SeedData();
            var userId = "1"; // Залишаємо string

            // Act
            var result = await _controller.GetExerciseRecords(userId);

            // Assert
            var squatRecord = result.First(r => ((dynamic)r).ExerciseName == "Squat");
            Assert.Equal(120, ((dynamic)squatRecord).MaxWeight); // 150 з шаблонного тренування не враховується
            Assert.Equal(10, ((dynamic)squatRecord).MaxReps);
        }
    }
}