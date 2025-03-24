using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq; 
using PL.Controllers;
using BLL.Models;
using DAL.Models;
using Xunit;

namespace Tests.ControllersTests
{
    public class TrainingsControllerTests
    {
        private Mock<ICreateTrainings> _createTrainingsMock;
        private TrainingsController _controller;

        private void SetupController()
        {
            _createTrainingsMock = new Mock<ICreateTrainings>();
            _controller = new TrainingsController(_createTrainingsMock.Object);
        }

        [Fact]
        public void Create_Get_ReturnsViewResult_WithEmptyTraining()
        {
            // Arrange
            SetupController();

            // Act
            var result = _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Training>(viewResult.Model);
            Assert.NotNull(model);
            Assert.Null(model.Name);
            Assert.Equal(default(DateTime), model.Date);
            Assert.Equal(default(TimeSpan), model.Time);
            Assert.Null(model.Description);
            Assert.Equal(0, model.UserId);
        }

        [Fact]
        public async Task CreateTraining_Post_ValidModel_CallsCreateNewTrainingAsync_AndReturnsViewWithSuccessMessage()
        {
            // Arrange
            SetupController();

            var training = new Training
            {
                Name = "Ранова пробіжка",
                Date = DateTime.Now,
                Time = TimeSpan.FromHours(1),
                Description = "Пробіжка в парку",
                UserId = 1
            };
            var createdTraining = new Training
            {
                Id = 1,
                Name = training.Name,
                Date = training.Date,
                Time = training.Time,
                Description = training.Description,
                UserId = training.UserId
            };

            _createTrainingsMock
                .Setup(x => x.CreateNewTrainingAsync(
                    training.Name,
                    training.Date,
                    training.Time,
                    training.Description,
                    training.UserId))
                .ReturnsAsync(createdTraining);

            // Act
            var result = await _controller.CreateTraining(training);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Create", viewResult.ViewName); // Змінюємо "CREATE" на "Create"
            Assert.Equal(createdTraining, viewResult.Model);
            Assert.Equal($"Тренування створено успішно! ID: {createdTraining.Id}", viewResult.ViewData["Message"]);
        }

        [Fact]
        public async Task CreateTraining_Post_InvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            SetupController();

            var training = new Training();
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _controller.CreateTraining(training);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Create", viewResult.ViewName); // Змінюємо "CREATE" на "Create"
            Assert.Equal(training, viewResult.Model);
            Assert.Null(viewResult.ViewData["Message"]);
        }

        [Fact]
        public async Task CreateTraining_Post_ThrowsException_ReturnsViewWithErrorMessage()
        {
            // Arrange
            SetupController();

            var training = new Training
            {
                Name = "Ранова пробіжка",
                Date = DateTime.Now,
                Time = TimeSpan.FromHours(1),
                Description = "Пробіжка в парку",
                UserId = 999
            };

            _createTrainingsMock
                .Setup(x => x.CreateNewTrainingAsync(
                    training.Name,
                    training.Date,
                    training.Time,
                    training.Description,
                    training.UserId))
                .ThrowsAsync(new Exception("Користувач з таким ID не знайдений"));

            // Act
            var result = await _controller.CreateTraining(training);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Create", viewResult.ViewName); 
            Assert.Equal(training, viewResult.Model);
            Assert.Equal("Помилка: Користувач з таким ID не знайдений", viewResult.ViewData["Message"]);
        }
    }
}