using BLL.Interfaces;
using DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class TrainingService2 : IGetTemplateTrainings, ICreateTrainingFromTemplate
    {
        private readonly IGetTemplateTrainings _getTemplateTrainings;
        private readonly ICreateTrainingFromTemplate _createTrainingFromTemplate;

        public TrainingService2(IGetTemplateTrainings getTemplateTrainings, ICreateTrainingFromTemplate createTrainingFromTemplate)
        {
            _getTemplateTrainings = getTemplateTrainings ?? throw new ArgumentNullException(nameof(getTemplateTrainings));
            _createTrainingFromTemplate = createTrainingFromTemplate ?? throw new ArgumentNullException(nameof(createTrainingFromTemplate));
        }

        public async Task<List<Training>> GetTemplateTrainingsWithExercisesAsync()
        {
            return await _getTemplateTrainings.GetTemplateTrainingsWithExercisesAsync();
        }

        public async Task<Training> CreateTrainingFromTemplateAsync(int templateTrainingId, int userId, List<Exercise> updatedExercises)
        {
            return await _createTrainingFromTemplate.CreateTrainingFromTemplateAsync(templateTrainingId, userId, updatedExercises);
        }
    }
}