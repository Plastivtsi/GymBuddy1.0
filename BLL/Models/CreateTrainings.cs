using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BLL.Models
{
    public class CreateTrainings
    {
        private readonly ApplicationDbContext _context;

        public CreateTrainings(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public virtual async Task<DAL.Models.Training> CreateNewTrainingAsync( // Додано virtual
            string name,
            DateTime date,
            TimeSpan time,
            string description,
            int userId)
        {
            var training = new DAL.Models.Training
            {
                Name = name,
                Date = date,
                Time = time,
                Description = description,
                UserId = userId,
                Template = true,
                Exercises = new List<Exercise>(),
            };

            try
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                if (!userExists)
                {
                    throw new Exception("Користувач з таким ID не знайдений");
                }

                _context.Trainings.Add(training);
                await _context.SaveChangesAsync();

                return training;
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка при створенні тренування: {ex.Message}");
            }
        }
    }
}