using System;
using MySql.Data;
using MySql.Data.MySqlClient;

using static System.Configuration.ConfigurationManager;

namespace TicketManagementSystem
{
    public class UserRepository : IDisposable
    {
        private MySqlConnection connection;
        
        public UserRepository()
        {
            InitDb();
        }

        private void InitDb()
        {
            string connectionString = ConnectionStrings["database"]?.ConnectionString; 
            connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"DB Connection Not valid: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }
        
        public User GetUser(string username)
        {
            // Assume this method does not need to change and is connected to a database with users populated.

            // Note: I used a MySQL Database to hold the users, which required a few changes to the method!
            // - SQL Syntax is different (No TOP 1 in MySQL)
            // - ExecuteScalar was only returning the username and not the username object so switched to MySQLDataReader
            // - Added Error Message to catch for my debugging purposes  
            try
            {
                string sql = "SELECT * FROM Users u WHERE u.Username = @p1 LIMIT 1";
                connection.Open();

                MySqlCommand command = new(sql, connection)
                {
                    CommandType = System.Data.CommandType.Text,
                };

                command.Parameters.AddWithValue("@p1", username);

                MySqlDataReader reader = command.ExecuteReader();
                User user = null;
                while (reader.Read())
                {
                    user = new();
                    user.Username = reader.GetString(0);
                    user.FirstName = reader.GetString(1);
                    user.LastName = reader.GetString(2);
                }

                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Reading Database {ex.Message}");
                return null;
            }
            finally
            {
                connection.Close();
            }
        }

        public User GetAccountManager()
        {
            // Assume this method does not need to change.
            return GetUser("Sarah");
        }

        public void Dispose()
        {
            // Assume this method does not need to change.
            connection.Dispose();
        }
    }
}
