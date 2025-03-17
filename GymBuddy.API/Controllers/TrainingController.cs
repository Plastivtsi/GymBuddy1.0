using BLL.Service;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymBuddy.API.Controllers
{
    [ApiController]
    [Route("api/trainings")]
    public class TrainingController : ControllerBase
    {
        private readonly TrainingService _trainingService;

        public TrainingController(TrainingService trainingService)
        {
            _trainingService = trainingService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchTrainings([FromQuery] string? name, [FromQuery] DateTime? date, [FromQuery] int? userId)
        {
            var trainings = await _trainingService.SearchTrainingsAsync(name, date, userId);
            return Ok(trainings);
        }
    }
}
