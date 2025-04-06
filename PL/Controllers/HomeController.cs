using Microsoft.AspNetCore.Mvc;
using PL.Models;
using System.Diagnostics;
using BLL.Service; // Додаємо для IUserService
using DAL.Models;
using DAL.Interfaces; // Додаємо для User
using Microsoft.AspNetCore.Identity;


namespace PL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<User> userManager)
        {
            _userManager = userManager;
            _logger = logger;

            //_userService = userService; // Ініціалізуємо через DI
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Profile()
        {
            // Отримуємо ID користувача з сесії
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId==0)
            {
                // Якщо користувач не авторизований, перенаправляємо на сторінку входу
                return RedirectToAction("Login", "Account");
            }

            // Отримуємо користувача через сервіс
            var user =  _userManager.GetUserAsync(User);
            //userId = user1.Id;
            //var user = _userService.GetUserById(userId.ToString());

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