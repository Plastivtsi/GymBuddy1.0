using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DAL.Models;

namespace BLL.Models.Interfaces
{
    public interface ITrainingCalendar
    {
        Task<List<DAL.Models.Training>> GetTrainingsByDateRangeAsync(DateTime startDate, DateTime endDate, int userId);
        Task<Dictionary<int, (int CurrentYearCount, int PreviousYearCount)>> GetMonthlyTrainingCountsAsync(int userId, int currentYear);
    }
}
