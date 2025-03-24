using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Models;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Tests
{
    public class FriendshipServiceTest
    {
        private readonly ApplicationDbContext _context;
        private readonly FriendshipService _service;

        public FriendshipServiceTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);
            _service = new FriendshipService(_context);
        }


        [Fact]
        public async Task GetFriendsAsync_UserHasNoFriends_ReturnsEmptyList()
        {
            // Act
            var result = await _service.GetFriendsAsync(1);

            // Assert
            Assert.Empty(result);
        }

       
        [Fact]
        public async Task SearchUserByName_NoMatchingUsers_ReturnsEmptyList()
        {
            // Act
            var result = await _service.SearchUserByName("John");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFriendsAsync_ExceptionThrown_ReturnsEmptyList()
        {
            // Arrange
            _context.Database.EnsureDeleted(); // Викликає помилку доступу до БД

            // Act
            var result = await _service.GetFriendsAsync(1);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchUserByName_ExceptionThrown_ReturnsEmptyList()
        {
            // Arrange
            _context.Database.EnsureDeleted(); // Викликає помилку доступу до БД

            // Act
            var result = await _service.SearchUserByName("Alice");

            // Assert
            Assert.Empty(result);
        }
    }
}