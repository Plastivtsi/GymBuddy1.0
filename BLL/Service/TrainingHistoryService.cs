using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Repositorie;
using DAL.Models;


namespace BLL.Services
{
    public class TrainingHistoryService : ITrainingHistoryService
    {
        private readonly ITrainingHistoryRepository _trainingHistoryRepository;

        public TrainingHistoryService(ITrainingHistoryRepository trainingHistoryRepository)
        {
            _trainingHistoryRepository = trainingHistoryRepository;
        }

        public IEnumerable<TrainingHistoryModel> GetUserTrainingHistory(int userId)
        {
            return _trainingHistoryRepository.GetUserTrainingHistory(userId)
                .Select(t => new TrainingHistoryModel
                {
                    Id = t.Id,
                    TrainingDate = t.TrainingDate,
                    Duration = t.Duration,
                    Exercises = t.Exercises.Select(e => new ExerciseModel
                    {
                        Name = e.Name,
                        Weight = e.Weight,
                        Reps = e.Reps
                    }).ToList()
                });
        }
    }
}

