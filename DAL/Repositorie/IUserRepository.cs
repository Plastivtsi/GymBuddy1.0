using DAL.Models;
namespace DAL.Interfaces
{
    public interface IUserRepository
    {
        User? GetById(int id); 
        void Update(User user);
    }
}
