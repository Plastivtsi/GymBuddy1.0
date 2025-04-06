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
            // �������� ID ����������� � Claims
            var userId = this.User?.Identity?.Name;

            if (string.IsNullOrEmpty(userId))
            {
                Log.Error("User is not authenticated.");
                return Unauthorized(); // ���� ���������� �� �������������
            }

            var user = await _userService.GetUserById(userId);
            if (user == null)
            {
                Log.Warning("User with ID {UserId} not found.", userId);
                return NotFound(); // ���� ����������� �� ��������
            }

            Log.Information("Loaded user profile for editing: {UserId}", userId);
            return View(user); // ��������� ����������� ��� �����������
        }

        [HttpPost]
        public async Task<IActionResult> Edit(User model)
        {
            Log.Information("Received update for user ID: {UserId}", model.Id);

            // ������ ���� ����� ��� ������
            Log.Information("Model values: Id={Id}, Name={Name}, Email={Email}, Weight={Weight}, Height={Height}",
                model.Id, model.UserName, model.Email, model.Weight, model.Height);

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

                //existingUser.UserName = model.UserName;
                //existingUser.Email = model.Email;
                //existingUser.Weight = model.Weight;
                //existingUser.Height = model.Height;

                Log.Information("Updating user profile for user ID: {UserId}", model.Id);
                _userService.UpdateUser(model);
                TempData["SuccessMessage"] = "���� ���������!";
                Log.Information("User profile updated successfully for user ID: {UserId}", model.Id);
                return RedirectToAction("Profile", "Index");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while updating user profile for user ID: {UserId}", model.Id);
                TempData["ErrorMessage"] = "������� ������� ��� ��������� �������.";
                return View(model);
            }
        }
    }
}

