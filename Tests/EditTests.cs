//using Moq;
//using Xunit;
//using PL.Controllers;
//using BLL.Models;
//using DAL.Interfaces;
//using Microsoft.AspNetCore.Mvc;
//using System;
//using DAL.Models;
//using YourProject.Controllers;

//namespace PL.Tests
//{
//    public class ProfileControllerTests
//    {
//        private readonly Mock<IUserRepository> _userRepositoryMock;
//        private readonly ProfileController _controller;

//        public ProfileControllerTests()
//        {
//            _userRepositoryMock = new Mock<IUserRepository>();
//            _controller = new ProfileController((IUserService)_userRepositoryMock.Object);
//        }

//        // Тест на успішне редагування профілю
//        [Fact]
//        public void EditProfile_Should_Update_User_Profile()
//        {
//            // Arrange
//            var userId = 1;
//            var updatedUser = new User
//            {
//                Id = userId,
//                Name = "Updated Name",
//                Email = "updated@email.com",
//                Weight = 70,
//                Height = 180
//            };

//            _userRepositoryMock.Setup(repo => repo.Update(It.IsAny<User>())).Verifiable();

//            // Act
//            var result = _controller.Edit(updatedUser) as RedirectToActionResult;

//            // Assert
//            _userRepositoryMock.Verify(repo => repo.Update(It.Is<User>(u => u.Id == userId && u.Name == "Updated Name")), Times.Once);
//            Assert.NotNull(result);
//            Assert.Equal("Profile", result.ActionName); // Assuming it redirects to the Profile action
//        }

//        // Тест на невірного користувача (не знайдено)
//        [Fact]
//        public void EditProfile_Should_Return_NotFound_When_User_Not_Found()
//        {
//            // Arrange
//            var userId = 999; // Ідентифікатор користувача, який не існує
//            var updatedUser = new User
//            {
//                Id = userId,
//                Name = "Non-Existent User",
//                Email = "nonexistent@email.com",
//                Weight = 80,
//                Height = 175
//            };

//            _userRepositoryMock.Setup(repo => repo.Update(It.IsAny<User>())).Throws(new InvalidOperationException("User not found"));

//            // Act
//            var result = _controller.Edit(updatedUser) as NotFoundObjectResult;

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(404, result.StatusCode);
//            Assert.Equal("User not found", result.Value);
//        }

//        // Тест на перевірку валідації
//        [Fact]
//        public void EditProfile_Should_Return_BadRequest_When_Validation_Fails()
//        {
//            // Arrange
//            var invalidUser = new User
//            {
//                Id = 0, // Некоректний Id
//                Name = "", // Пусте ім'я
//                Email = "invalidemail"
//            };

//            // Act
//            var result = _controller.Edit(invalidUser) as BadRequestObjectResult;

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(400, result.StatusCode);
//            Assert.Contains("Invalid data", result.Value.ToString());
//        }
//    }
//}
