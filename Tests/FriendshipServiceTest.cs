using BLL.Models;
using DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FriendshipServiceTests
{
    public class FriendshipServiceTests
    {
        private readonly FriendshipService _service;
        private readonly ApplicationDbContext _context;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<ILogger<FriendshipService>> _loggerMock;

        public FriendshipServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _userManagerMock = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
            _loggerMock = new Mock<ILogger<FriendshipService>>();

            _service = new FriendshipService(_userManagerMock.Object, _context, _loggerMock.Object);
        }

       
        [Fact]
        public async Task GetFriendsAsync_ReturnsEmptyList_WhenNoFriends()
        {
            // Arrange
            var userId = 1;

            // Act
            var result = await _service.GetFriendsAsync(userId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchUserByName_ReturnsUsers_WhenNameMatches()
        {
            // Arrange
            var user = new User { Id = 1, UserName = "TestUser" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _userManagerMock.Setup(um => um.IsInRoleAsync(user, "Blocked")).ReturnsAsync(false);

            // Act
            var result = await _service.SearchUserByName("Test");

            // Assert
            Assert.Single(result);
            Assert.Equal("TestUser", result[0].UserName);
        }

        [Fact]
        public async Task SearchUserByName_ReturnsEmptyList_WhenNameIsEmpty()
        {
            // Arrange
            // No setup needed

            // Act
            var result = await _service.SearchUserByName("");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task Follow_CreatesFriendship_WhenNoExistingFriendship()
        {
            // Arrange
            var userId1 = 1;
            var userId2 = 2;

            // Act
            await _service.Follow(userId1, userId2);
            var friendship = await _context.Friendships.FirstOrDefaultAsync(f => f.User1Id == userId1 && f.User2Id == userId2);

            // Assert
            Assert.NotNull(friendship);
            Assert.Equal(1, friendship.Request);
        }

        [Fact]
        public async Task Unfollow_UpdatesFriendship_WhenFriendshipExists()
        {
            // Arrange
            var userId1 = 1;
            var userId2 = 2;
            _context.Friendships.Add(new Friendship { User1Id = userId1, User2Id = userId2, Request = 0 });
            await _context.SaveChangesAsync();

            // Act
            await _service.Unfollow(userId1, userId2);
            var friendship = await _context.Friendships.FirstOrDefaultAsync(f => f.User1Id == userId2 && f.User2Id == userId1);

            // Assert
            Assert.NotNull(friendship);
            Assert.Equal(1, friendship.Request);
        }

        [Fact]
        public async Task AcceptRequest_UpdatesRequestStatus_WhenRequestExists()
        {
            // Arrange
            var userId1 = 1;
            var userId2 = 2;
            _context.Friendships.Add(new Friendship { User1Id = userId2, User2Id = userId1, Request = 1 });
            await _context.SaveChangesAsync();

            // Act
            await _service.AcceptRequest(userId1, userId2);
            var friendship = await _context.Friendships.FirstOrDefaultAsync(f => f.User1Id == userId2 && f.User2Id == userId1);

            // Assert
            Assert.NotNull(friendship);
            Assert.Equal(0, friendship.Request);
        }

        [Fact]
        public async Task Block_SetsRequestToBlocked_WhenFriendshipExists()
        {
            // Arrange
            var userId1 = 1;
            var userId2 = 2;
            _context.Friendships.Add(new Friendship { User1Id = userId2, User2Id = userId1, Request = 0 });
            await _context.SaveChangesAsync();

            // Act
            await _service.Block(userId1, userId2);
            var friendship = await _context.Friendships.FirstOrDefaultAsync(f => f.User1Id == userId2 && f.User2Id == userId1);

            // Assert
            Assert.NotNull(friendship);
            Assert.Equal(-1, friendship.Request);
        }

        [Fact]
        public async Task UnBlock_ResetsRequestStatus_WhenBlockedFriendshipExists()
        {
            // Arrange
            var userId1 = 1;
            var userId2 = 2;
            _context.Friendships.Add(new Friendship { User1Id = userId2, User2Id = userId1, Request = -1 });
            await _context.SaveChangesAsync();

            // Act
            await _service.UnBlock(userId1, userId2);
            var friendship = await _context.Friendships.FirstOrDefaultAsync(f => f.User1Id == userId2 && f.User2Id == userId1);

            // Assert
            Assert.NotNull(friendship);
            Assert.Equal(0, friendship.Request);
        }

        [Fact]
        public async Task FriendshipRequestList_ReturnsPendingRequests_WhenRequestsExist()
        {
            // Arrange
            var userId = 1;
            var requester = new User { Id = 2, UserName = "Requester" };
            _context.Users.Add(requester);
            _context.Friendships.Add(new Friendship { User1Id = 2, User2Id = userId, Request = 1 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.FriendshipRequestList(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal(2, result[0].Id);
        }
    }
}