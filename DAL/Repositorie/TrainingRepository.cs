using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositorie
{
    public interface ITrainingRepository
    {
        Task<IEnumerable<Training>> SearchTrainingsAsync(string? name, DateTime? date, int? userId);
        Task<List<Training>> GetTrainingsByUserId(int userId);
    }

    public class TrainingRepository : ITrainingRepository
    {
        private readonly DbContext _context;

        public TrainingRepository(DbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Training>> SearchTrainingsAsync(string? name, DateTime? date, int? userId)
        {
            var query = _context.Set<Training>().Include(t => t.Exercises).AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(t => t.Name.Contains(name));

            if (date.HasValue)
                query = query.Where(t => t.Date.Date == date.Value.Date);

            if (userId.HasValue)
                query = query.Where(t => t.UserId == userId.Value);

            return await query.ToListAsync();
        }
        public async Task<List<Training>> GetTrainingsByUserId(int userId)
        {
            return await _context.Set<Training>()
                .Include(t => t.Exercises) // Додаємо вправи
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

    }
}
