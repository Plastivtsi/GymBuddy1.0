using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using BLL.Service;
using DAL.Repositorie;
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

        public IActionResult Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var history = _trainingHistoryService.GetUserTrainingHistory(userId);
            return View(history);
        }
    }
}
