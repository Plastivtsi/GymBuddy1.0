using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Exercise
    {
        public int Id { get; set; }
        public int TrainingId { get; set; }
        public double Weight { get; set; }
        public int Repetitions { get; set; }
        public string Notes { get; set; }

        // Навігаційні властивості
        public Training Training { get; set; }
    }
}
