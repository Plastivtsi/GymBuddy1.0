using Microsoft.AspNetCore.Mvc;
using DAL.Models;
using BLL.Service;
using Serilog;


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
        public async Task<IActionResult> Edit()
        {
            var userId = this.User?.Identity?.Name;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(); // Якщо користувач не авторизований
            }

            var user = await _userService.GetUserById(userId);
            if (user == null)
            {
                return NotFound(); // Якщо користувача не знайдено
            }

            return View(user); // Повертаємо модель користувача для редагування
        }

        [HttpPost]
        public async Task<IActionResult> Edit(User model)
        {
            Log.Information("Отримано оновлення для користувача ID: {UserId}", model.Id);

            if (ModelState.IsValid)
            {
                _userService.UpdateUser(model);
                TempData["SuccessMessage"] = "Зміни збережено!";
                return RedirectToAction("Profile", "Index");
            }

            return View(model);
        }
    }
}

