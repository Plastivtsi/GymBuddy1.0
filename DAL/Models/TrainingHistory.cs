using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class TrainingHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime TrainingDate { get; set; }
        public TimeSpan Duration { get; set; }
        public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
    }

}
