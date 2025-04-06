using DAL.Models;

namespace BLL.Service
{
    public interface IUserService
    {
        Task<User?> GetUserById(string id);
        Task UpdateUser(User user);
    }
}
