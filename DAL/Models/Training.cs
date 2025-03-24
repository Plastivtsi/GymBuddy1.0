using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Training
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Description { get; set; }
        public int UserId { get; set; }
        public bool Template { get; set; }

        // Навігаційні властивості
        public User User { get; set; }
        public List<Exercise> Exercises { get; set; }
    }
}
