using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL.Repositorie;
using DAL.Models;
using BLL.Models;
using Training = DAL.Models.Training;


namespace BLL.Service
{
    public class TrainingHistoryService : ITrainingHistoryService
    {
        private readonly ITrainingHistoryRepository _trainingHistoryRepository;

        public TrainingHistoryService(ITrainingHistoryRepository trainingHistoryRepository)
        {
            _trainingHistoryRepository = trainingHistoryRepository;
        }

        public async Task<IEnumerable<DAL.Models.Training>> GetUserTrainingHistory(int userId)
        {
            var trainings = await _trainingHistoryRepository.GetUserTrainingHistory(userId);

            return trainings.Select(t => new Training
            {
                Id = t.Id,
                Name = t.Name,
                Date = t.Date,
                Time = t.Time,
                Description = t.Description,
                UserId = t.UserId,
                Exercises = t.Exercises.Select(e => new Exercise
                {
                    Name = e.Name,
                    Weight = e.Weight,
                    Repetitions = e.Repetitions,
                    Template = e.Template
                }).ToList(),
            });
        }
    }
}
