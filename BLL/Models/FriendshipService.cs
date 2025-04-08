using BLL.Models.Interfaces;
using DAL.Models;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.AspNetCore.Identity;


//userId1 - той хто надіслав 
// userId2 - кому надіслали
namespace BLL.Models
{
    public class FriendshipService : IFriendshipService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FriendshipService> _logger;
        private readonly UserManager<User> _userManager;

        public FriendshipService(UserManager<User> userManager,ApplicationDbContext context, ILogger<FriendshipService> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public FriendshipService(ApplicationDbContext context)
        {
        }

        public async Task<List<DAL.Models.User>> GetFriendsAsync(int userId)
        {
            try
            {
                var friends =  _context.Friendships
                    .Where(f => (f.User1Id == userId || f.User2Id == userId) && f.Request == 0)
                    .Select(f => f.User1Id == userId ? f.User2 : f.User1)
                    .ToList();

                var nonBlockedFriends = new List<DAL.Models.User>();
                foreach (var friend in friends)
                {
                    var isBlocked = await _userManager.IsInRoleAsync(friend, "Blocked");
                    if (!isBlocked)
                    {
                        nonBlockedFriends.Add(friend);
                    }
                }

                return nonBlockedFriends.Any() ? nonBlockedFriends : new List<DAL.Models.User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка отримання списку друзів для користувача {userId}: {ex.Message}");
                return new List<DAL.Models.User>();
            }
        }

        public async Task<List<User>> SearchUserByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new List<User>();

            var friends =  await _context.Users
                .Where(u => u.UserName.Contains(name))
                .ToListAsync();
            var nonBlockedFriends = new List<DAL.Models.User>();
            foreach (var friend in friends)
            {
                var isBlocked = await _userManager.IsInRoleAsync(friend, "Blocked");
                if (!isBlocked)
                {
                    nonBlockedFriends.Add(friend);
                }
            }

            return nonBlockedFriends.Any() ? nonBlockedFriends : new List<DAL.Models.User>();
        }

        public async Task Unfollow(int userId1, int userId2)
        {
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.User1Id == userId1 && f.User2Id == userId2) ||
                    (f.User1Id == userId2 && f.User2Id == userId1));

            if (friendship != null)
            {
                friendship.User1Id = userId2;
                friendship.User2Id = userId1;
                friendship.Request = 1;
            }

            await _context.SaveChangesAsync();

        }
        public async Task Follow(int userId1, int userId2)
        {
            var existingFriendship = await _context.Friendships
            .FirstOrDefaultAsync(f =>
                (f.User1Id == userId1 && f.User2Id == userId2) ||
                (f.User1Id == userId2 && f.User2Id == userId1));

            if (existingFriendship == null)
            {
                var friendship = new Friendship
                {
                    User1Id = userId1,
                    User2Id = userId2,
                    Request = 1
                };
                _context.Friendships.Add(friendship);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Запит на дружбу надіслано");
            }
            else
            {
                // Якщо запис уже існує, можна або пропустити, або оновити статус
                if (existingFriendship.Request != 0) // Якщо це не підтверджена дружба
                {
                    _logger.LogWarning("Запит на дружбу між {User1Id} та {User2Id} уже існує", userId1, userId2);
                    // Тут можна додати логіку для оновлення статусу, якщо потрібно
                }
            }

        }
        public async Task AcceptRequest(int userId1, int userId2)
        {
            var friendship = await _context.Friendships
            .FirstOrDefaultAsync(f =>
                (f.User1Id == userId2 && f.User2Id == userId1));

            if (friendship != null)
            {
                friendship.Request = 0;
            }

            await _context.SaveChangesAsync();
        }
        public async Task Block(int userId1, int userId2)
        {
            var friendship = await _context.Friendships
            .FirstOrDefaultAsync(f =>
                (f.User1Id == userId2 && f.User2Id == userId1));

            if (friendship != null)
            {
                friendship.Request = -1;
            }

            await _context.SaveChangesAsync();
        }
        public async Task UnBlock(int userId1, int userId2)
        {
            var friendship = await _context.Friendships
            .FirstOrDefaultAsync(f =>
                (f.User1Id == userId2 && f.User2Id == userId1 && f.Request == -1));

            if (friendship != null)
            {
                friendship.Request = 0;
            }

            await _context.SaveChangesAsync();
        }


        public async Task<List<DAL.Models.Friendship>> GetFriendshipRequests(int userId)
        {
            return await _context.Friendships
                .Where(f => f.User1Id == userId || f.User2Id == userId) // Враховуємо обидва варіанти
                .ToListAsync();
        }
        public async Task<List<DAL.Models.User>> FriendshipRequestList(int userId)
        {
            var existingRequest = await _context.Friendships
               .Where(f => (f.User2Id == userId && f.Request == 1))
               .Select(f => f.User1Id).ToListAsync();
            List<DAL.Models.User> Userlist = new List<User>();
            foreach (var user in existingRequest)
            {
                var u = await _context.Users.FirstOrDefaultAsync(u => u.Id == user);
                if (u != null)
                {
                    Userlist.Add(u);
                }
            }
            return Userlist;
        }
        public async Task<List<DAL.Models.User>> FriendshipBannedList(int userId)
        {
            var existingRequest = await _context.Friendships
               .Where(f => (f.User2Id == userId && f.Request == -1))
               .Select(f => f.User1Id).ToListAsync();
            List<DAL.Models.User> Userlist = new List<User>();
            foreach (var user in existingRequest)
            {
                var u = await _context.Users.FirstOrDefaultAsync(u => u.Id == user);
                if (u != null)
                {
                    Userlist.Add(u);
                }
            }
            return Userlist;
        }




    }


}
