using Npgsql;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Database_Projekt
{
    public class UserRegistrationWithPattern
    {
        private readonly IRepository repository;
        NpgsqlDataSource dataSource;
        string connectionString = "Host=localhost;Username=postgres;Password=sargon;Database=ovelse2";
        int amountToBuy;
        int amountToSell;
        int amountAvaliable;
        int day = 1;
        private int randomInt = 10;
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


            while (true)
            {
                Update();

                //Console.WriteLine("WELCOME TO BIG BUCKS\n" +
                //    ": 1) Create a new game, 2) Load an existing one");
                //string userInput = Console.ReadLine();
                //switch (userInput)
                //{

                //    case "1":
                //        CreateNewPlayer();
                //        break;
                //    case "2":
                //        LoadExistingPlayer();
                //        break;
                //    default:
                //        Console.WriteLine("Invalid input");
                //        break;
                //}

                //Console.WriteLine();
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
            NpgsqlCommand cmdCreateStocksTable = dataSource.CreateCommand(@"
                CREATE TABLE IF NOT EXISTS stocks (
                    stock_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    name VARCHAR(255) NOT NULL UNIQUE,
                    price INT NOT NULL,
                    amount INT NOT NULL,
                    avaliable BOOL NOT NULL,
                    purchase_history DATE
                );");

            NpgsqlCommand cmdUpdateStocksTable = dataSource.CreateCommand(@"
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
            cmdUpdateStocksTable.ExecuteNonQuery();
            cmdCreatePlayerTable.ExecuteNonQuery();
            cmdCreateAbilitiesTable.ExecuteNonQuery();
            cmdCreateContainsTable.ExecuteNonQuery();
            cmdCreateHasTable.ExecuteNonQuery();


            Console.WriteLine("Tables created");

        }

        private void Insert()
        {
            //BRUG EN READER TIL AT TJEKKE OM VÆRDIEN ALLEREDE FINDES

            NpgsqlCommand cmdInsertStocks = dataSource.CreateCommand($@"
            INSERT INTO stocks (name, price, amount, avaliable) 
            
            VALUES('Mærsk', 1000, 100, true),
                   ('Novo Nordisk', 500, 100, true),
                   ('PostNord', 50, 300, true),
                   ('bob', 35, 400, true)

            ");

            //cmdInsertStocks.ExecuteNonQuery();

            Console.WriteLine("Stocks inserted");
        }

        private void Update()
        {
            Console.WriteLine($"Day: {day}");

            Console.WriteLine("Do you want to buy or sell stocks?\nType BUY to buy or SELL to sell");

            wantToBuy = Console.ReadLine();

            if (wantToBuy == "buy")
            {
                Console.WriteLine("Which company would you like to purchase stocks from?");
                stockChosen = Console.ReadLine();

                Console.WriteLine("How many stocks would you like to buy?");
                amountToBuy = int.Parse(Console.ReadLine());

                NpgsqlCommand cmdBuyStocks = dataSource.CreateCommand($@"
            UPDATE stocks
            SET amount = amount - {amountToBuy}
            WHERE name = '{stockChosen}'
            ");

                if (amountToBuy <= amountAvaliable)
                {
                    cmdBuyStocks.ExecuteNonQuery();

                    Console.WriteLine("Stocks bought");
                    Console.ReadLine();
                }
                else if (amountToBuy > amountAvaliable)
                {
                    Console.WriteLine("Sorry, that many stocks aren't avaliable right now");
                }

                ForwardTime();

            }
            else if (wantToBuy == "sell")
            {
                SellStocks();

                ForwardTime();
            }

            else
            {
                Update();
            }

        }

        private void ForwardTime()
        {
            UpdateStocks();
            Console.WriteLine("Press ENTER to forward to the next day");
            Console.ReadKey();
            
            Console.Clear();
            day++;
        }

        private void SellStocks()
        {
            if (wantToBuy == "sell")
            {
                Console.WriteLine("Which company stocks would you like to sell?");
                stockChosen = Console.ReadLine();

                NpgsqlCommand cmd = dataSource.CreateCommand($"SELECT name FROM stocks WHERE (name = '{stockChosen}')");
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    if (stockChosen == reader.GetString(0))
                    {
                        Console.WriteLine("\nHow many stocks would you like to sell?\n");
                    }
                    else
                    {
                        Console.WriteLine("Sorry, invalid company name. Please try again");
                        SellStocks();
                    }
                }

                reader.Close();

                amountToSell = int.Parse(Console.ReadLine());

                NpgsqlCommand cmdSellStocks = dataSource.CreateCommand($@"
            UPDATE stocks
            SET amount = amount + {amountToSell}
            WHERE name = '{stockChosen}'
            ");

                cmdSellStocks.ExecuteNonQuery();

                Console.WriteLine("Stocks sold\n");

                Console.WriteLine("Press ENTER to forward to the next day");
                Console.ReadKey();

                Console.Clear();
                day++;

            }

            
        }

        private void MyRandom()
        {
            var rand = new Random();
            randomInt = rand.Next(-50, 51);
        }
        private void UpdateStocks()
        {
            MyRandom();

            NpgsqlCommand cmdUpdateStocksTable = dataSource.CreateCommand($@"
        UPDATE stocks 
        SET price = price + {randomInt}

        ");

            cmdUpdateStocksTable.ExecuteNonQuery();
            //Console.WriteLine($"{bob}" );
            Console.WriteLine("Stocks have been updated");
            Console.ReadLine();

        }
    }
}