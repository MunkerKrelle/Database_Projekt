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
            string connectionString = "Host=localhost;Username=postgres;Password=Saunire.124;Database=myDatabase";
            dataSource = NpgsqlDataSource.Create(connectionString);

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
                CREATE TABLE IF NOT EXISTS portfolio (
                    port_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    total_value INT NOT NULL
                );");

            NpgsqlCommand cmdCreatePlayerTable = dataSource.CreateCommand(@"
                CREATE TABLE IF NOT EXISTS player (
                    char_name VARCHAR(255) PRIMARY KEY,
                    capital INT NOT NULL
                );");

            NpgsqlCommand cmdCreateAbilitiesTable = dataSource.CreateCommand(@"
                CREATE TABLE IF NOT EXISTS abilities (
                    ability_name VARCHAR(255) PRIMARY KEY,
                    level INT NOT NULL,
                    cost INT NOT NULL,
                    unlocked BOOL NOT NULL,
                    char_name VARCHAR(255) REFERENCES player(char_name)


                );");
            NpgsqlCommand cmdCreateContainsTable = dataSource.CreateCommand(@"
                CREATE TABLE IF NOT EXISTS contains (
                    stock_id INT REFERENCES stocks(stock_id),
                    port_id INT REFERENCES portfolio(port_id),
                    price_purchased_at INT NOT NULL,
                    purchase_date INT NOT NULL
                );");
            NpgsqlCommand cmdCreateHasTable = dataSource.CreateCommand(@"
                CREATE TABLE IF NOT EXISTS has (
                    port_id INT REFERENCES portfolio(port_id),
                    char_name VARCHAR(255) REFERENCES player(char_name)
                );");


            //KALD CREATE TABLE
            cmdCreatePortfolioTable.ExecuteNonQuery();
            cmdCreateStocksTable.ExecuteNonQuery();
            cmdCreatePlayerTable.ExecuteNonQuery();
            cmdCreateAbilitiesTable.ExecuteNonQuery();
            cmdCreateContainsTable.ExecuteNonQuery();
            cmdCreateHasTable.ExecuteNonQuery();



            Console.WriteLine("Tables created");
            Console.ReadLine();
        }
    }
}