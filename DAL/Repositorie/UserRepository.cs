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
            var existingUser = this.context.Users.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser != null)
            {
                bool isModified = false;

                if (existingUser.Name != user.Name)
                {
                    existingUser.Name = user.Name;
                    isModified = true;
                }

                if (existingUser.Email != user.Email)
                {
                    existingUser.Email = user.Email;
                    isModified = true;
                }

                if (existingUser.Weight != user.Weight)
                {
                    existingUser.Weight = user.Weight;
                    isModified = true;
                }

                if (existingUser.Height != user.Height)
                {
                    existingUser.Height = user.Height;
                    isModified = true;
                }

                if (isModified)
                {
                    try
                    {
                        var changes = this.context.SaveChanges();
                        if (changes == 0)
                        {
                            throw new InvalidOperationException("No changes were made.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error saving changes: {ex.Message}");
                        throw;
                    }
                }
                else
                {
                    Console.WriteLine("No changes detected.");
                }
            }
            else
            {
                throw new InvalidOperationException("User not found");
            }
        }
    }
}