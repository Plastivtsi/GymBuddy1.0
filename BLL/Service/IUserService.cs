using DAL.Models;

namespace DAL.Interfaces
{
    public interface IUserService
    {
        User GetUserById(string id);
        void UpdateUser(User user);
    }
}
