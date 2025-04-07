using Microsoft.AspNetCore.Mvc;
using DAL.Models;
using BLL.Models;
using BLL.Service;
using DAL.Interfaces;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace YourProject.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;

        public ProfileController(UserManager<User> userManager,IUserService userService)
        {
            _userManager = userManager;
            _userService = userService;
        }

        // GET: /Profile/
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        // GET: /Profile/Edit/
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        // POST: /Profile/Edit/
        [HttpPost]
        public async Task<IActionResult> Edit(User model)
        {
            Log.Information("Received update for user ID: {UserId}", model.Id);

            Log.Information("Model values: Id={Id}, Name={Name}, Email={Email}, Weight={Weight}, Height={Height}",
                model.Id, model.UserName, model.Email, model.Weight, model.Height);

            try
            {
                var existingUser = await _userService.GetUserById(model.Id.ToString());
                if (existingUser == null)
                {
                    Log.Warning("User with ID {UserId} not found.", model.Id);
                    return NotFound();
                }

                existingUser.UserName = model.UserName;
                existingUser.Email = model.Email;
                existingUser.Weight = model.Weight;
                existingUser.Height = model.Height;

                await _userService.UpdateUser(existingUser);

                Log.Information("Updating user profile for user ID: {UserId}", model.Id);
                Log.Information("User profile updated successfully for user ID: {UserId}", model.Id);
                return RedirectToAction("Index", "Profile");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while updating user profile for user ID: {UserId}", model.Id);
                return View(model);
            }
        }
    
    }
}