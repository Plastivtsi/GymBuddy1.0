using DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IGetTemplateTrainings
    {
        Task<List<Training>> GetTemplateTrainingsWithExercisesAsync();
    }
}