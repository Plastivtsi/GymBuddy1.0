using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositorie
{
    public class TrainingHistoryRepository : ITrainingHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public TrainingHistoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Training>> GetAllAsync()
        {
            return await _context.Trainings.Include(t => t.Exercises).ToListAsync();
        }

        public async Task<Training> GetByIdAsync(int id)
        {
            return await _context.Trainings.Include(t => t.Exercises)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Training>> GetUserTrainingHistory(int userId)
        {
            return await _context.Trainings
                .Include(t => t.Exercises)
                .Where(t => t.UserId == userId && t.Exercises.Any(e => !e.Template))
                .ToListAsync();
        }

        public async Task<IEnumerable<Training>> GetFilteredTrainingsAsync()
        {
            return await _context.Trainings
                .Include(t => t.Exercises)
                .Where(t => t.Exercises.Any(e => e.Weight > 0))
                .ToListAsync();
        }

        public async Task AddAsync(Training training)
        {
            await _context.Trainings.AddAsync(training);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Training training)
        {
            _context.Trainings.Update(training);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var training = await _context.Trainings.FindAsync(id);
            if (training != null)
            {
                _context.Trainings.Remove(training);
                await _context.SaveChangesAsync();
            }
        }

        Task<IEnumerable<Training>> ITrainingHistoryRepository.GetTrainingsByUserId(int userId)
        {
            throw new NotImplementedException();
        }
    }
}
