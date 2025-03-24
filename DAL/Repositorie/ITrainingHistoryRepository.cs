using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;

namespace DAL.Repositorie
{
    public interface ITrainingHistoryRepository
    {
        Task<IEnumerable<Training>> GetAllAsync();
        Task<Training> GetByIdAsync(int id);
        Task<IEnumerable<Training>> GetUserTrainingHistory(int userId);
        Task<IEnumerable<Training>> GetTrainingsByUserId(int userId);
        Task<IEnumerable<Training>> GetFilteredTrainingsAsync();
        Task AddAsync(Training training);
        Task UpdateAsync(Training training);
        Task DeleteAsync(int id);
    }

}
