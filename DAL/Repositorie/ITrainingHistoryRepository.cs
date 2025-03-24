using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;

namespace DAL.Models
{
    public interface ITrainingHistoryRepository
    {
        Task<IEnumerable<TrainingHistory>> GetAllAsync();
        Task<TrainingHistory> GetByIdAsync(int id);
        Task<IEnumerable<TrainingHistory>> GetUserTrainingHistory(int userId);
        Task<IEnumerable<TrainingHistory>> GetFilteredTrainingsAsync();
        Task AddAsync(TrainingHistory trainingHistory);
        Task UpdateAsync(TrainingHistory trainingHistory);
        Task DeleteAsync(int id);
    }
}
