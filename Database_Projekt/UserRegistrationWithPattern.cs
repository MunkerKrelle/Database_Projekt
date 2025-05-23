﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace Database_Projekt
{
    public class UserRegistrationWithPattern
    {
        private readonly IRepository repository;
        NpgsqlDataSource dataSource;
        string connectionString = "Host=localhost;Username=postgres;Password=100899;Database=postgres";
        int amountToBuy;
        int amountToSell;
        int amountCost;
        int day = 1;
        private int randomInt = 10;
        string wantToBuy;
        string stockChosen;
        string inputUsername;

        public UserRegistrationWithPattern(IRepository repository)
        {
            this.repository = repository;
        }

        public void RunLoop()
        {
            dataSource = NpgsqlDataSource.Create(connectionString);

            DropTables();
            CreateTables();
            Insert();

            Console.WriteLine("WELCOME TO BIG BUCKS:\n" +
                "1) Create a new game, 2) Load an existing one");
            while (true)
            {
                string userInput = Console.ReadLine();
                switch (userInput)
                {
                    case "1":
                        Update(CreateNewPlayer());
                        return;
                    case "2":
                        LoadExistingPlayer();
                        return;
                    default:
                        Console.WriteLine("Invalid input, try again");
                        break;
                }
            }

        }

        private string CreateNewPlayer()
        {
            Console.WriteLine("Choose username:");
            inputUsername = Console.ReadLine();
            try
            {
                repository.InsertUser(new User
                {
                    username = inputUsername,
                    capital = 1000
                });

                NpgsqlCommand cmd = dataSource.CreateCommand($"SELECT char_name, capital FROM player WHERE (char_name = '{inputUsername}')");
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Console.WriteLine($"\nYo {inputUsername}, welcome to BIG BUCKS!");
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
            while (true)
            {
                string inputUsername = Console.ReadLine();

                NpgsqlCommand cmd = dataSource.CreateCommand($"SELECT char_name, capital FROM player WHERE (char_name = '{inputUsername}')");
                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read() == false)
                {
                    Console.WriteLine("Invalid credentials, try again");
                }
                else
                {
                    User foundUser = repository.GetUser(inputUsername);
                    Console.WriteLine($"Yo {inputUsername}, welcome back to BIG BUCKS. You have {reader.GetValue(1)} bucks.");
                    Update(inputUsername);
                    break;
                }
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

            NpgsqlCommand cmdCreatePlayerTable = dataSource.CreateCommand(@"
                CREATE TABLE IF NOT EXISTS player (
                    char_name VARCHAR(255) PRIMARY KEY,
                    capital INT DEFAULT 1000
                );");

            NpgsqlCommand cmdCreatePortfolioTable = dataSource.CreateCommand(@"
                CREATE TABLE IF NOT EXISTS portfolio (
                    port_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    name VARCHAR(255) UNIQUE,
                    amount INT,
                    price_purchased_at INT,
                    total_value INT
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
            cmdCreateStocksTable.ExecuteNonQuery();
            cmdCreatePlayerTable.ExecuteNonQuery();
            cmdCreatePortfolioTable.ExecuteNonQuery();
            cmdCreateAbilitiesTable.ExecuteNonQuery();
            cmdCreateContainsTable.ExecuteNonQuery();
            cmdCreateHasTable.ExecuteNonQuery();

            Console.WriteLine("Tables created");
        }

        private void Insert()
        {
            try
            {
                NpgsqlCommand cmdInsertStocks = dataSource.CreateCommand($@"
            INSERT INTO stocks (name, price, amount, avaliable) 
            
            VALUES('Mærsk', 1000, 100, true),
                   ('Novo Nordisk', 500, 100, true),
                   ('PostNord', 50, 300, true)
            ");

                cmdInsertStocks.ExecuteNonQuery();

                Console.WriteLine("Stocks inserted");
            }
            catch (Exception)
            {

            }
        }

        private void Update(string inputUsername)
        {
            Console.WriteLine($"Day: {day}");

            Console.WriteLine("Do you want to buy or sell stocks?\nType BUY to buy or SELL to sell\n\nAlternatively, type CONTINUE to move forward to the next day and update prices");

            NpgsqlCommand cmd = dataSource.CreateCommand($"SELECT char_name, capital FROM player WHERE (char_name = '{inputUsername}')");
            NpgsqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine($"You have {reader.GetInt16(1)} bucks");
            }
            reader.Close();

            wantToBuy = Console.ReadLine().ToLower();

            if (wantToBuy.ToLower() == "buy")
            {
                BuyStocks();
                Update(inputUsername);
            }

            else if (wantToBuy.ToLower() == "sell")
            {
                SellStocks();
                SellFromPortfolio();
                Update(inputUsername);
            }
            else if(wantToBuy.ToLower() == "continue")
            {
                ForwardTime();
                Update(inputUsername);
            }
            else
            {
                Update(inputUsername);
            }
        }

        private void ForwardTime()
        {
            Console.WriteLine("Press ENTER to forward to the next day");
            Console.ReadKey();

            UpdateStocks();
            Console.Clear();
            day++;
        }

        private void SellStocks()
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

        }


        private void BuyStocks()
        {
            NpgsqlCommand cmd = dataSource.CreateCommand($"SELECT name, price, amount FROM stocks");
            NpgsqlDataReader reader = cmd.ExecuteReader();

            Console.WriteLine("Which company would you like to purchase stocks from?");
            while (reader.Read())
            {
                Console.WriteLine($"Name: {reader.GetValue(0)}, Price: {reader.GetValue(1)}, Amount: {reader.GetValue(2)}");
            }
            stockChosen = Console.ReadLine();

            Console.WriteLine("How many stocks would you like to buy?");
            amountToBuy = int.Parse(Console.ReadLine());

            cmd = dataSource.CreateCommand($"SELECT * FROM public.player\r\n" +
               $"INNER JOIN stocks ON stocks.stock_id = stock_id\r\n" +
               $"Where (char_name = '{inputUsername}' AND name = '{stockChosen}')");
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                if (amountToBuy <= (int)reader.GetValue(5) && (int)reader.GetValue(1) >= (int)reader.GetValue(4) * amountToBuy)
                {
                    amountCost = (int)reader.GetValue(4) * amountToBuy;
                    NpgsqlCommand cmdBuyStocks = dataSource.CreateCommand($@"
            UPDATE stocks
            SET amount = amount - {amountToBuy}
            WHERE name = '{stockChosen}'
            ");
                    cmdBuyStocks.ExecuteNonQuery();

                    cmdBuyStocks = dataSource.CreateCommand($@"
            UPDATE player
            SET capital =  capital - {amountCost}
            WHERE char_name = '{inputUsername}'
            ");
                    cmdBuyStocks.ExecuteNonQuery();
                    Console.WriteLine("Stocks bought");
                    BuyToPortfolio();

                    Console.WriteLine("Press ENTER for next day");
                    Console.ReadLine();
                    Console.Clear() ;
                }
                else
                {
                    Console.WriteLine("Sorry, that many stocks aren't avaliable right now");
                }
            }
            reader.Close();
        }


        private void BuyToPortfolio()
        {
            //IF ROW DOES NOT EXIST
            NpgsqlCommand cmd = dataSource.CreateCommand($"SELECT name FROM portfolio WHERE (name = '{stockChosen}')");
            NpgsqlDataReader reader = cmd.ExecuteReader();

            bool portfolioBool = reader.Read();
                if (portfolioBool == false)
                {
                    NpgsqlCommand cmdInsertIntoPortfolioAfterBuy = dataSource.CreateCommand($@"
            INSERT INTO portfolio (name, amount, price_purchased_at, total_value) 
            
            VALUES('{stockChosen}', '{amountToBuy}', '{amountCost}', '{amountCost}')
            ");

                    cmdInsertIntoPortfolioAfterBuy.ExecuteNonQuery();
                }
                else
                {
                    //IF ROW ALREADY EXISTS
                    NpgsqlCommand cmdUpdatePortfolioAfterBuy = dataSource.CreateCommand($@"
                UPDATE portfolio
                SET amount = amount + {amountToBuy}
                SET total_value + {amountCost}
                WHERE (name = '{stockChosen}')
                ");
                }

                cmd = dataSource.CreateCommand($"SELECT * FROM portfolio");
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Console.WriteLine($"Stock Name: {reader.GetValue(1)} Amount: {reader.GetValue(2)} Total Value: {reader.GetValue(4)}");
                }
                reader.Close();
            }

            private void SellFromPortfolio()
            {
                //INDSÆT EGENTLIG VÆRDI PÅ PRICE_PURCHASED_AT

                NpgsqlCommand cmdUpdatePortfolioAfterSale = dataSource.CreateCommand($@"
            UPDATE portfolio
            SET amount = amount - {amountToSell}
            WHERE name = '{stockChosen}'
            ");

                //if (amountToSell >= amountCost)
                //{
                //    NpgsqlCommand cmdDeleteFromTable = dataSource.CreateCommand($@"
                //    DELETE FROM portfolio
                //    WHERE name = '{stockChosen}'
                //    ");

                //    cmdDeleteFromTable.ExecuteNonQuery();
                //}

                cmdUpdatePortfolioAfterSale.ExecuteNonQuery();
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
            private void DropTables()
            {
                try
                {
                    NpgsqlCommand cmdDropTables = dataSource.CreateCommand(@"
            DROP TABLE has;
            DROP TABLE contains;
            DROP TABLE abilities;
            DROP TABLE portfolio;
            DROP TABLE player;
            DROP TABLE stocks;
            ");
                    cmdDropTables.ExecuteNonQuery();

                    Console.WriteLine("Tables dropped");
                }

                catch (Exception)
                {
                }
            }

        }
    }
