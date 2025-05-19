using BLL.Interfaces;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PL.Controllers
{
    [Authorize]
    public class TrainingController : Controller
    {
        private readonly ITrainingService _trainingService;
        private readonly ILogger<TrainingController> _logger;

        public TrainingController(ITrainingService trainingService, ILogger<TrainingController> logger)
        {
            _trainingService = trainingService ?? throw new ArgumentNullException(nameof(trainingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Відображення особистих шаблонних тренувань
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogWarning("Unauthorized access to Index: User ID not found");
                return Unauthorized();
            }

            _logger.LogInformation("Fetching template trainings for User ID {UserId}", userId);
            var trainings = await _trainingService.GetTemplateTrainingsWithExercisesAsync(userId);
            return View(trainings);
        }

        // Відображення всіх шаблонних тренувань
        public async Task<IActionResult> AllTemplates()
        {
            _logger.LogInformation("Fetching all template trainings");
            var templates = await _trainingService.GetTemplateTrainingsWithExercisesAsync(null);
            return View(templates);
        }

        // Копіювання шаблонного тренування
        [HttpPost]
        public async Task<IActionResult> CopyTemplate(int templateTrainingId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogWarning("Unauthorized access to CopyTemplate: User ID not found");
                return Unauthorized();
            }

            try
            {
                _logger.LogInformation("Copying template training ID {TemplateTrainingId} for User ID {UserId}", templateTrainingId, userId);
                await _trainingService.CreateTrainingFromTemplateAsync(templateTrainingId, userId, null);
                TempData["SuccessMessage"] = "Тренування успішно додано до ваших шаблонів!";
                return RedirectToAction("Index");
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error copying template training ID {TemplateTrainingId}", templateTrainingId);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("AllTemplates");
            }
        }

        // Редагування тренування
        public async Task<IActionResult> Edit(int id)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogWarning("Unauthorized access to Edit: User ID not found");
                return Unauthorized();
            }

            _logger.LogInformation("Fetching training ID {TrainingId} for User ID {UserId}", id, userId);
            var training = (await _trainingService.SearchTrainingsAsync(null, null, userId))
                .FirstOrDefault(t => t.Id == id);

            if (training == null)
            {
                _logger.LogWarning("Training ID {TrainingId} not found for User ID {UserId}", id, userId);
                TempData["ErrorMessage"] = "Тренування не знайдено";
                return RedirectToAction("Index");
            }

            // Ініціалізація Exercises, якщо null
            training.Exercises = training.Exercises ?? new List<Exercise>();
            _logger.LogInformation("Training ID {TrainingId} loaded with {ExerciseCount} exercises", id, training.Exercises.Count);

            return View(training);
        }

        // Обробка збереження змін
        [HttpPost]
        public async Task<IActionResult> Edit(Training training)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogWarning("Unauthorized access to Edit POST: User ID not found");
                return Unauthorized();
            }

            _logger.LogInformation("Received training data for update: {@Training}", training);

            // Ігнорування навігаційних властивостей для валідації
            ModelState.Remove("User");
            ModelState.Remove("Exercises");
            if (training.Exercises != null)
            {
                for (int i = 0; i < training.Exercises.Count; i++)
                {
                    ModelState.Remove($"Exercises[{i}].Training");
                }
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("ModelState invalid for Training ID {TrainingId}. Errors: {Errors}", training.Id, string.Join("; ", errors));
                TempData["ErrorMessage"] = "Помилка валідації: " + string.Join("; ", errors);
                training.Exercises = training.Exercises ?? new List<Exercise>();
                return View(training);
            }

            try
            {
                training.UserId = userId;
                training.Template = true; // Ensure Template is always true
                training.Exercises = training.Exercises ?? new List<Exercise>();
                // Фільтрація порожніх вправ
                training.Exercises = training.Exercises
                    .Where(e => !string.IsNullOrEmpty(e.Name))
                    .Select(e => new Exercise
                    {
                        Id = e.Id,
                        TrainingId = training.Id,
                        Name = e.Name,
                        Weight = e.Weight,
                        Repetitions = e.Repetitions,
                        Notes = e.Notes,
                        Template = e.Template
                    }).ToList();

                _logger.LogInformation("Updating training ID {TrainingId} with {ExerciseCount} exercises", training.Id, training.Exercises.Count);
                await _trainingService.UpdateTrainingAsync(training);
                TempData["SuccessMessage"] = "Тренування успішно оновлено!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating training ID {TrainingId}", training.Id);
                TempData["ErrorMessage"] = $"Помилка при оновленні тренування: {ex.Message}";
                training.Exercises = training.Exercises ?? new List<Exercise>();
                return View(training);
            }
        }
    }
}