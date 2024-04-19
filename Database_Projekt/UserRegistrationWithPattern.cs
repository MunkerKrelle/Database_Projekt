using Npgsql;
using System;

namespace Database_Projekt
{
    public class UserRegistrationWithPattern
    {
        private readonly IRepository repository;


        public UserRegistrationWithPattern(IRepository repository)
        {
            this.repository = repository;
        }

        public void RunLoop()
        {
            CreateTables();

            while (true)
            {
                Console.WriteLine("WELCOME TO BIG BUCKS\n" +
                    ": 1) Create a new game, 2) Load an existing one");
                string userInput = Console.ReadLine();
                switch (userInput)
                {

                    case "1":
                        CreateNewPlayer();
                        break;
                    case "2":
                        LoadExistingPlayer();
                        break;
                    default:
                        Console.WriteLine("Invalid input");
                        break;
                }

                Console.WriteLine();
            }
        }

        private void CreateNewPlayer()
        {
            Console.WriteLine("Choose username:");
            string inputUsername = Console.ReadLine();

            try
            {
                repository.InsertUser(new User
                {
                    username = inputUsername
                });
                Console.WriteLine($"Yo {inputUsername}, welcome to BIG BUCKS!");
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to regiser, try a different username");
            }
        }

        private void LoadExistingPlayer()
        {
            Console.WriteLine("Specify your username:");
            string inputUsername = Console.ReadLine();

            User foundUser = repository.GetUser(inputUsername);
            if (foundUser == null)
            {
                Console.WriteLine("Invalid credentials");
                return;
            }
            else
            {
                Console.WriteLine($"Yo {inputUsername}, welcome back to BIG BUCKS");
            }
        }

        private void CreateTables()
        {
            NpgsqlDataSource dataSource;
            string connectionString = "Host=localhost;Username=postgres;Password=100899;Database=postgres";
            dataSource = NpgsqlDataSource.Create(connectionString);
            NpgsqlCommand cmdCreateUsersTable = dataSource.CreateCommand(@"
                CREATE TABLE IF NOT EXISTS BB_Players (
                    username VARCHAR(255) NOT NULL UNIQUE PRIMARY KEY,
                    monet INT NOT NULL DEFAULT(0)
                
                );");

            NpgsqlCommand cmdCreateLoginAttemptsTable = dataSource.CreateCommand(@"
                CREATE TABLE IF NOT EXISTS BB_Stocks (
                    attempt_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    user_id INTEGER NOT NULL,
                    attempt_time TIMESTAMP NOT NULL,
                    success BOOLEAN NOT NULL,
                    FOREIGN KEY (user_id) REFERENCES users(user_id)
                );");

            cmdCreateUsersTable.ExecuteNonQuery();
            cmdCreateLoginAttemptsTable.ExecuteNonQuery();
        }
    }
}