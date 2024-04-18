using Npgsql;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Database_Projekt
{
    public class User
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public interface IRepository
    {
        void InsertUser(User user);
        User GetUser(string username);
    }

    public class InMemoryRepository : IRepository
    {
        private List<User> _users = new List<User>();

        public User GetUser(string username)
        {
            try
            {
                return _users.First(x => x.username == username);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void InsertUser(User user)
        {
            bool alreadyExists = _users.Exists(x => x.username == user.username);
            if (alreadyExists)
                throw new Exception($"Username {user.username} already exists");

            _users.Add(user);
        }
    }

    public class PostgresRepository : IRepository
    {
        private NpgsqlDataSource dataSource;

        public PostgresRepository()
        {
            string connectionString = "Host=localhost;Username=postgres;Password=Saunire.124;Database=mydb";
            dataSource = NpgsqlDataSource.Create(connectionString);
        }

        public User GetUser(string username)
        {
            NpgsqlCommand cmdGetPassword = dataSource.CreateCommand(
                "SELECT password FROM users WHERE username = $1");
            cmdGetPassword.Parameters.AddWithValue(username);

            NpgsqlDataReader reader = cmdGetPassword.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return new User
            {
                username = username,
                password = reader.GetString(0),
            };
        }

        public void InsertUser(User user)
        {
            NpgsqlCommand cmdInsert = dataSource.CreateCommand(
                "INSERT INTO users (username, password) VALUES ($1, $2)");

            cmdInsert.Parameters.AddWithValue(user.username);
            cmdInsert.Parameters.AddWithValue(user.password);

            cmdInsert.ExecuteNonQuery();
        }
    }
}