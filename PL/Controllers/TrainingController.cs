using BLL.Service;
using BLL.Service;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GymBuddy.MVC.Controllers
{
    public class TrainingController : Controller
    {
        private readonly TrainingService _trainingService;

        public TrainingController(TrainingService trainingService)
        {
            _trainingService = trainingService;
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? name, DateTime? date, int? userId)
        {
            var trainings = await _trainingService.SearchTrainingsAsync(name, date, userId);
            return View(trainings);
        }
    }
}
