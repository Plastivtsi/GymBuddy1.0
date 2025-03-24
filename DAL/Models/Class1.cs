using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class WorkoutPlan
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public List<Training> Trainings { get; set; } = new List<Training>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
