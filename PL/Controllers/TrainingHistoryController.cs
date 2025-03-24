using Microsoft.AspNetCore.Mvc;
using BLL.Service;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;

namespace PL.Controllers
{
    public class TrainingHistoryController : Controller
    {
        private readonly ITrainingHistoryService _trainingHistoryService;

        public TrainingHistoryController(ITrainingHistoryService trainingHistoryService)
        {
            _trainingHistoryService = trainingHistoryService;
        }

        public async Task<IActionResult> Index()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            {
                return RedirectToAction("Login", "Account"); 
            }

            var history = await _trainingHistoryService.GetUserTrainingHistory(userId);
            return View(history);
        }
    }
}