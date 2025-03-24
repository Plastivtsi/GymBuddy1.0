using Microsoft.AspNetCore.Mvc;
using DAL.Models;
using BLL.Models;

namespace PL.Controllers
{
    public class TrainingsController : Controller
    {
        private readonly ICreateTrainings _createTrainings;

        public TrainingsController(ICreateTrainings createTrainings)
        {
            _createTrainings = createTrainings ?? throw new ArgumentNullException(nameof(createTrainings));
        }

        public IActionResult Create()
        {
            return View(new Training());
        }

        [HttpPost]
        public async Task<IActionResult> CreateTraining(Training training)
        {
            if (!ModelState.IsValid)
            {
                return View("Create", training); // Явно вказуємо "Create"
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
                return View("Create", createdTraining); // Явно вказуємо "Create"
            }
            catch (Exception ex)
            {
                ViewData["Message"] = $"Помилка: {ex.Message}";
                return View("Create", training); // Явно вказуємо "Create"
            }
        }
    }
}