using DAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositorie
{
    public interface ITrainingRepository
    {
        Task<IEnumerable<Training>> SearchTrainingsAsync(string? name, DateTime? date, int? userId);
        Task<List<Training>> GetTrainingsByUserId(int userId);
        Task CreateTrainingAsync(Training training); // Додаємо метод для створення тренування
    }
}