using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.Models;

namespace BLL.Service
{
    public interface ITrainingHistoryService
    {
        /// <summary>
        /// Отримати історію тренувань користувача, виключаючи тренування, де всі вправи мають нульову вагу.
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача</param>
        /// <returns>Список тренувань користувача</returns>
        Task<IEnumerable<Training>> GetUserTrainingHistory(int userId);
    }
}
