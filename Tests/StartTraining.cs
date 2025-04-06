//using BLL.Interfaces;
//using BLL.Models;
//using DAL;
//using DAL.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using PL.Controllers;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Xunit;

//namespace GymBuddy.Tests
//{
//    public class TrainingsControllerStartTests
//    {
//        private Mock<ICreateTrainings> _mockCreateTrainings;
//        private Mock<IGetTemplateTrainings> _mockGetTemplateTrainings;
//        private Mock<ICreateTrainingFromTemplate> _mockCreateTrainingFromTemplate;
//        private ApplicationDbContext _context;

//        public TrainingsControllerStartTests()
//        {
//            _mockCreateTrainings = new Mock<ICreateTrainings>();
//            _mockGetTemplateTrainings = new Mock<IGetTemplateTrainings>();
//            _mockCreateTrainingFromTemplate = new Mock<ICreateTrainingFromTemplate>();

//            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
//                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//                .Options;
//            _context = new ApplicationDbContext(options);
//        }

//        private void SeedData()
//        {
//            // Додаємо тестового користувача з усіма обов’язковими полями
//            var user = new User
//            {
//                Id = 1,
//                UserName = "TestUser",
//                Email = "testuser@example.com", // Додаємо Email
//                Password = "password123" // Додаємо Password
//            };
//            _context.Users.Add(user);

//            var templateTraining = new Training
//            {
//                Id = 1,
//                Name = "Template Training",
//                Date = DateTime.Now,
//                Time = TimeSpan.FromHours(1),
//                Description = "Test template training",
//                UserId = 1,
//                Template = true,
//                Exercises = new List<Exercise>
//                {
//                    new Exercise { Id = 1, Name = "Squats", Weight = 0, Repetitions = 0, Notes = "", Template = true }
//                }
//            };
//            _context.Trainings.Add(templateTraining);

//            var completedTraining = new Training
//            {
//                Id = 2,
//                Name = "Completed Training",
//                Date = DateTime.Now,
//                Time = TimeSpan.FromHours(1),
//                Description = "Test completed training",
//                UserId = 1,
//                Template = false,
//                Exercises = new List<Exercise>
//                {
//                    new Exercise { Id = 2, Name = "Squats", Weight = 50, Repetitions = 10, Notes = "Done", Template = false }
//                }
//            };
//            _context.Trainings.Add(completedTraining);

//            _context.SaveChanges();
//        }

//        [Fact]
//        public void CreateTraining_Get_ReturnsViewWithTrainingModel()
//        {
//            // Arrange
//            var controller = new TrainingsControllerStart(
//                _mockCreateTrainings.Object,
//                _mockGetTemplateTrainings.Object,
//                _mockCreateTrainingFromTemplate.Object,
//                _context);

//            // Act
//            var result = controller.CreateTraining();

//            // Assert
//            var viewResult = Assert.IsType<ViewResult>(result);
//            Assert.Equal("CreateTraining", viewResult.ViewName);
//            Assert.IsType<Training>(viewResult.Model);
//        }

//        [Fact]
//        public async Task CreateTrainingPost_ValidModel_CreatesTraining()
//        {
//            // Arrange
//            var training = new Training
//            {
//                Name = "New Training",
//                Date = DateTime.Now,
//                Time = TimeSpan.FromHours(1),
//                Description = "Test training",
//                UserId = 1
//            };

//            _mockCreateTrainings
//                .Setup(x => x.CreateNewTrainingAsync(
//                    training.Name,
//                    training.Date,
//                    training.Time,
//                    training.Description,
//                    training.UserId))
//                .ReturnsAsync(new Training { Id = 3, Name = "New Training" });

//            var controller = new TrainingsControllerStart(
//                _mockCreateTrainings.Object,
//                _mockGetTemplateTrainings.Object,
//                _mockCreateTrainingFromTemplate.Object,
//                _context);

//            // Act
//            var result = await controller.CreateTrainingPost(training);

//            // Assert
//            var viewResult = Assert.IsType<ViewResult>(result);
//            Assert.Equal("CreateTraining", viewResult.ViewName);
//            Assert.Equal("Тренування створено успішно! ID: 3", viewResult.ViewData["Message"]);
//        }

//        [Fact]
//        public async Task StartTraining_ReturnsViewWithTemplateTrainings()
//        {
//            // Arrange
//            var templateTrainings = new List<Training>
//            {
//                new Training { Id = 1, Name = "Template Training", Template = true }
//            };

//            _mockGetTemplateTrainings
//                .Setup(x => x.GetTemplateTrainingsWithExercisesAsync())
//                .ReturnsAsync(templateTrainings);

//            var controller = new TrainingsControllerStart(
//                _mockCreateTrainings.Object,
//                _mockGetTemplateTrainings.Object,
//                _mockCreateTrainingFromTemplate.Object,
//                _context);

//            // Act
//            var result = await controller.StartTraining();

//            // Assert
//            var viewResult = Assert.IsType<ViewResult>(result);
//            var model = Assert.IsAssignableFrom<List<Training>>(viewResult.Model);
//            Assert.Equal(templateTrainings, model);
//        }

//        [Fact]
//        public async Task SelectTraining_TrainingExists_ReturnsView()
//        {
//            // Arrange
//            SeedData();
//            var controller = new TrainingsControllerStart(
//                _mockCreateTrainings.Object,
//                _mockGetTemplateTrainings.Object,
//                _mockCreateTrainingFromTemplate.Object,
//                _context);

//            // Act
//            var result = await controller.SelectTraining(1);

//            // Assert
//            var viewResult = Assert.IsType<ViewResult>(result);
//            var model = Assert.IsType<Training>(viewResult.Model);
//            Assert.Equal(1, model.Id);
//            Assert.True(model.Template);
//            Assert.Single(model.Exercises);
//        }

//        [Fact]
//        public async Task SelectTraining_TrainingNotFound_ReturnsNotFound()
//        {
//            // Arrange
//            SeedData();
//            var controller = new TrainingsControllerStart(
//                _mockCreateTrainings.Object,
//                _mockGetTemplateTrainings.Object,
//                _mockCreateTrainingFromTemplate.Object,
//                _context);

//            // Act
//            var result = await controller.SelectTraining(999);

//            // Assert
//            Assert.IsType<NotFoundResult>(result);
//        }

//        [Fact]
//        public async Task CompleteTraining_Success_RedirectsToTrainingCompleted()
//        {
//            // Arrange
//            var templateTrainingId = 1;
//            var userId = 1;
//            var updatedExercises = new List<Exercise>
//            {
//                new Exercise { Id = 1, Weight = 50, Repetitions = 10 }
//            };

//            var newTraining = new Training { Id = 3, Name = "New Training", Template = false };

//            _mockCreateTrainingFromTemplate
//                .Setup(x => x.CreateTrainingFromTemplateAsync(templateTrainingId, userId, updatedExercises))
//                .ReturnsAsync(newTraining);

//            SeedData();
//            var controller = new TrainingsControllerStart(
//                _mockCreateTrainings.Object,
//                _mockGetTemplateTrainings.Object,
//                _mockCreateTrainingFromTemplate.Object,
//                _context);

//            // Act
//            var result = await controller.CompleteTraining(templateTrainingId, updatedExercises);

//            // Assert
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal("TrainingCompleted", redirectResult.ActionName);
//            Assert.Equal(3, redirectResult.RouteValues["id"]);
//        }

//        [Fact]
//        public async Task TrainingCompleted_TrainingExists_ReturnsView()
//        {
//            // Arrange
//            SeedData();
//            var controller = new TrainingsControllerStart(
//                _mockCreateTrainings.Object,
//                _mockGetTemplateTrainings.Object,
//                _mockCreateTrainingFromTemplate.Object,
//                _context);

//            // Act
//            var result = await controller.TrainingCompleted(2);

//            // Assert
//            var viewResult = Assert.IsType<ViewResult>(result);
//            var model = Assert.IsType<Training>(viewResult.Model);
//            Assert.Equal(2, model.Id);
//            Assert.False(model.Template);
//            Assert.Single(model.Exercises);
//        }

//        [Fact]
//        public async Task TrainingCompleted_TrainingNotFound_ReturnsNotFound()
//        {
//            // Arrange
//            SeedData();
//            var controller = new TrainingsControllerStart(
//                _mockCreateTrainings.Object,
//                _mockGetTemplateTrainings.Object,
//                _mockCreateTrainingFromTemplate.Object,
//                _context);

//            // Act
//            var result = await controller.TrainingCompleted(999);

//            // Assert
//            Assert.IsType<NotFoundResult>(result);
//        }
//    }
//}