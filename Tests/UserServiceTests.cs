//using System;
//using System.Collections.Generic;
//using Moq;
//using Xunit;
//using DAL.Interfaces;
//using DAL.Models;
//using BLL.Service;

//namespace Tests.ServicesTests
//{
//    public class UserServiceTests
//    {
//        private readonly Mock<IUserRepository> _userRepositoryMock;
//        private readonly IUserService _userService;

//        public UserServiceTests()
//        {
//            _userRepositoryMock = new Mock<IUserRepository>();
//            _userService = new UserService(_userRepositoryMock.Object);
//        }

//        [Fact]
//        public void GetUserById_UserExists_ReturnsUser()
//        {
//            // Arrange
//            var user = new User { Id = 1, UserName = "Test User" };
//            _userRepositoryMock.Setup(repo => repo.GetById(1)).Returns(user);

//            // Act
//            var result = _userService.GetUserById("1");

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(user.Id, result.Id);
//            Assert.Equal(user.UserName, result.UserName);
//        }

//        [Fact]
//        public void GetUserById_UserDoesNotExist_ReturnsNull()
//        {
//            // Arrange
//            _userRepositoryMock.Setup(repo => repo.GetById(2)).Returns((User)null);

//            // Act
//            var result = _userService.GetUserById("2");

//            // Assert
//            Assert.Null(result);
//        }

//        [Fact]
//        public void UpdateUser_ValidUser_CallsRepositoryUpdate()
//        {
//            // Arrange
//            var user = new User { Id = 1, UserName = "Updated User" };

//            // Act
//            _userService.UpdateUser(user);

//            // Assert
//            _userRepositoryMock.Verify(repo => repo.Update(user), Times.Once);
//        }
//    }
//}
