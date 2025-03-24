using BLL.Models.Interfaces;
using DAL.Models;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class FriendshipService : IFriendshipService
    {
        private readonly ApplicationDbContext _context;
        public FriendshipService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DAL.Models.User>> GetFriendsAsync(int userId)
        {
            try
            {
                var friends = _context.Friendships
                    .Where(f => f.User1Id == userId || f.User2Id == userId)
                    .Select(f => f.User1Id == userId ? f.User2 : f.User1)
                    .ToList();

                return friends.Any() ? friends : new List<DAL.Models.User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка отримання списку друзів для користувача {userId}: {ex.Message}");
                return new List<DAL.Models.User>();
            }
        }
        
        public async Task<List<DAL.Models.User>> SearchUserByName(string name)
        {
            return await _context.Users
                .Where(u => u.Name.Contains(name))  
                .ToListAsync();
        }
    }
}
