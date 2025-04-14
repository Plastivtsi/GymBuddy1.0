using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BLL.Service;
using DAL.Models;
using YourProject.Controllers;
using System.Collections.Generic;

namespace YourProject.Tests
{
    public class ProfileControllerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly ProfileController _controller;
        private readonly ApplicationDbContext _context;

        public ProfileControllerTests()
        {
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            _userServiceMock = new Mock<IUserService>();
            _controller = new ProfileController(_userManagerMock.Object, _userServiceMock.Object, _context);
        }

        [Fact]
        public async Task Edit_Post_Should_Update_User_And_Redirect()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                UserName = "UpdatedUser",
                Email = "updated@email.com",
                Weight = 70,
                Height = 180
            };

            _userServiceMock.Setup(s => s.GetUserById("1")).ReturnsAsync(user);
            _userServiceMock.Setup(s => s.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Edit(user);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _userServiceMock.Verify(s => s.UpdateUser(It.Is<User>(u =>
                u.Id == 1 &&
                u.UserName == "UpdatedUser" &&
                u.Email == "updated@email.com")), Times.Once);
        }

        [Fact]
        public async Task Edit_Post_Should_Return_NotFound_If_User_NotExists()
        {
            // Arrange
            var user = new User { Id = 999, UserName = "Ghost" };

            _userServiceMock.Setup(s => s.GetUserById("999")).ReturnsAsync((User)null);

            // Act
            var result = await _controller.Edit(user);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_Should_Return_View_If_Exception_Occurs()
        {
            // Arrange
            var user = new User { Id = 1, UserName = "User", Email = "test@email.com", Weight = 80, Height = 175 };

            _userServiceMock.Setup(s => s.GetUserById("1")).ReturnsAsync(user);
            _userServiceMock.Setup(s => s.UpdateUser(It.IsAny<User>())).ThrowsAsync(new System.Exception("DB error"));

            // Act
            var result = await _controller.Edit(user);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(user, viewResult.Model);
        }

        [Fact]
        public async Task Edit_Post_Should_Call_GetUserById_With_Correct_Id()
        {
            // Arrange
            var user = new User { Id = 42, UserName = "Tester" };
            _userServiceMock.Setup(s => s.GetUserById("42")).ReturnsAsync(user);

            // Act
            var result = await _controller.Edit(user);

            // Assert
            _userServiceMock.Verify(s => s.GetUserById("42"), Times.Once);
        }
    }
}
