using Npgsql;
using System;
using System.Collections.Generic;

namespace Database_Projekt
{
    public class UserRegistrationWithPattern
    {
        private readonly IRepository repository;
        NpgsqlDataSource dataSource;
        string connectionString = "Host=localhost;Username=postgres;Password=100899;Database=postgres";
        int amountToBuy;
        int amountToSell;
        int amountAvaliable;
        string wantToBuy;
        string stockChosen;

        public UserRegistrationWithPattern(IRepository repository)
        {
            this.repository = repository;
        }

        public void RunLoop()
        {
            dataSource = NpgsqlDataSource.Create(connectionString);

            CreateTables();

            Insert();

            Console.WriteLine("WELCOME TO BIG BUCKS:\n" +
                "1) Create a new game, 2) Load an existing one");
            string userInput = Console.ReadLine();
            switch (userInput)
            {
                case "1":
                    Update(CreateNewPlayer());
                    break;
                case "2":
                    LoadExistingPlayer();
                    break;
                default:
                    Console.WriteLine("Invalid input");
                    break;
            }

        }

        private string CreateNewPlayer()
        {
            Console.WriteLine("Choose username:");
            string inputUsername = Console.ReadLine();
            try
            {
                repository.InsertUser(new User
                {
                    username = inputUsername
                });

                NpgsqlCommand cmd = dataSource.CreateCommand($"SELECT char_name, capital FROM player WHERE (char_name = '{inputUsername}')");
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Console.WriteLine($"Yo {inputUsername}, welcome to BIG BUCKS!\nYou have {reader.GetInt16(1)} bucks");
                }
                reader.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to regiser, try a different username");
            }
            return inputUsername;
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
            NpgsqlCommand cmdCreateStocksTable = dataSource.CreateCommand(@"
                CREATE TABLE IF NOT EXISTS stocks (
                    stock_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    name VARCHAR(255) NOT NULL UNIQUE,
                    price INT NOT NULL,
                    amount INT NOT NULL,
                    avaliable BOOL NOT NULL,
                    purchase_history DATE
                );");

            NpgsqlCommand cmdCreatePortfolioTable = dataSource.CreateCommand(@"
                CREATE TABLE IF NOT EXISTS portfolio (
                    port_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    total_value INT NOT NULL
                );");

            NpgsqlCommand cmdCreatePlayerTable = dataSource.CreateCommand(@"
                CREATE TABLE IF NOT EXISTS player (
                    char_name VARCHAR(255) PRIMARY KEY,
                    capital INT DEFAULT 1000
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

        }

        private void Insert()
        {
            //BRUG EN READER TIL AT TJEKKE OM VÆRDIEN ALLEREDE FINDES

            try
            {
                NpgsqlCommand cmdInsertStocks = dataSource.CreateCommand($@"
            INSERT INTO stocks (name, price, amount, avaliable) 
            
            VALUES('Mærsk', 1000, 100, true),
                   ('Novo Nordisk', 500, 100, true),
                   ('PostNord', 50, 300, true)
            ");
            }
            catch (Exception)
            {

            }

            //cmdInsertStocks.ExecuteNonQuery();

            Console.WriteLine("Stocks inserted");
        }

        private void Update(string inputUsername)
        {
            Console.WriteLine("Do you want to buy or sell stocks?\nType BUY to buy or SELL to sell");

            wantToBuy = Console.ReadLine().ToLower();

            if (wantToBuy == "buy")
            {
                Console.WriteLine("Which company would you like to purchase stocks from?");
                Console.WriteLine("Name: Mærsk, Price: 1000, Amount: 100\n" +
                    "Name: Novo Nordisk, Price: 500, Amount: 100\n" +
                    "Name: PostNord, Price: 50, Amount: 300");
                stockChosen = Console.ReadLine();

                Console.WriteLine("How many stocks would you like to buy?");
                amountToBuy = int.Parse(Console.ReadLine());

                NpgsqlCommand cmdBuyStocks = dataSource.CreateCommand($@"
            UPDATE stocks
            SET amount = amount - {amountToBuy}
            WHERE name = '{stockChosen}'
            ");

                NpgsqlCommand cmd = dataSource.CreateCommand($"SELECT char_name, capital FROM player WHERE (char_name = '{inputUsername}')");
                NpgsqlDataReader reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    int bucks = reader.GetInt16(1);
                    if (amountToBuy <= amountAvaliable && bucks >= 50)
                    {
                        cmdBuyStocks.ExecuteNonQuery();

                        Console.WriteLine("Stocks bought");
                        Console.ReadLine();
                    }
                    else if (amountToBuy > amountAvaliable)
                    {
                        Console.WriteLine("Sorry, that many stocks aren't avaliable right now");
                    }
                    else
                    {
                        if (wantToBuy == "sell")
                        {
                            Console.WriteLine("Which company stocks would you like to sell?");
                            stockChosen = Console.ReadLine();

                            Console.WriteLine("How many stocks would you like to sell?");
                            amountToSell = int.Parse(Console.ReadLine());

                            NpgsqlCommand cmdSellStocks = dataSource.CreateCommand($@"
            UPDATE stocks
            SET amount = amount + {amountToSell}
            WHERE name = '{stockChosen}'
            ");

                            cmdSellStocks.ExecuteNonQuery();

                            Console.WriteLine("Stocks sold");
                            Console.ReadLine();
                        }
                    }
                    reader.Close();
                }
            }
        }
    }
}