using DAL.Models;

namespace BLL.Interfaces
{
    public interface IUserService
    {
        User GetUserById(string id);
        void UpdateUser(User user);
    }
}
