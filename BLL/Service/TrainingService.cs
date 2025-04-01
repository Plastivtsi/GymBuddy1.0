using DAL.Models;
using DAL.Repositorie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL.Interfaces;

namespace BLL.Service
{
    public class TrainingService : ITrainingService
    {
        private readonly ITrainingRepository _trainingRepository;

        public TrainingService(ITrainingRepository trainingRepository)
        {
            _trainingRepository = trainingRepository ?? throw new ArgumentNullException(nameof(trainingRepository));
        }

        public async Task<List<Training>> GetTrainingsByUserIdAsync(int userId)
        {
            return await _trainingRepository.GetTrainingsByUserId(userId);
        }

        public async Task CreateTrainingAsync(Training training)
        {
            if (training == null)
            {
                throw new ArgumentNullException(nameof(training));
            }

            await _trainingRepository.CreateTrainingAsync(training);
        }

        public async Task UpdateTrainingAsync(Training training)
        {
            if (training == null)
            {
                throw new ArgumentNullException(nameof(training));
            }

            await _trainingRepository.UpdateTrainingAsync(training);
        }

        public async Task<IEnumerable<Training>> SearchTrainingsAsync(string? name, DateTime? date, int? userId)
        {
            return await _trainingRepository.SearchTrainingsAsync(name, date, userId);
        }

        public async Task<List<Training>> GetUserTrainingHistoryAsync(int userId)
        {
            var trainings = await _trainingRepository.GetTrainingsByUserId(userId);
            return trainings
                .Where(t => (t.Exercises ?? new List<Exercise>()).Any(e => e.Weight > 0))
                .ToList();
        }

        public async Task<List<Training>> GetTemplateTrainingsWithExercisesAsync()
        {
            var trainings = await _trainingRepository.SearchTrainingsAsync(null, null, null);
            return trainings
                .Where(t => t.Template)
                .ToList();
        }

        public async Task<Training> CreateTrainingFromTemplateAsync(int templateTrainingId, int userId, List<Exercise> updatedExercises)
        {
            var template = (await _trainingRepository.SearchTrainingsAsync(null, null, null))
                .FirstOrDefault(t => t.Id == templateTrainingId && t.Template);

            if (template == null)
            {
                throw new ArgumentException("Template training not found");
            }

            var newTraining = new Training
            {
                Name = template.Name,
                Date = DateTime.Now,
                Time = template.Time,
                Description = template.Description,
                UserId = userId,
                Template = false,
                Exercises = updatedExercises ?? template.Exercises.Select(e => new Exercise
                {
                    Name = e.Name,
                    Weight = e.Weight,
                    Repetitions = e.Repetitions,
                    Notes = e.Notes,
                    Template = false
                }).ToList(),
            };

            await _trainingRepository.CreateTrainingAsync(newTraining);
            return newTraining;
        }
    }
}