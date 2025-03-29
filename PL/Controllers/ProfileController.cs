using Microsoft.AspNetCore.Mvc;
using DAL.Models;
using BLL.Models;
using BLL.Service;
using DAL.Interfaces;

namespace YourProject.Controllers
{
    public class ProfileController : Controller
    {
        // Служба для доступу до даних користувача (можливо, вам потрібна інша реалізація)
        private readonly IUserService _userService;

        public ProfileController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: /Profile/
        public IActionResult Index()
        {
            // Отримуємо поточного користувача
            // Тут може бути ваша логіка автентифікації
            var userId = Autorization.CurrentUserId;
            var user = _userService.GetUserById(userId.ToString()); // Приклад методу

            if (user == null)
            {
                return RedirectToAction("Login", "Account"); // Якщо користувач не авторизований
            }

            return View(user);
        }

        // GET: /Profile/Edit/
        public IActionResult Edit()
        {
            var userId = Autorization.CurrentUserId;
            var user = _userService.GetUserById(userId.ToString());

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        // POST: /Profile/Edit/
        [HttpPost]
        public IActionResult Edit(User model)
        {
            if (ModelState.IsValid)
            {
                _userService.UpdateUser(model); // Оновлюємо користувача
                return RedirectToAction("Index"); // Перенаправляємо на профіль після редагування
            }

            return View(model); // Повертаємо на сторінку редагування, якщо є помилки в моделі
        }
    }
}