using Microsoft.AspNetCore.Mvc;
using BLL.Service;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using BLL.Models;
using DAL.Models;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace PL.Controllers
{
    public class TrainingHistoryController : Controller
    {
        private readonly ITrainingHistoryService _trainingHistoryService;
        private readonly ILogger<TrainingHistoryController> _logger;
        private readonly UserManager<User> _userManager;

        public TrainingHistoryController(UserManager<User> userManager,ITrainingHistoryService trainingHistoryService, ILogger<TrainingHistoryController> logger)
        {
            _userManager = userManager;
            _trainingHistoryService = trainingHistoryService;
            _logger = logger;
        }

        // GET: TrainingHistory/Index
        public async Task<IActionResult> Index()
        {
            // Отримуємо userId з claims
            string userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out userId))
            {
                var user = await _userManager.GetUserAsync(User);
                userId = user.Id;
                if (userId == 0) // Якщо немає користувача, перенаправляємо на сторінку входу
                {
                    _logger.LogError("User is not authenticated and Autorization.CurrentUserId is not set.");
                    return RedirectToAction("Login", "Account");
                }

                _logger.LogWarning($"Could not retrieve user ID from claims. Using Autorization.CurrentUserId = {userId}.");
            }

            try
            {
                var history = await _trainingHistoryService.GetUserTrainingHistory(userId);

                // Перетворюємо DAL.Training в BLL.TrainingHistoryModel
                var historyModels = history.Select(training => new TrainingHistoryModel
                {
                    Id = training.Id,
                    Name = training.Name,
                    Date = training.Date,
                    Time = training.Time,
                    Description = training.Description,
                    UserId = training.UserId,
                    Exercises = training.Exercises.Select(exercise => new Exercise
                    {
                        Name = exercise.Name,
                        Weight = exercise.Weight,
                        Repetitions = exercise.Repetitions
                    }).ToList(),
                }).ToList();

                // Повертаємо перетворену модель у представлення
                return View("~/Views/TrainingHistory/History.cshtml", historyModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving training history.");
                return View("Error", new { message = "Не вдалося отримати історію тренувань." });
            }
        }
    }
}
