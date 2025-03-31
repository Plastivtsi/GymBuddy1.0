using DAL.Models;
using DAL.Repositorie;
using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.Models;
using BLL.Service;
using DAL.Interfaces;

namespace PL.Controllers
{
    public class TrainingController : Controller
    {
        private readonly ITrainingRepository _trainingRepository;
        private readonly IUserService _userService; 

        // Оновіть конструктор, щоб впровадити IUserService
        public TrainingController(ITrainingRepository trainingRepository, IUserService userService)
        {
            _trainingRepository = trainingRepository;
            _userService = userService; 
        }

        public async Task<IActionResult> Index()
        {
            var userId = Autorization.CurrentUserId;
            var user = _userService.GetUserById(userId.ToString());

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var trainings = await _trainingRepository.GetTrainingsByUserId(userId);
            return View(trainings);
        }

        public async Task<IActionResult> Start(int id)
        {
            var training = await _trainingRepository.SearchTrainingsAsync(null, null, null)
                .ContinueWith(t => t.Result.FirstOrDefault(tr => tr.Id == id));

            if (training == null)
            {
                return NotFound();
            }

            // Логіка для початку тренування (наприклад, збереження часу початку)
            // Тут можна додати запис у БД або перенаправлення на сторінку виконання
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var training = await _trainingRepository.SearchTrainingsAsync(null, null, null)
                .ContinueWith(t => t.Result.FirstOrDefault(tr => tr.Id == id));

            if (training == null)
            {
                return NotFound();
            }

            // Перенаправлення на сторінку редагування
            return View("Edit", training); // Потрібно створити Edit.cshtml
        }
    }
}