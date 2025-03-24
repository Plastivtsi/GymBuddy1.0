using DAL.Models;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class TrainingHistoryRepository : ITrainingHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public TrainingHistoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TrainingHistory>> GetAllAsync()
        {
            return await _context.TrainingHistories.Include(t => t.Exercises).ToListAsync();
        }

        public async Task<TrainingHistory> GetByIdAsync(int id)
        {
            return await _context.TrainingHistories.Include(t => t.Exercises)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<TrainingHistory>> GetFilteredTrainingsAsync()
        {
            return await _context.TrainingHistories
                .Include(t => t.Exercises)
                .Where(th => th.Exercises.Any(e => e.Weight > 0))
                .ToListAsync();
        }

        public async Task AddAsync(TrainingHistory trainingHistory)
        {
            await _context.TrainingHistories.AddAsync(trainingHistory);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TrainingHistory trainingHistory)
        {
            _context.TrainingHistories.Update(trainingHistory);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var history = await _context.TrainingHistories.FindAsync(id);
            if (history != null)
            {
                _context.TrainingHistories.Remove(history);
                await _context.SaveChangesAsync();
            }
        }
    }
}
