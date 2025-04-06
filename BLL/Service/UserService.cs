using DAL.Models;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;

        public UserService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

       
        public async Task<User?> GetUserById(string id)
        {
            // Отримуємо користувача за ID через UserManager
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                Log.Warning("Користувача з ID {UserId} не знайдено", id);
                return null;
            }

            Log.Information("Отримано користувача з ID {UserId}", id);
            return user;
        }

       
        public async Task UpdateUser(User user)
        {
            if (user == null)
            {
                Log.Error("Спроба оновити null користувача");
                throw new ArgumentNullException(nameof(user));
            }

            Log.Information("Оновлення користувача ID: {UserId}", user.Id);
          
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    Log.Error("Помилка оновлення користувача ID {UserId}: {Description}", user.Id, error.Description);
                }
                throw new InvalidOperationException("Не вдалося оновити користувача");
            }

            Log.Information("Користувача ID {UserId} успішно оновлено", user.Id);
        }
    }
}