using System;
using Moq;
using Xunit;
using PL.Controllers;
using DAL.Interfaces;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;

namespace Tests.Tests
{
    public class EditProfileControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly EditProfileController _controller;

        public EditProfileControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _controller = new EditProfileController(_userServiceMock.Object);
        }

        [Fact]
        public void EditProfile_ReturnsOk_WhenProfileUpdated()
        {
            // Arrange
            var user = new User
            {
                Id = 1, 
                UserName = "testUser",
                Email = "test@example.com",
                Password = "securePassword123"
            };

            _userServiceMock.Setup(service => service.UpdateUser(user));

            // Act
            var result = _controller.Edit(user);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Profile updated successfully", okResult.Value);
        }

        [Fact]
        public void EditProfile_ReturnsBadRequest_WhenUpdateFails()
        {
            // Arrange
            var user = new User
            {
                Id = 1, 
                UserName = "testUser",
                Email = "test@example.com",
                Password = "securePassword123"
            };

            _userServiceMock.Setup(service => service.UpdateUser(user)).Throws(new Exception("Update failed"));

            // Act
            var result = _controller.Edit(user);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to update profile", badRequestResult.Value);
        }
    }
}
