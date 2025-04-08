using DAL.Models;
using DAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL.Interfaces;

namespace BLL
{
    public class TrainingRepository2 : IGetTemplateTrainings, ICreateTrainingFromTemplate
    {
        private readonly ApplicationDbContext _context;

        public TrainingRepository2(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Training>> GetTemplateTrainingsWithExercisesAsync()
        {
            return await _context.Trainings
                .Where(t => t.Template == true)
                .Include(t => t.Exercises)
                .ToListAsync();
        }

        public async Task<Training> CreateTrainingFromTemplateAsync(int templateTrainingId, int userId, List<Exercise> updatedExercises)
        {
            var templateTraining = await _context.Trainings
                .Include(t => t.Exercises)
                .FirstOrDefaultAsync(t => t.Id == templateTrainingId && t.Template == true);

            if (templateTraining == null)
            {
                throw new Exception("Шаблон тренування не знайдено.");
            }

            var newTraining = new Training
            {
                Name = templateTraining.Name,
                Date = DateTime.Now,
                Time = templateTraining.Time,
                Description = templateTraining.Description,
                UserId = userId,
                Template = false,
                Exercises = new List<Exercise>()
            };

            foreach (var updatedExercise in updatedExercises)
            {
                var templateExercise = templateTraining.Exercises.FirstOrDefault(e => e.Id == updatedExercise.Id);
                if (templateExercise != null)
                {
                    newTraining.Exercises.Add(new Exercise
                    {
                        Name = templateExercise.Name,
                        Weight = updatedExercise.Weight,
                        Repetitions = updatedExercise.Repetitions,
                        Notes = updatedExercise.Notes,
                        Template = false
                    });
                }
            }

            _context.Trainings.Add(newTraining);
            await _context.SaveChangesAsync();

            return newTraining;
        }

        // Реалізація методів ITrainingRepository для сумісності
        public async Task<List<Training>> GetTrainingsByUserId(int userId)
        {
            return await _context.Trainings
                .Where(t => t.UserId == userId)
                .Include(t => t.Exercises)
                .ToListAsync();
        }

        public async Task CreateTrainingAsync(Training training)
        {
            _context.Trainings.Add(training);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTrainingAsync(Training training)
        {
            _context.Trainings.Update(training);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Training>> SearchTrainingsAsync(string? name, DateTime? date, int? userId)
        {
            var query = _context.Trainings.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(t => t.Name.Contains(name));
            if (date.HasValue)
                query = query.Where(t => t.Date == date.Value.Date);
            if (userId.HasValue)
                query = query.Where(t => t.UserId == userId.Value);

            return await query.Include(t => t.Exercises).ToListAsync();
        }
    }
}