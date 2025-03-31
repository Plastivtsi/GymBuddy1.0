using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositorie
{
    public class TrainingRepository : ITrainingRepository
    {
        private readonly ApplicationDbContext _context;

        public TrainingRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Training>> GetTrainingsByUserId(int userId)
        {
            return await _context.Trainings
                .Include(t => t.Exercises)
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        public async Task CreateTrainingAsync(Training training)
        {
            if (training == null)
            {
                throw new ArgumentNullException(nameof(training));
            }

            await _context.Trainings.AddAsync(training);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Training>> SearchTrainingsAsync(string? name, DateTime? date, int? userId)
        {
            IQueryable<Training> query = _context.Trainings
                .Include(t => t.Exercises);

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(t => t.Name.Contains(name));
            }

            if (date.HasValue)
            {
                query = query.Where(t => t.Date == date);
            }

            if (userId.HasValue)
            {
                query = query.Where(t => t.UserId == userId);
            }

            return await query.ToListAsync();
        }
    }
}