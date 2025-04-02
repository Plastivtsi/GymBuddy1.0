using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.Models;

namespace BLL.Service
{
    public interface ITrainingHistoryService
    {
        Task<IEnumerable<Training>> GetUserTrainingHistory(int userId);
    }
}
