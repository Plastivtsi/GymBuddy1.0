using BLL.Models;
using BLL.Models.Interfaces;
using DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YourProject.Controllers;

namespace PL.Controllers
{
    public class FriendshipController : Controller
    {
        private readonly IFriendshipService _friendshipService;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context; // Added field

        public FriendshipController(UserManager<User> userManager,IFriendshipService friendshipService, ApplicationDbContext context)
        {
            _userManager = userManager;
            _friendshipService = friendshipService;
            _context = context;

        }

        public async Task<IActionResult> Index()
        { 
            var userId = Int32.Parse(_userManager.GetUserId(User));
            ViewBag.UserId = userId;
            var friends = await _friendshipService.GetFriendsAsync(userId);

            return View(friends);
        }

        public async Task<IActionResult> Search(string name)
        {
            //var userId = Autorization.CurrentUserId;

            var userId = Int32.Parse(_userManager.GetUserId(User));

            var users = await _friendshipService.SearchUserByName(name);
            var friendships = await _friendshipService.GetFriendshipRequests(userId);

            ViewBag.Friendships = friendships;
            ViewBag.UserId = userId;

            if (string.IsNullOrWhiteSpace(name))
                return View("SearchResults", new List<User>());

            var filteredUsers = users.Where(u => u.Id != userId).ToList();

            if (!filteredUsers.Any())
                ViewBag.Message = "Не знайдено користувача за цим ім'ям";

            return View("SearchResults", filteredUsers);
        }
        [HttpPost]
        public async Task<IActionResult> Unfollow(int friendId)
        {
            var userId = Int32.Parse(_userManager.GetUserId(User));
            await _friendshipService.Unfollow(userId, friendId);
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Follow(int friendId)
        {
            var userId = Int32.Parse(_userManager.GetUserId(User));
            await _friendshipService.Follow(userId, friendId);
            return RedirectToAction("Search");
        }
        public async Task<IActionResult> RequestAndBanned()
        {
            var userId = Int32.Parse(_userManager.GetUserId(User));
            var requests = await _friendshipService.FriendshipRequestList(userId);
            var banned = await _friendshipService.FriendshipBannedList(userId);
            return View(new Tuple<List<User>, List<User>>(requests, banned));
        }


        [HttpPost]
        public async Task<IActionResult> AcceptRequest(int friendId)
        {
            var userId = Int32.Parse(_userManager.GetUserId(User));
            await _friendshipService.AcceptRequest(userId, friendId);
            return RedirectToAction("RequestAndBanned");
        }

        [HttpPost]
        public async Task<IActionResult> BlockUser(int friendId)
        {
            var userId = Int32.Parse(_userManager.GetUserId(User));
            await _friendshipService.Block(userId, friendId);
            return RedirectToAction("RequestAndBanned");
        }

        [HttpPost]
        public async Task<IActionResult> UnblockUser(int friendId)
        {
            var userId = Int32.Parse(_userManager.GetUserId(User));
            await _friendshipService.UnBlock(userId, friendId);
            return RedirectToAction("RequestAndBanned");
        }

        public async Task<IActionResult> ViewFriendRecords(int friendId)
        {
            var userId = Int32.Parse(_userManager.GetUserId(User));
            var friendRecords = await _friendshipService.GetFriendExerciseRecords(friendId);

            var friend = await _context.Users.FirstOrDefaultAsync(u => u.Id == friendId);
            ViewBag.FriendName = friend?.UserName ?? "Friend";

            return View(friendRecords);
        }

        //[HttpPost]
        //public async Task<IActionResult> UnblockUser(int friendId)
        //{
        //    var friendship = await _context.Friendships
        //        .FirstOrDefaultAsync(f => f.User2Id == Autorization.CurrentUserId && f.User1Id == friendId && f.Request == -1);

        //    if (friendship != null)
        //    {
        //        _context.Friendships.Remove(friendship);
        //        await _context.SaveChangesAsync();
        //    }

        //    return RedirectToAction("FriendshipList");
        //}


    }
}

