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
            _logger.LogInformation("Fetching trainings for User ID {UserId}", userId);
            return await _context.Trainings
                .Include(t => t.Exercises)
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        public async Task CreateTrainingAsync(Training training)
        {
            if (training == null)
            {
                _logger.LogError("Training is null in CreateTrainingAsync");
                throw new ArgumentNullException(nameof(training));
            }

            if (training.Date.HasValue && training.Date.Value.Kind != DateTimeKind.Utc)
            {
                training.Date = DateTime.SpecifyKind(training.Date.Value, DateTimeKind.Utc);
            }

            _logger.LogInformation("Creating new training: {@Training}", training);
            await _context.Trainings.AddAsync(training);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Training created with ID {TrainingId}", training.Id);
        }

        public async Task UpdateTrainingAsync(Training training)
        {
            _logger.LogInformation("Entering UpdateTrainingAsync for Training ID {TrainingId}", training.Id);

            if (training == null)
            {
                _logger.LogError("Training is null in UpdateTrainingAsync");
                throw new ArgumentNullException(nameof(training));
            }

            var existingTraining = await _context.Trainings
                .Include(t => t.Exercises)
                .FirstOrDefaultAsync(t => t.Id == training.Id);

            if (existingTraining == null)
            {
                _logger.LogWarning("Training with ID {TrainingId} not found in UpdateTrainingAsync", training.Id);
                throw new ArgumentException("Training not found", nameof(training));
            }

            _logger.LogInformation("Existing training found: {@ExistingTraining}", existingTraining);

            if (training.Date.HasValue && training.Date.Value.Kind != DateTimeKind.Utc)
            {
                training.Date = DateTime.SpecifyKind(training.Date.Value, DateTimeKind.Utc);
            }

            // Оновлення основних властивостей
            _context.Entry(existingTraining).CurrentValues.SetValues(training);

            // Видалення старих вправ
            var existingExercises = existingTraining.Exercises.ToList();
            foreach (var exercise in existingExercises)
            {
                _context.Exercises.Remove(exercise);
            }
            existingTraining.Exercises.Clear();
            _logger.LogInformation("Cleared {ExerciseCount} existing exercises for Training ID {TrainingId}", existingExercises.Count, training.Id);

            // Додавання нових вправ
            if (training.Exercises != null && training.Exercises.Any())
            {
                _logger.LogInformation("Adding {ExerciseCount} new exercises for Training ID {TrainingId}", training.Exercises.Count, training.Id);
                foreach (var exercise in training.Exercises)
                {
                    exercise.TrainingId = training.Id;
                    exercise.Id = 0; // Скидання Id для нових вправ
                    existingTraining.Exercises.Add(exercise);
                }
            }
            else
            {
                _logger.LogInformation("No exercises provided for Training ID {TrainingId}", training.Id);
            }

            _logger.LogInformation("Saving changes to database for Training ID {TrainingId}", training.Id);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Training ID {TrainingId} updated successfully", training.Id);
        }

        public async Task<IEnumerable<Training>> SearchTrainingsAsync(string? name, DateTime? date, int? userId)
        {
            _logger.LogInformation("Searching trainings with Name: {Name}, Date: {Date}, UserId: {UserId}", name, date, userId);
            IQueryable<Training> query = _context.Trainings
                .Include(t => t.Exercises);

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(t => t.Name.Contains(name));
            }

            if (date.HasValue)
            {
                var utcDate = date.Value.Kind != DateTimeKind.Utc
                    ? DateTime.SpecifyKind(date.Value, DateTimeKind.Utc)
                    : date.Value;
                query = query.Where(t => t.Date == utcDate);
            }

            if (userId.HasValue)
            {
                query = query.Where(t => t.UserId == userId);
            }

            var result = await query.ToListAsync();
            _logger.LogInformation("Found {TrainingCount} trainings", result.Count);
            return result;
        }
    }
}