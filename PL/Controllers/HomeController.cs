using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PL.Models;
using System.Diagnostics;
using BLL.Service;
using DAL.Models;
using DAL.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly ApplicationDbContext _dbContext;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            ApplicationDbContext dbContext)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Отримуємо дані за останні 30 днів
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-29);

            // Отримуємо всі вправи користувача за цей період
            var exercises = await _dbContext.Exercises
                .Include(e => e.Training)
                .Where(e => e.Training.UserId == user.Id &&
                            e.Training.Date.HasValue &&
                            e.Training.Date.Value >= startDate &&
                            e.Training.Date.Value <= endDate)
                .ToListAsync();

            // Групуємо за датою та рахуємо суму (вага * повторення)
            var groupedData = exercises
                .GroupBy(e => e.Training.Date.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalWeight = g.Sum(e => e.Repetitions * e.Weight)
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Створюємо списки для діаграми
            var chartLabels = new List<string>();
            var chartData = new List<double>();

            // Заповнюємо дані для всіх днів у діапазоні
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                chartLabels.Add(date.ToString("dd.MM.yyyy"));

                var dataForDate = groupedData.FirstOrDefault(g => g.Date == date);
                chartData.Add(dataForDate != null ? dataForDate.TotalWeight : 0);
            }

            ViewBag.ChartLabels = chartLabels;
            ViewBag.ChartData = chartData;

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