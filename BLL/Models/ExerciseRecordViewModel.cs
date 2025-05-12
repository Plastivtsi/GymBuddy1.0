using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class ExerciseRecordViewModel
    {
        public string ExerciseName { get; set; }
        public double MaxWeight { get; set; }
        public int MaxReps { get; set; }
    }
}
