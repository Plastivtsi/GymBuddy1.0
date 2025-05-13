using BLL.Interfaces;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace PL.Controllers
{
    [Authorize]
    public class TrainingController : Controller
    {
        private readonly ITrainingService _trainingService;
        public TrainingController(ITrainingService trainingService)
        {
            _trainingService = trainingService;
        }

        // Відображення особистих шаблонних тренувань
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }

            var trainings = await _trainingService.GetTemplateTrainingsWithExercisesAsync(userId);
            return View(trainings);
        }

        // Відображення всіх шаблонних тренувань
        public async Task<IActionResult> AllTemplates()
        {
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
                return Unauthorized();
            }

            try
            {
                await _trainingService.CreateTrainingFromTemplateAsync(templateTrainingId, userId, null);
                TempData["SuccessMessage"] = "Тренування успішно додано до ваших шаблонів!";
                return RedirectToAction("Index");
            }
            catch (ArgumentException ex)
            {
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
                return Unauthorized();
            }

            var training = (await _trainingService.SearchTrainingsAsync(null, null, userId))
                .FirstOrDefault(t => t.Id == id);

            if (training == null)
            {
                TempData["ErrorMessage"] = "Тренування не знайдено";
                return RedirectToAction("Index");
            }

            // Переконайтеся, що Exercises ініціалізовані
            if (training.Exercises == null)
            {
                training.Exercises = new List<Exercise>();
            }

            return View(training);
        }

        // Обробка збереження змін
        [HttpPost]
        public async Task<IActionResult> Edit(Training training)
        {
            if (!ModelState.IsValid)
            {
                return View(training);
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }

            try
            {
                training.UserId = userId;
                await _trainingService.UpdateTrainingAsync(training);
                TempData["SuccessMessage"] = "Тренування успішно оновлено!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Помилка при оновленні тренування: {ex.Message}";
                return View(training);
            }
        }
    }
}