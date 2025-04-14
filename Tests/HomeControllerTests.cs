using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using PL.Controllers;
using DAL.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

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
            _userManagerMock = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
            _roleManagerMock = new Mock<RoleManager<IdentityRole<int>>>(
                Mock.Of<IRoleStore<IdentityRole<int>>>(), null, null, null, null);
            _loggerMock = new Mock<ILogger<HomeController>>();

            _controller = new HomeController(_loggerMock.Object, _userManagerMock.Object, _roleManagerMock.Object);
        }

        [Fact]
        public async Task HomeAdmin_UserNotAuthenticated_RedirectsToLogin()
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
        public async Task HomeAdmin_AuthenticatedAdmin_ReturnsViewWithUserList()
        {
            // Arrange
            var user = new User { Id = 1, UserName = "AdminUser", Email = "admin@example.com" };
            var users = new List<User>
            {
                user,
                new User { Id = 2, UserName = "TestUser", Email = "test@example.com" }
            };

            // Create an IQueryable that supports async operations
            var dbSetMock = new Mock<DbSet<User>>();
            dbSetMock.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.AsQueryable().Provider);
            dbSetMock.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.AsQueryable().Expression);
            dbSetMock.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.AsQueryable().ElementType);
            dbSetMock.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.AsQueryable().GetEnumerator());
            dbSetMock.As<IAsyncEnumerable<User>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<User>(users.GetEnumerator()));

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _userManagerMock.Setup(um => um.Users).Returns(dbSetMock.Object);
            _userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });
            _userManagerMock.Setup(um => um.GetRolesAsync(users[1])).ReturnsAsync(new List<string>());

            // Act
            var result = await _controller.HomeAdmin();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<UserViewModel>>(viewResult.Model);
            Assert.Equal(2, model.Count);
            Assert.Equal("AdminUser", model[0].UserName);
            Assert.Equal("TestUser", model[1].UserName);
        }

        [Fact]
        public void RegisterAdmin_Get_ReturnsView()
        {
            // Act
            var result = _controller.RegisterAdmin();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task RegisterAdmin_Post_ValidModel_CreatesAdminAndRedirects()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                UserName = "NewAdmin",
                Email = "newadmin@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _roleManagerMock.Setup(rm => rm.RoleExistsAsync("Admin")).ReturnsAsync(false);
            _roleManagerMock.Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole<int>>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(um => um.AddToRoleAsync(It.IsAny<User>(), "Admin"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.RegisterAdmin(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HomeAdmin", redirectResult.ActionName);
            _userManagerMock.Verify(um => um.CreateAsync(It.Is<User>(u => u.UserName == "NewAdmin"), model.Password), Times.Once());
            _userManagerMock.Verify(um => um.AddToRoleAsync(It.IsAny<User>(), "Admin"), Times.Once());
        }

        [Fact]
        public async Task RegisterAdmin_Post_InvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                UserName = "NewAdmin",
                Email = "invalid-email",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };
            _controller.ModelState.AddModelError("Email", "Invalid email format");

            // Act
            var result = await _controller.RegisterAdmin(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task RegisterAdmin_Post_CreateFails_ReturnsViewWithErrors()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                UserName = "NewAdmin",
                Email = "newadmin@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };
            var identityErrors = new List<IdentityError> { new IdentityError { Description = "Username already exists" } };
            _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act
            var result = await _controller.RegisterAdmin(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.True(_controller.ModelState.ContainsKey(string.Empty));
            Assert.Equal("Username already exists", _controller.ModelState[string.Empty].Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task BlockUser_UserNotFound_RedirectsToHomeAdmin()
        {
            // Arrange
            _userManagerMock.Setup(um => um.FindByIdAsync("1")).ReturnsAsync((User)null);

            // Act
            var result = await _controller.BlockUser("1", "Test reason");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HomeAdmin", redirectResult.ActionName);
        }

        [Fact]
        public async Task BlockUser_ValidUser_BlocksUserAndRedirects()
        {
            // Arrange
            var user = new User { Id = 1, UserName = "TestUser" };
            _userManagerMock.Setup(um => um.FindByIdAsync("1")).ReturnsAsync(user);
            _roleManagerMock.Setup(rm => rm.RoleExistsAsync("Blocked")).ReturnsAsync(false);
            _roleManagerMock.Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole<int>>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(um => um.IsInRoleAsync(user, "Blocked")).ReturnsAsync(false);
            _userManagerMock.Setup(um => um.AddToRoleAsync(user, "Blocked"))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(um => um.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.BlockUser("1", "Test reason");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HomeAdmin", redirectResult.ActionName);
            Assert.Equal("Test reason", user.BlockedReason);
            _userManagerMock.Verify(um => um.AddToRoleAsync(user, "Blocked"), Times.Once());
            _userManagerMock.Verify(um => um.UpdateAsync(user), Times.Once());
        }

        [Fact]
        public async Task UnblockUser_UserNotFound_RedirectsToHomeAdmin()
        {
            // Arrange
            _userManagerMock.Setup(um => um.FindByIdAsync("1")).ReturnsAsync((User)null);

            // Act
            var result = await _controller.UnblockUser("1");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HomeAdmin", redirectResult.ActionName);
        }

        [Fact]
        public async Task UnblockUser_ValidUser_UnblocksUserAndRedirects()
        {
            // Arrange
            var user = new User { Id = 1, UserName = "TestUser", BlockedReason = "Test reason" };
            _userManagerMock.Setup(um => um.FindByIdAsync("1")).ReturnsAsync(user);
            _userManagerMock.Setup(um => um.IsInRoleAsync(user, "Blocked")).ReturnsAsync(true);
            _userManagerMock.Setup(um => um.RemoveFromRoleAsync(user, "Blocked"))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(um => um.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.UnblockUser("1");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HomeAdmin", redirectResult.ActionName);
            Assert.Null(user.BlockedReason);
            _userManagerMock.Verify(um => um.RemoveFromRoleAsync(user, "Blocked"), Times.Once());
            _userManagerMock.Verify(um => um.UpdateAsync(user), Times.Once());
        }
    }

    // Helper class for async enumeration
    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return default;
        }
    }
}