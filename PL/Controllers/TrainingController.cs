using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace PL.Controllers
{
    public class TrainingController : Controller
    {
        private readonly ITrainingService _trainingService;
        private readonly ILogger<TrainingController> _logger;
        private readonly UserManager<User> _userManager;

        public TrainingController(UserManager<User> userManager, ITrainingService trainingService, ILogger<TrainingController> logger)
        {
            _userManager = userManager;
            _trainingService = trainingService;
            _logger = logger;
        }

        // Відображає лише шаблонні тренування
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Отримуємо шаблонні тренування для поточного користувача
            var trainings = await _trainingService.GetTemplateTrainingsWithExercisesAsync(user.Id);
            return View(trainings);
        }

        public async Task<IActionResult> Start(int id)
        {
            var training = await _trainingService.SearchTrainingsAsync(null, null, null)
                .ContinueWith(t => t.Result.FirstOrDefault(tr => tr.Id == id));

            if (training == null)
            {
                return NotFound();
            }

            return RedirectToAction("Index");
        }

        // GET: Відображення сторінки редагування
        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("Entering Edit GET method for Training ID {TrainingId}", id);

            var training = await _trainingService.SearchTrainingsAsync(null, null, null)
                .ContinueWith(t => t.Result.FirstOrDefault(tr => tr.Id == id));

            if (training == null)
            {
                _logger.LogWarning("Training with ID {TrainingId} not found in Edit GET method", id);
                return NotFound();
            }

            _logger.LogInformation("Training found: {@Training}", training);
            return View(training);
        }

        // POST: Обробка збереження змін
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Training training)
        {
            _logger.LogInformation("Entering Edit POST method for Training ID {TrainingId}", id);
            _logger.LogInformation("Received training data: {@Training}", training);

            if (id != training.Id)
            {
                _logger.LogWarning("Training ID mismatch: URL ID = {UrlId}, Model ID = {ModelId}", id, training.Id);
                return NotFound();
            }

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
                _logger.LogInformation("ModelState is valid. Proceeding with update for Training ID {TrainingId}", id);

                try
                {
                    var existingTraining = await _trainingService.SearchTrainingsAsync(null, null, null)
                        .ContinueWith(t => t.Result.FirstOrDefault(tr => tr.Id == id));

                    if (existingTraining == null)
                    {
                        _logger.LogWarning("Training with ID {TrainingId} not found in Edit POST method", id);
                        return NotFound();
                    }

                    _logger.LogInformation("Existing training found: {@ExistingTraining}", existingTraining);

                    training.UserId = existingTraining.UserId;
                    await _trainingService.UpdateTrainingAsync(training);

                    _logger.LogInformation("Training updated successfully with ID {TrainingId}", training.Id);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating training with ID {TrainingId}", training.Id);
                    ModelState.AddModelError("", "Не вдалося зберегти зміни. Спробуйте ще раз.");
                }
            }
            else
            {
                _logger.LogWarning("ModelState is invalid for Training ID {TrainingId}", id);
                foreach (var modelState in ModelState)
                {
                    foreach (var error in modelState.Value.Errors)
                    {
                        _logger.LogError("Validation error for {Key}: {ErrorMessage}", modelState.Key, error.ErrorMessage);
                    }
                }
            }

            _logger.LogInformation("Returning to Edit view due to errors for Training ID {TrainingId}", id);
            return View(training);
        }
    }
}