using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PL.Controllers;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using DAL.Models;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace PL.Tests
{
    public class HomeControllerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole<int>>> _roleManagerMock;
        private readonly Mock<ILogger<HomeController>> _loggerMock;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _loggerMock = new Mock<ILogger<HomeController>>();

            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            var roleStoreMock = new Mock<IRoleStore<IdentityRole<int>>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole<int>>>(roleStoreMock.Object, null, null, null, null);

            _controller = new HomeController(_loggerMock.Object, _userManagerMock.Object, _roleManagerMock.Object);

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task HomeAdmin_RedirectsToLogin_WhenUserNotAuthenticated()
        {
            // Arrange
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((User)null);

            // Act
            var result = await _controller.HomeAdmin();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task BlockUser_BlocksUserAndSetsReason_WhenUserExists()
        {
            // Arrange
            var user = new User { Id = 2, UserName = "User1" };
            _userManagerMock.Setup(um => um.FindByIdAsync("2")).ReturnsAsync(user);
            _roleManagerMock.Setup(rm => rm.RoleExistsAsync("Blocked")).ReturnsAsync(false);
            _roleManagerMock.Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole<int>>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(um => um.IsInRoleAsync(user, "Blocked")).ReturnsAsync(false);
            _userManagerMock.Setup(um => um.AddToRoleAsync(user, "Blocked")).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(um => um.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.BlockUser("2", "Spam");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HomeAdmin", redirectResult.ActionName);
            Assert.Equal("Spam", user.BlockedReason);
            _userManagerMock.Verify(um => um.AddToRoleAsync(user, "Blocked"), Times.Once);
            _userManagerMock.Verify(um => um.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task BlockUser_RedirectsToHomeAdmin_WhenUserNotFound()
        {
            // Arrange
            _userManagerMock.Setup(um => um.FindByIdAsync("999")).ReturnsAsync((User)null);

            // Act
            var result = await _controller.BlockUser("999", "Spam");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HomeAdmin", redirectResult.ActionName);
        }

        [Fact]
        public async Task UnblockUser_UnblocksUserAndClearsReason_WhenUserExists()
        {
            // Arrange
            var user = new User { Id = 2, UserName = "User1", BlockedReason = "Spam" };
            _userManagerMock.Setup(um => um.FindByIdAsync("2")).ReturnsAsync(user);
            _userManagerMock.Setup(um => um.IsInRoleAsync(user, "Blocked")).ReturnsAsync(true);
            _userManagerMock.Setup(um => um.RemoveFromRoleAsync(user, "Blocked")).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(um => um.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.UnblockUser("2");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HomeAdmin", redirectResult.ActionName);
            Assert.Null(user.BlockedReason);
            _userManagerMock.Verify(um => um.RemoveFromRoleAsync(user, "Blocked"), Times.Once);
            _userManagerMock.Verify(um => um.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task UnblockUser_RedirectsToHomeAdmin_WhenUserNotFound()
        {
            // Arrange
            _userManagerMock.Setup(um => um.FindByIdAsync("999")).ReturnsAsync((User)null);

            // Act
            var result = await _controller.UnblockUser("999");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HomeAdmin", redirectResult.ActionName);
        }
    }
}