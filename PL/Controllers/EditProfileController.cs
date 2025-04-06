using Microsoft.AspNetCore.Mvc;
using DAL.Models;
using BLL.Interfaces;
using Serilog;
using System.Security.Claims;

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
            // ќтримуЇмо ID користувача з Claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(); // якщо користувач не авторизований
            }

            // ѕерев≥р€Їмо чи знайдений користувач
            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                return NotFound(); // якщо користувача не знайдено
            }

            return View(user); // ѕовертаЇмо користувача дл€ редагуванн€
        }

        [HttpPost]
        public IActionResult Edit(User model)
        {
            Log.Information("ќтримано оновленн€ дл€ користувача ID: {UserId}", model.Id);

            if (ModelState.IsValid)
            {
                _userService.UpdateUser(model); // ќновлюЇмо дан≥ користувача
                TempData["SuccessMessage"] = "«м≥ни збережено!";
                return RedirectToAction("Profile", "Index"); // ѕеренаправленн€ на стор≥нку проф≥лю
            }

            return View(model); // якщо модель не вал≥дна, повертаЇмо назад на форму
        }
    }
}
