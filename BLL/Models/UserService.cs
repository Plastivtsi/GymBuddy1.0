using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using static BLL.Models.UserService;
using static System.Net.Mime.MediaTypeNames;

namespace BLL.Models
{
    public class UserService
    {
        private const string ConnectionString = "Host=breakdatabase.postgres.database.azure.com;Port=5432;Database=GymBuddy;Username=postgres;Password=12345678bp!";
        public User GetUserById(int userId)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand("SELECT \"Name\", \"Email\", \"Password\", \"Weight\", \"Height\", \"Role\"  FROM \"Users\" WHERE \"Id\" = @UserId", connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                UserName = reader.GetString(0),
                                Email = reader.GetString(1),
                                Password = reader.GetString(2),
                                Weight = reader.GetDouble(3),
                                Height = reader.GetDouble(4),
                                Role = reader.GetBoolean(5),
                            };
                        }
                    }
                }
            }

            return null;
        }

        public bool UpdateUser(int userId, string userName, string email, string newPassword, double height, double weight)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand("UPDATE \"Users\" SET \"Name\" = @UserName, \"Email\" = @Email, \"Password\" = @Password, \"Weight\" = @Weight, \"Height\" = @Height WHERE \"Id\" = @UserId", connection))
                {
                    command.Parameters.AddWithValue("@UserName", userName);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Password", newPassword);
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("Height", height);
                    command.Parameters.AddWithValue("@Weight", weight);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }
    }

    public class User
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public double Weight { get; set; }
        public double Height { get; set; }
        public bool Role { get; set; } // true = Admin, false = Non-Admin
    }
}
