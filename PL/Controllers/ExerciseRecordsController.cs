using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymBuddy1._0.Controllers
{
    public class ExerciseRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExerciseRecordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<object>> GetExerciseRecords(string userId)
        {
            var records = await _context.Trainings
                .Where(w => w.UserId.ToString() == userId && w.Template == false)
                .SelectMany(w => w.Exercises)
                .GroupBy(e => e.Name)
                .Select(g => new
                {
                    ExerciseName = g.Key,
                    MaxWeight = g.Max(e => e.Weight),
                    MaxReps = g.Max(e => e.Repetitions) // Змінено з Reps на Repetitions
                })
                .ToListAsync();

            return records.Cast<object>().ToList();
        }
    }
}