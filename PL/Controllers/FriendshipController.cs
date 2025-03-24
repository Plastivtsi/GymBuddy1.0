using BLL.Models;
using BLL.Models.Interfaces;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PL.Controllers
{
    public class FriendshipController : Controller
    {
        private readonly IFriendshipService _friendshipService;
        
        public FriendshipController(IFriendshipService friendshipService)
        {
            _friendshipService = friendshipService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = Autorization.CurrentUserId;

            var friends = await _friendshipService.GetFriendsAsync(userId);

            return View(friends);
        }
     
        public async Task<IActionResult> Search(string name)
        {
            var userId = Autorization.CurrentUserId;
            if (string.IsNullOrWhiteSpace(name))
            {
                return View("SearchResults", new List<DAL.Models.User>());
            }

            var users = await _friendshipService.SearchUserByName(name);
            users = users.Where(u => u.Id != userId).ToList();

            if (!users.Any())
            {
                ViewBag.Message = "Не знайдено користувача за цим ім'ям";
                return View("SearchResults", new List<DAL.Models.User>());
            }

            return View("SearchResults", users);  // Показуємо результати пошуку}
        }
    }
}
