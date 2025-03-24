using DAL.Interfaces;
using DAL.Models;

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
            return this.context.Users.FirstOrDefault(u => u.Id == id);
        }

        public void Update(User user)
        {
            this.context.Users.Update(user);
            this.context.SaveChanges();
        }
    }
}
