using Microsoft.AspNetCore.Mvc;
using DAL.Models;
using BLL.Models;
using BLL.Service;
using DAL.Interfaces;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YourProject.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(UserManager<User> userManager, IUserService userService, ApplicationDbContext context)
        {
            _userManager = userManager;
            _userService = userService;
            _context = context;
        }

        // GET: /Profile/
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var exerciseRecords = await GetExerciseRecords(user.Id.ToString());
            ViewData["ExerciseRecords"] = exerciseRecords;

            return View(user);
        }

        private async Task<List<object>> GetExerciseRecords(string userId)
        {
            var records = await _context.Trainings
                .Where(w => w.UserId.ToString() == userId && w.Template == false)
                .SelectMany(w => w.Exercises)
                .GroupBy(e => e.Name)
                .Select(g => new
                {
                    ExerciseName = g.Key,
                    MaxWeight = g.Max(e => e.Weight),
                    MaxReps = g.Max(e => e.Repetitions) // Змінено з Reps на Repetitions
                })
                .ToListAsync();

            return records.Cast<object>().ToList();
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