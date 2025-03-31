using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using DAL.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using BLL.Models;

namespace PL.Controllers
{
    public class TrainingCreationController : Controller
    {
        private readonly ITrainingService _trainingService;
        private readonly ILogger<TrainingCreationController> _logger;

        public TrainingCreationController(ITrainingService trainingService, ILogger<TrainingCreationController> logger)
        {
            _trainingService = trainingService;
            _logger = logger;
        }

        // GET: TrainingCreation/Create
        public IActionResult Create()
        {
            var training = new Training
            {
                Template = true,
                Exercises = new List<Exercise>()
            };
            return View("~/Views/Training/Create.cshtml", training);
        }

        // POST: TrainingCreation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Training training)
        {
            // Видаляємо помилки валідації для навігаційних властивостей
            ModelState.Remove("User");
            if (training.Exercises != null)
            {
                for (int i = 0; i < training.Exercises.Count; i++)
                {
                    ModelState.Remove($"Exercises[{i}].Training");
                }
            }

            if (ModelState.IsValid)
            {
                // Отримуємо ID поточного користувача
                string userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int userId;
                if (!int.TryParse(userIdString, out userId))
                {
                    userId = Autorization.CurrentUserId;
                    if (userId == 0) // Перевіряємо, чи встановлений CurrentUserId
                    {
                        _logger.LogError("User is not authenticated and Autorization.CurrentUserId is not set.");
                        ModelState.AddModelError("", "Ви повинні бути авторизовані для створення тренування.");
                        return View("~/Views/Training/Create.cshtml", training);
                    }
                    _logger.LogWarning($"Could not retrieve user ID from claims. Using Autorization.CurrentUserId = {userId}.");
                }

                training.UserId = userId;
                training.Template = true;

                // Автоматично встановлюємо Time на поточний час дня
                training.Time = DateTime.Now.TimeOfDay;

                if (training.Exercises != null)
                {
                    foreach (var exercise in training.Exercises)
                    {
                        exercise.Template = true;
                    }
                }

                _logger.LogInformation("Saving training to database: {@Training}", training);
                await _trainingService.CreateTrainingAsync(training);
                _logger.LogInformation("Training saved successfully.");
                return RedirectToAction("Index", "Training");
            }

            // Логуємо помилки валідації
            foreach (var modelState in ModelState)
            {
                foreach (var error in modelState.Value.Errors)
                {
                    _logger.LogError($"Validation error for {modelState.Key}: {error.ErrorMessage}");
                }
            }

            return View("~/Views/Training/Create.cshtml", training);
        }
    }
}