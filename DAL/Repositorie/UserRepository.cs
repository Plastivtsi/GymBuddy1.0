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
                context.Users.Update(user); // позначаємо як Modified
                context.SaveChanges();      // зберігаємо
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving changes: {ex.Message}");
                throw;
            }
        }
    }
}