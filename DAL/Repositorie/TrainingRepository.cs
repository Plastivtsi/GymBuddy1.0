using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DAL.Repositorie
{
    public class TrainingRepository : ITrainingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TrainingRepository> _logger;

        public TrainingRepository(ApplicationDbContext context, ILogger<TrainingRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Training>> GetTrainingsByUserId(int userId)
        {
            return await _context.Trainings
                .Include(t => t.Exercises)
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        public async Task CreateTrainingAsync(Training training)
        {
            if (training == null)
            {
                throw new ArgumentNullException(nameof(training));
            }

            // Перетворюємо Date у UTC перед збереженням
            if (training.Date.HasValue && training.Date.Value.Kind != DateTimeKind.Utc)
            {
                training.Date = DateTime.SpecifyKind(training.Date.Value, DateTimeKind.Utc);
            }

            await _context.Trainings.AddAsync(training);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTrainingAsync(Training training)
        {
            _logger.LogInformation("Entering UpdateTrainingAsync for Training ID {TrainingId}", training.Id);
            _logger.LogInformation("Training data to update: {@Training}", training);

            if (training == null)
            {
                _logger.LogError("Training is null in UpdateTrainingAsync");
                throw new ArgumentNullException(nameof(training));
            }

            // Знаходимо існуюче тренування разом із пов’язаними вправами
            var existingTraining = await _context.Trainings
                .Include(t => t.Exercises)
                .FirstOrDefaultAsync(t => t.Id == training.Id);

            if (existingTraining == null)
            {
                _logger.LogWarning("Training with ID {TrainingId} not found in UpdateTrainingAsync", training.Id);
                throw new ArgumentException("Training not found", nameof(training));
            }

            _logger.LogInformation("Existing training found: {@ExistingTraining}", existingTraining);

            // Перетворюємо Date у UTC перед оновленням
            if (training.Date.HasValue && training.Date.Value.Kind != DateTimeKind.Utc)
            {
                training.Date = DateTime.SpecifyKind(training.Date.Value, DateTimeKind.Utc);
            }

            // Оновлюємо основні поля тренування
            _logger.LogInformation("Updating main fields of Training ID {TrainingId}", training.Id);
            _context.Entry(existingTraining).CurrentValues.SetValues(training);

            // Оновлюємо вправи
            _logger.LogInformation("Removing old exercises for Training ID {TrainingId}", training.Id);
            var existingExercises = existingTraining.Exercises.ToList();
            foreach (var exercise in existingExercises)
            {
                _context.Exercises.Remove(exercise);
            }
            existingTraining.Exercises.Clear();

            // Додаємо нові вправи
            if (training.Exercises != null && training.Exercises.Any())
            {
                _logger.LogInformation("Adding new exercises for Training ID {TrainingId}", training.Id);
                foreach (var exercise in training.Exercises)
                {
                    exercise.TrainingId = training.Id; // Переконуємося, що TrainingId встановлений
                    exercise.Id = 0; // Скидаємо Id, щоб Entity Framework створив нові записи
                    existingTraining.Exercises.Add(exercise);
                }
            }
            else
            {
                _logger.LogInformation("No exercises to add for Training ID {TrainingId}", training.Id);
            }

            // Зберігаємо зміни
            _logger.LogInformation("Saving changes to database for Training ID {TrainingId}", training.Id);
            var changesSaved = await _context.SaveChangesAsync();
            _logger.LogInformation("Changes saved successfully for Training ID {TrainingId}. Number of changes: {ChangesSaved}", training.Id, changesSaved);
        }

        public async Task<IEnumerable<Training>> SearchTrainingsAsync(string? name, DateTime? date, int? userId)
        {
            IQueryable<Training> query = _context.Trainings
                .Include(t => t.Exercises);

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(t => t.Name.Contains(name));
            }

            if (date.HasValue)
            {
                // Перетворюємо Date у UTC для пошуку
                var utcDate = date.Value.Kind != DateTimeKind.Utc
                    ? DateTime.SpecifyKind(date.Value, DateTimeKind.Utc)
                    : date.Value;
                query = query.Where(t => t.Date == utcDate);
            }

            if (userId.HasValue)
            {
                query = query.Where(t => t.UserId == userId);
            }

            return await query.ToListAsync();
        }
    }
}