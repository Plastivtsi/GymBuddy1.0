using DAL.Models;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface ICreateTrainingFromTemplate
    {
        Task<Training> CreateTrainingFromTemplateAsync(int templateTrainingId, int userId, List<Exercise> updatedExercises);
    }
}