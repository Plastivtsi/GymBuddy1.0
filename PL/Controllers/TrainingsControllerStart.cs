using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using DAL;
using DAL.Models;
using Microsoft.EntityFrameworkCore; // Додаємо для Include
using System.Threading.Tasks;
using System.Collections.Generic;
using BLL.Models;

namespace PL.Controllers
{
    public class TrainingsControllerStart : Controller
    {
        private readonly ICreateTrainings _createTrainings;
        private readonly IGetTemplateTrainings _getTemplateTrainings;
        private readonly ICreateTrainingFromTemplate _createTrainingFromTemplate;
        private readonly ApplicationDbContext _context; // Додаємо контекст

        public TrainingsControllerStart(
            ICreateTrainings createTrainingsService,
            IGetTemplateTrainings getTemplateTrainingsService,
            ICreateTrainingFromTemplate createTrainingFromTemplateService,
            ApplicationDbContext dbContext)
        {
            this._createTrainings = createTrainingsService ?? throw new ArgumentNullException(nameof(createTrainingsService));
            this._getTemplateTrainings = getTemplateTrainingsService ?? throw new ArgumentNullException(nameof(getTemplateTrainingsService));
            this._createTrainingFromTemplate = createTrainingFromTemplateService ?? throw new ArgumentNullException(nameof(createTrainingFromTemplateService));
            this._context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        [HttpGet]
        public IActionResult CreateTraining()
        {
            return View("CreateTraining", new DAL.Models.Training());
        }

        [HttpPost]
        public async Task<IActionResult> CreateTrainingPost(DAL.Models.Training training) // Перейменовано на CreateTrainingPost
        {
            if (!ModelState.IsValid)
            {
                return View("CreateTraining", training);
            }

            try
            {
                var createdTraining = await _createTrainings.CreateNewTrainingAsync(
                    training.Name,
                    training.Date,
                    training.Time,
                    training.Description,
                    training.UserId);

                ViewData["Message"] = $"Тренування створено успішно! ID: {createdTraining.Id}";
                return View("CreateTraining", createdTraining);
            }
            catch (Exception ex)
            {
                ViewData["Message"] = $"Помилка: {ex.Message}";
                return View("CreateTraining", training);
            }
        }

        [HttpGet]
        public async Task<IActionResult> StartTraining()
        {
            var templateTrainings = await _getTemplateTrainings.GetTemplateTrainingsWithExercisesAsync();
            return View(templateTrainings);
        }

        [HttpGet]
        public async Task<IActionResult> SelectTraining(int id)
        {
            var templateTraining = await _context.Trainings
                .Include(t => t.Exercises)
                .FirstOrDefaultAsync(t => t.Id == id && t.Template == true);

            if (templateTraining == null)
            {
                return NotFound();
            }

            return View(templateTraining);
        }

        [HttpPost]
        public async Task<IActionResult> CompleteTraining(int templateTrainingId, List<Exercise> updatedExercises)
        {
            try
            {
                // Припускаємо, що UserId отримується з автентифікації (наприклад, через Claims)
                int userId = 1; // Замініть на реальний UserId

                var newTraining = await _createTrainingFromTemplate.CreateTrainingFromTemplateAsync(
                    templateTrainingId,
                    userId,
                    updatedExercises);

                return RedirectToAction("TrainingCompleted", new { id = newTraining.Id });
            }
            catch (Exception ex)
            {
                ViewData["Message"] = $"Помилка: {ex.Message}";
                return View("SelectTraining", await _context.Trainings.Include(t => t.Exercises).FirstOrDefaultAsync(t => t.Id == templateTrainingId));
            }
        }

        [HttpGet]
        public async Task<IActionResult> TrainingCompleted(int id)
        {
            var training = await _context.Trainings
                .Include(t => t.Exercises)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (training == null)
            {
                return NotFound();
            }

            return View(training);
        }
    }
}