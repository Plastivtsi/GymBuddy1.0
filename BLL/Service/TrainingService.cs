using DAL.Repositorie;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class TrainingService
    {
        private readonly ITrainingRepository _trainingRepository;

        public TrainingService(ITrainingRepository trainingRepository)
        {
            _trainingRepository = trainingRepository;
        }

        public async Task<IEnumerable<Training>> SearchTrainingsAsync(string? name, DateTime? date, int? userId)
        {
            return await _trainingRepository.SearchTrainingsAsync(name, date, userId);
        }
    }
}
