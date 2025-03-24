using BLL.Interfaces;
using DAL;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                .Where(t => t.Template == true) // Фільтруємо тренування, де Template = true
                .Include(t => t.Exercises)     // Включаємо пов'язані вправи
                .ToListAsync();
        }

        public async Task<Training> CreateTrainingFromTemplateAsync(int templateTrainingId, int userId, List<Exercise> updatedExercises)
        {
            // Отримуємо шаблон тренування
            var templateTraining = await _context.Trainings
                .Include(t => t.Exercises)
                .FirstOrDefaultAsync(t => t.Id == templateTrainingId && t.Template == true);

            if (templateTraining == null)
            {
                throw new Exception("Шаблон тренування не знайдено.");
            }

            // Створюємо нове тренування на основі шаблону
            var newTraining = new Training
            {
                Name = templateTraining.Name,
                Date = DateTime.Now, // Поточна дата
                Time = templateTraining.Time,
                Description = templateTraining.Description,
                UserId = userId,
                Template = false, // Нове тренування не є шаблоном
                Exercises = new List<Exercise>()
            };

            // Додаємо вправи з оновленими даними (вага, повторення, нотатки)
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
                        Template = false // Вправи в новому тренуванні не є шаблонами
                    });
                }
            }

            // Додаємо нове тренування до бази
            _context.Trainings.Add(newTraining);
            await _context.SaveChangesAsync();

            return newTraining;
        }
    }
}