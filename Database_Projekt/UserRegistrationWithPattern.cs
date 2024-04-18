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
            NpgsqlCommand cmdCreateStocksTable = dataSource.CreateCommand(@"
                CREATE TABLE IF NOT EXISTS stocks (
                    stock_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    name VARCHAR(255) NOT NULL UNIQUE,
                    price INT NOT NULL,
                    amount INT NOT NULL,
                    avaliable BOOL NOT NULL,
                    purchase_history DATE NOT NULL
                );");

            NpgsqlCommand cmdCreatePortfolioTable = dataSource.CreateCommand(@"
                CREATE TABLE IF NOT EXISTS portfoilio (
                    port_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    total_value INT NOT NULL
                );");

            //TJEK GENERATED ALWAS AS IDENTITY
            NpgsqlCommand cmdCreatePlayerTable = dataSource.CreateCommand(@"
                CREATE TABLE IF NOT EXISTS player (
                    char_name VARCHAR(255) ALWAYS AS IDENTITY PRIMARY KEY, 
                    capital INT NOT NULL
                );");

            NpgsqlCommand cmdCreateAbilitiesTable = dataSource.CreateCommand(@"
                CREATE TABLE IF NOT EXISTS abilities (
                    ability_name VARCHAT(255) GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    char_name VARCHAR(255) ALWAYS AS IDENTITY FOREIGN KEY,
                    level INT NOT NULL,
                    cost INT NOT NULL,
                    unlocked BOOL NOT NULL
                );");
            NpgsqlCommand cmdCreateContainsTable = dataSource.CreateCommand(@"
                CREATE TABLE IF NOT EXISTS contains (
                    stock_id INT FROM stocks,
                    port_id INT FROM portfolio,
                    price_purchased_at INT NOT NULL,
                    purchase_date INT NOT NULL
                );");
            NpgsqlCommand cmdCreatePortHasTable = dataSource.CreateCommand(@"
                CREATE TABLE IF NOT EXISTS has (
                    port_id INT FROM portfolio,
                    char_name VARCHAR(255) FROM player
                );");


            //KALD CREATE TABLE
            cmdCreateAbilitiesTable.ExecuteNonQuery();
            cmdCreateContainsTable.ExecuteNonQuery();
            cmdCreatePlayerTable.ExecuteNonQuery();
            cmdCreatePortfolioTable.ExecuteNonQuery();
            cmdCreatePortHasTable.ExecuteNonQuery();
            cmdCreateStocksTable.ExecuteNonQuery();
        }
    }
}