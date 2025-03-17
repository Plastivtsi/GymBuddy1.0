using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace BLL.Models
{
    public class Friends
    {
        private const string ConnectionString = "Host=breakdatabase.postgres.database.azure.com;Port=5432;Database=GymBuddy;Username=postgres;Password=12345678bp!";
        public void AddFriendToMe(string NameFriend)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();

                    int friendId;
                    using (var command = new NpgsqlCommand("SELECT \"Id\" FROM \"Users\" WHERE \"Name\" = @FriendNickname", connection))
                    {
                        command.Parameters.AddWithValue("@FriendNickname", NameFriend);

                        var result = command.ExecuteScalar();
                        if (result == null)
                        {
                            Console.WriteLine("This user does not exist");
                            return;
                        }

                        friendId = (int)result;
                    }

                    using (var checkCommand = new NpgsqlCommand(
                        "SELECT COUNT(*) FROM \"Friendships\" " +
                        "WHERE (\"User1Id\" = @User1Id AND \"User2Id\" = @User2Id) OR (\"User1Id\" = @User2Id AND \"User2Id\" = @User1Id)", connection))
                    {
                        checkCommand.Parameters.AddWithValue("@User1Id", Autorization.CurrentUserId);
                        checkCommand.Parameters.AddWithValue("@User2Id", friendId);

                        var existingFriendship = (long)checkCommand.ExecuteScalar();
                        if (existingFriendship > 0)
                        {
                            Console.WriteLine("This friendship already exists.");
                            return;
                        }
                    }

                    using (var insertCommand = new NpgsqlCommand(
                        "INSERT INTO \"Friendships\" (\"User1Id\", \"User2Id\") VALUES (@User1Id, @User2Id)", connection))
                    {
                        insertCommand.Parameters.AddWithValue("@User1Id", Autorization.CurrentUserId);
                        insertCommand.Parameters.AddWithValue("@User2Id", friendId);

                        var rowsAffected = insertCommand.ExecuteNonQuery();
                        Console.WriteLine(rowsAffected > 0 ? "Friend added successfully." : "Failed to add friend.");
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid operation: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        public void RemoveFriend(int friendId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    try
                    {
                        connection.Open();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error opening database connection: " + ex.Message);
                        throw new InvalidOperationException("Unable to connect to the database.", ex);
                    }

                    using (var command = new NpgsqlCommand(
                        "DELETE FROM \"Friendships\" WHERE (\"User1Id\" = @CurrentId AND \"User2Id\" = @FriendId) " +
                        "OR (\"User1Id\" = @FriendId AND \"User2Id\" = @CurrentId)", connection))
                    {
                        try
                        {
                            command.Parameters.AddWithValue("@CurrentId", Autorization.CurrentUserId);
                            command.Parameters.AddWithValue("@FriendId", friendId);

                            var rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected == 0)
                            {
                                Console.WriteLine("No friendship record found to delete.");
                                throw new InvalidOperationException("The specified friendship does not exist.");
                            }

                            Console.WriteLine($"Successfully removed friendship with FriendId: {friendId}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error executing SQL command: " + ex.Message);
                            throw new InvalidOperationException("Failed to remove friendship.", ex);
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine("Database error: " + ex.Message);
                throw new InvalidOperationException("A database error occurred.", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error: " + ex.Message);
                throw new InvalidOperationException("An unexpected error occurred.", ex);
            }
        }
    }
}
