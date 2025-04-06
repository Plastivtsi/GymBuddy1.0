using DAL.Interfaces;
using DAL.Models;
using Serilog;


namespace BLL.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User? GetUserById(string id)
        {
            if (int.TryParse(id, out var userId))
            {
                return _userRepository.GetById(userId);
            }
            return null; // Повертаємо null, якщо ID не вірний
        }

        public void UpdateUser(User user)
        {
            Log.Information("Оновлення користувача ID: {UserId}", user.Id);
            _userRepository.Update(user); // Оновлюємо дані користувача
        }
    }
}
