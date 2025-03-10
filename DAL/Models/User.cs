using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public double Weight { get; set; }
        public double Height { get; set; }
        public bool Role { get; set; } // true = Admin, false = Non-Admin

        // Навігаційні властивості
        public List<Training> Trainings { get; set; }
        public List<Friendship> FriendshipsAsUser1 { get; set; }
        public List<Friendship> FriendshipsAsUser2 { get; set; }
    }
}
