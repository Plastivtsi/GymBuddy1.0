using Microsoft.AspNetCore.Mvc;
using PL.Models;
using System.Diagnostics;
using BLL.Service; // Додаємо для IUserService
using DAL.Models;
using DAL.Interfaces; // Додаємо для User

namespace PL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public readonly IUserService _userService;
        public HomeController(ILogger<HomeController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService; // Ініціалізуємо через DI
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Profile()
        {
            // Отримуємо ID користувача з сесії
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                // Якщо користувач не авторизований, перенаправляємо на сторінку входу
                return RedirectToAction("Login", "Account");
            }

            // Отримуємо користувача через сервіс
            var user = _userService.GetUserById(userId);

            if (user == null)
            {
                // Якщо користувача не знайдено, показуємо помилку або перенаправляємо
                return RedirectToAction("Error");
            }

            // Передаємо модель у представлення
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
}