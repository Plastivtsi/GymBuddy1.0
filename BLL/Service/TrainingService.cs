using DAL.Repositorie;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.Repositorie;

namespace BLL.Service
{
    public class TrainingService
    {
        private readonly ITrainingRepository _trainingRepository;
        private readonly ITrainingHistoryRepository _trainingHistoryRepository;

        public TrainingService(ITrainingRepository trainingRepository)
        {
            _trainingRepository = trainingRepository;
        }

        public async Task<IEnumerable<Training>> SearchTrainingsAsync(string? name, DateTime? date, int? userId)
        {
            return await _trainingRepository.SearchTrainingsAsync(name, date, userId);
        }
        public async Task<List<Training>> GetUserTrainingHistory(int userId)
        {
            var trainings = await _trainingRepository.GetTrainingsByUserId(userId) ?? new List<Training>();

            return trainings
            .Where(t => (t.Exercises ?? new List<Exercise>()).Any(e => e.Weight > 0))
            .ToList();
        }
    }
}
