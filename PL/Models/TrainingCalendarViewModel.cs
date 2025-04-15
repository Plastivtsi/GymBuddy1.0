using DAL.Models;
using System.Collections.Generic;

namespace PL.ViewModels
{
    public class MonthlyTrainingCount
    {
        public int CurrentYearCount { get; set; }
        public int PreviousYearCount { get; set; }
    }

    public class TrainingCalendarViewModel
    {
        public List<Training> Trainings { get; set; }

        public Dictionary<int, MonthlyTrainingCount> MonthlyTrainingCounts { get; set; }

        public int CurrentYear { get; set; }
    }
}
