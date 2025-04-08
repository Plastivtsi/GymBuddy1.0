using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using System;

namespace PL.Controllers
{
    [Route("Training/StartTraining")] // Додаємо маршрут
    public class StartTrainingController : Controller
    {
        private readonly ITrainingService _trainingService;
        private readonly ILogger<StartTrainingController> _logger;
        private readonly UserManager<User> _userManager;

        public StartTrainingController(
            ITrainingService trainingService,
            ILogger<StartTrainingController> logger,
            UserManager<User> userManager)
        {
            _trainingService = trainingService;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet("{id}")] // Вказуємо параметр id у маршруті
        public async Task<IActionResult> Index(int id)
        {
            var training = await _trainingService.SearchTrainingsAsync(null, null, null)
                .ContinueWith(t => t.Result.FirstOrDefault(tr => tr.Id == id));

            if (training == null)
            {
                _logger.LogWarning("Training with ID {TrainingId} not found", id);
                return NotFound();
            }

            return View("~/Views/Training/StartTraining.cshtml", training);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id, Training updatedTraining)
        {
            if (id != updatedTraining.Id)
            {
                return NotFound();
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var originalTraining = await _trainingService.SearchTrainingsAsync(null, null, null)
                    .ContinueWith(t => t.Result.FirstOrDefault(tr => tr.Id == id));

                if (originalTraining == null)
                {
                    return NotFound();
                }

                var newTraining = new Training
                {
                    Name = originalTraining.Name,
                    Date = DateTime.Now,
                    Time = originalTraining.Time,
                    Description = originalTraining.Description,
                    UserId = user.Id,
                    Template = false,
                    Exercises = updatedTraining.Exercises.Select(e => new Exercise
                    {
                        Name = e.Name,
                        Weight = e.Weight,
                        Repetitions = e.Repetitions,
                        Notes = e.Notes,
                        Template = false
                    }).ToList()
                };

                await _trainingService.CreateTrainingAsync(newTraining);
                _logger.LogInformation("Training completed and saved with ID {TrainingId}", newTraining.Id);

                return RedirectToAction("Index", "Training");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing training with ID {TrainingId}", id);
                ModelState.AddModelError("", "Не вдалося завершити тренування. Спробуйте ще раз.");
                return View("~/Views/Training/StartTraining.cshtml", updatedTraining);
            }
        }
    }
}