using BLL.Models;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Identity;

namespace BLL.Tests
{
    public class FriendshipServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly Mock<ILogger<FriendshipService>> _mockLogger;
        private readonly Mock<UserManager<User>> _mockUserManager;

        public FriendshipServiceTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique database per test
                .EnableSensitiveDataLogging()
                .Options;
            _mockLogger = new Mock<ILogger<FriendshipService>>();
            _mockUserManager = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
        }

        private FriendshipService CreateService(ApplicationDbContext context)
        {
            return new FriendshipService(_mockUserManager.Object, context, _mockLogger.Object);
        }

        [Fact]
        public async Task GetFriendExerciseRecords_ReturnsSingleExerciseRecord()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var service = CreateService(context);
            var friendId = 1;
            var training = new Training
            {
                UserId = friendId,
                Template = false,
                Name = "Workout 1",
                Description = "Strength training",
                Exercises = new List<Exercise>
                {
                    new Exercise { Name = "Bench Press", Weight = 100, Repetitions = 10, Notes = "Good form" }
                }
            };
            context.Trainings.Add(training);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetFriendExerciseRecords(friendId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Bench Press", result[0].ExerciseName);
            Assert.Equal(100, result[0].MaxWeight);
            Assert.Equal(10, result[0].MaxReps);
        }

        [Fact]
        public async Task GetFriendExerciseRecords_ReturnsMultipleExercises()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var service = CreateService(context);
            var friendId = 1;
            var training = new Training
            {
                UserId = friendId,
                Template = false,
                Name = "Workout 1",
                Description = "Full body",
                Exercises = new List<Exercise>
                {
                    new Exercise { Name = "Squat", Weight = 150, Repetitions = 8, Notes = "Deep squat" },
                    new Exercise { Name = "Deadlift", Weight = 200, Repetitions = 6, Notes = "Strong pull" }
                }
            };
            context.Trainings.Add(training);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetFriendExerciseRecords(friendId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.ExerciseName == "Squat" && r.MaxWeight == 150 && r.MaxReps == 8);
            Assert.Contains(result, r => r.ExerciseName == "Deadlift" && r.MaxWeight == 200 && r.MaxReps == 6);
        }

        [Fact]
        public async Task GetFriendExerciseRecords_IgnoresTemplateTrainings()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var service = CreateService(context);
            var friendId = 1;
            var trainings = new List<Training>
            {
                new Training
                {
                    UserId = friendId,
                    Template = true, // Ignored
                    Name = "Template Workout",
                    Description = "Template",
                    Exercises = new List<Exercise>
                    {
                        new Exercise { Name = "Push-up", Weight = 0, Repetitions = 20, Notes = "Bodyweight" }
                    }
                },
                new Training
                {
                    UserId = friendId,
                    Template = false,
                    Name = "Real Workout",
                    Description = "Strength",
                    Exercises = new List<Exercise>
                    {
                        new Exercise { Name = "Bench Press", Weight = 120, Repetitions = 12, Notes = "Stable" }
                    }
                }
            };
            context.Trainings.AddRange(trainings);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetFriendExerciseRecords(friendId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Bench Press", result[0].ExerciseName);
        }

        [Fact]
        public async Task GetFriendExerciseRecords_ReturnsEmptyWhenNoTrainings()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var service = CreateService(context);
            var friendId = 1;

            // Act
            var result = await service.GetFriendExerciseRecords(friendId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFriendExerciseRecords_HandlesSameExerciseDifferentWeights()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var service = CreateService(context);
            var friendId = 1;
            var training = new Training
            {
                UserId = friendId,
                Template = false,
                Name = "Workout 1",
                Description = "Strength",
                Exercises = new List<Exercise>
                {
                    new Exercise { Name = "Squat", Weight = 100, Repetitions = 10, Notes = "Light set" },
                    new Exercise { Name = "Squat", Weight = 120, Repetitions = 8, Notes = "Heavy set" }
                }
            };
            context.Trainings.Add(training);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetFriendExerciseRecords(friendId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Squat", result[0].ExerciseName);
            Assert.Equal(120, result[0].MaxWeight);
            Assert.Equal(10, result[0].MaxReps); // Max reps across all instances
        }

        [Fact]
        public async Task GetFriendExerciseRecords_HandlesZeroWeight()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var service = CreateService(context);
            var friendId = 1;
            var training = new Training
            {
                UserId = friendId,
                Template = false,
                Name = "Workout 1",
                Description = "Bodyweight",
                Exercises = new List<Exercise>
                {
                    new Exercise { Name = "Push-up", Weight = 0, Repetitions = 25, Notes = "Endurance" }
                }
            };
            context.Trainings.Add(training);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetFriendExerciseRecords(friendId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Push-up", result[0].ExerciseName);
            Assert.Equal(0, result[0].MaxWeight);
            Assert.Equal(25, result[0].MaxReps);
        }

        [Fact]
        public async Task GetFriendExerciseRecords_HandlesWhitespaceInExerciseName()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var service = CreateService(context);
            var friendId = 1;
            var training = new Training
            {
                UserId = friendId,
                Template = false,
                Name = "Workout 1",
                Description = "Strength",
                Exercises = new List<Exercise>
                {
                    new Exercise { Name = " Barbell Bench Press ", Weight = 130, Repetitions = 10, Notes = "Controlled" }
                }
            };
            context.Trainings.Add(training);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetFriendExerciseRecords(friendId);

            // Assert
            Assert.Single(result);
            Assert.Equal(" Barbell Bench Press ", result[0].ExerciseName);
            Assert.Equal(130, result[0].MaxWeight);
            Assert.Equal(10, result[0].MaxReps);
        }

        [Fact]
        public async Task GetFriendExerciseRecords_HandlesMultipleTrainings()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var service = CreateService(context);
            var friendId = 1;
            var trainings = new List<Training>
            {
                new Training
                {
                    UserId = friendId,
                    Template = false,
                    Name = "Workout 1",
                    Description = "Day 1",
                    Exercises = new List<Exercise>
                    {
                        new Exercise { Name = "Squat", Weight = 140, Repetitions = 10, Notes = "Warm-up" }
                    }
                },
                new Training
                {
                    UserId = friendId,
                    Template = false,
                    Name = "Workout 2",
                    Description = "Day 2",
                    Exercises = new List<Exercise>
                    {
                        new Exercise { Name = "Squat", Weight = 150, Repetitions = 8, Notes = "Max effort" }
                    }
                }
            };
            context.Trainings.AddRange(trainings);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetFriendExerciseRecords(friendId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Squat", result[0].ExerciseName);
            Assert.Equal(150, result[0].MaxWeight);
            Assert.Equal(10, result[0].MaxReps);
        }

        [Fact]
        public async Task GetFriendExerciseRecords_HandlesNoExercisesInTraining()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var service = CreateService(context);
            var friendId = 1;
            var training = new Training
            {
                UserId = friendId,
                Template = false,
                Name = "Empty Workout",
                Description = "No exercises",
                Exercises = new List<Exercise>()
            };
            context.Trainings.Add(training);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetFriendExerciseRecords(friendId);

            // Assert
            Assert.Empty(result);
        }

    }
}