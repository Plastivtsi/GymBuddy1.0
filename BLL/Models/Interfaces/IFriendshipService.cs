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
        Task Unfollow(int userId1, int userId2);
        Task Follow(int userId1, int userId2);
        Task<List<DAL.Models.Friendship>> GetFriendshipRequests(int userId);
        Task<List<DAL.Models.User>> FriendshipRequestList(int userId);
        Task<List<DAL.Models.User>> FriendshipBannedList(int userId);
        Task UnBlock(int userId1, int userId2);
        Task Block(int userId1, int userId2);
        Task AcceptRequest(int userId1, int userId2);


    }
}
