using System;
using System.Threading.Tasks;
using DAL.Models;

namespace BLL.Models
{
    public interface ICreateTrainings
    {
        Task<DAL.Models.Training> CreateNewTrainingAsync(string name, DateTime date, TimeSpan time, string description, int userId);
    }
}