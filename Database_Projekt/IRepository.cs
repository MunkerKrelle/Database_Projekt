using Npgsql;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Database_Projekt
{
    public class User
    {
        public string username { get; set; }
        public int capital { get; set; }
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
        public NpgsqlDataSource dataSource;

        public PostgresRepository()
        {
            string connectionString = "Host=localhost;Username=postgres;Password=sargon;Database=ovelse2";
            dataSource = NpgsqlDataSource.Create(connectionString);
        }

        public User GetUser(string username)
        {
            return new User
            {
                username = username
            };
        }

        public void InsertUser(User user)
        {
            NpgsqlCommand cmdInsert = dataSource.CreateCommand(
                "INSERT INTO player (char_name) VALUES ($1)");

            cmdInsert.Parameters.AddWithValue(user.username);

            cmdInsert.ExecuteNonQuery();
        }
    }
}