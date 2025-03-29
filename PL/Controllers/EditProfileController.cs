using Microsoft.AspNetCore.Mvc;
using DAL.Models;
using DAL.Interfaces;

namespace PL.Controllers
{
    public class EditProfileController : Controller
    {
        private readonly IUserService _userService;

        public EditProfileController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Edit()
        {
            var userId = this.User?.Identity?.Name;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                return NotFound();
            }

            return View(user); // Повертаємо модель користувача для редагування
        }

        [HttpPost]
        public IActionResult Edit(User model)
        {
            if (ModelState.IsValid)
            {
                _userService.UpdateUser(model); // Оновлюємо дані користувача
                return RedirectToAction("Profile", "Index"); // Перенаправляємо на профіль
            }

            return View(model); // Повертаємо назад на сторінку редагування з помилками
        }
    }
}
