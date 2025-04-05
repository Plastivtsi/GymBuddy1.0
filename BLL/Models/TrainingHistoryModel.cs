using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;
namespace BLL.Models
{
    public class TrainingHistoryModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public DateTime? Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Description { get; set; }
        public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
    }

}
