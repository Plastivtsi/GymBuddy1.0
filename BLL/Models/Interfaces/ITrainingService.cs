using DAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface ITrainingService
    {
        Task<List<Training>> GetTrainingsByUserIdAsync(int userId);
        Task CreateTrainingAsync(Training training);
        Task UpdateTrainingAsync(Training training); // Додаємо метод для оновлення
        Task<IEnumerable<Training>> SearchTrainingsAsync(string? name, DateTime? date, int? userId);
        Task<List<Training>> GetUserTrainingHistoryAsync(int userId);
        Task<List<Training>> GetTemplateTrainingsWithExercisesAsync();
        Task<Training> CreateTrainingFromTemplateAsync(int templateTrainingId, int userId, List<Exercise> updatedExercises);
    }
}