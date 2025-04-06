using DAL.Interfaces;
using DAL.Models;
using Serilog;

namespace DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext context;

        public UserRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public User? GetById(int id)
        {
            return this.context.Users.FirstOrDefault(u => u.Id == id); // Пошук за ID
        }

        public void Update(User user)
        {
            try
            {
                var existingUser = context.Users.FirstOrDefault(u => u.Id == user.Id);
                if (existingUser == null)
                {
                    Log.Warning("User with ID {UserId} not found in database.", user.Id);
                    throw new Exception($"User with ID {user.Id} not found.");
                }

                existingUser.Name = user.Name;
                existingUser.Email = user.Email;
                existingUser.Weight = user.Weight;
                existingUser.Height = user.Height;

                Log.Information("Saving changes to database for user ID: {UserId}", user.Id);
                context.SaveChanges();
                Log.Information("Changes saved successfully for user ID: {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving changes for user ID: {UserId}", user.Id);
                throw;
            }
        }
    }
}