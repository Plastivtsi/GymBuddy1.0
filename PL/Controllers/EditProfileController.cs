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
            // Отримуємо ID користувача з Claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                Log.Error("User is not authenticated.");
                return Unauthorized(); // Якщо користувач не авторизований
            }

            // Перевіряємо чи знайдений користувач
            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                Log.Warning("User with ID {UserId} not found.", userId);
                return NotFound(); // Якщо користувача не знайдено
            }

            Log.Information("Loaded user profile for editing: {UserId}", userId);
            return View(user); // Повертаємо користувача для редагування
        }

        [HttpPost]
        public IActionResult Edit([Bind("Id,Name,Email,Weight,Height")] User model)
        {
            Log.Information("Received update for user ID: {UserId}", model.Id);

            // Логуємо стан моделі для дебагу
            Log.Information("Model values: Id={Id}, Name={Name}, Email={Email}, Weight={Weight}, Height={Height}",
                model.Id, model.Name, model.Email, model.Weight, model.Height);

            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState)
                {
                    foreach (var error in modelState.Value.Errors)
                    {
                        Log.Error("Validation error for {Field}: {ErrorMessage}", modelState.Key, error.ErrorMessage);
                    }
                }
                return View(model);
            }

            try
            {
                var existingUser = _userService.GetUserById(model.Id.ToString());
                if (existingUser == null)
                {
                    Log.Warning("User with ID {UserId} not found.", model.Id);
                    return NotFound();
                }

                existingUser.Name = model.Name;
                existingUser.Email = model.Email;
                existingUser.Weight = model.Weight;
                existingUser.Height = model.Height;

                Log.Information("Updating user profile for user ID: {UserId}", model.Id);
                _userService.UpdateUser(existingUser);
                TempData["SuccessMessage"] = "Зміни збережено!";
                Log.Information("User profile updated successfully for user ID: {UserId}", model.Id);
                return RedirectToAction("Profile", "Index");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while updating user profile for user ID: {UserId}", model.Id);
                TempData["ErrorMessage"] = "Сталася помилка при оновленні профілю.";
                return View(model);
            }
        }
    }
}