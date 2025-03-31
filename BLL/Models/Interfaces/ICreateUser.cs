using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace BLL.Models.Interfaces
{
    public interface ICreateUser
    {
        Task<DAL.Models.User> CreateNewUser(string nickname, string email, string password);
    }
}
