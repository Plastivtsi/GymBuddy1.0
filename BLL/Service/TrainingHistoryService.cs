using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL.Repositorie;
using DAL.Models;

namespace BLL.Service
{
    public class TrainingHistoryService : ITrainingHistoryService
    {
        private readonly ITrainingHistoryRepository _trainingHistoryRepository;

        public TrainingHistoryService(ITrainingHistoryRepository trainingHistoryRepository)
        {
            _trainingHistoryRepository = trainingHistoryRepository;
        }

        public async Task<IEnumerable<TrainingHistoryModel>> GetUserTrainingHistory(int userId)
        {
            var trainings = await _trainingHistoryRepository.GetUserTrainingHistory(userId);

            return trainings.Select(t => new TrainingHistoryModel
            {
                Id = t.Id,
                Exercises = t.Exercises.Select(e => new Exercise
                {
                    Name = e.Name,
                    Weight = e.Weight,
                    Repetitions = e.Repetitions
                }).ToList()
            });
        }

        Task<IEnumerable<Training>> ITrainingHistoryService.GetUserTrainingHistory(int userId)
        {
            throw new NotImplementedException();
        }
    }
}
