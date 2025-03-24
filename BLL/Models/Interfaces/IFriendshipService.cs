using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models.Interfaces
{
    public interface IFriendshipService
    {
        Task<List<DAL.Models.User>> GetFriendsAsync(int userId);
        Task<List<DAL.Models.User>> SearchUserByName(string name);
    }
}
