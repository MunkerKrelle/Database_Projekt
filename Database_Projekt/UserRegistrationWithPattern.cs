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
                Console.WriteLine("Choose to either: 1) Regiser a new user, 2) Login with existing user");
                string userInput = Console.ReadLine();
                switch (userInput)
                {

                    case "1":
                        RegisterNewUser();
                        break;
                    case "2":
                        LoginWithExistingUser();
                        break;
                    default:
                        Console.WriteLine("Invalid input");
                        break;
                }

                Console.WriteLine();
            }
        }

        private void RegisterNewUser()
        {
            Console.WriteLine("Choose username:");
            string inputUsername = Console.ReadLine();

            Console.WriteLine("Choose password:");
            string inputPassword = Console.ReadLine();

            try
            {
                repository.InsertUser(new User
                {
                    username = inputUsername,
                    password = inputPassword
                });
                Console.WriteLine("Successfully registered user");
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to regiser, try a different username");
            }
        }

        private void LoginWithExistingUser()
        {
            Console.WriteLine("Specify your username:");
            string inputUsername = Console.ReadLine();

            Console.WriteLine("Specify your password:");
            string inputPassword = Console.ReadLine();

            User foundUser = repository.GetUser(inputUsername);
            if (foundUser == null)
            {
                Console.WriteLine("Invalid credentials");
                return;
            }

            string storedPassword = foundUser.password;
            if (inputPassword == storedPassword)
                Console.WriteLine("Successfully logged in!");
            else
                Console.WriteLine("Invalid credentials");
        }

        private void CreateTables()
        {
            //NpgsqlCommand cmdCreateUsersTable =  dataSource.CreateCommand(@"
            //    CREATE TABLE IF NOT EXISTS users (
            //        user_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            //        username VARCHAR(255) NOT NULL UNIQUE,
            //        password VARCHAR(255) NOT NULL
            //    );");

            //NpgsqlCommand cmdCreateLoginAttemptsTable = dataSource.CreateCommand(@"
            //    CREATE TABLE IF NOT EXISTS login_attempts (
            //        attempt_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            //        user_id INTEGER NOT NULL,
            //        attempt_time TIMESTAMP NOT NULL,
            //        success BOOLEAN NOT NULL,
            //        FOREIGN KEY (user_id) REFERENCES users(user_id)
            //    );");

            //cmdCreateUsersTable.ExecuteNonQuery();
            //cmdCreateLoginAttemptsTable.ExecuteNonQuery();
        }
    }
}