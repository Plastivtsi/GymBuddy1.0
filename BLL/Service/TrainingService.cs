using DAL.Models;
using DAL.Repositorie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL.Interfaces;
using Microsoft.Extensions.Logging;

namespace BLL.Service
{
    public class TrainingService : ITrainingService
    {
        private readonly ITrainingRepository _trainingRepository;
        private readonly ILogger<TrainingService> _logger;

        public TrainingService(ITrainingRepository trainingRepository, ILogger<TrainingService> logger)
        {
            _trainingRepository = trainingRepository ?? throw new ArgumentNullException(nameof(trainingRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Training>> GetTrainingsByUserIdAsync(int userId)
        {
            _logger.LogInformation("Getting trainings for User ID {UserId}", userId);
            return await _trainingRepository.GetTrainingsByUserId(userId);
        }

        public async Task CreateTrainingAsync(Training training)
        {
            if (training == null)
            {
                _logger.LogError("Training is null in CreateTrainingAsync");
                throw new ArgumentNullException(nameof(training));
            }

            _logger.LogInformation("Creating training: {@Training}", training);
            await _trainingRepository.CreateTrainingAsync(training);
            _logger.LogInformation("Training created with ID {TrainingId}", training.Id);
        }

        public async Task UpdateTrainingAsync(Training training)
        {
            if (training == null)
            {
                _logger.LogError("Training is null in UpdateTrainingAsync");
                throw new ArgumentNullException(nameof(training));
            }

            _logger.LogInformation("Updating training with ID {TrainingId}", training.Id);
            await _trainingRepository.UpdateTrainingAsync(training);
            _logger.LogInformation("Training ID {TrainingId} updated successfully", training.Id);
        }

        public async Task<IEnumerable<Training>> SearchTrainingsAsync(string? name, DateTime? date, int? userId)
        {
            _logger.LogInformation("Searching trainings with Name: {Name}, Date: {Date}, UserId: {UserId}", name, date, userId);
            return await _trainingRepository.SearchTrainingsAsync(name, date, userId);
        }

        public async Task<List<Training>> GetUserTrainingHistoryAsync(int userId)
        {
            _logger.LogInformation("Getting training history for User ID {UserId}", userId);
            var trainings = await _trainingRepository.GetTrainingsByUserId(userId);
            var result = trainings
                .Where(t => (t.Exercises ?? new List<Exercise>()).Any(e => e.Weight > 0))
                .ToList();
            _logger.LogInformation("Found {TrainingCount} trainings in history for User ID {UserId}", result.Count, userId);
            return result;
        }

        public async Task<List<Training>> GetTemplateTrainingsWithExercisesAsync(int? userId = null)
        {
            _logger.LogInformation("Getting template trainings for User ID {UserId}", userId);
            var trainings = await _trainingRepository.SearchTrainingsAsync(null, null, userId);
            var result = trainings
                .Where(t => t.Template)
                .ToList();
            _logger.LogInformation("Found {TrainingCount} template trainings", result.Count);
            return result;
        }

        public async Task<Training> CreateTrainingFromTemplateAsync(int templateTrainingId, int userId, List<Exercise> updatedExercises)
        {
            _logger.LogInformation("Creating training from template ID {TemplateTrainingId} for User ID {UserId}", templateTrainingId, userId);
            var template = (await _trainingRepository.SearchTrainingsAsync(null, null, null))
                .FirstOrDefault(t => t.Id == templateTrainingId && t.Template);

            if (template == null)
            {
                _logger.LogWarning("Template training ID {TemplateTrainingId} not found", templateTrainingId);
                throw new ArgumentException("Шаблон тренування не знайдено");
            }

            var newTraining = new Training
            {
                Name = template.Name,
                Date = DateTime.UtcNow,
                Time = template.Time,
                Description = template.Description,
                UserId = userId,
                Template = true,
                Exercises = updatedExercises ?? template.Exercises.Select(e => new Exercise
                {
                    Name = e.Name,
                    Weight = e.Weight,
                    Repetitions = e.Repetitions,
                    Notes = e.Notes,
                    Template = true
                }).ToList()
            };

            await _trainingRepository.CreateTrainingAsync(newTraining);
            _logger.LogInformation("Training created from template with ID {TrainingId}", newTraining.Id);
            return newTraining;
        }
    }
}