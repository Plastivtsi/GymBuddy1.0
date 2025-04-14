using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PL.Models;
using System.Diagnostics;
using BLL.Service;
using DAL.Models;
using DAL.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace PL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public HomeController(ILogger<HomeController> logger, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;
            _logger = logger;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> HomeAdmin()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("Користувач не авторизований для доступу до HomeAdmin");
                return RedirectToAction("Login", "Account");
            }

            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    BlockedReason = user.BlockedReason,
                    IsBlocked = roles.Contains("Blocked"),
                    IsAdmin = roles.Contains("Admin")
                });
            }

            ViewBag.UserName = currentUser.UserName;
            return View(userViewModels);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult RegisterAdmin()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> RegisterAdmin(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Admin"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole<int> { Name = "Admin" });
                    }

                    await _userManager.AddToRoleAsync(user, "Admin");
                    _logger.LogInformation("Новий адміністратор {UserName} успішно зареєстрований", user.UserName);
                    return RedirectToAction("HomeAdmin");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> BlockUser(string userId, string blockedReason)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Користувач з ID {UserId} не знайдений для блокування", userId);
                return RedirectToAction("HomeAdmin");
            }

            if (!await _roleManager.RoleExistsAsync("Blocked"))
            {
                await _roleManager.CreateAsync(new IdentityRole<int> { Id = 3, Name = "Blocked" });
            }

            if (!await _userManager.IsInRoleAsync(user, "Blocked"))
            {
                await _userManager.AddToRoleAsync(user, "Blocked");
            }

            user.BlockedReason = blockedReason;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("Користувач {UserName} заблокований з причиною: {BlockedReason}", user.UserName, blockedReason);
            }
            else
            {
                _logger.LogError("Помилка при блокуванні користувача {UserName}", user.UserName);
            }

            return RedirectToAction("HomeAdmin");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UnblockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Користувач з ID {UserId} не знайдений для розблокування", userId);
                return RedirectToAction("HomeAdmin");
            }

            if (await _userManager.IsInRoleAsync(user, "Blocked"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Blocked");
            }

            user.BlockedReason = null;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("Користувач {UserName} розблокований", user.UserName);
            }
            else
            {
                _logger.LogError("Помилка при розблокуванні користувача {UserName}", user.UserName);
            }

            return RedirectToAction("HomeAdmin");
        }

        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Error");
            }

            return View(user);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class UserViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string BlockedReason { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class RegisterViewModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}