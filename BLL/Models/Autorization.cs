using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using Npgsql;

namespace BLL.Models
{
    public class Autorization
    {
        private const string ConnectionString = "Host=breakdatabase.postgres.database.azure.com;Port=5432;Database=GymBuddy;Username=postgres;Password=12345678bp!";
        private static int currentUserId;
        public static int CurrentUserId { get => currentUserId; set => currentUserId = value; }

        public bool Register(string nickname, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(nickname) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("All fields must be filled.");
            }

            if (!email.Contains("@"))
            {
                throw new ArgumentException("Invalid email format.");
            }

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using (var checkCommand = new NpgsqlCommand("SELECT COUNT(*) FROM \"Users\" WHERE \"Name\" = @Nickname", connection))
                {
                    checkCommand.Parameters.AddWithValue("@Nickname", nickname);
                    var userExists = (long)checkCommand.ExecuteScalar() > 0;

                    if (userExists)
                    {
                        throw new InvalidOperationException("User with this nickname already exists.");
                    }
                }

                using (var insertCommand = new NpgsqlCommand(
                    "INSERT INTO \"Users\" (\"Name\", \"Password\", \"Email\", \"Role\") VALUES (@Nickname, @Password, @Email, @IsAdmin)", connection))
                {
                    insertCommand.Parameters.AddWithValue("@Nickname", nickname);
                    insertCommand.Parameters.AddWithValue("@Password", password);
                    insertCommand.Parameters.AddWithValue("@Email", email);
                    insertCommand.Parameters.AddWithValue("@IsAdmin", false);

                    var rowsAffected = insertCommand.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        Console.WriteLine($"User registered: {nickname}, {email}");
                        return true;
                    }
                }
            }

            return false;
        }

        public bool Login(string nickname, string password)
        {
            if (string.IsNullOrWhiteSpace(nickname) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Both fields must be filled.");
            }

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using (var checkCommand = new NpgsqlCommand("SELECT \"Id\", \"Password\" FROM \"Users\" WHERE \"Name\" = @Nickname", connection))
                {
                    checkCommand.Parameters.AddWithValue("@Nickname", nickname);
                    using (var reader = checkCommand.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            throw new InvalidOperationException("User not found.");
                        }

                        int userId = reader.GetInt32(0);
                        string storedPassword = reader.GetString(1);

                        if (storedPassword != password)
                        {
                            throw new InvalidOperationException("Invalid password.");
                        }

                        CurrentUserId = userId;
                        Console.WriteLine($"User logged in successfully. UserID: {CurrentUserId}");
                        return true;
                    }
                }
            }
        }
    }
}
