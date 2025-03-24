using DAL.Interfaces;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return _userRepository.GetById(int.Parse(id));
        }

        public void UpdateUser(User user)
        {
            _userRepository.Update(user);
        }
    }
}
